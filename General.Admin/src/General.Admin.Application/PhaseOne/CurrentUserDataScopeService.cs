using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using Volo.Abp.Identity;

namespace General.Admin.PhaseOne;

public class CurrentUserDataScopeService : ITransientDependency
{
    private readonly ICurrentUser _currentUser;
    private readonly IRepository<OrganizationUnit, Guid> _organizationUnitRepository;
    private readonly IRepository<IdentityUserOrganizationUnit> _userOrganizationUnitRepository;

    public CurrentUserDataScopeService(
        ICurrentUser currentUser,
        IRepository<OrganizationUnit, Guid> organizationUnitRepository,
        IRepository<IdentityUserOrganizationUnit> userOrganizationUnitRepository)
    {
        _currentUser = currentUser;
        _organizationUnitRepository = organizationUnitRepository;
        _userOrganizationUnitRepository = userOrganizationUnitRepository;
    }

    public async Task<HashSet<Guid>> GetAccessibleOrganizationUnitIdsAsync()
    {
        var organizationUnits = (await _organizationUnitRepository.GetListAsync())
            .OrderBy(x => x.Code)
            .ToList();

        if (_currentUser.IsInRole(PhaseOneRoleNames.Admin))
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
            return [];
        }

        var ownUnits = organizationUnits
            .Where(x => currentUserOrganizationUnits.Contains(x.Id))
            .ToList();

        var descendantIds = organizationUnits
            .Where(candidate => ownUnits.Any(unit => candidate.Code.StartsWith(unit.Code, StringComparison.Ordinal)))
            .Select(x => x.Id)
            .ToHashSet();

        return descendantIds;
    }

    public async Task<HashSet<Guid>> GetVisibleOrganizationUnitIdsAsync()
    {
        var organizationUnits = (await _organizationUnitRepository.GetListAsync())
            .OrderBy(x => x.Code)
            .ToList();

        if (_currentUser.IsInRole(PhaseOneRoleNames.Admin))
        {
            return organizationUnits.Select(x => x.Id).ToHashSet();
        }

        var accessibleIds = await GetAccessibleOrganizationUnitIdsAsync();
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
}
