using TunRTC.Server.Models.DTOs;

namespace TunRTC.Server.Services;

public class IceServerService : IIceServerService
{
    private readonly IConfiguration _configuration;

    public IceServerService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public WebRTCConfig GetIceServers()
    {
        var config = new WebRTCConfig();

        // Add STUN servers
        var stunServers = _configuration.GetSection("IceServers:StunServers").Get<List<string>>();
        if (stunServers != null && stunServers.Any())
        {
            foreach (var stunServer in stunServers)
            {
                config.IceServers.Add(new RTCIceServer
                {
                    Urls = new List<string> { stunServer }
                });
            }
        }

        // Add TURN server if enabled
        var turnEnabled = _configuration.GetValue<bool>("IceServers:TurnServer:Enabled");
        if (turnEnabled)
        {
            var turnUrls = _configuration.GetSection("IceServers:TurnServer:Urls").Get<List<string>>();
            var turnUsername = _configuration["IceServers:TurnServer:Username"];
            var turnCredential = _configuration["IceServers:TurnServer:Credential"];

            if (turnUrls != null && turnUrls.Any())
            {
                config.IceServers.Add(new RTCIceServer
                {
                    Urls = turnUrls,
                    Username = turnUsername,
                    Credential = turnCredential
                });
            }
        }

        return config;
    }
}
