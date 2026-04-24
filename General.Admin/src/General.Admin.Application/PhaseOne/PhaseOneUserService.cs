using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Linq;
using Volo.Abp.TenantManagement;
using Volo.Abp.Users;

namespace General.Admin.PhaseOne;

public class PhaseOneUserService : ITransientDependency
{
    private readonly IRepository<AppExternalAccountMapping, Guid> _externalAccountMappingRepository;
    private readonly IRepository<AppRoleAuthorization, Guid> _roleAuthorizationRepository;
    private readonly IRepository<AppUserProfile, Guid> _userProfileRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IAsyncQueryableExecuter _asyncQueryableExecuter;
    private readonly CurrentUserDataScopeService _dataScopeService;
    private readonly IRepository<OrganizationUnit, Guid> _organizationUnitRepository;
    private readonly IRepository<IdentityRole, Guid> _roleRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;
    private readonly IRepository<IdentityUserOrganizationUnit> _userOrganizationUnitRepository;
    private readonly IdentityUserManager _userManager;
    private readonly ITenantRepository _tenantRepository;

    public PhaseOneUserService(
        ICurrentUser currentUser,
        IAsyncQueryableExecuter asyncQueryableExecuter,
        CurrentUserDataScopeService dataScopeService,
        IRepository<AppExternalAccountMapping, Guid> externalAccountMappingRepository,
        IRepository<AppRoleAuthorization, Guid> roleAuthorizationRepository,
        IRepository<AppUserProfile, Guid> userProfileRepository,
        IRepository<OrganizationUnit, Guid> organizationUnitRepository,
        IRepository<IdentityRole, Guid> roleRepository,
        IRepository<IdentityUser, Guid> userRepository,
        IRepository<IdentityUserOrganizationUnit> userOrganizationUnitRepository,
        IdentityUserManager userManager,
        ITenantRepository tenantRepository)
    {
        _currentUser = currentUser;
        _asyncQueryableExecuter = asyncQueryableExecuter;
        _dataScopeService = dataScopeService;
        _externalAccountMappingRepository = externalAccountMappingRepository;
        _roleAuthorizationRepository = roleAuthorizationRepository;
        _organizationUnitRepository = organizationUnitRepository;
        _roleRepository = roleRepository;
        _userProfileRepository = userProfileRepository;
        _userRepository = userRepository;
        _userOrganizationUnitRepository = userOrganizationUnitRepository;
        _userManager = userManager;
        _tenantRepository = tenantRepository;
    }

    public async Task<CurrentUserInfoDto> GetCurrentUserInfoAsync(string accessToken)
    {
        if (!_currentUser.Id.HasValue)
        {
            throw new InvalidOperationException("Current user is not available.");
        }

        var user = await _userManager.GetByIdAsync(_currentUser.Id.Value);
        var roles = (await _userManager.GetRolesAsync(user))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();
        var organizationUnitIds = (await _userOrganizationUnitRepository.GetListAsync())
            .Where(x => x.UserId == user.Id)
            .Select(x => x.OrganizationUnitId)
            .ToHashSet();
        var organizationUnitNames = (await _organizationUnitRepository.GetListAsync())
            .Where(x => organizationUnitIds.Contains(x.Id))
            .Select(x => x.DisplayName)
            .Distinct()
            .OrderBy(x => x)
            .ToList();
        var profile = await _userProfileRepository.FindAsync(x => x.UserId == user.Id);

        return new CurrentUserInfoDto
        {
            Desc = BuildUserDescription(roles),
            HomePath = ResolveHomePath(roles),
            LastLoginTime = profile?.LastLoginTime,
            OrganizationUnitNames = organizationUnitNames,
            RealName = string.IsNullOrWhiteSpace($"{user.Name}{user.Surname}".Trim())
                ? user.UserName ?? string.Empty
                : $"{user.Name}{user.Surname}".Trim(),
            Roles = roles,
            Token = accessToken,
            Username = user.UserName ?? string.Empty
        };
    }

