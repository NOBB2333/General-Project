using General.Admin.Infrastructure;
using General.Admin.PhaseOne;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Auditing;

namespace General.Admin.Controllers;

[ApiController]
[DisableAuditing]
[Authorize]
[Route("api/app/platform/scheduler")]
public class PlatformSchedulerController : ControllerBase
{
    private readonly PlatformSchedulerService _schedulerService;

    public PlatformSchedulerController(PlatformSchedulerService schedulerService)
    {
        _schedulerService = schedulerService;
    }

    [HttpGet("list")]
    public async Task<ActionResult<ApiResponse<List<PlatformScheduledJobDto>>>> GetListAsync()
    {
        return ApiResponse<List<PlatformScheduledJobDto>>.Ok(await _schedulerService.GetListAsync());
    }

    [HttpPost("{jobKey}/run")]
    public async Task<ActionResult<ApiResponse<string>>> RunAsync(string jobKey)
    {
        await _schedulerService.RunAsync(jobKey);
        return ApiResponse<string>.Ok("ok");
    }

    [HttpPost("{jobKey}/toggle")]
    public async Task<ActionResult<ApiResponse<string>>> ToggleAsync(string jobKey, [FromBody] PlatformScheduledJobToggleInput input)
    {
        await _schedulerService.ToggleAsync(jobKey, input.IsEnabled);
        return ApiResponse<string>.Ok("ok");
    }
}
