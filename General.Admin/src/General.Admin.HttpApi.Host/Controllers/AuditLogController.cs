using General.Admin.Infrastructure;
using General.Admin.PhaseOne;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Auditing;
using Volo.Abp.Users;

namespace General.Admin.Controllers;

[ApiController]
[Authorize(Roles = PhaseOneRoleNames.Admin)]
[ApiExplorerSettings(GroupName = ApiDocGroups.Platform)]
[Route("api/app/audit-log")]
public class AuditLogController : ControllerBase
{
    private readonly PhaseOneAuditLogService _auditLogService;
    private readonly ICurrentUser _currentUser;

    public AuditLogController(PhaseOneAuditLogService auditLogService, ICurrentUser currentUser)
    {
        _auditLogService = auditLogService;
        _currentUser = currentUser;
    }

    [HttpGet("list")]
    public async Task<ActionResult<ApiResponse<List<PhaseOneAuditLogItemDto>>>> GetListAsync([FromQuery] PhaseOneAuditLogQueryInput input)
    {
        return ApiResponse<List<PhaseOneAuditLogItemDto>>.Ok(await _auditLogService.GetListAsync(input));
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<PhaseOneLogDashboardDto>>> GetDashboardAsync([FromQuery] PhaseOneAuditLogQueryInput input)
    {
        return ApiResponse<PhaseOneLogDashboardDto>.Ok(await _auditLogService.GetDashboardAsync(input));
    }

    [AllowAnonymous]
    [Authorize]
    [DisableAuditing]
    [HttpPost("page-visit")]
    public async Task<ActionResult<ApiResponse<bool>>> RecordPageVisitAsync([FromBody] PhaseOnePageVisitInput input)
    {
        var userName = _currentUser.UserName;
        var tenantName = _currentUser.TenantId.HasValue ? _currentUser.TenantId.ToString() : null;
        await _auditLogService.RecordPageVisitAsync(userName, tenantName, input);
        return ApiResponse<bool>.Ok(true);
    }
}
