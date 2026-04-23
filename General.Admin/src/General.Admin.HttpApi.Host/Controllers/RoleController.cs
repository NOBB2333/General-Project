using General.Admin.Infrastructure;
using General.Admin.PhaseOne;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Auditing;

namespace General.Admin.Controllers;

[ApiController]
[Authorize(Roles = PhaseOneRoleNames.Admin)]
[Route("api/app/role")]
public class RoleController : ControllerBase
{
    private readonly PhaseOneRoleService _roleService;

    public RoleController(PhaseOneRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet("list")]
    public async Task<ActionResult<ApiResponse<List<PhaseOneRoleDto>>>> GetListAsync()
    {
        return ApiResponse<List<PhaseOneRoleDto>>.Ok(await _roleService.GetListAsync());
    }

    [HttpGet("{id:guid}/authorization")]
    public async Task<ActionResult<ApiResponse<PhaseOneRoleAuthorizationDto>>> GetAuthorizationAsync(Guid id)
    {
        return ApiResponse<PhaseOneRoleAuthorizationDto>.Ok(await _roleService.GetAuthorizationAsync(id));
    }

    [HttpPut("{id:guid}/authorization")]
    [PlatformEndpoint("Platform.Role.Manage")]
    public async Task<ActionResult<ApiResponse<bool>>> SaveAuthorizationAsync(Guid id, [FromBody] PhaseOneRoleAuthorizationSaveInput input)
    {
        await _roleService.SaveAuthorizationAsync(id, input);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpPost]
    [PlatformEndpoint("Platform.Role.Manage")]
    public async Task<ActionResult<ApiResponse<bool>>> CreateAsync([FromBody] PhaseOneRoleSaveInput input)
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
