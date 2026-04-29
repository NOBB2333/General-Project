using General.Admin.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace General.Admin.Controllers;

[ApiController]
[AllowAnonymous]
[ApiExplorerSettings(IgnoreApi = true)]
[Route("health")]
public class HealthController : ControllerBase
{
    private readonly PlatformHealthProbeService _healthProbeService;

    public HealthController(PlatformHealthProbeService healthProbeService)
    {
        _healthProbeService = healthProbeService;
    }

    [HttpGet("live")]
    public async Task<ActionResult<PlatformHealthResult>> LiveAsync()
    {
        return await _healthProbeService.CheckLiveAsync();
    }

    [HttpGet("ready")]
    public async Task<ActionResult<PlatformHealthResult>> ReadyAsync()
    {
        var result = await _healthProbeService.CheckReadyAsync();
        return result.Status == "Unhealthy"
            ? StatusCode(StatusCodes.Status503ServiceUnavailable, result)
            : Ok(result);
    }
}
