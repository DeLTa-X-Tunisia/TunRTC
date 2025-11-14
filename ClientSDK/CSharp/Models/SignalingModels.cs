namespace TunRTC.Client.Models;

public class SignalingMessage
{
    public string Type { get; set; } = string.Empty;
    public string FromConnectionId { get; set; } = string.Empty;
    public int FromUserId { get; set; }
    public object? Data { get; set; }
    public DateTime Timestamp { get; set; }
}

public class ParticipantEvent
{
    public int UserId { get; set; }
    public string ConnectionId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class ParticipantStatusEvent : ParticipantEvent
{
    public bool IsMuted { get; set; }
    public bool IsVideoEnabled { get; set; }
}

public class ChatMessage
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
