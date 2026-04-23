using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using Volo.Abp.Identity;

namespace General.Admin.PhaseOne;

public class CurrentUserDataScopeService : ITransientDependency
{
    private readonly ICurrentUser _currentUser;
    private readonly IRepository<OrganizationUnit, Guid> _organizationUnitRepository;
    private readonly IRepository<AppRoleAuthorization, Guid> _roleAuthorizationRepository;
    private readonly IRepository<IdentityRole, Guid> _roleRepository;
    private readonly IRepository<IdentityUserOrganizationUnit> _userOrganizationUnitRepository;

    public CurrentUserDataScopeService(
        ICurrentUser currentUser,
        IRepository<OrganizationUnit, Guid> organizationUnitRepository,
        IRepository<AppRoleAuthorization, Guid> roleAuthorizationRepository,
        IRepository<IdentityRole, Guid> roleRepository,
        IRepository<IdentityUserOrganizationUnit> userOrganizationUnitRepository)
    {
        _currentUser = currentUser;
        _organizationUnitRepository = organizationUnitRepository;
        _roleAuthorizationRepository = roleAuthorizationRepository;
        _roleRepository = roleRepository;
        _userOrganizationUnitRepository = userOrganizationUnitRepository;
    }

    public async Task<HashSet<Guid>> GetAccessibleOrganizationUnitIdsAsync()
    {
        var organizationUnits = (await _organizationUnitRepository.GetListAsync())
            .OrderBy(x => x.Code)
            .ToList();

        return await GetAccessibleOrganizationUnitIdsAsync(organizationUnits);
    }

    public async Task<HashSet<Guid>> GetVisibleOrganizationUnitIdsAsync()
    {
        // 加载一次组织树，复用给两个内部方法，避免重复查询。
        var organizationUnits = (await _organizationUnitRepository.GetListAsync())
            .OrderBy(x => x.Code)
            .ToList();

        if (_currentUser.IsInRole(PhaseOneRoleNames.Admin))
        {
            return organizationUnits.Select(x => x.Id).ToHashSet();
        }

        var accessibleIds = await GetAccessibleOrganizationUnitIdsAsync(organizationUnits);
        var allById = organizationUnits.ToDictionary(x => x.Id);
        var visibleIds = new HashSet<Guid>(accessibleIds);
        foreach (var unitId in accessibleIds.ToList())
        {
            var currentId = unitId;
            while (allById.TryGetValue(currentId, out var unit) && unit.ParentId.HasValue)
            {
                currentId = unit.ParentId.Value;
                if (!visibleIds.Add(currentId))
                {
                    break;
                }
            }
        }

        return visibleIds;
    }

    // 内部重载：接受已加载的组织树，避免在同一请求中重复查库。
    private async Task<HashSet<Guid>> GetAccessibleOrganizationUnitIdsAsync(List<OrganizationUnit> organizationUnits)
    {
        if (_currentUser.IsInRole(PhaseOneRoleNames.Admin))
        {
            return organizationUnits.Select(x => x.Id).ToHashSet();
        }

        var roleMode = await GetCurrentRoleDataScopeModeAsync();
        if (roleMode.Mode == PhaseOneAuthorizationDefaults.DataScopeAll)
        {
            return organizationUnits.Select(x => x.Id).ToHashSet();
        }

        if (!_currentUser.Id.HasValue)
        {
            return [];
        }

        var currentUserOrganizationUnits = (await _userOrganizationUnitRepository.GetListAsync())
            .Where(x => x.UserId == _currentUser.Id.Value)
            .Select(x => x.OrganizationUnitId)
            .ToList();

        if (currentUserOrganizationUnits.Count == 0)
        {
            return roleMode.Mode == PhaseOneAuthorizationDefaults.DataScopeCustom
                ? roleMode.CustomOrganizationUnitIds.ToHashSet()
                : [];
        }

        var ownUnits = organizationUnits
            .Where(x => currentUserOrganizationUnits.Contains(x.Id))
            .ToList();

        var descendantIds = roleMode.Mode switch
        {
            PhaseOneAuthorizationDefaults.DataScopeCurrentOrganization => organizationUnits
                .Where(candidate => ownUnits.Any(unit => candidate.Id == unit.Id))
                .Select(x => x.Id)
                .ToHashSet(),
            PhaseOneAuthorizationDefaults.DataScopeSelf => ownUnits.Select(x => x.Id).ToHashSet(),
            PhaseOneAuthorizationDefaults.DataScopeCustom => roleMode.CustomOrganizationUnitIds.ToHashSet(),
            _ => organizationUnits
                .Where(candidate => ownUnits.Any(unit => candidate.Code.StartsWith(unit.Code, StringComparison.Ordinal)))
                .Select(x => x.Id)
                .ToHashSet()
        };

        return descendantIds;
    }

    private async Task<(List<Guid> CustomOrganizationUnitIds, string Mode)> GetCurrentRoleDataScopeModeAsync()
    {
        var currentRoleNames = _currentUser.Roles?.Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? [];
        if (currentRoleNames.Count == 0)
        {
            return ([], PhaseOneAuthorizationDefaults.DataScopeCurrentOrganizationAndDescendants);
        }

        var roleIds = (await _roleRepository.GetListAsync())
            .Where(x => currentRoleNames.Contains(x.Name, StringComparer.OrdinalIgnoreCase))
            .Select(x => x.Id)
            .ToHashSet();

        var authorizations = (await _roleAuthorizationRepository.GetListAsync())
            .Where(x => roleIds.Contains(x.RoleId))
            .ToList();

        if (authorizations.Count == 0)
        {
            return ([], PhaseOneAuthorizationDefaults.DataScopeCurrentOrganizationAndDescendants);
        }

        if (authorizations.Any(x => x.DataScopeMode == PhaseOneAuthorizationDefaults.DataScopeAll))
        {
            return ([], PhaseOneAuthorizationDefaults.DataScopeAll);
        }

        var first = authorizations
            .OrderBy(x => ResolvePriority(x.DataScopeMode))
            .First();

        return (
            PhaseOneSerializationHelper.DeserializeGuidList(first.CustomOrganizationUnitIds),
            first.DataScopeMode);
    }

    private static int ResolvePriority(string mode)
    {
        return mode switch
        {
            PhaseOneAuthorizationDefaults.DataScopeCurrentOrganizationAndDescendants => 1,
            PhaseOneAuthorizationDefaults.DataScopeCurrentOrganization => 2,
            PhaseOneAuthorizationDefaults.DataScopeSelf => 3,
            PhaseOneAuthorizationDefaults.DataScopeCustom => 4,
            _ => 10
        };
    }
}
