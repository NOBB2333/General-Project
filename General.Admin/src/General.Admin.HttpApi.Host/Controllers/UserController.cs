using System.IdentityModel.Tokens.Jwt;
using General.Admin.Infrastructure;
using General.Admin.PhaseOne;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Auditing;

namespace General.Admin.Controllers;

[ApiController]
[DisableAuditing]
[Authorize]
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

    [Authorize(Roles = PhaseOneRoleNames.Admin)]
    [HttpGet("list")]
    public async Task<ActionResult<ApiResponse<List<PhaseOneUserListItemDto>>>> GetListAsync([FromQuery] PhaseOneUserListInput input)
    {
        return ApiResponse<List<PhaseOneUserListItemDto>>.Ok(await _userService.GetListAsync(input));
    }

    [Authorize(Roles = PhaseOneRoleNames.Admin)]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<bool>>> CreateAsync([FromBody] PhaseOneUserSaveInput input)
    {
        await _userService.CreateAsync(input);
        return ApiResponse<bool>.Ok(true);
    }

    [Authorize(Roles = PhaseOneRoleNames.Admin)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateAsync(Guid id, [FromBody] PhaseOneUserSaveInput input)
    {
        await _userService.UpdateAsync(id, input);
        return ApiResponse<bool>.Ok(true);
    }

    [Authorize(Roles = PhaseOneRoleNames.Admin)]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteAsync(Guid id)
    {
        await _userService.DeleteAsync(id);
        return ApiResponse<bool>.Ok(true);
    }
}