    public async Task<List<PhaseOneUserListItemDto>> GetListAsync(PhaseOneUserListInput input)
    {
        var keyword = input.Keyword?.Trim();
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
            return [];
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

        var users = await _asyncQueryableExecuter.ToListAsync(usersQueryable.OrderBy(x => x.UserName));
        if (users.Count == 0)
        {
            return [];
        }

        var userIds = users.Select(x => x.Id).ToList();
        var profilesQueryable = await _userProfileRepository.GetQueryableAsync();
        var profiles = (await _asyncQueryableExecuter.ToListAsync(
                profilesQueryable.Where(x => userIds.Contains(x.UserId))))
            .ToDictionary(x => x.UserId);

        var mappingsQueryable = await _externalAccountMappingRepository.GetQueryableAsync();
        var mappings = (await _asyncQueryableExecuter.ToListAsync(
                mappingsQueryable.Where(x => userIds.Contains(x.UserId))))
            .GroupBy(x => x.UserId)
            .ToDictionary(
                x => x.Key,
                x => x.OrderByDescending(item => item.BoundAt)
                    .Select(item => new PhaseOneExternalAccountMappingDto
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
                x => x.Select(item => organizationUnitNames[item.OrganizationUnitId]).Distinct().OrderBy(name => name).ToList());

        var userRoleMap = await BuildUserRoleMapAsync(users);

        // Build tenant name lookup from distinct TenantIds present in this user set
        var distinctTenantIds = users.Select(x => x.TenantId).Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();
        var tenants = distinctTenantIds.Count > 0
            ? (await _tenantRepository.GetListAsync()).Where(t => distinctTenantIds.Contains(t.Id)).ToDictionary(t => t.Id, t => t.Name)
            : [];

        var result = new List<PhaseOneUserListItemDto>();
        foreach (var user in users)
        {
            var roles = userRoleMap.GetValueOrDefault(user.Id) ?? [];

            if (!string.IsNullOrWhiteSpace(input.RoleName) &&
                !roles.Contains(input.RoleName, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            result.Add(new PhaseOneUserListItemDto
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
                IsOnline = PhaseOneUserActivityService.IsOnline(profiles.GetValueOrDefault(user.Id)?.LastSeenAt),
                LastLoginTime = profiles.GetValueOrDefault(user.Id)?.LastLoginTime,
                OrganizationUnitNames = userOrganizationMap.GetValueOrDefault(user.Id) ?? [],
                PhoneNumber = profiles.GetValueOrDefault(user.Id)?.PhoneNumber,
                Roles = roles,
                TenantName = user.TenantId.HasValue && tenants.TryGetValue(user.TenantId.Value, out var tenantName) ? tenantName : null,
                Username = user.UserName ?? string.Empty
            });
        }

        return result;
    }

    private async Task<Dictionary<Guid, List<string>>> BuildUserRoleMapAsync(List<IdentityUser> users)
    {
        var rolePairs = await Task.WhenAll(users.Select(async user =>
        {
            var roleNames = (await _userManager.GetRolesAsync(user))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name)
                .ToList();

            return new KeyValuePair<Guid, List<string>>(user.Id, roleNames);
        }));

        return rolePairs.ToDictionary(x => x.Key, x => x.Value);
    }

    private async Task<HashSet<Guid>> ResolveVisibleUserIdsAsync(HashSet<Guid> dataScopedUserIds)
    {
        if (_currentUser.IsInRole(PhaseOneRoleNames.Admin))
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

        var roleIds = (await _roleRepository.GetListAsync())
            .Where(x => currentRoleNames.Contains(x.Name, StringComparer.OrdinalIgnoreCase))
            .Select(x => x.Id)
            .ToHashSet();

        if (roleIds.Count == 0)
        {
            return new HashSet<Guid> { _currentUser.Id.Value };
        }

        var authorizations = (await _roleAuthorizationRepository.GetListAsync())
            .Where(x => roleIds.Contains(x.RoleId))
            .ToList();

        if (authorizations.Count == 0)
        {
            return dataScopedUserIds.Count == 0
                ? new HashSet<Guid> { _currentUser.Id.Value }
                : dataScopedUserIds;
        }

        if (authorizations.Any(x => x.AccountScopeMode == PhaseOneAuthorizationDefaults.AccountScopeAll))
        {
            return (await _userRepository.GetListAsync())
                .Select(x => x.Id)
                .ToHashSet();
        }

        var explicitlyAllowedUserIds = authorizations
            .SelectMany(x => PhaseOneSerializationHelper.DeserializeGuidList(x.AllowedUserIds))
            .ToHashSet();
        explicitlyAllowedUserIds.Add(_currentUser.Id.Value);

        if (authorizations.Any(x => x.AccountScopeMode == PhaseOneAuthorizationDefaults.AccountScopeDataAndUsers))
        {
            var merged = dataScopedUserIds.ToHashSet();
            merged.UnionWith(explicitlyAllowedUserIds);
            return merged;
        }

        if (authorizations.Any(x => x.AccountScopeMode == PhaseOneAuthorizationDefaults.AccountScopeOnlyUsers))
        {
            return explicitlyAllowedUserIds;
        }

        return dataScopedUserIds.Count == 0
            ? explicitlyAllowedUserIds
            : dataScopedUserIds;
    }

    public async Task CreateAsync(PhaseOneUserSaveInput input)
    {
        await EnsureOrganizationUnitAccessibleAsync(input.OrganizationUnitId);

        var user = new IdentityUser(Guid.NewGuid(), input.Username.Trim(), input.Email.Trim())
        {
            Name = input.DisplayName.Trim(),
            Surname = string.Empty
        };
        user.SetIsActive(input.IsActive);

        EnsureSucceeded(await _userManager.CreateAsync(user, string.IsNullOrWhiteSpace(input.Password) ? "1q2w3E*" : input.Password));
        await UpsertUserProfileAsync(user.Id, input);
        await SyncUserRolesAsync(user, input.RoleNames);
        await SyncUserOrganizationUnitsAsync(user.Id, input.OrganizationUnitId);
    }

    public async Task UpdateAsync(Guid id, PhaseOneUserSaveInput input)
    {
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
        var profile = await _userProfileRepository.FindAsync(x => x.UserId == id);
        if (profile != null)
        {
            await _userProfileRepository.DeleteAsync(profile, autoSave: true);
        }

        var mappings = (await _externalAccountMappingRepository.GetListAsync())
            .Where(x => x.UserId == id)
            .ToList();
        if (mappings.Count > 0)
        {
            await _externalAccountMappingRepository.DeleteManyAsync(mappings, autoSave: true);
        }

        EnsureSucceeded(await _userManager.DeleteAsync(user));
    }

    public async Task ChangePasswordAsync(PhaseOnePasswordChangeInput input)
    {
        if (!_currentUser.Id.HasValue)
        {
            throw new InvalidOperationException("Current user is not available.");
        }

        var user = await _userManager.GetByIdAsync(_currentUser.Id.Value);
        EnsureSucceeded(await _userManager.ChangePasswordAsync(user, input.CurrentPassword, input.NewPassword));
    }

    public async Task ResetPasswordAsync(Guid id, PhaseOneAdminResetPasswordInput input)
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
        if (roles.Contains(PhaseOneRoleNames.Admin, StringComparer.OrdinalIgnoreCase))
        {
            return "/platform/workspace";
        }

        if (roles.Contains(PhaseOneRoleNames.Pmo, StringComparer.OrdinalIgnoreCase))
        {
            return "/project/pmo-overview";
        }

        if (roles.Contains(PhaseOneRoleNames.Pm, StringComparer.OrdinalIgnoreCase))
        {
            return "/project/pm-dashboard";
        }

        if (roles.Contains(PhaseOneRoleNames.Member, StringComparer.OrdinalIgnoreCase))
        {
            return "/project/my-related";
        }

        return "/project/projects";
    }

