namespace TunRTC.Server.Models.DTOs;

public class IceServerConfig
{
    public List<string> StunServers { get; set; } = new();
    public TurnServerConfig? TurnServer { get; set; }
}

public class TurnServerConfig
{
    public List<string> Urls { get; set; } = new();
    public string Username { get; set; } = string.Empty;
    public string Credential { get; set; } = string.Empty;
}

public class WebRTCConfig
{
    public List<RTCIceServer> IceServers { get; set; } = new();
}

public class RTCIceServer
{
    public List<string> Urls { get; set; } = new();
    public string? Username { get; set; }
    public string? Credential { get; set; }
}
