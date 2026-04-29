using General.Admin.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Auditing;

namespace General.Admin.Controllers;

[ApiController]
[Authorize(AdminPermissions.Platform.ConfigManage)]
[ApiExplorerSettings(GroupName = ApiDocGroups.Platform)]
[Route("api/app/platform/config")]
public class ConfigController : ControllerBase
{
    private readonly PlatformConfigService _configService;

    public ConfigController(PlatformConfigService configService)
    {
        _configService = configService;
    }

    [HttpGet("list")]
    public async Task<ActionResult<ApiResponse<List<PlatformConfigDto>>>> GetListAsync()
    {
        return ApiResponse<List<PlatformConfigDto>>.Ok(await _configService.GetListAsync());
    }

    [HttpPut("{code}")]
    [PlatformEndpoint("Platform.Config.Manage")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateAsync(
        string code,
        [FromBody] PlatformConfigSaveInput input)
    {
        await _configService.UpdateAsync(code, input);
        return ApiResponse<bool>.Ok(true);
    }
}
