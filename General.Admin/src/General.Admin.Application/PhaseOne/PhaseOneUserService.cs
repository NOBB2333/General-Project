using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace General.Admin.PhaseOne;

public class PhaseOneUserService : ITransientDependency
{
    private readonly ICurrentUser _currentUser;
    private readonly CurrentUserDataScopeService _dataScopeService;
    private readonly IRepository<OrganizationUnit, Guid> _organizationUnitRepository;
    private readonly IRepository<IdentityRole, Guid> _roleRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;
    private readonly IRepository<IdentityUserOrganizationUnit> _userOrganizationUnitRepository;
    private readonly IdentityUserManager _userManager;

    public PhaseOneUserService(
        ICurrentUser currentUser,
        CurrentUserDataScopeService dataScopeService,
        IRepository<OrganizationUnit, Guid> organizationUnitRepository,
        IRepository<IdentityRole, Guid> roleRepository,
        IRepository<IdentityUser, Guid> userRepository,
        IRepository<IdentityUserOrganizationUnit> userOrganizationUnitRepository,
        IdentityUserManager userManager)
    {
        _currentUser = currentUser;
        _dataScopeService = dataScopeService;
        _organizationUnitRepository = organizationUnitRepository;
        _roleRepository = roleRepository;
        _userRepository = userRepository;
        _userOrganizationUnitRepository = userOrganizationUnitRepository;
        _userManager = userManager;
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

        return new CurrentUserInfoDto
        {
            Desc = BuildUserDescription(roles),
            HomePath = ResolveHomePath(roles),
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
        var organizationUnits = (await _organizationUnitRepository.GetListAsync())
            .OrderBy(x => x.Code)
            .ToList();
        var accessibleIds = await _dataScopeService.GetAccessibleOrganizationUnitIdsAsync();
        var filteredIds = FilterOrganizationUnitIds(input.OrganizationUnitId, organizationUnits, accessibleIds);

        var userOrganizationUnits = (await _userOrganizationUnitRepository.GetListAsync())
            .Where(x => filteredIds.Contains(x.OrganizationUnitId))
            .ToList();

        var userIds = userOrganizationUnits
            .Select(x => x.UserId)
            .Distinct()
            .ToHashSet();

        var users = (await _userRepository.GetListAsync())
            .Where(x => userIds.Contains(x.Id))
            .Where(x => string.IsNullOrWhiteSpace(input.Keyword)
                || (x.UserName?.Contains(input.Keyword, StringComparison.OrdinalIgnoreCase) ?? false)
                || (x.Name?.Contains(input.Keyword, StringComparison.OrdinalIgnoreCase) ?? false)
                || (x.Email?.Contains(input.Keyword, StringComparison.OrdinalIgnoreCase) ?? false))
            .OrderBy(x => x.UserName)
            .ToList();

        var organizationUnitNames = organizationUnits.ToDictionary(x => x.Id, x => x.DisplayName);
        var userOrganizationMap = userOrganizationUnits
            .GroupBy(x => x.UserId)
            .ToDictionary(
                x => x.Key,
                x => x.Select(item => organizationUnitNames[item.OrganizationUnitId]).Distinct().OrderBy(name => name).ToList());

        var result = new List<PhaseOneUserListItemDto>();
        foreach (var user in users)
        {
            var roles = (await _userManager.GetRolesAsync(user))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();

            result.Add(new PhaseOneUserListItemDto
            {
                DisplayName = string.IsNullOrWhiteSpace($"{user.Name}{user.Surname}".Trim())
                    ? user.UserName ?? string.Empty
                    : $"{user.Name}{user.Surname}".Trim(),
                Email = user.Email ?? string.Empty,
                Id = user.Id,
                IsActive = user.IsActive,
                OrganizationUnitNames = userOrganizationMap.GetValueOrDefault(user.Id) ?? [],
                Roles = roles,
                Username = user.UserName ?? string.Empty
            });
        }

        return result;
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
        EnsureSucceeded(await _userManager.DeleteAsync(user));
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
