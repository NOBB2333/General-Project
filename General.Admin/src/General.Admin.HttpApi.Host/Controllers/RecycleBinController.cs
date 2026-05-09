using General.Admin.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace General.Admin.Controllers;

[ApiController]
[Authorize(AdminPermissions.Platform.RecycleBinManage)]
[ApiExplorerSettings(GroupName = ApiDocGroups.Platform)]
[Route("api/app/platform/recycle-bin")]
public class RecycleBinController : ControllerBase
{
    private readonly PlatformRecycleBinService _recycleBinService;

    public RecycleBinController(PlatformRecycleBinService recycleBinService)
    {
        _recycleBinService = recycleBinService;
    }

    [HttpGet("items")]
    public async Task<ActionResult<ApiResponse<PlatformPagedResultDto<PlatformRecycleBinItemDto>>>> GetListAsync([FromQuery] PlatformRecycleBinListInput input)
    {
        return ApiResponse<PlatformPagedResultDto<PlatformRecycleBinItemDto>>.Ok(await _recycleBinService.GetListAsync(input));
    }

    [HttpPost("{entityType}/{id:guid}/restore")]
    [PlatformEndpoint("Platform.RecycleBin.Manage")]
    public async Task<ActionResult<ApiResponse<bool>>> RestoreAsync(string entityType, Guid id)
    {
        await _recycleBinService.RestoreAsync(entityType, id);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpDelete("{entityType}/{id:guid}")]
    [PlatformEndpoint("Platform.RecycleBin.Manage")]
    public async Task<ActionResult<ApiResponse<bool>>> DeletePermanentlyAsync(string entityType, Guid id)
    {
        await _recycleBinService.DeletePermanentlyAsync(entityType, id, HttpContext.RequestAborted);
        return ApiResponse<bool>.Ok(true);
    }
}
