using General.Admin.Infrastructure;
using General.Admin.PhaseOne;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Auditing;

namespace General.Admin.Controllers;

[ApiController]
[Authorize(Roles = PhaseOneRoleNames.Admin)]
[ApiExplorerSettings(GroupName = ApiDocGroups.Platform)]
[Route("api/app/online-user")]
public class OnlineUserController : ControllerBase
{
    private readonly PhaseOneOnlineUserService _onlineUserService;

    public OnlineUserController(PhaseOneOnlineUserService onlineUserService)
    {
        _onlineUserService = onlineUserService;
    }

    [HttpGet("list")]
    public async Task<ActionResult<ApiResponse<List<PhaseOneOnlineUserDto>>>> GetListAsync()
    {
        return ApiResponse<List<PhaseOneOnlineUserDto>>.Ok(await _onlineUserService.GetListAsync());
    }

    [HttpPost("{userId:guid}/force-logout")]
    [PlatformEndpoint("Platform.User.Manage")]
    public async Task<ActionResult<ApiResponse<bool>>> ForceLogoutAsync(Guid userId)
    {
        await _onlineUserService.ForceLogoutAsync(userId);
        return ApiResponse<bool>.Ok(true);
    }
}
