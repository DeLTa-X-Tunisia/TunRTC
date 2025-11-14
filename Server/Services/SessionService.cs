using Microsoft.EntityFrameworkCore;
using TunRTC.Server.Data;
using TunRTC.Server.Models;
using TunRTC.Server.Models.DTOs;

namespace TunRTC.Server.Services;

public class SessionService : ISessionService
{
    private readonly TunRTCContext _context;

    public SessionService(TunRTCContext context)
    {
        _context = context;
    }

    public async Task<SessionResponse?> CreateSessionAsync(int userId, CreateSessionRequest request)
    {
        var session = new Session
        {
            SessionId = Guid.NewGuid().ToString(),
            Name = request.Name,
            CreatorId = userId,
            MaxParticipants = request.MaxParticipants,
            Type = request.Type,
            Status = SessionStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();

        var creator = await _context.Users.FindAsync(userId);

        return new SessionResponse
        {
            SessionId = session.SessionId,
            Name = session.Name,
            MaxParticipants = session.MaxParticipants,
            CurrentParticipants = 0,
            Status = session.Status,
            Type = session.Type,
            CreatedAt = session.CreatedAt,
            CreatorUsername = creator?.Username ?? "Unknown"
        };
    }

    public async Task<SessionDetailResponse?> JoinSessionAsync(int userId, string sessionId, string connectionId)
    {
        var session = await _context.Sessions
            .Include(s => s.Creator)
            .Include(s => s.Participants.Where(p => p.LeftAt == null))
            .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.Status == SessionStatus.Active);

        if (session == null)
        {
            return null;
        }

        // Check if session is full
        var activeParticipants = session.Participants.Count(p => p.LeftAt == null);
        if (activeParticipants >= session.MaxParticipants)
        {
            return null;
        }

        // Check if user is already in session
        var existingParticipant = session.Participants
            .FirstOrDefault(p => p.UserId == userId && p.LeftAt == null);

        if (existingParticipant == null)
        {
            var participant = new SessionParticipant
            {
                SessionId = session.Id,
                UserId = userId,
                ConnectionId = connectionId,
                JoinedAt = DateTime.UtcNow,
                Status = ParticipantStatus.Connected
            };

            _context.SessionParticipants.Add(participant);
            await _context.SaveChangesAsync();
        }
        else
        {
            // Update connection ID if reconnecting
            existingParticipant.ConnectionId = connectionId;
            existingParticipant.Status = ParticipantStatus.Connected;
            await _context.SaveChangesAsync();
        }

        return await GetSessionAsync(sessionId);
    }

    public async Task<SessionDetailResponse?> GetSessionAsync(string sessionId)
    {
        var session = await _context.Sessions
            .Include(s => s.Creator)
            .Include(s => s.Participants.Where(p => p.LeftAt == null))
            .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);

        if (session == null)
        {
            return null;
        }

        return new SessionDetailResponse
        {
            SessionId = session.SessionId,
            Name = session.Name,
            MaxParticipants = session.MaxParticipants,
            CurrentParticipants = session.Participants.Count(p => p.LeftAt == null),
            Status = session.Status,
            Type = session.Type,
            CreatedAt = session.CreatedAt,
            CreatorUsername = session.Creator.Username,
            Participants = session.Participants
                .Where(p => p.LeftAt == null)
                .Select(p => new ParticipantInfo
                {
                    Username = p.User.Username,
                    JoinedAt = p.JoinedAt,
                    Status = p.Status,
                    IsMuted = p.IsMuted,
                    IsVideoEnabled = p.IsVideoEnabled
                })
                .ToList()
        };
    }

    public async Task<List<SessionResponse>> GetActiveSessionsAsync()
    {
        var sessions = await _context.Sessions
            .Include(s => s.Creator)
            .Include(s => s.Participants.Where(p => p.LeftAt == null))
            .Where(s => s.Status == SessionStatus.Active)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return sessions.Select(s => new SessionResponse
        {
            SessionId = s.SessionId,
            Name = s.Name,
            MaxParticipants = s.MaxParticipants,
            CurrentParticipants = s.Participants.Count(p => p.LeftAt == null),
            Status = s.Status,
            Type = s.Type,
            CreatedAt = s.CreatedAt,
            CreatorUsername = s.Creator.Username
        }).ToList();
    }

    public async Task<bool> LeaveSessionAsync(int userId, string sessionId)
    {
        var session = await _context.Sessions
            .Include(s => s.Participants)
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);

        if (session == null)
        {
            return false;
        }

        var participant = session.Participants
            .FirstOrDefault(p => p.UserId == userId && p.LeftAt == null);

        if (participant == null)
        {
            return false;
        }

        participant.LeftAt = DateTime.UtcNow;
        participant.Status = ParticipantStatus.Disconnected;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> EndSessionAsync(int userId, string sessionId)
    {
        var session = await _context.Sessions
            .Include(s => s.Participants)
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);

        if (session == null || session.CreatorId != userId)
        {
            return false;
        }

        session.Status = SessionStatus.Ended;
        session.EndedAt = DateTime.UtcNow;

        // Disconnect all participants
        foreach (var participant in session.Participants.Where(p => p.LeftAt == null))
        {
            participant.LeftAt = DateTime.UtcNow;
            participant.Status = ParticipantStatus.Disconnected;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateParticipantStatusAsync(string connectionId, bool isMuted, bool isVideoEnabled)
    {
        var participant = await _context.SessionParticipants
            .FirstOrDefaultAsync(p => p.ConnectionId == connectionId && p.LeftAt == null);

        if (participant == null)
        {
            return false;
        }

        participant.IsMuted = isMuted;
        participant.IsVideoEnabled = isVideoEnabled;
        await _context.SaveChangesAsync();

        return true;
    }
}
