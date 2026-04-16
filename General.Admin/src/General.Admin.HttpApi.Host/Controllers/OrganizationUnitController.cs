using General.Admin.Infrastructure;
using General.Admin.PhaseOne;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Auditing;

namespace General.Admin.Controllers;

[ApiController]
[DisableAuditing]
[Authorize]
[Route("api/app/organization-unit")]
public class OrganizationUnitController : ControllerBase
{
    private readonly PhaseOneOrganizationUnitService _organizationUnitService;

    public OrganizationUnitController(PhaseOneOrganizationUnitService organizationUnitService)
    {
        _organizationUnitService = organizationUnitService;
    }

    [HttpGet("tree")]
    public async Task<ActionResult<ApiResponse<List<OrganizationUnitTreeDto>>>> GetTreeAsync()
    {
        return ApiResponse<List<OrganizationUnitTreeDto>>.Ok(await _organizationUnitService.GetTreeAsync());
    }

    [Authorize(Roles = PhaseOneRoleNames.Admin)]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<bool>>> CreateAsync([FromBody] OrganizationUnitSaveInput input)
    {
        await _organizationUnitService.CreateAsync(input);
        return ApiResponse<bool>.Ok(true);
    }

    [Authorize(Roles = PhaseOneRoleNames.Admin)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateAsync(Guid id, [FromBody] OrganizationUnitSaveInput input)
    {
        await _organizationUnitService.UpdateAsync(id, input);
        return ApiResponse<bool>.Ok(true);
    }

    [Authorize(Roles = PhaseOneRoleNames.Admin)]
    [HttpPut("{id:guid}/move")]
    public async Task<ActionResult<ApiResponse<bool>>> MoveAsync(Guid id, [FromBody] OrganizationUnitMoveInput input)
    {
        await _organizationUnitService.MoveAsync(id, input.ParentId);
        return ApiResponse<bool>.Ok(true);
    }

    [Authorize(Roles = PhaseOneRoleNames.Admin)]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteAsync(Guid id)
    {
        await _organizationUnitService.DeleteAsync(id);
        return ApiResponse<bool>.Ok(true);
    }
}
