using General.Admin.Infrastructure;
using General.Admin.PhaseOne;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Auditing;

namespace General.Admin.Controllers;

[ApiController]
[DisableAuditing]
[Authorize(Roles = PhaseOneRoleNames.Admin)]
[Route("api/app/audit-log")]
public class AuditLogController : ControllerBase
{
    private readonly PhaseOneAuditLogService _auditLogService;

    public AuditLogController(PhaseOneAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet("list")]
    public async Task<ActionResult<ApiResponse<List<PhaseOneAuditLogItemDto>>>> GetListAsync([FromQuery] PhaseOneAuditLogQueryInput input)
    {
        return ApiResponse<List<PhaseOneAuditLogItemDto>>.Ok(await _auditLogService.GetListAsync(input));
    }
}
