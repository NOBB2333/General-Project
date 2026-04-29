using General.Admin.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace General.Admin.Controllers;

[ApiController]
[AllowAnonymous]
[ApiExplorerSettings(GroupName = ApiDocGroups.Platform)]
[Route("api/open/dict")]
public class OpenDictController : ControllerBase
{
    private readonly PlatformDictService _dictService;

    public OpenDictController(PlatformDictService dictService)
    {
        _dictService = dictService;
    }

    [HttpGet("{dictTypeCode}/items")]
    [OpenApiScope("dict:read")]
    public async Task<ActionResult<ApiResponse<List<PlatformDictItemDto>>>> GetItemsAsync(string dictTypeCode)
    {
        return ApiResponse<List<PlatformDictItemDto>>.Ok(await _dictService.GetItemsAsync(dictTypeCode));
    }
}
