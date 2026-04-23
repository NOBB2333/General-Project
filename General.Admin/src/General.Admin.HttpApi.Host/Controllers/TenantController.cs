using General.Admin.Infrastructure;
using General.Admin.PhaseOne;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Auditing;

namespace General.Admin.Controllers;

[ApiController]
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

    [HttpGet("{id:guid}/authorization")]
    public async Task<ActionResult<ApiResponse<PhaseOneTenantAuthorizationDto>>> GetAuthorizationAsync(Guid id)
    {
        return ApiResponse<PhaseOneTenantAuthorizationDto>.Ok(await _tenantService.GetAuthorizationAsync(id));
    }

    [HttpPut("{id:guid}/authorization")]
    [PlatformEndpoint("Platform.Tenant.Manage")]
    public async Task<ActionResult<ApiResponse<bool>>> SaveAuthorizationAsync(
        Guid id,
        [FromBody] PhaseOneTenantAuthorizationSaveInput input)
    {
        await _tenantService.SaveAuthorizationAsync(id, input);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpPut("{id:guid}/status")]
    [PlatformEndpoint("Platform.Tenant.Manage")]
    public async Task<ActionResult<ApiResponse<bool>>> SetStatusAsync(Guid id, [FromQuery] bool isActive)
    {
        await _tenantService.SetStatusAsync(id, isActive);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpPost]
    [PlatformEndpoint("Platform.Tenant.Manage")]
    public async Task<ActionResult<ApiResponse<bool>>> CreateAsync([FromBody] PhaseOneTenantSaveInput input)
    {
        await _tenantService.CreateAsync(input);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpDelete("{id:guid}")]
    [PlatformEndpoint("Platform.Tenant.Manage")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteAsync(Guid id)
    {
        await _tenantService.DeleteAsync(id);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpGet("{id:guid}/users")]
    public async Task<ActionResult<ApiResponse<List<PhaseOneTenantUserDto>>>> GetUsersAsync(Guid id)
    {
        return ApiResponse<List<PhaseOneTenantUserDto>>.Ok(await _tenantService.GetUsersAsync(id));
    }
}
