using General.Admin.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Auditing;

namespace General.Admin.Controllers;

[ApiController]
[Authorize(AdminPermissions.Platform.SystemMonitorView)]
[ApiExplorerSettings(GroupName = ApiDocGroups.Platform)]
[Route("api/app/system-monitor")]
public class SystemMonitorController : ControllerBase
{
    private readonly PlatformSystemMonitorService _systemMonitorService;

    public SystemMonitorController(PlatformSystemMonitorService systemMonitorService)
    {
        _systemMonitorService = systemMonitorService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PlatformSystemMonitorDto>>> GetAsync()
    {
        return ApiResponse<PlatformSystemMonitorDto>.Ok(await _systemMonitorService.GetAsync());
    }
}