    private static string BuildUserDescription(IReadOnlyCollection<string> roles)
    {
        if (roles.Contains(PhaseOneRoleNames.Admin, StringComparer.OrdinalIgnoreCase))
        {
            return "系统管理员";
        }

        if (roles.Contains(PhaseOneRoleNames.Pmo, StringComparer.OrdinalIgnoreCase))
        {
            return "PMO 管理角色";
        }

        if (roles.Contains(PhaseOneRoleNames.Pm, StringComparer.OrdinalIgnoreCase))
        {
            return "项目经理角色";
        }

        if (roles.Contains(PhaseOneRoleNames.Member, StringComparer.OrdinalIgnoreCase))
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

    private async Task SyncUserOrganizationUnitsAsync(Guid userId, Guid? organizationUnitId)
    {
        var existingOrganizationUnits = (await _userOrganizationUnitRepository.GetListAsync())
            .Where(x => x.UserId == userId)
            .Select(x => x.OrganizationUnitId)
            .Distinct()
            .ToList();

        foreach (var existingOrganizationUnitId in existingOrganizationUnits)
        {
            await _userManager.RemoveFromOrganizationUnitAsync(userId, existingOrganizationUnitId);
        }

        if (organizationUnitId.HasValue)
        {
            await _userManager.AddToOrganizationUnitAsync(userId, organizationUnitId.Value);
        }
    }

    private async Task SyncUserRolesAsync(IdentityUser user, IReadOnlyCollection<string> roleNames)
    {
        var requestedRoles = NormalizeRoleNames(roleNames);
        var existingRoles = (await _roleRepository.GetListAsync())
            .Where(x => requestedRoles.Contains(x.Name, StringComparer.OrdinalIgnoreCase))
            .Select(x => x.Name)
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
    }

    private async Task UpsertUserProfileAsync(Guid userId, PhaseOneUserSaveInput input)
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

    private static void EnsureSucceeded(Microsoft.AspNetCore.Identity.IdentityResult result)
    {
        if (result.Succeeded)
        {
            return;
        }

        throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
    }
}
