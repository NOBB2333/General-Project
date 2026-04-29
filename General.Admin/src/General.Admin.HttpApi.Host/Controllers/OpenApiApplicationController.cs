using General.Admin.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace General.Admin.Controllers;

[ApiController]
[Authorize(AdminPermissions.Platform.OpenApiManage)]
[ApiExplorerSettings(GroupName = ApiDocGroups.Platform)]
[Route("api/app/platform/open-api")]
public class OpenApiApplicationController : ControllerBase
{
    private readonly PlatformOpenApiApplicationService _openApiApplicationService;

    public OpenApiApplicationController(PlatformOpenApiApplicationService openApiApplicationService)
    {
        _openApiApplicationService = openApiApplicationService;
    }

    [HttpGet("applications")]
    public async Task<ActionResult<ApiResponse<List<PlatformOpenApiApplicationDto>>>> GetListAsync()
    {
        return ApiResponse<List<PlatformOpenApiApplicationDto>>.Ok(await _openApiApplicationService.GetListAsync());
    }

    [HttpPost("applications")]
    [PlatformEndpoint("Platform.OpenApi.Manage")]
    public async Task<ActionResult<ApiResponse<PlatformOpenApiApplicationSecretDto>>> CreateAsync(
        [FromBody] PlatformOpenApiApplicationSaveInput input)
    {
        return ApiResponse<PlatformOpenApiApplicationSecretDto>.Ok(await _openApiApplicationService.CreateAsync(input));
    }

    [HttpPut("applications/{id:guid}")]
    [PlatformEndpoint("Platform.OpenApi.Manage")]
    public async Task<ActionResult<ApiResponse<PlatformOpenApiApplicationDto>>> UpdateAsync(
        Guid id,
        [FromBody] PlatformOpenApiApplicationSaveInput input)
    {
        return ApiResponse<PlatformOpenApiApplicationDto>.Ok(await _openApiApplicationService.UpdateAsync(id, input));
    }

    [HttpPost("applications/{id:guid}/reset-secret")]
    [PlatformEndpoint("Platform.OpenApi.Manage")]
    public async Task<ActionResult<ApiResponse<PlatformOpenApiApplicationSecretDto>>> ResetSecretAsync(Guid id)
    {
        return ApiResponse<PlatformOpenApiApplicationSecretDto>.Ok(await _openApiApplicationService.ResetSecretAsync(id));
    }

    [HttpDelete("applications/{id:guid}")]
    [PlatformEndpoint("Platform.OpenApi.Manage")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteAsync(Guid id)
    {
        await _openApiApplicationService.DeleteAsync(id);
        return ApiResponse<bool>.Ok(true);
    }
}
