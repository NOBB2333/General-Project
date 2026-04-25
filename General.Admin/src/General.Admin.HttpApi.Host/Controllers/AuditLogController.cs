using General.Admin.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Auditing;
using Volo.Abp.Users;

namespace General.Admin.Controllers;

[ApiController]
[Authorize(Roles = PlatformRoleNames.Admin)]
[ApiExplorerSettings(GroupName = ApiDocGroups.Platform)]
[Route("api/app/audit-log")]
public class AuditLogController : ControllerBase
{
    private readonly PlatformAuditLogService _auditLogService;
    private readonly ICurrentUser _currentUser;

    public AuditLogController(PlatformAuditLogService auditLogService, ICurrentUser currentUser)
    {
        _auditLogService = auditLogService;
        _currentUser = currentUser;
    }

    [HttpGet("list")]
    public async Task<ActionResult<ApiResponse<List<PlatformAuditLogItemDto>>>> GetListAsync([FromQuery] PlatformAuditLogQueryInput input)
    {
        return ApiResponse<List<PlatformAuditLogItemDto>>.Ok(await _auditLogService.GetListAsync(input));
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<PlatformLogDashboardDto>>> GetDashboardAsync([FromQuery] PlatformAuditLogQueryInput input)
    {
        return ApiResponse<PlatformLogDashboardDto>.Ok(await _auditLogService.GetDashboardAsync(input));
    }

    [AllowAnonymous]
    [Authorize]
    [DisableAuditing]
    [HttpPost("page-visit")]
    public async Task<ActionResult<ApiResponse<bool>>> RecordPageVisitAsync([FromBody] PlatformPageVisitInput input)
    {
        var userName = _currentUser.UserName;
        var tenantName = _currentUser.TenantId.HasValue ? _currentUser.TenantId.ToString() : null;
        await _auditLogService.RecordPageVisitAsync(userName, tenantName, input);
        return ApiResponse<bool>.Ok(true);
    }
}
