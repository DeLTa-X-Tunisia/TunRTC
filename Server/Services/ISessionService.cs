using TunRTC.Server.Models;
using TunRTC.Server.Models.DTOs;

namespace TunRTC.Server.Services;

public interface ISessionService
{
    Task<SessionResponse?> CreateSessionAsync(int userId, CreateSessionRequest request);
    Task<SessionDetailResponse?> JoinSessionAsync(int userId, string sessionId, string connectionId);
    Task<SessionDetailResponse?> GetSessionAsync(string sessionId);
    Task<List<SessionResponse>> GetActiveSessionsAsync();
    Task<bool> LeaveSessionAsync(int userId, string sessionId);
    Task<bool> EndSessionAsync(int userId, string sessionId);
    Task<bool> UpdateParticipantStatusAsync(string connectionId, bool isMuted, bool isVideoEnabled);
}
