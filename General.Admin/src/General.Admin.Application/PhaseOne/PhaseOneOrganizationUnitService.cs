using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace General.Admin.PhaseOne;

public class PhaseOneOrganizationUnitService : ITransientDependency
{
    private readonly CurrentUserDataScopeService _dataScopeService;
    private readonly OrganizationUnitManager _organizationUnitManager;
    private readonly IRepository<OrganizationUnit, Guid> _organizationUnitRepository;
    private readonly IRepository<IdentityUserOrganizationUnit> _userOrganizationUnitRepository;

    public PhaseOneOrganizationUnitService(
        CurrentUserDataScopeService dataScopeService,
        OrganizationUnitManager organizationUnitManager,
        IRepository<OrganizationUnit, Guid> organizationUnitRepository,
        IRepository<IdentityUserOrganizationUnit> userOrganizationUnitRepository)
    {
        _dataScopeService = dataScopeService;
        _organizationUnitManager = organizationUnitManager;
        _organizationUnitRepository = organizationUnitRepository;
        _userOrganizationUnitRepository = userOrganizationUnitRepository;
    }

    public async Task<List<OrganizationUnitTreeDto>> GetTreeAsync()
    {
        var accessibleIds = await _dataScopeService.GetAccessibleOrganizationUnitIdsAsync();
        var visibleIds = await _dataScopeService.GetVisibleOrganizationUnitIdsAsync();

        var organizationUnits = (await _organizationUnitRepository.GetListAsync())
            .OrderBy(x => x.Code)
            .ToList();

        var members = (await _userOrganizationUnitRepository.GetListAsync())
            .GroupBy(x => x.OrganizationUnitId)
            .Select(x => new
            {
                OrganizationUnitId = x.Key,
                Count = x.Count()
            })
            .ToList();

        var memberCountMap = members.ToDictionary(x => x.OrganizationUnitId, x => x.Count);
        var visibleUnits = organizationUnits
            .Where(x => visibleIds.Contains(x.Id))
            .ToList();

        return BuildTree(visibleUnits, memberCountMap, accessibleIds, null);
    }

    public async Task CreateAsync(OrganizationUnitSaveInput input)
    {
        await EnsureParentAccessibleAsync(input.ParentId);
        var organizationUnit = new OrganizationUnit(Guid.NewGuid(), input.DisplayName.Trim(), input.ParentId);
        await _organizationUnitManager.CreateAsync(organizationUnit);
    }

    public async Task UpdateAsync(Guid id, OrganizationUnitSaveInput input)
    {
        await EnsureAccessibleAsync(id);
        await EnsureParentAccessibleAsync(input.ParentId);

        var organizationUnit = await _organizationUnitRepository.GetAsync(id);
        organizationUnit.DisplayName = input.DisplayName.Trim();
        await _organizationUnitManager.UpdateAsync(organizationUnit);

        if (organizationUnit.ParentId != input.ParentId)
        {
            await _organizationUnitManager.MoveAsync(id, input.ParentId);
        }
    }

    public async Task MoveAsync(Guid id, Guid? parentId)
    {
        await EnsureAccessibleAsync(id);
        await EnsureParentAccessibleAsync(parentId);

        await _organizationUnitManager.MoveAsync(id, parentId);
    }

    public async Task DeleteAsync(Guid id)
    {
        await EnsureAccessibleAsync(id);
        await _organizationUnitManager.DeleteAsync(id);
    }

    private static List<OrganizationUnitTreeDto> BuildTree(
        IReadOnlyCollection<OrganizationUnit> organizationUnits,
        IReadOnlyDictionary<Guid, int> memberCountMap,
        IReadOnlySet<Guid> accessibleIds,
        Guid? parentId)
    {
        return organizationUnits
            .Where(x => x.ParentId == parentId)
            .OrderBy(x => x.Code)
            .Select(x => new OrganizationUnitTreeDto
            {
                Id = x.Id,
                ParentId = x.ParentId,
                Code = x.Code,
                Disabled = !accessibleIds.Contains(x.Id),
                DisplayName = x.DisplayName,
                MemberCount = memberCountMap.TryGetValue(x.Id, out var count) ? count : 0,
                Children = BuildTree(organizationUnits, memberCountMap, accessibleIds, x.Id)
            })
            .ToList();
    }

    private async Task EnsureAccessibleAsync(Guid id)
    {
        var accessibleIds = await _dataScopeService.GetAccessibleOrganizationUnitIdsAsync();
        if (!accessibleIds.Contains(id))
        {
            throw new InvalidOperationException("当前组织节点不在可操作范围内。");
        }
    }

    private async Task EnsureParentAccessibleAsync(Guid? parentId)
    {
        if (!parentId.HasValue)
        {
            return;
        }

        await EnsureAccessibleAsync(parentId.Value);
    }
}
