using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Linq;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Users;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace General.Admin.Platform;

public class PlatformUserService : ITransientDependency
{
    private const string EmployeeNoPrefix = "EMP";
    private static readonly Regex EmployeeNoRegex = new(@"^EMP(?<number>\d{6})$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex PhoneNumberRegex = new(@"^1[3-9]\d{9}$", RegexOptions.Compiled);
    private static readonly Regex UsernameRegex = new(@"^[A-Za-z][A-Za-z0-9._-]{2,63}$", RegexOptions.Compiled);

    private readonly IRepository<AppExternalAccountMapping, Guid> _externalAccountMappingRepository;
    private readonly IRepository<AppRoleAuthorization, Guid> _roleAuthorizationRepository;
    private readonly IRepository<AppUserProfile, Guid> _userProfileRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;
    private readonly IAsyncQueryableExecuter _asyncQueryableExecuter;
    private readonly CurrentUserDataScopeService _dataScopeService;
    private readonly IPlatformIdentityLookupService _identityLookupService;
    private readonly IRepository<OrganizationUnit, Guid> _organizationUnitRepository;
    private readonly IRepository<IdentityRole, Guid> _roleRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;
    private readonly IRepository<IdentityUserOrganizationUnit> _userOrganizationUnitRepository;
    private readonly IdentityUserManager _userManager;
    private readonly IDataFilter<IMultiTenant> _multiTenantFilter;
    private readonly IRepository<Tenant, Guid> _tenantRepository;
    private readonly PlatformCacheService _platformCacheService;
    private readonly ILogger<PlatformUserService> _logger;

    public PlatformUserService(
        ICurrentUser currentUser,
        ICurrentTenant currentTenant,
        IAsyncQueryableExecuter asyncQueryableExecuter,
        CurrentUserDataScopeService dataScopeService,
        IPlatformIdentityLookupService identityLookupService,
        IRepository<AppExternalAccountMapping, Guid> externalAccountMappingRepository,
        IRepository<AppRoleAuthorization, Guid> roleAuthorizationRepository,
        IRepository<AppUserProfile, Guid> userProfileRepository,
        IRepository<OrganizationUnit, Guid> organizationUnitRepository,
        IRepository<IdentityRole, Guid> roleRepository,
        IRepository<IdentityUser, Guid> userRepository,
        IRepository<IdentityUserOrganizationUnit> userOrganizationUnitRepository,
        IdentityUserManager userManager,
        IDataFilter<IMultiTenant> multiTenantFilter,
        IRepository<Tenant, Guid> tenantRepository,
        PlatformCacheService platformCacheService,
        ILogger<PlatformUserService> logger)
    {
        _currentUser = currentUser;
        _currentTenant = currentTenant;
        _asyncQueryableExecuter = asyncQueryableExecuter;
        _dataScopeService = dataScopeService;
        _identityLookupService = identityLookupService;
        _externalAccountMappingRepository = externalAccountMappingRepository;
        _roleAuthorizationRepository = roleAuthorizationRepository;
        _organizationUnitRepository = organizationUnitRepository;
        _roleRepository = roleRepository;
        _userProfileRepository = userProfileRepository;
        _userRepository = userRepository;
        _userOrganizationUnitRepository = userOrganizationUnitRepository;
        _userManager = userManager;
        _multiTenantFilter = multiTenantFilter;
        _tenantRepository = tenantRepository;
        _platformCacheService = platformCacheService;
        _logger = logger;
    }

    public async Task<CurrentUserInfoDto> GetCurrentUserInfoAsync(string accessToken)
    {
        if (!_currentUser.Id.HasValue)
        {
            throw new InvalidOperationException("Current user is not available.");
        }

        var isHostTenantOperation = IsHostTenantOperation();
        var user = await GetCurrentIdentityUserAsync(isHostTenantOperation);
        var roles = isHostTenantOperation
            ? (_currentUser.Roles ?? [])
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList()
            : (await _userManager.GetRolesAsync(user))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();
        var userOrganizationUnitQueryable = await _userOrganizationUnitRepository.GetQueryableAsync();
        var organizationUnitIds = await _asyncQueryableExecuter.ToListAsync(
            userOrganizationUnitQueryable
                .Where(x => x.UserId == user.Id)
                .Select(x => x.OrganizationUnitId));
        var organizationUnitQueryable = await _organizationUnitRepository.GetQueryableAsync();
        var organizationUnitNames = await _asyncQueryableExecuter.ToListAsync(
            organizationUnitQueryable
                .Where(x => organizationUnitIds.Contains(x.Id))
                .Select(x => x.DisplayName)
                .Distinct()
                .OrderBy(x => x));
        var profile = await _userProfileRepository.FindAsync(x => x.UserId == user.Id);

        return new CurrentUserInfoDto
        {
            Desc = BuildUserDescription(roles),
            HomePath = ResolveHomePath(roles),
            IsHostTenantOperation = isHostTenantOperation,
            LastLoginTime = profile?.LastLoginTime,
            OperationTenantId = isHostTenantOperation ? _currentTenant.Id : null,
            OperationTenantName = isHostTenantOperation
                ? GetClaimValue(PlatformTenantOperationClaimTypes.OperationTenantName)
                : null,
            OrganizationUnitNames = organizationUnitNames,
            RealName = string.IsNullOrWhiteSpace($"{user.Name}{user.Surname}".Trim())
                ? user.UserName ?? string.Empty
                : $"{user.Name}{user.Surname}".Trim(),
            Roles = roles,
            Token = accessToken,
            Username = user.UserName ?? string.Empty
        };
    }

    private async Task<IdentityUser> GetCurrentIdentityUserAsync(bool disableTenantFilter)
    {
        if (!disableTenantFilter)
        {
            return await _userManager.GetByIdAsync(_currentUser.Id!.Value);
        }

        using (_multiTenantFilter.Disable())
        {
            return await _userManager.GetByIdAsync(_currentUser.Id!.Value);
        }
    }

    private string? GetClaimValue(string claimType)
    {
        return _currentUser.FindClaim(claimType)?.Value;
    }

    private bool IsHostTenantOperation()
    {
        return string.Equals(
            GetClaimValue(PlatformTenantOperationClaimTypes.HostTenantOperation),
            "true",
            StringComparison.OrdinalIgnoreCase);
    }

    public async Task<PlatformPagedResultDto<PlatformUserListItemDto>> GetListAsync(PlatformUserListInput input)
    {
        try
        {
            var keyword = input.Keyword?.Trim();
            var maxResultCount = Math.Clamp(input.MaxResultCount, 1, 100);
            var skipCount = Math.Max(0, input.SkipCount);
            var organizationUnits = (await _organizationUnitRepository.GetListAsync())
                .OrderBy(x => x.Code)
                .ToList();
            var accessibleIds = await _dataScopeService.GetAccessibleOrganizationUnitIdsAsync();
            var filteredIds = FilterOrganizationUnitIds(input.OrganizationUnitId, organizationUnits, accessibleIds);

            var userOrganizationUnitsQueryable = await _userOrganizationUnitRepository.GetQueryableAsync();
            var userOrganizationUnits = await _asyncQueryableExecuter.ToListAsync(
                userOrganizationUnitsQueryable.Where(x => filteredIds.Contains(x.OrganizationUnitId)));

            var dataScopedUserIds = userOrganizationUnits
                .Select(x => x.UserId)
                .Distinct()
                .ToHashSet();
            var visibleUserIds = await ResolveVisibleUserIdsAsync(dataScopedUserIds);
            if (visibleUserIds.Count == 0)
            {
                return new PlatformPagedResultDto<PlatformUserListItemDto>();
            }

            if (!string.IsNullOrWhiteSpace(input.RoleName))
            {
                var roleUserIds = await _identityLookupService.GetUserIdsByRoleNameAsync(input.RoleName);
                visibleUserIds.IntersectWith(roleUserIds);
                if (visibleUserIds.Count == 0)
                {
                    return new PlatformPagedResultDto<PlatformUserListItemDto>();
                }
            }

            var usersQueryable = await _userRepository.GetQueryableAsync();
            usersQueryable = usersQueryable.Where(x => visibleUserIds.Contains(x.Id));

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                usersQueryable = usersQueryable.Where(x =>
                    (x.UserName != null && x.UserName.Contains(keyword)) ||
                    (x.Name != null && x.Name.Contains(keyword)) ||
                    (x.Email != null && x.Email.Contains(keyword)));
            }

            if (input.IsActive.HasValue)
            {
                usersQueryable = usersQueryable.Where(x => x.IsActive == input.IsActive.Value);
            }

            var totalCount = await _asyncQueryableExecuter.CountAsync(usersQueryable);
            var users = await _asyncQueryableExecuter.ToListAsync(
                usersQueryable
                    .OrderBy(x => x.UserName)
                    .Skip(skipCount)
                    .Take(maxResultCount));
            if (users.Count == 0)
            {
                return new PlatformPagedResultDto<PlatformUserListItemDto>
                {
                    TotalCount = totalCount
                };
            }

            var userIds = users.Select(x => x.Id).ToList();
            var profilesQueryable = await _userProfileRepository.GetQueryableAsync();
            var profiles = (await _asyncQueryableExecuter.ToListAsync(
                    profilesQueryable.Where(x => userIds.Contains(x.UserId))))
                .GroupBy(x => x.UserId)
                .ToDictionary(
                    x => x.Key,
                    x => x.OrderByDescending(item => item.LastModificationTime ?? item.CreationTime)
                        .ThenByDescending(item => item.Id)
                        .First());

            var mappingsQueryable = await _externalAccountMappingRepository.GetQueryableAsync();
            var mappings = (await _asyncQueryableExecuter.ToListAsync(
                    mappingsQueryable.Where(x => userIds.Contains(x.UserId))))
                .GroupBy(x => x.UserId)
                .ToDictionary(
                    x => x.Key,
                    x => x.OrderByDescending(item => item.BoundAt)
                        .Select(item => new PlatformExternalAccountMappingDto
                        {
                            BoundAt = item.BoundAt,
                            ExternalSource = item.ExternalSource,
                            ExternalUserId = item.ExternalUserId,
                            Id = item.Id,
                            LastSyncedAt = item.LastSyncedAt,
                            Remark = item.Remark,
                            Status = item.Status,
                            UserId = item.UserId
                        })
                        .ToList());
            var organizationUnitNames = organizationUnits.ToDictionary(x => x.Id, x => x.DisplayName);
            var userOrganizationMap = userOrganizationUnits
                .GroupBy(x => x.UserId)
                .ToDictionary(
                    x => x.Key,
                    x => x.Select(item => organizationUnitNames.GetValueOrDefault(item.OrganizationUnitId))
                        .Where(name => !string.IsNullOrWhiteSpace(name))
                        .Select(name => name!)
                        .Distinct()
                        .OrderBy(name => name)
                        .ToList());

            var userRoleMap = await _identityLookupService.GetRoleNamesByUserIdsAsync(userIds);

            var distinctTenantIds = users.Select(x => x.TenantId).Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();
            var tenants = distinctTenantIds.Count > 0
                ? (await _asyncQueryableExecuter.ToListAsync(
                        (await _tenantRepository.GetQueryableAsync()).Where(t => distinctTenantIds.Contains(t.Id))))
                    .ToDictionary(t => t.Id, t => t.Name)
                : [];

            var result = new List<PlatformUserListItemDto>();
            foreach (var user in users)
            {
                var roles = userRoleMap.GetValueOrDefault(user.Id) ?? [];

                result.Add(new PlatformUserListItemDto
                {
                    DisplayName = string.IsNullOrWhiteSpace($"{user.Name}{user.Surname}".Trim())
                        ? user.UserName ?? string.Empty
                        : $"{user.Name}{user.Surname}".Trim(),
                    Email = user.Email ?? string.Empty,
                    EmployeeNo = profiles.GetValueOrDefault(user.Id)?.EmployeeNo,
                    ExternalAccounts = mappings.GetValueOrDefault(user.Id) ?? [],
                    ExternalSource = profiles.GetValueOrDefault(user.Id)?.ExternalSource,
                    ExternalUserId = profiles.GetValueOrDefault(user.Id)?.ExternalUserId,
                    Id = user.Id,
                    IsActive = user.IsActive,
                    IsOnline = PlatformUserActivityService.IsOnline(profiles.GetValueOrDefault(user.Id)?.LastSeenAt),
                    LastLoginTime = profiles.GetValueOrDefault(user.Id)?.LastLoginTime,
                    OrganizationUnitNames = userOrganizationMap.GetValueOrDefault(user.Id) ?? [],
                    PhoneNumber = profiles.GetValueOrDefault(user.Id)?.PhoneNumber,
                    Roles = roles,
                    TenantName = user.TenantId.HasValue
                        ? tenants.GetValueOrDefault(user.TenantId.Value) ?? "未知租户"
                        : "宿主",
                    Username = user.UserName ?? string.Empty
                });
            }

            return new PlatformPagedResultDto<PlatformUserListItemDto>
            {
                Items = result,
                TotalCount = totalCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to get user list. CurrentUserId={CurrentUserId}, Roles={Roles}, OrganizationUnitId={OrganizationUnitId}, Keyword={Keyword}, IsActive={IsActive}, RoleName={RoleName}",
                _currentUser.Id,
                _currentUser.Roles == null ? string.Empty : string.Join(",", _currentUser.Roles),
                input.OrganizationUnitId,
                input.Keyword,
                input.IsActive,
                input.RoleName);
            throw;
        }
    }

