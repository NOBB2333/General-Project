using General.Admin.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Auditing;

namespace General.Admin.Controllers;

[ApiController]
[Authorize]
[ApiExplorerSettings(GroupName = ApiDocGroups.Platform)]
[Route("api/app/menu")]
public class MenuController : ControllerBase
{
    private readonly PlatformMenuService _menuService;

    public MenuController(PlatformMenuService menuService)
    {
        _menuService = menuService;
    }

    [HttpGet("all")]
    public async Task<ActionResult<ApiResponse<List<BackendRouteDto>>>> GetAllAsync([FromQuery] PlatformMenuQueryInput input)
    {
        return ApiResponse<List<BackendRouteDto>>.Ok(
            await _menuService.GetCurrentMenusAsync(ParseAppCodes(input.AppCodes)));
    }

    [HttpGet("tree")]
    public async Task<ActionResult<ApiResponse<List<MenuPermissionTreeDto>>>> GetTreeAsync([FromQuery] PlatformMenuQueryInput input)
    {
        return ApiResponse<List<MenuPermissionTreeDto>>.Ok(
            await _menuService.GetPermissionTreeAsync(ParseAppCodes(input.AppCodes)));
    }

    [Authorize(AdminPermissions.Platform.RoleManage)]
    [HttpGet("role/{roleId:guid}")]
    public async Task<ActionResult<ApiResponse<List<Guid>>>> GetRoleMenuIdsAsync(Guid roleId)
    {
        return ApiResponse<List<Guid>>.Ok(await _menuService.GetRoleMenuIdsAsync(roleId));
    }

    [Authorize(AdminPermissions.Platform.RoleManage)]
    [HttpPost("role/{roleId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> GrantRoleMenusAsync(Guid roleId, [FromBody] RoleMenuGrantInput input)
    {
        await _menuService.GrantRoleMenusAsync(roleId, input.MenuIds);
        return ApiResponse<bool>.Ok(true);
    }

    [Authorize(AdminPermissions.Platform.MenuManage)]
    [PlatformEndpoint("Platform.Menu.Manage")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<bool>>> CreateAsync([FromBody] PlatformMenuSaveInput input)
    {
        await _menuService.CreateAsync(input);
        return ApiResponse<bool>.Ok(true);
    }

    [Authorize(AdminPermissions.Platform.MenuManage)]
    [PlatformEndpoint("Platform.Menu.Manage")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateAsync(Guid id, [FromBody] PlatformMenuSaveInput input)
    {
        await _menuService.UpdateAsync(id, input);
        return ApiResponse<bool>.Ok(true);
    }

    [Authorize(AdminPermissions.Platform.MenuManage)]
    [PlatformEndpoint("Platform.Menu.Manage")]
    [HttpPut("{id:guid}/enabled")]
    public async Task<ActionResult<ApiResponse<bool>>> SetEnabledAsync(Guid id, [FromQuery] bool isEnabled)
    {
        await _menuService.SetEnabledAsync(id, isEnabled);
        return ApiResponse<bool>.Ok(true);
    }

    [Authorize(AdminPermissions.Platform.MenuManage)]
    [PlatformEndpoint("Platform.Menu.Manage")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteAsync(Guid id)
    {
        await _menuService.DeleteAsync(id);
        return ApiResponse<bool>.Ok(true);
    }

    private static IReadOnlyCollection<string> ParseAppCodes(string? appCodes)
    {
        return string.IsNullOrWhiteSpace(appCodes)
            ? []
            : appCodes
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
    }
}
