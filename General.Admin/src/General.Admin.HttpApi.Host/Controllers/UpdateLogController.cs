using General.Admin.Infrastructure;
using General.Admin.PhaseOne;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Auditing;

namespace General.Admin.Controllers;

[ApiController]
[Authorize]
[Route("api/app/update-log")]
public class UpdateLogController : ControllerBase
{
    private readonly PhaseOneUpdateLogService _updateLogService;

    public UpdateLogController(PhaseOneUpdateLogService updateLogService)
    {
        _updateLogService = updateLogService;
    }

    [HttpGet("list")]
    public async Task<ActionResult<ApiResponse<List<PhaseOneUpdateLogDto>>>> GetListAsync()
    {
        return ApiResponse<List<PhaseOneUpdateLogDto>>.Ok(await _updateLogService.GetListAsync());
    }

    [Authorize(Roles = PhaseOneRoleNames.Admin)]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<bool>>> CreateAsync([FromBody] PhaseOneUpdateLogSaveInput input)
    {
        await _updateLogService.CreateAsync(input);
        return ApiResponse<bool>.Ok(true);
    }

    [Authorize(Roles = PhaseOneRoleNames.Admin)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateAsync(Guid id, [FromBody] PhaseOneUpdateLogSaveInput input)
    {
        await _updateLogService.UpdateAsync(id, input);
        return ApiResponse<bool>.Ok(true);
    }

    [Authorize(Roles = PhaseOneRoleNames.Admin)]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteAsync(Guid id)
    {
        await _updateLogService.DeleteAsync(id);
        return ApiResponse<bool>.Ok(true);
    }
}
