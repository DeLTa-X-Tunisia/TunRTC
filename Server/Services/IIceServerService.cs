using TunRTC.Server.Models.DTOs;

namespace TunRTC.Server.Services;

public interface IIceServerService
{
    WebRTCConfig GetIceServers();
}
