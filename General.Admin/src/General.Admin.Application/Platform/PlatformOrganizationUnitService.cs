using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace General.Admin.Platform;

public class PlatformOrganizationUnitService : ITransientDependency
{
    private static readonly DistributedCacheEntryOptions OrganizationCacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
    };

    private readonly CurrentUserDataScopeService _dataScopeService;
    private readonly ICurrentUser _currentUser;
    private readonly IDistributedCache _distributedCache;
    private readonly IdentityUserManager _identityUserManager;
    private readonly OrganizationUnitManager _organizationUnitManager;
    private readonly PlatformCacheService _platformCacheService;
    private readonly IRepository<OrganizationUnit, Guid> _organizationUnitRepository;
    private readonly IRepository<IdentityUserOrganizationUnit> _userOrganizationUnitRepository;

    public PlatformOrganizationUnitService(
        CurrentUserDataScopeService dataScopeService,
        ICurrentUser currentUser,
        IDistributedCache distributedCache,
        IdentityUserManager identityUserManager,
        OrganizationUnitManager organizationUnitManager,
        PlatformCacheService platformCacheService,
        IRepository<OrganizationUnit, Guid> organizationUnitRepository,
        IRepository<IdentityUserOrganizationUnit> userOrganizationUnitRepository)
    {
        _dataScopeService = dataScopeService;
        _currentUser = currentUser;
        _distributedCache = distributedCache;
        _identityUserManager = identityUserManager;
        _organizationUnitManager = organizationUnitManager;
        _platformCacheService = platformCacheService;
        _organizationUnitRepository = organizationUnitRepository;
        _userOrganizationUnitRepository = userOrganizationUnitRepository;
    }

    public async Task<List<OrganizationUnitTreeDto>> GetTreeAsync()
    {
        var cacheKey = await _platformCacheService.BuildVersionedKeyAsync("organization", $"tree:{BuildCurrentUserCachePart()}");
        var cached = await _distributedCache.GetStringAsync(cacheKey);
        if (!string.IsNullOrWhiteSpace(cached))
        {
            return JsonSerializer.Deserialize<List<OrganizationUnitTreeDto>>(cached) ?? [];
        }

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

        var result = BuildTree(visibleUnits, memberCountMap, accessibleIds, null);
        await _distributedCache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), OrganizationCacheOptions);
        return result;
    }

    public async Task CreateAsync(OrganizationUnitSaveInput input)
    {
        await EnsureParentAccessibleAsync(input.ParentId);
        var organizationUnit = new OrganizationUnit(Guid.NewGuid(), input.DisplayName.Trim(), input.ParentId);
        await _organizationUnitManager.CreateAsync(organizationUnit);
        await _platformCacheService.InvalidateAsync("organization");
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
        await _platformCacheService.InvalidateAsync("organization");
    }

    public async Task MoveAsync(Guid id, Guid? parentId)
    {
        await EnsureAccessibleAsync(id);
        await EnsureParentAccessibleAsync(parentId);

        await _organizationUnitManager.MoveAsync(id, parentId);
        await _platformCacheService.InvalidateAsync("organization");
    }

    public async Task DeleteAsync(Guid id)
    {
        await EnsureAccessibleAsync(id);
        await _organizationUnitManager.DeleteAsync(id);
        await _platformCacheService.InvalidateAsync("organization");
    }

    public async Task TransferMembersAsync(Guid sourceOrganizationUnitId, OrganizationUnitMemberTransferInput input)
    {
        await EnsureAccessibleAsync(sourceOrganizationUnitId);
        await EnsureAccessibleAsync(input.TargetOrganizationUnitId);

        if (sourceOrganizationUnitId == input.TargetOrganizationUnitId)
        {
            throw new InvalidOperationException("目标部门不能与当前部门相同。");
        }

        var requestedUserIds = input.UserIds
            .Distinct()
            .ToList();
        if (requestedUserIds.Count == 0)
        {
            throw new InvalidOperationException("至少选择一名成员。");
        }

        var organizationUnits = (await _organizationUnitRepository.GetListAsync())
            .OrderBy(x => x.Code)
            .ToList();
        var sourceOrganizationUnit = organizationUnits.FirstOrDefault(x => x.Id == sourceOrganizationUnitId)
            ?? throw new InvalidOperationException("当前组织节点不存在。");
        var sourceScopeOrganizationUnitIds = organizationUnits
            .Where(x => x.Code.StartsWith(sourceOrganizationUnit.Code, StringComparison.Ordinal))
            .Select(x => x.Id)
            .ToHashSet();

        var memberships = (await _userOrganizationUnitRepository.GetListAsync())
            .Where(x =>
                sourceScopeOrganizationUnitIds.Contains(x.OrganizationUnitId) &&
                requestedUserIds.Contains(x.UserId))
            .ToList();

        if (memberships
                .Select(x => x.UserId)
                .Distinct()
                .Count() != requestedUserIds.Count)
        {
            throw new InvalidOperationException("部分成员不在当前组织节点范围内，请刷新后重试。");
        }

        var targetMembershipUserIds = (await _userOrganizationUnitRepository.GetListAsync())
            .Where(x =>
                x.OrganizationUnitId == input.TargetOrganizationUnitId &&
                requestedUserIds.Contains(x.UserId))
            .Select(x => x.UserId)
            .ToHashSet();

        foreach (var userId in requestedUserIds)
        {
            var sourceMembershipIds = memberships
                .Where(x => x.UserId == userId && x.OrganizationUnitId != input.TargetOrganizationUnitId)
                .Select(x => x.OrganizationUnitId)
                .Distinct()
                .ToList();

            foreach (var sourceMembershipId in sourceMembershipIds)
            {
                await _identityUserManager.RemoveFromOrganizationUnitAsync(userId, sourceMembershipId);
            }

            if (!targetMembershipUserIds.Contains(userId))
            {
                await _identityUserManager.AddToOrganizationUnitAsync(userId, input.TargetOrganizationUnitId);
            }
        }
        await _platformCacheService.InvalidateAsync("organization");
    }

    private string BuildCurrentUserCachePart()
    {
        var userId = _currentUser.Id?.ToString("N") ?? "anonymous";
        var roles = _currentUser.Roles == null
            ? "no-role"
            : string.Join('-', _currentUser.Roles.OrderBy(x => x, StringComparer.OrdinalIgnoreCase));
        return $"{userId}:{roles}";
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
            .Select(x =>
            {
                var children = BuildTree(organizationUnits, memberCountMap, accessibleIds, x.Id);
                var currentCount = memberCountMap.TryGetValue(x.Id, out var count) ? count : 0;
                return new OrganizationUnitTreeDto
                {
                    Id = x.Id,
                    ParentId = x.ParentId,
                    Code = x.Code,
                    Disabled = !accessibleIds.Contains(x.Id),
                    DirectMemberCount = currentCount,
                    DisplayName = x.DisplayName,
                    MemberCount = currentCount + children.Sum(child => child.MemberCount),
                    Children = children
                };
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
