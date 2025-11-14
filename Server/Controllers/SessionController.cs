using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TunRTC.Server.Models.DTOs;
using TunRTC.Server.Services;

namespace TunRTC.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SessionController : ControllerBase
{
    private readonly ISessionService _sessionService;
    private readonly ILogger<SessionController> _logger;

    public SessionController(ISessionService sessionService, ILogger<SessionController> logger)
    {
        _sessionService = sessionService;
        _logger = logger;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    /// <summary>
    /// Create a new WebRTC session
    /// </summary>
    [HttpPost("create")]
    [ProducesResponseType(typeof(SessionResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest request)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Session name is required" });
        }

        if (request.MaxParticipants < 2 || request.MaxParticipants > 100)
        {
            return BadRequest(new { message = "MaxParticipants must be between 2 and 100" });
        }

        var session = await _sessionService.CreateSessionAsync(userId, request);

        if (session == null)
        {
            return BadRequest(new { message = "Failed to create session" });
        }

        _logger.LogInformation("Session created: {SessionId} by user {UserId}", session.SessionId, userId);
        return Ok(session);
    }

    /// <summary>
    /// Join an existing session
    /// </summary>
    [HttpPost("join")]
    [ProducesResponseType(typeof(SessionDetailResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> JoinSession([FromBody] JoinSessionRequest request)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(request.SessionId))
        {
            return BadRequest(new { message = "SessionId is required" });
        }

        // Connection ID will be set when connecting via SignalR
        var connectionId = Guid.NewGuid().ToString();
        var session = await _sessionService.JoinSessionAsync(userId, request.SessionId, connectionId);

        if (session == null)
        {
            return NotFound(new { message = "Session not found or is full" });
        }

        _logger.LogInformation("User {UserId} joined session {SessionId}", userId, request.SessionId);
        return Ok(session);
    }

    /// <summary>
    /// Get session details
    /// </summary>
    [HttpGet("{sessionId}")]
    [ProducesResponseType(typeof(SessionDetailResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetSession(string sessionId)
    {
        var session = await _sessionService.GetSessionAsync(sessionId);

        if (session == null)
        {
            return NotFound(new { message = "Session not found" });
        }

        return Ok(session);
    }

    /// <summary>
    /// Get all active sessions
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(List<SessionResponse>), 200)]
    public async Task<IActionResult> GetActiveSessions()
    {
        var sessions = await _sessionService.GetActiveSessionsAsync();
        return Ok(sessions);
    }

    /// <summary>
    /// Leave a session
    /// </summary>
    [HttpPost("leave")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> LeaveSession([FromBody] JoinSessionRequest request)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        var success = await _sessionService.LeaveSessionAsync(userId, request.SessionId);

        if (!success)
        {
            return NotFound(new { message = "Session not found or user not in session" });
        }

        _logger.LogInformation("User {UserId} left session {SessionId}", userId, request.SessionId);
        return Ok(new { message = "Left session successfully" });
    }

    /// <summary>
    /// End a session (creator only)
    /// </summary>
    [HttpPost("end")]
    [ProducesResponseType(200)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> EndSession([FromBody] JoinSessionRequest request)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        var success = await _sessionService.EndSessionAsync(userId, request.SessionId);

        if (!success)
        {
            return Forbid();
        }

        _logger.LogInformation("Session {SessionId} ended by user {UserId}", request.SessionId, userId);
        return Ok(new { message = "Session ended successfully" });
    }
}
