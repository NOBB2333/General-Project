using General.Admin.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Auditing;

namespace General.Admin.Controllers;

[ApiController]
[Authorize(AdminPermissions.Platform.UserManage)]
[ApiExplorerSettings(GroupName = ApiDocGroups.Platform)]
[Route("api/app/online-user")]
public class OnlineUserController : ControllerBase
{
    private readonly PlatformOnlineUserService _onlineUserService;

    public OnlineUserController(PlatformOnlineUserService onlineUserService)
    {
        _onlineUserService = onlineUserService;
    }

    [HttpGet("list")]
    public async Task<ActionResult<ApiResponse<List<PlatformOnlineUserDto>>>> GetListAsync()
    {
        return ApiResponse<List<PlatformOnlineUserDto>>.Ok(await _onlineUserService.GetListAsync());
    }

    [HttpPost("{userId:guid}/force-logout")]
    [PlatformEndpoint("Platform.User.Manage")]
    public async Task<ActionResult<ApiResponse<bool>>> ForceLogoutAsync(Guid userId)
    {
        await _onlineUserService.ForceLogoutAsync(userId);
        return ApiResponse<bool>.Ok(true);
    }
}
