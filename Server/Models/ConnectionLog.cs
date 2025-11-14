namespace TunRTC.Server.Models;

public class ConnectionLog
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? SessionId { get; set; }
    public string ConnectionId { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DisconnectedAt { get; set; }
    public string? DisconnectReason { get; set; }
}
