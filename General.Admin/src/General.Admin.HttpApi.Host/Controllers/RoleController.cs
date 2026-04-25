using General.Admin.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Auditing;

namespace General.Admin.Controllers;

[ApiController]
[Authorize(AdminPermissions.Platform.RoleManage)]
[ApiExplorerSettings(GroupName = ApiDocGroups.Platform)]
[Route("api/app/role")]
public class RoleController : ControllerBase
{
    private readonly PlatformRoleService _roleService;

    public RoleController(PlatformRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet("list")]
    public async Task<ActionResult<ApiResponse<List<PlatformRoleDto>>>> GetListAsync()
    {
        return ApiResponse<List<PlatformRoleDto>>.Ok(await _roleService.GetListAsync());
    }

    [HttpGet("{id:guid}/authorization")]
    public async Task<ActionResult<ApiResponse<PlatformRoleAuthorizationDto>>> GetAuthorizationAsync(Guid id)
    {
        return ApiResponse<PlatformRoleAuthorizationDto>.Ok(await _roleService.GetAuthorizationAsync(id));
    }

    [HttpPut("{id:guid}/authorization")]
    [PlatformEndpoint("Platform.Role.Manage")]
    public async Task<ActionResult<ApiResponse<bool>>> SaveAuthorizationAsync(Guid id, [FromBody] PlatformRoleAuthorizationSaveInput input)
    {
        await _roleService.SaveAuthorizationAsync(id, input);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpPost]
    [PlatformEndpoint("Platform.Role.Manage")]
    public async Task<ActionResult<ApiResponse<bool>>> CreateAsync([FromBody] PlatformRoleSaveInput input)
    {
        await _roleService.CreateAsync(input);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpDelete("{id:guid}")]
    [PlatformEndpoint("Platform.Role.Manage")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteAsync(Guid id)
    {
        await _roleService.DeleteAsync(id);
        return ApiResponse<bool>.Ok(true);
    }
}
