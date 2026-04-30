using General.Admin.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace General.Admin.Controllers;

[ApiController]
[Authorize(AdminPermissions.Platform.FileManage)]
[ApiExplorerSettings(GroupName = ApiDocGroups.Platform)]
[Route("api/app/platform/file-storage-sources")]
public class FileStorageSourceController : ControllerBase
{
    private readonly PlatformFileStorageSourceService _sourceService;

    public FileStorageSourceController(PlatformFileStorageSourceService sourceService)
    {
        _sourceService = sourceService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<PlatformFileStorageSourceDto>>>> GetListAsync(
        [FromQuery] bool enabledOnly = false)
    {
        return ApiResponse<List<PlatformFileStorageSourceDto>>.Ok(await _sourceService.GetListAsync(enabledOnly));
    }

    [HttpPost]
    [PlatformEndpoint("Platform.File.Manage")]
    public async Task<ActionResult<ApiResponse<PlatformFileStorageSourceDto>>> CreateAsync(
        [FromBody] PlatformFileStorageSourceSaveInput input)
    {
        return ApiResponse<PlatformFileStorageSourceDto>.Ok(await _sourceService.CreateAsync(input));
    }

    [HttpPut("{id:guid}")]
    [PlatformEndpoint("Platform.File.Manage")]
    public async Task<ActionResult<ApiResponse<PlatformFileStorageSourceDto>>> UpdateAsync(
        Guid id,
        [FromBody] PlatformFileStorageSourceSaveInput input)
    {
        return ApiResponse<PlatformFileStorageSourceDto>.Ok(await _sourceService.UpdateAsync(id, input));
    }

    [HttpPost("{id:guid}/toggle")]
    [PlatformEndpoint("Platform.File.Manage")]
    public async Task<ActionResult<ApiResponse<bool>>> ToggleAsync(
        Guid id,
        [FromBody] PlatformFileStorageSourceToggleInput input)
    {
        await _sourceService.ToggleAsync(id, input.IsEnabled);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpPost("{id:guid}/default")]
    [PlatformEndpoint("Platform.File.Manage")]
    public async Task<ActionResult<ApiResponse<bool>>> SetDefaultAsync(Guid id)
    {
        await _sourceService.SetDefaultAsync(id);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpDelete("{id:guid}")]
    [PlatformEndpoint("Platform.File.Manage")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteAsync(Guid id)
    {
        await _sourceService.DeleteAsync(id);
        return ApiResponse<bool>.Ok(true);
    }
}
