using General.Admin.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Auditing;

namespace General.Admin.Controllers;

[ApiController]
[Authorize]
[ApiExplorerSettings(GroupName = ApiDocGroups.Platform)]
[Route("api/app/role")]
public class RoleController : ControllerBase
{
    private readonly PlatformRoleService _roleService;

    public RoleController(PlatformRoleService roleService)
    {
        _roleService = roleService;
    }

    [Authorize(AdminPermissions.Platform.RoleManage)]
    [HttpGet("list")]
    public async Task<ActionResult<ApiResponse<List<PlatformRoleDto>>>> GetListAsync()
    {
        return ApiResponse<List<PlatformRoleDto>>.Ok(await _roleService.GetListAsync());
    }

    [HttpGet("options")]
    public async Task<ActionResult<ApiResponse<List<PlatformRoleOptionDto>>>> GetOptionsAsync()
    {
        return ApiResponse<List<PlatformRoleOptionDto>>.Ok(await _roleService.GetOptionsAsync());
    }

    [Authorize(AdminPermissions.Platform.RoleManage)]
    [HttpGet("{id:guid}/authorization")]
    public async Task<ActionResult<ApiResponse<PlatformRoleAuthorizationDto>>> GetAuthorizationAsync(Guid id)
    {
        return ApiResponse<PlatformRoleAuthorizationDto>.Ok(await _roleService.GetAuthorizationAsync(id));
    }

    [Authorize(AdminPermissions.Platform.RoleManage)]
    [HttpPut("{id:guid}/authorization")]
    [PlatformEndpoint("Platform.Role.Manage")]
    public async Task<ActionResult<ApiResponse<bool>>> SaveAuthorizationAsync(Guid id, [FromBody] PlatformRoleAuthorizationSaveInput input)
    {
        await _roleService.SaveAuthorizationAsync(id, input);
        return ApiResponse<bool>.Ok(true);
    }

    [Authorize(AdminPermissions.Platform.RoleManage)]
    [HttpPost]
    [PlatformEndpoint("Platform.Role.Manage")]
    public async Task<ActionResult<ApiResponse<bool>>> CreateAsync([FromBody] PlatformRoleSaveInput input)
    {
        await _roleService.CreateAsync(input);
        return ApiResponse<bool>.Ok(true);
    }

    [Authorize(AdminPermissions.Platform.RoleManage)]
    [HttpDelete("{id:guid}")]
    [PlatformEndpoint("Platform.Role.Manage")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteAsync(Guid id)
    {
        await _roleService.DeleteAsync(id);
        return ApiResponse<bool>.Ok(true);
    }
}
