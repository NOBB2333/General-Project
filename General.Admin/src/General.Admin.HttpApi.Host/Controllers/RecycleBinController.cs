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
    public async Task<ActionResult<ApiResponse<List<PlatformRecycleBinItemDto>>>> GetListAsync([FromQuery] string? entityType)
    {
        return ApiResponse<List<PlatformRecycleBinItemDto>>.Ok(await _recycleBinService.GetListAsync(entityType));
    }

    [HttpPost("{entityType}/{id:guid}/restore")]
    [PlatformEndpoint("Platform.RecycleBin.Manage")]
    public async Task<ActionResult<ApiResponse<bool>>> RestoreAsync(string entityType, Guid id)
    {
        await _recycleBinService.RestoreAsync(entityType, id);
        return ApiResponse<bool>.Ok(true);
    }
}
