namespace TunRTC.Server.Models;

public class SessionParticipant
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public int UserId { get; set; }
    public string ConnectionId { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LeftAt { get; set; }
    public ParticipantStatus Status { get; set; } = ParticipantStatus.Connected;
    public bool IsMuted { get; set; } = false;
    public bool IsVideoEnabled { get; set; } = true;

    // Navigation properties
    public Session Session { get; set; } = null!;
    public User User { get; set; } = null!;
}

public enum ParticipantStatus
{
    Connected,
    Disconnected,
    Reconnecting
}
