using General.Admin.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Auditing;

namespace General.Admin.Controllers;

[ApiController]
[Authorize]
[ApiExplorerSettings(GroupName = ApiDocGroups.Platform)]
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
        return ApiResponse<string>.Ok(await _schedulerService.RunAsync(jobKey));
    }

    [HttpPost("{jobKey}/toggle")]
    public async Task<ActionResult<ApiResponse<string>>> ToggleAsync(string jobKey, [FromBody] PlatformScheduledJobToggleInput input)
    {
        await _schedulerService.ToggleAsync(jobKey, input.IsEnabled);
        return ApiResponse<string>.Ok("ok");
    }
}
