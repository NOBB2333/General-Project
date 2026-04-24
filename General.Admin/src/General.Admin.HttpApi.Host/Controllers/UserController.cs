using System.IdentityModel.Tokens.Jwt;
using General.Admin.Infrastructure;
using General.Admin.PhaseOne;
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
    private readonly PhaseOneUserService _userService;

    public UserController(PhaseOneUserService userService)
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
    public async Task<ActionResult<ApiResponse<List<PhaseOneUserListItemDto>>>> GetListAsync([FromQuery] PhaseOneUserListInput input)
    {
        return ApiResponse<List<PhaseOneUserListItemDto>>.Ok(await _userService.GetListAsync(input));
    }

    [Authorize(Roles = PhaseOneRoleNames.Admin)]
    [PlatformEndpoint("Platform.User.Manage")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<bool>>> CreateAsync([FromBody] PhaseOneUserSaveInput input)
    {
        await _userService.CreateAsync(input);
        return ApiResponse<bool>.Ok(true);
    }

    [Authorize(Roles = PhaseOneRoleNames.Admin)]
    [PlatformEndpoint("Platform.User.Manage")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateAsync(Guid id, [FromBody] PhaseOneUserSaveInput input)
    {
        await _userService.UpdateAsync(id, input);
        return ApiResponse<bool>.Ok(true);
    }

    [Authorize(Roles = PhaseOneRoleNames.Admin)]
    [PlatformEndpoint("Platform.User.Manage")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteAsync(Guid id)
    {
        await _userService.DeleteAsync(id);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpPut("password")]
    public async Task<ActionResult<ApiResponse<bool>>> ChangePasswordAsync([FromBody] PhaseOnePasswordChangeInput input)
    {
        await _userService.ChangePasswordAsync(input);
        return ApiResponse<bool>.Ok(true);
    }

    [Authorize(Roles = PhaseOneRoleNames.Admin)]
    [PlatformEndpoint("Platform.User.Manage")]
    [HttpPut("{id:guid}/reset-password")]
    public async Task<ActionResult<ApiResponse<bool>>> ResetPasswordAsync(
        Guid id,
        [FromBody] PhaseOneAdminResetPasswordInput input)
    {
        await _userService.ResetPasswordAsync(id, input);
        return ApiResponse<bool>.Ok(true);
    }
}
