namespace TunRTC.Server.Models;

public class Session
{
    public int Id { get; set; }
    public string SessionId { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public int CreatorId { get; set; }
    public int MaxParticipants { get; set; } = 10;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.Active;
    public SessionType Type { get; set; } = SessionType.VideoCall;
    
    // Navigation properties
    public User Creator { get; set; } = null!;
    public ICollection<SessionParticipant> Participants { get; set; } = new List<SessionParticipant>();
}

public enum SessionStatus
{
    Active,
    Ended,
    Paused
}

public enum SessionType
{
    AudioOnly,
    VideoCall,
    ScreenShare,
    Mixed
}
