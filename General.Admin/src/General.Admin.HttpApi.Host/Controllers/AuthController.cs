using General.Admin.Infrastructure;
using General.Admin.PhaseOne;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Auditing;
using Volo.Abp.Identity;

namespace General.Admin.Controllers;

[ApiController]
[DisableAuditing]
[ApiExplorerSettings(GroupName = ApiDocGroups.Common)]
[Route("api/app/auth")]
public class AuthController : ControllerBase
{
    private readonly JwtTokenService _jwtTokenService;
    private readonly PhaseOneUserActivityService _userActivityService;
    private readonly IdentityUserManager _userManager;

    public AuthController(
        JwtTokenService jwtTokenService,
        PhaseOneUserActivityService userActivityService,
        IdentityUserManager userManager)
    {
        _jwtTokenService = jwtTokenService;
        _userActivityService = userActivityService;
        _userManager = userManager;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResultDto>>> LoginAsync([FromBody] LoginInput input)
    {
        var user = await _userManager.FindByNameAsync(input.Username);
        if (user == null || !await _userManager.CheckPasswordAsync(user, input.Password))
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<LoginResultDto>
                {
                    Code = -1,
                    Error = "Username or password is incorrect.",
                    Message = "Username or password is incorrect."
                });
        }

        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        await _userActivityService.MarkLoginAsync(user.Id);
        var token = _jwtTokenService.CreateAccessToken(user, roles);

        return ApiResponse<LoginResultDto>.Ok(new LoginResultDto
        {
            AccessToken = token,
            HomePath = PhaseOneUserService.ResolveHomePath(roles),
            RealName = string.IsNullOrWhiteSpace($"{user.Name}{user.Surname}".Trim())
                ? user.UserName ?? string.Empty
                : $"{user.Name}{user.Surname}".Trim(),
            Roles = roles,
            Username = user.UserName ?? string.Empty
        });
    }

    [Authorize]
    [HttpPost("logout")]
    public ActionResult<ApiResponse<bool>> LogoutAsync()
    {
        return ApiResponse<bool>.Ok(true);
    }

    [Authorize]
    [HttpGet("codes")]
    public async Task<ActionResult<ApiResponse<List<string>>>> GetCodesAsync([FromServices] PhaseOneMenuService menuService)
    {
        return ApiResponse<List<string>>.Ok(await menuService.GetCurrentAccessCodesAsync());
    }
}
