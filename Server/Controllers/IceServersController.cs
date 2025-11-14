using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TunRTC.Server.Models.DTOs;
using TunRTC.Server.Services;

namespace TunRTC.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IceServersController : ControllerBase
{
    private readonly IIceServerService _iceServerService;

    public IceServersController(IIceServerService iceServerService)
    {
        _iceServerService = iceServerService;
    }

    /// <summary>
    /// Get ICE servers configuration (STUN/TURN) for WebRTC connection
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(WebRTCConfig), 200)]
    public IActionResult GetIceServers()
    {
        var config = _iceServerService.GetIceServers();
        return Ok(config);
    }
}
