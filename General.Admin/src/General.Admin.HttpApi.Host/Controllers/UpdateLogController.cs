using General.Admin.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Auditing;

namespace General.Admin.Controllers;

[ApiController]
[Authorize]
[ApiExplorerSettings(GroupName = ApiDocGroups.Platform)]
[Route("api/app/update-log")]
public class UpdateLogController : ControllerBase
{
    private readonly PlatformUpdateLogService _updateLogService;

    public UpdateLogController(PlatformUpdateLogService updateLogService)
    {
        _updateLogService = updateLogService;
    }

    [HttpGet("list")]
    public async Task<ActionResult<ApiResponse<List<PlatformUpdateLogDto>>>> GetListAsync()
    {
        return ApiResponse<List<PlatformUpdateLogDto>>.Ok(await _updateLogService.GetListAsync());
    }

    [Authorize(Roles = PlatformRoleNames.Admin)]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<bool>>> CreateAsync([FromBody] PlatformUpdateLogSaveInput input)
    {
        await _updateLogService.CreateAsync(input);
        return ApiResponse<bool>.Ok(true);
    }

    [Authorize(Roles = PlatformRoleNames.Admin)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateAsync(Guid id, [FromBody] PlatformUpdateLogSaveInput input)
    {
        await _updateLogService.UpdateAsync(id, input);
        return ApiResponse<bool>.Ok(true);
    }

    [Authorize(Roles = PlatformRoleNames.Admin)]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteAsync(Guid id)
    {
        await _updateLogService.DeleteAsync(id);
        return ApiResponse<bool>.Ok(true);
    }
}
