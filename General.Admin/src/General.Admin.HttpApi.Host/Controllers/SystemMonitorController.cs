using General.Admin.Infrastructure;
using General.Admin.PhaseOne;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Auditing;

namespace General.Admin.Controllers;

[ApiController]
[Authorize(Roles = PhaseOneRoleNames.Admin)]
[Route("api/app/system-monitor")]
public class SystemMonitorController : ControllerBase
{
    private readonly PhaseOneSystemMonitorService _systemMonitorService;

    public SystemMonitorController(PhaseOneSystemMonitorService systemMonitorService)
    {
        _systemMonitorService = systemMonitorService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PhaseOneSystemMonitorDto>>> GetAsync()
    {
        return ApiResponse<PhaseOneSystemMonitorDto>.Ok(await _systemMonitorService.GetAsync());
    }
}