    private async Task<HashSet<Guid>> ResolveVisibleUserIdsAsync(HashSet<Guid> dataScopedUserIds)
    {
        if (_currentUser.IsInRole(PlatformRoleNames.Admin))
        {
            return dataScopedUserIds;
        }

        if (!_currentUser.Id.HasValue)
        {
            return [];
        }

        var currentRoleNames = _currentUser.Roles?.Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? [];
        if (currentRoleNames.Count == 0)
        {
            return new HashSet<Guid> { _currentUser.Id.Value };
        }

        var normalizedRoleNames = currentRoleNames
            .Select(x => x.Trim().ToUpperInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var roleQueryable = await _roleRepository.GetQueryableAsync();
        var roleIds = (await _asyncQueryableExecuter.ToListAsync(
                roleQueryable
                    .Where(x => x.NormalizedName != null && normalizedRoleNames.Contains(x.NormalizedName))
                    .Select(x => x.Id)))
            .ToHashSet();

        if (roleIds.Count == 0)
        {
            return new HashSet<Guid> { _currentUser.Id.Value };
        }

        var authorizationQueryable = await _roleAuthorizationRepository.GetQueryableAsync();
        var authorizations = await _asyncQueryableExecuter.ToListAsync(
            authorizationQueryable.Where(x => roleIds.Contains(x.RoleId)));

        if (authorizations.Count == 0)
        {
            return dataScopedUserIds.Count == 0
                ? new HashSet<Guid> { _currentUser.Id.Value }
                : dataScopedUserIds;
        }

        if (authorizations.Any(x => x.AccountScopeMode == PlatformAuthorizationDefaults.AccountScopeAll))
        {
            var userQueryable = await _userRepository.GetQueryableAsync();
            return (await _asyncQueryableExecuter.ToListAsync(userQueryable.Select(x => x.Id)))
                .ToHashSet();
        }

        var explicitlyAllowedUserIds = authorizations
            .SelectMany(x => PlatformSerializationHelper.DeserializeGuidList(x.AllowedUserIds))
            .ToHashSet();
        explicitlyAllowedUserIds.Add(_currentUser.Id.Value);

        if (authorizations.Any(x => x.AccountScopeMode == PlatformAuthorizationDefaults.AccountScopeDataAndUsers))
        {
            var merged = dataScopedUserIds.ToHashSet();
            merged.UnionWith(explicitlyAllowedUserIds);
            return merged;
        }

        if (authorizations.Any(x => x.AccountScopeMode == PlatformAuthorizationDefaults.AccountScopeOnlyUsers))
        {
            return explicitlyAllowedUserIds;
        }

        return dataScopedUserIds.Count == 0
            ? explicitlyAllowedUserIds
            : dataScopedUserIds;
    }

    public async Task CreateAsync(PlatformUserSaveInput input)
    {
        await NormalizeAndValidateInputAsync(input, null);
        if (string.IsNullOrWhiteSpace(input.Password))
        {
            throw new InvalidOperationException("创建用户时密码不能为空。");
        }

        await EnsureOrganizationUnitAccessibleAsync(input.OrganizationUnitId);

        var user = new IdentityUser(Guid.NewGuid(), input.Username.Trim(), input.Email.Trim(), _currentTenant.Id)
        {
            Name = input.DisplayName.Trim(),
            Surname = string.Empty
        };
        user.SetIsActive(input.IsActive);

        EnsureSucceeded(await _userManager.CreateAsync(user, input.Password.Trim()));
        await UpsertUserProfileAsync(user.Id, input);
        await SyncUserRolesAsync(user, input.RoleNames);
        await SyncUserOrganizationUnitsAsync(user.Id, input.OrganizationUnitId);
    }

    public async Task UpdateAsync(Guid id, PlatformUserSaveInput input)
    {
        await NormalizeAndValidateInputAsync(input, id);
        await EnsureOrganizationUnitAccessibleAsync(input.OrganizationUnitId);

        var user = await _userManager.GetByIdAsync(id);
        user.SetUserNameWithoutValidation(input.Username.Trim(), input.Username.Trim().ToUpperInvariant());
        user.SetEmailWithoutValidation(input.Email.Trim(), input.Email.Trim().ToUpperInvariant());
        user.Name = input.DisplayName.Trim();
        user.Surname = string.Empty;
        user.SetIsActive(input.IsActive);

        EnsureSucceeded(await _userManager.UpdateAsync(user));
        await UpsertUserProfileAsync(user.Id, input);

        if (!string.IsNullOrWhiteSpace(input.Password))
        {
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            EnsureSucceeded(await _userManager.ResetPasswordAsync(user, resetToken, input.Password));
        }

        await SyncUserRolesAsync(user, input.RoleNames);
        await SyncUserOrganizationUnitsAsync(user.Id, input.OrganizationUnitId);
    }

    public async Task DeleteAsync(Guid id)
    {
        if (_currentUser.Id == id)
        {
            throw new InvalidOperationException("当前登录用户不允许删除自己。");
        }

        var user = await _userManager.GetByIdAsync(id);
        var userOrganizationUnitQueryable = await _userOrganizationUnitRepository.GetQueryableAsync();
        var userOrganizationUnits = await _asyncQueryableExecuter.ToListAsync(
            userOrganizationUnitQueryable.Where(x => x.UserId == id));
        if (userOrganizationUnits.Count > 0)
        {
            await _userOrganizationUnitRepository.DeleteManyAsync(userOrganizationUnits, autoSave: true);
        }

        var profile = await _userProfileRepository.FindAsync(x => x.UserId == id);
        if (profile != null)
        {
            await _userProfileRepository.DeleteAsync(profile, autoSave: true);
        }

        var mappingQueryable = await _externalAccountMappingRepository.GetQueryableAsync();
        var mappings = await _asyncQueryableExecuter.ToListAsync(
            mappingQueryable.Where(x => x.UserId == id));
        if (mappings.Count > 0)
        {
            await _externalAccountMappingRepository.DeleteManyAsync(mappings, autoSave: true);
        }

        EnsureSucceeded(await _userManager.DeleteAsync(user));
        await _platformCacheService.InvalidateAsync("menu");
        await _platformCacheService.InvalidateAsync("organization");
        await _platformCacheService.InvalidateAsync("role");
    }

    public async Task ChangePasswordAsync(PlatformPasswordChangeInput input)
    {
        if (!_currentUser.Id.HasValue)
        {
            throw new InvalidOperationException("Current user is not available.");
        }

        var user = await _userManager.GetByIdAsync(_currentUser.Id.Value);
        EnsureSucceeded(await _userManager.ChangePasswordAsync(user, input.CurrentPassword, input.NewPassword));
    }

    public async Task ResetPasswordAsync(Guid id, PlatformAdminResetPasswordInput input)
    {
        if (string.IsNullOrWhiteSpace(input.NewPassword))
        {
            throw new InvalidOperationException("新密码不能为空。");
        }

        var user = await _userManager.GetByIdAsync(id);
        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        EnsureSucceeded(await _userManager.ResetPasswordAsync(user, resetToken, input.NewPassword.Trim()));
    }

    public static string ResolveHomePath(IReadOnlyCollection<string> roles)
    {
        if (roles.Contains(PlatformRoleNames.Admin, StringComparer.OrdinalIgnoreCase))
        {
            return "/platform/workspace";
        }

        if (roles.Contains(PlatformRoleNames.Pmo, StringComparer.OrdinalIgnoreCase))
        {
            return "/project/pmo-overview";
        }

        if (roles.Contains(PlatformRoleNames.Pm, StringComparer.OrdinalIgnoreCase))
        {
            return "/project/pm-dashboard";
        }

        if (roles.Contains(PlatformRoleNames.Member, StringComparer.OrdinalIgnoreCase))
        {
            return "/project/my-related";
        }

        return "/project/projects";
    }

    private static string BuildUserDescription(IReadOnlyCollection<string> roles)
    {
        if (roles.Contains(PlatformRoleNames.Admin, StringComparer.OrdinalIgnoreCase))
        {
            return "系统管理员";
        }

        if (roles.Contains(PlatformRoleNames.Pmo, StringComparer.OrdinalIgnoreCase))
        {
            return "PMO 管理角色";
        }

        if (roles.Contains(PlatformRoleNames.Pm, StringComparer.OrdinalIgnoreCase))
        {
            return "项目经理角色";
        }

        if (roles.Contains(PlatformRoleNames.Member, StringComparer.OrdinalIgnoreCase))
        {
            return "项目成员角色";
        }

        return "经营查看角色";
    }

    private static HashSet<Guid> FilterOrganizationUnitIds(
        Guid? organizationUnitId,
        IReadOnlyCollection<OrganizationUnit> organizationUnits,
        IReadOnlySet<Guid> accessibleIds)
    {
        if (!organizationUnitId.HasValue)
        {
            return accessibleIds.ToHashSet();
        }

        var selectedOrganizationUnit = organizationUnits.FirstOrDefault(x => x.Id == organizationUnitId.Value);
        if (selectedOrganizationUnit == null)
        {
            return accessibleIds.ToHashSet();
        }

        return organizationUnits
            .Where(x =>
                accessibleIds.Contains(x.Id) &&
                x.Code.StartsWith(selectedOrganizationUnit.Code, StringComparison.Ordinal))
            .Select(x => x.Id)
            .ToHashSet();
    }

    private async Task EnsureOrganizationUnitAccessibleAsync(Guid? organizationUnitId)
    {
        if (!organizationUnitId.HasValue)
        {
            return;
        }

        var accessibleIds = await _dataScopeService.GetAccessibleOrganizationUnitIdsAsync();
        if (!accessibleIds.Contains(organizationUnitId.Value))
        {
            throw new InvalidOperationException("所选部门不在当前用户可操作范围内。");
        }
    }

    private async Task NormalizeAndValidateInputAsync(PlatformUserSaveInput input, Guid? currentUserId)
    {
        input.Username = input.Username.Trim();
        input.DisplayName = input.DisplayName.Trim();
        input.Email = input.Email.Trim();
        input.EmployeeNo = NormalizeOptional(input.EmployeeNo);
        input.ExternalSource = NormalizeOptional(input.ExternalSource);
        input.ExternalUserId = NormalizeOptional(input.ExternalUserId);
        input.PhoneNumber = NormalizeOptional(input.PhoneNumber);

        if (!UsernameRegex.IsMatch(input.Username))
        {
            throw new InvalidOperationException("账号必须以英文字母开头，只能包含英文、数字、点、下划线或中划线，长度 3-64 位。");
        }

        if (!string.IsNullOrWhiteSpace(input.Email) && !IsValidEmail(input.Email))
        {
            throw new InvalidOperationException("邮箱格式不正确，请输入有效邮箱地址。");
        }

        if (!string.IsNullOrWhiteSpace(input.PhoneNumber) && !PhoneNumberRegex.IsMatch(input.PhoneNumber))
        {
            throw new InvalidOperationException("手机号格式不正确，请输入 11 位中国大陆手机号。");
        }

        input.EmployeeNo ??= await GenerateNextEmployeeNoAsync();
        await EnsureEmployeeNoUniqueAsync(input.EmployeeNo, currentUserId);
    }

    private async Task<string> GenerateNextEmployeeNoAsync()
    {
        var profileQueryable = await _userProfileRepository.GetQueryableAsync();
        var employeeNos = await _asyncQueryableExecuter.ToListAsync(
            profileQueryable
                .Where(x => x.EmployeeNo != null && x.EmployeeNo != string.Empty)
                .Select(x => x.EmployeeNo!));
        var maxNumber = 0;
        foreach (var employeeNo in employeeNos)
        {
            if (string.IsNullOrWhiteSpace(employeeNo))
            {
                continue;
            }

            var match = EmployeeNoRegex.Match(employeeNo.Trim());
            if (match.Success && int.TryParse(match.Groups["number"].Value, out var number))
            {
                maxNumber = Math.Max(maxNumber, number);
            }
        }

        return $"{EmployeeNoPrefix}{maxNumber + 1:000000}";
    }

    private async Task EnsureEmployeeNoUniqueAsync(string employeeNo, Guid? currentUserId)
    {
        var normalizedEmployeeNo = employeeNo.Trim();
        var profileQueryable = await _userProfileRepository.GetQueryableAsync();
        var duplicated = await _asyncQueryableExecuter.AnyAsync(
            profileQueryable.Where(x =>
                x.EmployeeNo != null &&
                x.EmployeeNo.ToUpper() == normalizedEmployeeNo.ToUpper() &&
                (!currentUserId.HasValue || x.UserId != currentUserId.Value)));
        if (duplicated)
        {
            throw new InvalidOperationException($"工号 {employeeNo} 已存在，请更换后重试。");
        }
    }

    private async Task SyncUserOrganizationUnitsAsync(Guid userId, Guid? organizationUnitId)
    {
        var userOrganizationUnitQueryable = await _userOrganizationUnitRepository.GetQueryableAsync();
        var existingOrganizationUnits = await _asyncQueryableExecuter.ToListAsync(
            userOrganizationUnitQueryable
                .Where(x => x.UserId == userId)
                .Select(x => x.OrganizationUnitId)
                .Distinct());

        foreach (var existingOrganizationUnitId in existingOrganizationUnits)
        {
            await _userManager.RemoveFromOrganizationUnitAsync(userId, existingOrganizationUnitId);
        }

        if (organizationUnitId.HasValue)
        {
            await _userManager.AddToOrganizationUnitAsync(userId, organizationUnitId.Value);
        }

        await _platformCacheService.InvalidateAsync("organization");
    }

    private async Task SyncUserRolesAsync(IdentityUser user, IReadOnlyCollection<string> roleNames)
    {
        var requestedRoles = NormalizeRoleNames(roleNames);
        var normalizedRequestedRoles = requestedRoles
            .Select(x => x.Trim().ToUpperInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var roleQueryable = await _roleRepository.GetQueryableAsync();
        var existingRoles = (await _asyncQueryableExecuter.ToListAsync(
                roleQueryable
                    .Where(x => x.NormalizedName != null && normalizedRequestedRoles.Contains(x.NormalizedName))
                    .Select(x => x.Name)))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var currentRoles = (await _userManager.GetRolesAsync(user))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var rolesToAdd = existingRoles
            .Where(x => !currentRoles.Contains(x, StringComparer.OrdinalIgnoreCase))
            .ToList();
        var rolesToRemove = currentRoles
            .Where(x => !existingRoles.Contains(x, StringComparer.OrdinalIgnoreCase))
            .ToList();

        if (rolesToRemove.Count > 0)
        {
            EnsureSucceeded(await _userManager.RemoveFromRolesAsync(user, rolesToRemove));
        }

        if (rolesToAdd.Count > 0)
        {
            EnsureSucceeded(await _userManager.AddToRolesAsync(user, rolesToAdd));
        }

        if (rolesToRemove.Count > 0 || rolesToAdd.Count > 0)
        {
            await _platformCacheService.InvalidateAsync("menu");
            await _platformCacheService.InvalidateAsync("organization");
        }
    }

    private async Task UpsertUserProfileAsync(Guid userId, PlatformUserSaveInput input)
    {
        var profile = await _userProfileRepository.FindAsync(x => x.UserId == userId);
        if (profile == null)
        {
            profile = new AppUserProfile(
                Guid.NewGuid(),
                userId,
                input.EmployeeNo,
                input.PhoneNumber,
                input.ExternalSource,
                input.ExternalUserId,
                null);
            await _userProfileRepository.InsertAsync(profile, autoSave: true);
        }
        else
        {
            profile.Update(
                input.EmployeeNo,
                input.PhoneNumber,
                input.ExternalSource,
                input.ExternalUserId);
            await _userProfileRepository.UpdateAsync(profile, autoSave: true);
        }

        if (!string.IsNullOrWhiteSpace(input.ExternalSource) && !string.IsNullOrWhiteSpace(input.ExternalUserId))
        {
            var mapping = await _externalAccountMappingRepository.FindAsync(x =>
                x.UserId == userId &&
                x.ExternalSource == input.ExternalSource &&
                x.ExternalUserId == input.ExternalUserId);

            if (mapping == null)
            {
                await _externalAccountMappingRepository.InsertAsync(
                    new AppExternalAccountMapping(
                        Guid.NewGuid(),
                        userId,
                        input.ExternalSource,
                        input.ExternalUserId,
                        "active",
                        DateTime.UtcNow,
                        DateTime.UtcNow,
                        "绑定来源于平台用户维护"),
                    autoSave: true);
            }
            else
            {
                mapping.Update("active", DateTime.UtcNow, mapping.Remark);
                await _externalAccountMappingRepository.UpdateAsync(mapping, autoSave: true);
            }
        }
    }

    private static List<string> NormalizeRoleNames(IEnumerable<string> roleNames)
    {
        return roleNames
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().ToLowerInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static bool IsValidEmail(string value)
    {
        try
        {
            var address = new MailAddress(value);
            return string.Equals(address.Address, value, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static void EnsureSucceeded(Microsoft.AspNetCore.Identity.IdentityResult result)
    {
        if (result.Succeeded)
        {
            return;
        }

        throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
    }
}
