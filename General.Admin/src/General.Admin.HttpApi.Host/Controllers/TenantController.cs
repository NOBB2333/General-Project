using General.Admin.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Auditing;

namespace General.Admin.Controllers;

[ApiController]
[Authorize(AdminPermissions.Platform.TenantManage)]
[ApiExplorerSettings(GroupName = ApiDocGroups.Platform)]
[Route("api/app/tenant")]
public class TenantController : ControllerBase
{
    private readonly PlatformTenantService _tenantService;

    public TenantController(PlatformTenantService tenantService)
    {
        _tenantService = tenantService;
    }

    [HttpGet("list")]
    public async Task<ActionResult<ApiResponse<List<PlatformTenantListItemDto>>>> GetListAsync()
    {
        return ApiResponse<List<PlatformTenantListItemDto>>.Ok(await _tenantService.GetListAsync());
    }

    [HttpGet("{id:guid}/authorization")]
    public async Task<ActionResult<ApiResponse<PlatformTenantAuthorizationDto>>> GetAuthorizationAsync(Guid id)
    {
        return ApiResponse<PlatformTenantAuthorizationDto>.Ok(await _tenantService.GetAuthorizationAsync(id));
    }

    [HttpPut("{id:guid}/authorization")]
    [PlatformEndpoint("Platform.Tenant.Manage")]
    public async Task<ActionResult<ApiResponse<bool>>> SaveAuthorizationAsync(
        Guid id,
        [FromBody] PlatformTenantAuthorizationSaveInput input)
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
    public async Task<ActionResult<ApiResponse<bool>>> CreateAsync([FromBody] PlatformTenantSaveInput input)
    {
        await _tenantService.CreateAsync(input);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpPut("{id:guid}")]
    [PlatformEndpoint("Platform.Tenant.Manage")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateAsync(Guid id, [FromBody] PlatformTenantSaveInput input)
    {
        await _tenantService.UpdateAsync(id, input);
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
    public async Task<ActionResult<ApiResponse<List<PlatformTenantUserDto>>>> GetUsersAsync(Guid id)
    {
        return ApiResponse<List<PlatformTenantUserDto>>.Ok(await _tenantService.GetUsersAsync(id));
    }
}
