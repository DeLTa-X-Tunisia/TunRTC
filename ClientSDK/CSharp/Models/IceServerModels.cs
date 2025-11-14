namespace TunRTC.Client.Models;

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
