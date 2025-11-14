namespace TunRTC.Client.Models;

public class SessionResponse
{
    public string SessionId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int MaxParticipants { get; set; }
    public int CurrentParticipants { get; set; }
    public SessionStatus Status { get; set; }
    public SessionType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatorUsername { get; set; } = string.Empty;
}

public class SessionDetailResponse : SessionResponse
{
    public List<ParticipantInfo> Participants { get; set; } = new();
}

public class CreateSessionRequest
{
    public string Name { get; set; } = string.Empty;
    public int MaxParticipants { get; set; } = 10;
    public SessionType Type { get; set; } = SessionType.VideoCall;
}

public class JoinSessionRequest
{
    public string SessionId { get; set; } = string.Empty;
}

public class ParticipantInfo
{
    public string Username { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
    public ParticipantStatus Status { get; set; }
    public bool IsMuted { get; set; }
    public bool IsVideoEnabled { get; set; }
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

public enum ParticipantStatus
{
    Connected,
    Disconnected,
    Reconnecting
}
