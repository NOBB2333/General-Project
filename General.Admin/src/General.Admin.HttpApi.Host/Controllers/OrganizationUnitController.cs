using General.Admin.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Auditing;

namespace General.Admin.Controllers;

[ApiController]
[Authorize]
[ApiExplorerSettings(GroupName = ApiDocGroups.Platform)]
[Route("api/app/organization-unit")]
public class OrganizationUnitController : ControllerBase
{
    private readonly PlatformOrganizationUnitService _organizationUnitService;

    public OrganizationUnitController(PlatformOrganizationUnitService organizationUnitService)
    {
        _organizationUnitService = organizationUnitService;
    }

    [HttpGet("tree")]
    public async Task<ActionResult<ApiResponse<List<OrganizationUnitTreeDto>>>> GetTreeAsync()
    {
        return ApiResponse<List<OrganizationUnitTreeDto>>.Ok(await _organizationUnitService.GetTreeAsync());
    }

    [Authorize(AdminPermissions.Platform.OrganizationManage)]
    [PlatformEndpoint("Platform.Organization.Manage")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<bool>>> CreateAsync([FromBody] OrganizationUnitSaveInput input)
    {
        await _organizationUnitService.CreateAsync(input);
        return ApiResponse<bool>.Ok(true);
    }

    [Authorize(AdminPermissions.Platform.OrganizationManage)]
    [PlatformEndpoint("Platform.Organization.Manage")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateAsync(Guid id, [FromBody] OrganizationUnitSaveInput input)
    {
        await _organizationUnitService.UpdateAsync(id, input);
        return ApiResponse<bool>.Ok(true);
    }

    [Authorize(AdminPermissions.Platform.OrganizationManage)]
    [PlatformEndpoint("Platform.Organization.Manage")]
    [HttpPut("{id:guid}/move")]
    public async Task<ActionResult<ApiResponse<bool>>> MoveAsync(Guid id, [FromBody] OrganizationUnitMoveInput input)
    {
        await _organizationUnitService.MoveAsync(id, input.ParentId);
        return ApiResponse<bool>.Ok(true);
    }

    [Authorize(AdminPermissions.Platform.OrganizationManage)]
    [PlatformEndpoint("Platform.Organization.Manage")]
    [HttpPut("{id:guid}/members/transfer")]
    public async Task<ActionResult<ApiResponse<bool>>> TransferMembersAsync(
        Guid id,
        [FromBody] OrganizationUnitMemberTransferInput input)
    {
        await _organizationUnitService.TransferMembersAsync(id, input);
        return ApiResponse<bool>.Ok(true);
    }

    [Authorize(AdminPermissions.Platform.OrganizationManage)]
    [PlatformEndpoint("Platform.Organization.Manage")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteAsync(Guid id)
    {
        await _organizationUnitService.DeleteAsync(id);
        return ApiResponse<bool>.Ok(true);
    }
}
