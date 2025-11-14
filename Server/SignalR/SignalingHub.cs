using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TunRTC.Server.Data;
using TunRTC.Server.Services;

namespace TunRTC.Server.SignalR;

[Authorize]
public class SignalingHub : Hub
{
    private readonly ISessionService _sessionService;
    private readonly IConnectionManager _connectionManager;
    private readonly TunRTCContext _context;
    private readonly ILogger<SignalingHub> _logger;

    public SignalingHub(
        ISessionService sessionService,
        IConnectionManager connectionManager,
        TunRTCContext context,
        ILogger<SignalingHub> logger)
    {
        _sessionService = sessionService;
        _connectionManager = connectionManager;
        _context = context;
        _logger = logger;
    }

    private int GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        _logger.LogInformation("User {UserId} connected with ConnectionId {ConnectionId}", userId, Context.ConnectionId);

        // Log connection
        var ipAddress = Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var userAgent = Context.GetHttpContext()?.Request.Headers["User-Agent"].ToString() ?? "Unknown";

        _context.ConnectionLogs.Add(new Models.ConnectionLog
        {
            UserId = userId,
            ConnectionId = Context.ConnectionId,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            ConnectedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        var connectionInfo = _connectionManager.GetConnectionInfo(Context.ConnectionId);

        if (connectionInfo.HasValue)
        {
            var (_, sessionId) = connectionInfo.Value;

            // Notify other participants
            await Clients.OthersInGroup(sessionId).SendAsync("ParticipantLeft", new
            {
                userId,
                connectionId = Context.ConnectionId,
                timestamp = DateTime.UtcNow
            });

            _connectionManager.RemoveConnection(Context.ConnectionId);
        }

        // Update connection log
        var log = await _context.ConnectionLogs
            .FirstOrDefaultAsync(l => l.ConnectionId == Context.ConnectionId && l.DisconnectedAt == null);

        if (log != null)
        {
            log.DisconnectedAt = DateTime.UtcNow;
            log.DisconnectReason = exception?.Message ?? "Normal disconnect";
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation("User {UserId} disconnected from ConnectionId {ConnectionId}", userId, Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Join a session room for signaling
    /// </summary>
    public async Task JoinSession(string sessionId)
    {
        var userId = GetUserId();

        var session = await _sessionService.JoinSessionAsync(userId, sessionId, Context.ConnectionId);

        if (session == null)
        {
            await Clients.Caller.SendAsync("Error", new { message = "Failed to join session" });
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
        _connectionManager.AddConnection(Context.ConnectionId, userId, sessionId);

        // Notify others in the session
        await Clients.OthersInGroup(sessionId).SendAsync("ParticipantJoined", new
        {
            userId,
            connectionId = Context.ConnectionId,
            timestamp = DateTime.UtcNow
        });

        _logger.LogInformation("User {UserId} joined session {SessionId}", userId, sessionId);
    }

    /// <summary>
    /// Leave a session room
    /// </summary>
    public async Task LeaveSession(string sessionId)
    {
        var userId = GetUserId();

        await _sessionService.LeaveSessionAsync(userId, sessionId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionId);
        _connectionManager.RemoveConnection(Context.ConnectionId);

        await Clients.OthersInGroup(sessionId).SendAsync("ParticipantLeft", new
        {
            userId,
            connectionId = Context.ConnectionId,
            timestamp = DateTime.UtcNow
        });

        _logger.LogInformation("User {UserId} left session {SessionId}", userId, sessionId);
    }

    /// <summary>
    /// Send WebRTC offer to a specific participant
    /// </summary>
    public async Task SendOffer(string targetConnectionId, object offer)
    {
        var userId = GetUserId();

        await Clients.Client(targetConnectionId).SendAsync("ReceiveOffer", new
        {
            fromConnectionId = Context.ConnectionId,
            fromUserId = userId,
            offer,
            timestamp = DateTime.UtcNow
        });

        _logger.LogDebug("Offer sent from {From} to {To}", Context.ConnectionId, targetConnectionId);
    }

    /// <summary>
    /// Send WebRTC answer to a specific participant
    /// </summary>
    public async Task SendAnswer(string targetConnectionId, object answer)
    {
        var userId = GetUserId();

        await Clients.Client(targetConnectionId).SendAsync("ReceiveAnswer", new
        {
            fromConnectionId = Context.ConnectionId,
            fromUserId = userId,
            answer,
            timestamp = DateTime.UtcNow
        });

        _logger.LogDebug("Answer sent from {From} to {To}", Context.ConnectionId, targetConnectionId);
    }

    /// <summary>
    /// Send ICE candidate to a specific participant
    /// </summary>
    public async Task SendIceCandidate(string targetConnectionId, object candidate)
    {
        await Clients.Client(targetConnectionId).SendAsync("ReceiveIceCandidate", new
        {
            fromConnectionId = Context.ConnectionId,
            candidate,
            timestamp = DateTime.UtcNow
        });

        _logger.LogDebug("ICE candidate sent from {From} to {To}", Context.ConnectionId, targetConnectionId);
    }

    /// <summary>
    /// Update participant status (muted, video enabled)
    /// </summary>
    public async Task UpdateStatus(string sessionId, bool isMuted, bool isVideoEnabled)
    {
        var userId = GetUserId();

        await _sessionService.UpdateParticipantStatusAsync(Context.ConnectionId, isMuted, isVideoEnabled);

        await Clients.OthersInGroup(sessionId).SendAsync("ParticipantStatusChanged", new
        {
            userId,
            connectionId = Context.ConnectionId,
            isMuted,
            isVideoEnabled,
            timestamp = DateTime.UtcNow
        });

        _logger.LogDebug("Status updated for user {UserId}: Muted={Muted}, Video={Video}",
            userId, isMuted, isVideoEnabled);
    }

    /// <summary>
    /// Send a chat message to all participants in a session
    /// </summary>
    public async Task SendMessage(string sessionId, string message)
    {
        var userId = GetUserId();
        var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";

        await Clients.Group(sessionId).SendAsync("ReceiveMessage", new
        {
            userId,
            username,
            message,
            timestamp = DateTime.UtcNow
        });

        _logger.LogDebug("Message sent by {Username} in session {SessionId}", username, sessionId);
    }
}
