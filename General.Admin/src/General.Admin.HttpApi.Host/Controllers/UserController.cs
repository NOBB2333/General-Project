using System.IdentityModel.Tokens.Jwt;
using General.Admin.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Auditing;

namespace General.Admin.Controllers;

[ApiController]
[Authorize]
[ApiExplorerSettings(GroupName = ApiDocGroups.Platform)]
[Route("api/app/user")]
public class UserController : ControllerBase
{
    private readonly PlatformUserService _userService;

    public UserController(PlatformUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("info")]
    public async Task<ActionResult<ApiResponse<CurrentUserInfoDto>>> GetInfoAsync()
    {
        var token = string.Empty;
        if (Request.Headers.Authorization.Count > 0)
        {
            token = Request.Headers.Authorization.ToString().Replace("Bearer ", string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        return ApiResponse<CurrentUserInfoDto>.Ok(await _userService.GetCurrentUserInfoAsync(token));
    }

    [HttpGet("list")]
    public async Task<ActionResult<ApiResponse<List<PlatformUserListItemDto>>>> GetListAsync([FromQuery] PlatformUserListInput input)
    {
        return ApiResponse<List<PlatformUserListItemDto>>.Ok(await _userService.GetListAsync(input));
    }

    [Authorize(AdminPermissions.Platform.UserManage)]
    [PlatformEndpoint("Platform.User.Manage")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<bool>>> CreateAsync([FromBody] PlatformUserSaveInput input)
    {
        await _userService.CreateAsync(input);
        return ApiResponse<bool>.Ok(true);
    }

    [Authorize(AdminPermissions.Platform.UserManage)]
    [PlatformEndpoint("Platform.User.Manage")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateAsync(Guid id, [FromBody] PlatformUserSaveInput input)
    {
        await _userService.UpdateAsync(id, input);
        return ApiResponse<bool>.Ok(true);
    }

    [Authorize(AdminPermissions.Platform.UserManage)]
    [PlatformEndpoint("Platform.User.Manage")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteAsync(Guid id)
    {
        await _userService.DeleteAsync(id);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpPut("password")]
    public async Task<ActionResult<ApiResponse<bool>>> ChangePasswordAsync([FromBody] PlatformPasswordChangeInput input)
    {
        await _userService.ChangePasswordAsync(input);
        return ApiResponse<bool>.Ok(true);
    }

    [Authorize(AdminPermissions.Platform.UserManage)]
    [PlatformEndpoint("Platform.User.Manage")]
    [HttpPut("{id:guid}/reset-password")]
    public async Task<ActionResult<ApiResponse<bool>>> ResetPasswordAsync(
        Guid id,
        [FromBody] PlatformAdminResetPasswordInput input)
    {
        await _userService.ResetPasswordAsync(id, input);
        return ApiResponse<bool>.Ok(true);
    }
}
