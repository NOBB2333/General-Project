using General.Admin.Infrastructure;
using General.Admin.PhaseOne;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Auditing;

namespace General.Admin.Controllers;

[ApiController]
[DisableAuditing]
[Authorize(Roles = PhaseOneRoleNames.Admin)]
[Route("api/app/tenant")]
public class TenantController : ControllerBase
{
    private readonly PhaseOneTenantService _tenantService;

    public TenantController(PhaseOneTenantService tenantService)
    {
        _tenantService = tenantService;
    }

    [HttpGet("list")]
    public async Task<ActionResult<ApiResponse<List<PhaseOneTenantListItemDto>>>> GetListAsync()
    {
        return ApiResponse<List<PhaseOneTenantListItemDto>>.Ok(await _tenantService.GetListAsync());
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<bool>>> CreateAsync([FromBody] PhaseOneTenantSaveInput input)
    {
        await _tenantService.CreateAsync(input);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteAsync(Guid id)
    {
        await _tenantService.DeleteAsync(id);
        return ApiResponse<bool>.Ok(true);
    }
}
