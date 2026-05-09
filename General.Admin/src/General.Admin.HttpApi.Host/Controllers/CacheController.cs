using General.Admin.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace General.Admin.Controllers;

[ApiController]
[Authorize(AdminPermissions.Platform.CacheManage)]
[ApiExplorerSettings(GroupName = ApiDocGroups.Platform)]
[Route("api/app/platform/cache")]
public class CacheController : ControllerBase
{
    private readonly PlatformCacheService _cacheService;

    public CacheController(PlatformCacheService cacheService)
    {
        _cacheService = cacheService;
    }

    [HttpGet("areas")]
    public async Task<ActionResult<ApiResponse<List<PlatformCacheAreaDto>>>> GetAreasAsync()
    {
        return ApiResponse<List<PlatformCacheAreaDto>>.Ok(await _cacheService.GetAreasAsync());
    }

    [HttpPost("refresh")]
    [PlatformEndpoint("Platform.Cache.Manage")]
    public async Task<ActionResult<ApiResponse<bool>>> RefreshAsync([FromBody] PlatformCacheRefreshInput input)
    {
        await _cacheService.RefreshAsync(input.Area);
        return ApiResponse<bool>.Ok(true);
    }
}
