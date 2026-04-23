using General.Admin.Infrastructure;
using General.Admin.PhaseOne;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Auditing;

namespace General.Admin.Controllers;

[ApiController]
[Authorize]
[Route("api/app/menu")]
public class MenuController : ControllerBase
{
    private readonly PhaseOneMenuService _menuService;

    public MenuController(PhaseOneMenuService menuService)
    {
        _menuService = menuService;
    }

    [HttpGet("all")]
    public async Task<ActionResult<ApiResponse<List<BackendRouteDto>>>> GetAllAsync([FromQuery] PhaseOneMenuQueryInput input)
    {
        return ApiResponse<List<BackendRouteDto>>.Ok(
            await _menuService.GetCurrentMenusAsync(ParseAppCodes(input.AppCodes)));
    }

    [Authorize(Roles = PhaseOneRoleNames.Admin)]
    [HttpGet("tree")]
    public async Task<ActionResult<ApiResponse<List<MenuPermissionTreeDto>>>> GetTreeAsync([FromQuery] PhaseOneMenuQueryInput input)
    {
        return ApiResponse<List<MenuPermissionTreeDto>>.Ok(
            await _menuService.GetPermissionTreeAsync(ParseAppCodes(input.AppCodes)));
    }

    [Authorize(Roles = PhaseOneRoleNames.Admin)]
    [HttpGet("role/{roleId:guid}")]
    public async Task<ActionResult<ApiResponse<List<Guid>>>> GetRoleMenuIdsAsync(Guid roleId)
    {
        return ApiResponse<List<Guid>>.Ok(await _menuService.GetRoleMenuIdsAsync(roleId));
    }

    [Authorize(Roles = PhaseOneRoleNames.Admin)]
    [HttpPost("role/{roleId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> GrantRoleMenusAsync(Guid roleId, [FromBody] RoleMenuGrantInput input)
    {
        await _menuService.GrantRoleMenusAsync(roleId, input.MenuIds);
        return ApiResponse<bool>.Ok(true);
    }

    [Authorize(Roles = PhaseOneRoleNames.Admin)]
    [PlatformEndpoint("Platform.Menu.Manage")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<bool>>> CreateAsync([FromBody] PhaseOneMenuSaveInput input)
    {
        await _menuService.CreateAsync(input);
        return ApiResponse<bool>.Ok(true);
    }

    [Authorize(Roles = PhaseOneRoleNames.Admin)]
    [PlatformEndpoint("Platform.Menu.Manage")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateAsync(Guid id, [FromBody] PhaseOneMenuSaveInput input)
    {
        await _menuService.UpdateAsync(id, input);
        return ApiResponse<bool>.Ok(true);
    }

    [Authorize(Roles = PhaseOneRoleNames.Admin)]
    [PlatformEndpoint("Platform.Menu.Manage")]
    [HttpPut("{id:guid}/enabled")]
    public async Task<ActionResult<ApiResponse<bool>>> SetEnabledAsync(Guid id, [FromQuery] bool isEnabled)
    {
        await _menuService.SetEnabledAsync(id, isEnabled);
        return ApiResponse<bool>.Ok(true);
    }

    [Authorize(Roles = PhaseOneRoleNames.Admin)]
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
