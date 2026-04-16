using General.Admin.Infrastructure;
using General.Admin.PhaseOne;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Auditing;

namespace General.Admin.Controllers;

[ApiController]
[DisableAuditing]
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

    [HttpPost]
    public async Task<ActionResult<ApiResponse<bool>>> CreateAsync([FromBody] PhaseOneRoleSaveInput input)
    {
        await _roleService.CreateAsync(input);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteAsync(Guid id)
    {
        await _roleService.DeleteAsync(id);
        return ApiResponse<bool>.Ok(true);
    }
}
