using General.Admin.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Auditing;

namespace General.Admin.Controllers;

[ApiController]
[Authorize]
[ApiExplorerSettings(GroupName = ApiDocGroups.Platform)]
[Route("api/app/platform/dict")]
public class DictController : ControllerBase
{
    private readonly PlatformDictService _dictService;

    public DictController(PlatformDictService dictService)
    {
        _dictService = dictService;
    }

    [HttpGet("types")]
    public async Task<ActionResult<ApiResponse<List<PlatformDictTypeDto>>>> GetTypesAsync()
    {
        return ApiResponse<List<PlatformDictTypeDto>>.Ok(await _dictService.GetTypesAsync());
    }

    [HttpGet("{dictTypeCode}/items")]
    public async Task<ActionResult<ApiResponse<List<PlatformDictItemDto>>>> GetItemsAsync(string dictTypeCode)
    {
        return ApiResponse<List<PlatformDictItemDto>>.Ok(await _dictService.GetItemsAsync(dictTypeCode));
    }

    [HttpGet("items")]
    public async Task<ActionResult<ApiResponse<Dictionary<string, List<PlatformDictItemDto>>>>> GetBatchItemsAsync([FromQuery] string codes)
    {
        return ApiResponse<Dictionary<string, List<PlatformDictItemDto>>>.Ok(await _dictService.GetBatchItemsAsync(codes));
    }

    [HttpGet("{dictTypeCode}/data")]
    public async Task<ActionResult<ApiResponse<List<PlatformDictDataDto>>>> GetDataAsync(string dictTypeCode)
    {
        return ApiResponse<List<PlatformDictDataDto>>.Ok(await _dictService.GetDataAsync(dictTypeCode));
    }

    [Authorize(AdminPermissions.Platform.DictManage)]
    [PlatformEndpoint("Platform.Dict.Manage")]
    [HttpPost("types")]
    public async Task<ActionResult<ApiResponse<PlatformDictTypeDto>>> CreateTypeAsync([FromBody] PlatformDictTypeSaveInput input)
    {
        return ApiResponse<PlatformDictTypeDto>.Ok(await _dictService.CreateTypeAsync(input));
    }

    [Authorize(AdminPermissions.Platform.DictManage)]
    [PlatformEndpoint("Platform.Dict.Manage")]
    [HttpPut("types/{id:guid}")]
    public async Task<ActionResult<ApiResponse<PlatformDictTypeDto>>> UpdateTypeAsync(
        Guid id,
        [FromBody] PlatformDictTypeSaveInput input)
    {
        return ApiResponse<PlatformDictTypeDto>.Ok(await _dictService.UpdateTypeAsync(id, input));
    }

    [Authorize(AdminPermissions.Platform.DictManage)]
    [PlatformEndpoint("Platform.Dict.Manage")]
    [HttpDelete("types/{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteTypeAsync(Guid id)
    {
        await _dictService.DeleteTypeAsync(id);
        return ApiResponse<bool>.Ok(true);
    }

    [Authorize(AdminPermissions.Platform.DictManage)]
    [PlatformEndpoint("Platform.Dict.Manage")]
    [HttpPost("{dictTypeCode}/data")]
    public async Task<ActionResult<ApiResponse<PlatformDictDataDto>>> CreateDataAsync(
        string dictTypeCode,
        [FromBody] PlatformDictDataSaveInput input)
    {
        return ApiResponse<PlatformDictDataDto>.Ok(await _dictService.CreateDataAsync(dictTypeCode, input));
    }

    [Authorize(AdminPermissions.Platform.DictManage)]
    [PlatformEndpoint("Platform.Dict.Manage")]
    [HttpPut("data/{id:guid}")]
    public async Task<ActionResult<ApiResponse<PlatformDictDataDto>>> UpdateDataAsync(
        Guid id,
        [FromBody] PlatformDictDataSaveInput input)
    {
        return ApiResponse<PlatformDictDataDto>.Ok(await _dictService.UpdateDataAsync(id, input));
    }

    [Authorize(AdminPermissions.Platform.DictManage)]
    [PlatformEndpoint("Platform.Dict.Manage")]
    [HttpDelete("data/{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteDataAsync(Guid id)
    {
        await _dictService.DeleteDataAsync(id);
        return ApiResponse<bool>.Ok(true);
    }
}
