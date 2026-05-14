using General.Admin.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Auditing;
using Volo.Abp.Identity;
using Volo.Abp.Linq;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Claims;
using Volo.Abp.TenantManagement;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace General.Admin.Controllers;

[ApiController]
[DisableAuditing]
[ApiExplorerSettings(GroupName = ApiDocGroups.Platform)]
[Route("api/app/auth")]
public class AuthController : ControllerBase
{
    private readonly JwtTokenService _jwtTokenService;
    private readonly PlatformUserActivityService _userActivityService;
    private readonly IdentityUserManager _userManager;
    private readonly IRepository<IdentityUser, Guid> _userRepository;
    private readonly IRepository<Tenant, Guid> _tenantRepository;
    private readonly IRepository<AppTenantAuthorization, Guid> _tenantAuthorizationRepository;
    private readonly IAsyncQueryableExecuter _asyncQueryableExecuter;
    private readonly ICurrentTenant _currentTenant;
    private readonly IDataFilter<IMultiTenant> _multiTenantFilter;

    public AuthController(
        JwtTokenService jwtTokenService,
        PlatformUserActivityService userActivityService,
        IdentityUserManager userManager,
        IRepository<IdentityUser, Guid> userRepository,
        IRepository<Tenant, Guid> tenantRepository,
        IRepository<AppTenantAuthorization, Guid> tenantAuthorizationRepository,
        IAsyncQueryableExecuter asyncQueryableExecuter,
        ICurrentTenant currentTenant,
        IDataFilter<IMultiTenant> multiTenantFilter)
    {
        _jwtTokenService = jwtTokenService;
        _userActivityService = userActivityService;
        _userManager = userManager;
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
        _tenantAuthorizationRepository = tenantAuthorizationRepository;
        _asyncQueryableExecuter = asyncQueryableExecuter;
        _currentTenant = currentTenant;
        _multiTenantFilter = multiTenantFilter;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResultDto>>> LoginAsync([FromBody] LoginInput input)
    {
        var user = await ResolveLoginUserAsync(input.Username);
        if (user == null)
        {
            return InvalidLoginResult();
        }

        using (_currentTenant.Change(user.TenantId))
        {
            if (!user.IsActive)
            {
                return InvalidLoginResult();
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                return InvalidLoginResult();
            }

            if (!await _userManager.GetLockoutEnabledAsync(user))
            {
                EnsureSucceeded(await _userManager.SetLockoutEnabledAsync(user, true));
            }

            if (!await _userManager.CheckPasswordAsync(user, input.Password))
            {
                EnsureSucceeded(await _userManager.AccessFailedAsync(user));
                return InvalidLoginResult();
            }

            if (await _userManager.GetAccessFailedCountAsync(user) > 0)
            {
                EnsureSucceeded(await _userManager.ResetAccessFailedCountAsync(user));
            }

            var roles = (await _userManager.GetRolesAsync(user)).ToList();
            await _userActivityService.MarkLoginAsync(user.Id);
            var token = _jwtTokenService.CreateAccessToken(user, roles);

            return ApiResponse<LoginResultDto>.Ok(BuildLoginResult(user, roles, token));
        }
    }

    [Authorize]
    [HttpPost("logout")]
    public ActionResult<ApiResponse<bool>> LogoutAsync()
    {
        return ApiResponse<bool>.Ok(true);
    }

    [Authorize]
    [Authorize(AdminPermissions.Platform.TenantManage)]
    [HttpPost("tenant-operation/enter")]
    public async Task<ActionResult<ApiResponse<LoginResultDto>>> EnterTenantOperationAsync(
        [FromBody] EnterTenantOperationInput input)
    {
        if (_currentTenant.Id.HasValue)
        {
            return Forbid();
        }

        var currentUser = await GetCurrentHostUserAsync();
        if (!await _userManager.IsInRoleAsync(currentUser, PlatformRoleNames.Admin))
        {
            return Forbid();
        }

        var tenant = await _tenantRepository.GetAsync(input.TenantId);
        var tenantOperator = await ResolveTenantOperatorAsync(tenant.Id);
        List<string> roles;
        using (_currentTenant.Change(tenant.Id, tenant.Name))
        {
            roles = (await _userManager.GetRolesAsync(tenantOperator)).ToList();
        }
        var operationSessionId = Guid.NewGuid().ToString("N");
        var token = _jwtTokenService.CreateAccessToken(
            tenantOperator,
            roles,
            isHostTenantOperation: true,
            operationTenantId: tenant.Id,
            operationTenantName: tenant.Name,
            hostOperatorUserId: currentUser.Id,
            hostOperatorUserName: currentUser.UserName,
            operationSessionId: operationSessionId);

        return ApiResponse<LoginResultDto>.Ok(BuildLoginResult(tenantOperator, roles, token, tenant.Id, tenant.Name, true));
    }

    [Authorize]
    [HttpPost("tenant-operation/exit")]
    public async Task<ActionResult<ApiResponse<LoginResultDto>>> ExitTenantOperationAsync()
    {
        if (!IsHostTenantOperation())
        {
            return Forbid();
        }

        IdentityUser currentUser;
        List<string> roles;
        using (_currentTenant.Change(null))
        {
            currentUser = await GetHostOperatorUserAsync();
            roles = (await _userManager.GetRolesAsync(currentUser)).ToList();
        }

        var token = _jwtTokenService.CreateAccessToken(currentUser, roles);
        return ApiResponse<LoginResultDto>.Ok(BuildLoginResult(currentUser, roles, token));
    }

    [Authorize]
    [HttpGet("codes")]
    public async Task<ActionResult<ApiResponse<List<string>>>> GetCodesAsync([FromServices] PlatformMenuService menuService)
    {
        return ApiResponse<List<string>>.Ok(await menuService.GetCurrentAccessCodesAsync());
    }

    private static ActionResult<ApiResponse<LoginResultDto>> InvalidLoginResult()
    {
        return new ObjectResult(new ApiResponse<LoginResultDto>
        {
            Code = -1,
            Error = "Username or password is incorrect.",
            Message = "Username or password is incorrect."
        })
        {
            StatusCode = StatusCodes.Status403Forbidden
        };
    }

    private async Task<IdentityUser?> ResolveLoginUserAsync(string userName)
    {
        var normalizedUserName = _userManager.NormalizeName(userName);

        using (_currentTenant.Change(null))
        {
            var hostUser = await _userManager.FindByNameAsync(userName);
            if (hostUser != null)
            {
                return hostUser;
            }
        }

        List<IdentityUser> candidates;
        using (_multiTenantFilter.Disable())
        {
            var userQueryable = await _userRepository.GetQueryableAsync();
            candidates = await _asyncQueryableExecuter.ToListAsync(
                userQueryable
                .Where(x => x.TenantId.HasValue && x.NormalizedUserName == normalizedUserName)
                    .Take(2));
        }
        if (candidates.Count != 1)
        {
            return null;
        }

        using (_currentTenant.Change(candidates[0].TenantId))
        {
            return await _userManager.FindByNameAsync(userName);
        }
    }

    private async Task<IdentityUser> GetCurrentHostUserAsync()
    {
        var userId = User.FindFirst(AbpClaimTypes.UserId)?.Value;
        if (!Guid.TryParse(userId, out var id))
        {
            throw new InvalidOperationException("Current user is not available.");
        }

        using (_multiTenantFilter.Disable())
        {
            var user = await _userRepository.FindAsync(id);
            if (user == null || user.TenantId.HasValue || !user.IsActive)
            {
                throw new InvalidOperationException("Current host user is not available.");
            }

            return user;
        }
    }

    private async Task<IdentityUser> GetHostOperatorUserAsync()
    {
        var userId = User.FindFirst(PlatformTenantOperationClaimTypes.HostOperatorUserId)?.Value;
        if (!Guid.TryParse(userId, out var id))
        {
            throw new InvalidOperationException("Host operator user is not available.");
        }

        using (_multiTenantFilter.Disable())
        {
            var user = await _userRepository.FindAsync(id);
            if (user == null || user.TenantId.HasValue || !user.IsActive)
            {
                throw new InvalidOperationException("Host operator user is not available.");
            }

            return user;
        }
    }

    private async Task<IdentityUser> ResolveTenantOperatorAsync(Guid tenantId)
    {
        var authorization = await _tenantAuthorizationRepository.FindAsync(x => x.TenantId == tenantId);
        using (_currentTenant.Change(tenantId))
        {
            if (authorization?.AdminUserId.HasValue == true)
            {
                var configuredAdmin = await _userRepository.FindAsync(authorization.AdminUserId.Value);
                if (configuredAdmin is { IsActive: true })
                {
                    return configuredAdmin;
                }
            }

            var userQueryable = await _userRepository.GetQueryableAsync();
            var activeUsers = await _asyncQueryableExecuter.ToListAsync(
                userQueryable.Where(x => x.IsActive).Take(200));
            foreach (var user in activeUsers)
            {
                if (await _userManager.IsInRoleAsync(user, PlatformRoleNames.Admin))
                {
                    return user;
                }
            }
        }

        throw new InvalidOperationException("当前租户未配置可用管理员，无法进入租户运维。");
    }

    private bool IsHostTenantOperation()
    {
        return string.Equals(
            User.FindFirst(PlatformTenantOperationClaimTypes.HostTenantOperation)?.Value,
            "true",
            StringComparison.OrdinalIgnoreCase);
    }

    private static LoginResultDto BuildLoginResult(
        IdentityUser user,
        List<string> roles,
        string token,
        Guid? operationTenantId = null,
        string? operationTenantName = null,
        bool isHostTenantOperation = false)
    {
        return new LoginResultDto
        {
            AccessToken = token,
            HomePath = PlatformUserService.ResolveHomePath(roles),
            IsHostTenantOperation = isHostTenantOperation,
            OperationTenantId = operationTenantId,
            OperationTenantName = operationTenantName,
            RealName = string.IsNullOrWhiteSpace($"{user.Name}{user.Surname}".Trim())
                ? user.UserName ?? string.Empty
                : $"{user.Name}{user.Surname}".Trim(),
            Roles = roles,
            Username = user.UserName ?? string.Empty
        };
    }

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(";", result.Errors.Select(x => x.Description)));
        }
    }
}
