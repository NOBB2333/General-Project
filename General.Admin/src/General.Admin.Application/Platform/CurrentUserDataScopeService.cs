using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using Volo.Abp.Identity;
using Volo.Abp.Linq;

namespace General.Admin.Platform;

public class CurrentUserDataScopeService : ITransientDependency
{
    private readonly IAsyncQueryableExecuter _asyncQueryableExecuter;
    private readonly ICurrentUser _currentUser;
    private readonly IRepository<OrganizationUnit, Guid> _organizationUnitRepository;
    private readonly IRepository<AppRoleAuthorization, Guid> _roleAuthorizationRepository;
    private readonly IRepository<IdentityRole, Guid> _roleRepository;
    private readonly IRepository<IdentityUserOrganizationUnit> _userOrganizationUnitRepository;

    public CurrentUserDataScopeService(
        IAsyncQueryableExecuter asyncQueryableExecuter,
        ICurrentUser currentUser,
        IRepository<OrganizationUnit, Guid> organizationUnitRepository,
        IRepository<AppRoleAuthorization, Guid> roleAuthorizationRepository,
        IRepository<IdentityRole, Guid> roleRepository,
        IRepository<IdentityUserOrganizationUnit> userOrganizationUnitRepository)
    {
        _asyncQueryableExecuter = asyncQueryableExecuter;
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

        if (_currentUser.IsInRole(PlatformRoleNames.Admin))
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
        if (_currentUser.IsInRole(PlatformRoleNames.Admin))
        {
            return organizationUnits.Select(x => x.Id).ToHashSet();
        }

        var roleMode = await GetCurrentRoleDataScopeModeAsync();
        if (roleMode.Mode == PlatformAuthorizationDefaults.DataScopeAll)
        {
            return organizationUnits.Select(x => x.Id).ToHashSet();
        }

        if (!_currentUser.Id.HasValue)
        {
            return [];
        }

        var userOrganizationUnitQueryable = await _userOrganizationUnitRepository.GetQueryableAsync();
        var currentUserOrganizationUnits = await _asyncQueryableExecuter.ToListAsync(
            userOrganizationUnitQueryable
                .Where(x => x.UserId == _currentUser.Id.Value)
                .Select(x => x.OrganizationUnitId));

        if (currentUserOrganizationUnits.Count == 0)
        {
            return roleMode.Mode == PlatformAuthorizationDefaults.DataScopeCustom
                ? roleMode.CustomOrganizationUnitIds.ToHashSet()
                : [];
        }

        var ownUnits = organizationUnits
            .Where(x => currentUserOrganizationUnits.Contains(x.Id))
            .ToList();

        var descendantIds = roleMode.Mode switch
        {
            PlatformAuthorizationDefaults.DataScopeCurrentOrganization => organizationUnits
                .Where(candidate => ownUnits.Any(unit => candidate.Id == unit.Id))
                .Select(x => x.Id)
                .ToHashSet(),
            PlatformAuthorizationDefaults.DataScopeSelf => ownUnits.Select(x => x.Id).ToHashSet(),
            PlatformAuthorizationDefaults.DataScopeCustom => roleMode.CustomOrganizationUnitIds.ToHashSet(),
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
            return ([], PlatformAuthorizationDefaults.DataScopeCurrentOrganizationAndDescendants);
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
            return ([], PlatformAuthorizationDefaults.DataScopeCurrentOrganizationAndDescendants);
        }

        var authorizationQueryable = await _roleAuthorizationRepository.GetQueryableAsync();
        var authorizations = await _asyncQueryableExecuter.ToListAsync(
            authorizationQueryable.Where(x => roleIds.Contains(x.RoleId)));

        if (authorizations.Count == 0)
        {
            return ([], PlatformAuthorizationDefaults.DataScopeCurrentOrganizationAndDescendants);
        }

        if (authorizations.Any(x => x.DataScopeMode == PlatformAuthorizationDefaults.DataScopeAll))
        {
            return ([], PlatformAuthorizationDefaults.DataScopeAll);
        }

        var first = authorizations
            .OrderBy(x => ResolvePriority(x.DataScopeMode))
            .First();

        return (
            PlatformSerializationHelper.DeserializeGuidList(first.CustomOrganizationUnitIds),
            first.DataScopeMode);
    }

    private static int ResolvePriority(string mode)
    {
        return mode switch
        {
            PlatformAuthorizationDefaults.DataScopeCurrentOrganizationAndDescendants => 1,
            PlatformAuthorizationDefaults.DataScopeCurrentOrganization => 2,
            PlatformAuthorizationDefaults.DataScopeSelf => 3,
            PlatformAuthorizationDefaults.DataScopeCustom => 4,
            _ => 10
        };
    }
}
