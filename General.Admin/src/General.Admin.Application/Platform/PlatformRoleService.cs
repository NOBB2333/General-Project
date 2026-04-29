using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace General.Admin.Platform;

public class PlatformRoleService : ITransientDependency
{
    private static readonly DistributedCacheEntryOptions RoleCacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
    };

    private static readonly IReadOnlyList<string> BuiltInRoleNames =
    [
        PlatformRoleNames.Admin,
        PlatformRoleNames.Pmo,
        PlatformRoleNames.Pm,
        PlatformRoleNames.Member,
        PlatformRoleNames.Viewer,
    ];

    private readonly IRepository<AppRoleAuthorization, Guid> _roleAuthorizationRepository;
    private readonly IDistributedCache _distributedCache;
    private readonly PlatformCacheService _platformCacheService;
    private readonly IdentityUserManager _userManager;
    private readonly IdentityRoleManager _roleManager;
    private readonly IRepository<AppRoleMenu, Guid> _roleMenuRepository;
    private readonly IRepository<IdentityRole, Guid> _roleRepository;

    public PlatformRoleService(
        IdentityUserManager userManager,
        IdentityRoleManager roleManager,
        IDistributedCache distributedCache,
        PlatformCacheService platformCacheService,
        IRepository<AppRoleAuthorization, Guid> roleAuthorizationRepository,
        IRepository<AppRoleMenu, Guid> roleMenuRepository,
        IRepository<IdentityRole, Guid> roleRepository)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _distributedCache = distributedCache;
        _platformCacheService = platformCacheService;
        _roleAuthorizationRepository = roleAuthorizationRepository;
        _roleMenuRepository = roleMenuRepository;
        _roleRepository = roleRepository;
    }

    public async Task<List<PlatformRoleDto>> GetListAsync()
    {
        var roles = (await _roleRepository.GetListAsync())
            .OrderBy(x => GetOrder(x.Name))
            .ThenBy(x => x.Name)
            .ToList();

        var roleMenus = await _roleMenuRepository.GetListAsync();
        var roleAuthorizations = await _roleAuthorizationRepository.GetListAsync();
        var result = new List<PlatformRoleDto>();

        foreach (var role in roles)
        {
            var users = await _userManager.GetUsersInRoleAsync(role.Name);
            var authorization = roleAuthorizations.FirstOrDefault(x => x.RoleId == role.Id);
            var effectiveMenuIds = roleMenus
                .Where(x => x.RoleId == role.Id)
                .Select(x => x.MenuId)
                .Distinct()
                .ToList();

            if (effectiveMenuIds.Count == 0 && PlatformRoleNames.All.Contains(role.Name))
            {
                effectiveMenuIds = GetDefaultMenuIds(role.Name, roleMenus)
                    .Distinct()
                    .ToList();
            }

            result.Add(new PlatformRoleDto
            {
                ApiBlacklist = authorization == null
                    ? []
                    : PlatformSerializationHelper.DeserializeStringList(authorization.ApiBlacklist),
                AccountScopeMode = authorization?.AccountScopeMode ?? PlatformAuthorizationDefaults.AccountScopeDataAndUsers,
                AccountUserIds = authorization == null
                    ? []
                    : PlatformSerializationHelper.DeserializeGuidList(authorization.AllowedUserIds),
                CustomOrganizationUnitIds = authorization == null
                    ? []
                    : PlatformSerializationHelper.DeserializeGuidList(authorization.CustomOrganizationUnitIds),
                DataScopeMode = authorization?.DataScopeMode ?? PlatformAuthorizationDefaults.DataScopeCurrentOrganizationAndDescendants,
                Description = GetDescription(role.Name),
                HomePath = PlatformUserService.ResolveHomePath([role.Name]),
                Id = role.Id,
                MenuAuthorizationCount = effectiveMenuIds.Count,
                MenuCount = effectiveMenuIds.Count,
                Name = role.Name,
                Status = true,
                UserCount = users.Count
            });
        }

        return result;
    }

    public async Task CreateAsync(PlatformRoleSaveInput input)
    {
        var roleName = NormalizeRoleName(input.Name);
        if (await _roleManager.FindByNameAsync(roleName) != null)
        {
            throw new InvalidOperationException($"角色 {roleName} 已存在。");
        }

        var role = new IdentityRole(Guid.NewGuid(), roleName);
        EnsureSucceeded(await _roleManager.CreateAsync(role));
        await _platformCacheService.InvalidateAsync("role");
    }

    public async Task DeleteAsync(Guid id)
    {
        var role = await _roleManager.GetByIdAsync(id);
        if (BuiltInRoleNames.Contains(role.Name, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("内置角色不允许删除。");
        }

        var users = await _userManager.GetUsersInRoleAsync(role.Name);
        foreach (var user in users)
        {
            EnsureSucceeded(await _userManager.RemoveFromRoleAsync(user, role.Name));
        }

        var mappings = (await _roleMenuRepository.GetListAsync())
            .Where(x => x.RoleId == id)
            .ToList();
        if (mappings.Count > 0)
        {
            await _roleMenuRepository.DeleteManyAsync(mappings, autoSave: true);
        }

        var authorization = await _roleAuthorizationRepository.FindAsync(x => x.RoleId == id);
        if (authorization != null)
        {
            await _roleAuthorizationRepository.DeleteAsync(authorization, autoSave: true);
        }

        EnsureSucceeded(await _roleManager.DeleteAsync(role));
        await _platformCacheService.InvalidateAsync("role");
        await _platformCacheService.InvalidateAsync("menu");
    }

    public async Task<PlatformRoleAuthorizationDto> GetAuthorizationAsync(Guid roleId)
    {
        var cacheKey = await _platformCacheService.BuildVersionedKeyAsync("role", $"authorization:{roleId:N}");
        var cached = await _distributedCache.GetStringAsync(cacheKey);
        if (!string.IsNullOrWhiteSpace(cached))
        {
            return JsonSerializer.Deserialize<PlatformRoleAuthorizationDto>(cached) ?? new PlatformRoleAuthorizationDto();
        }

        var role = await _roleManager.GetByIdAsync(roleId);
        var authorization = await _roleAuthorizationRepository.FindAsync(x => x.RoleId == role.Id);
        var allRoleMenus = await _roleMenuRepository.GetListAsync();
        var menuIds = allRoleMenus
            .Where(x => x.RoleId == roleId)
            .Select(x => x.MenuId)
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        if (menuIds.Count == 0 && PlatformRoleNames.All.Contains(role.Name))
        {
            menuIds = GetDefaultMenuIds(role.Name, allRoleMenus)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }

        var result = new PlatformRoleAuthorizationDto
        {
            ApiBlacklist = authorization == null
                ? []
                : PlatformSerializationHelper.DeserializeStringList(authorization.ApiBlacklist),
            AccountScopeMode = authorization?.AccountScopeMode ?? PlatformAuthorizationDefaults.AccountScopeDataAndUsers,
            AccountUserIds = authorization == null
                ? []
                : PlatformSerializationHelper.DeserializeGuidList(authorization.AllowedUserIds),
            CustomOrganizationUnitIds = authorization == null
                ? []
                : PlatformSerializationHelper.DeserializeGuidList(authorization.CustomOrganizationUnitIds),
            DataScopeMode = authorization?.DataScopeMode ?? PlatformAuthorizationDefaults.DataScopeCurrentOrganizationAndDescendants,
            MenuIds = menuIds
        };
        await _distributedCache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), RoleCacheOptions);
        return result;
    }

    private static IReadOnlyCollection<Guid> GetDefaultMenuIds(string roleName, IReadOnlyCollection<AppRoleMenu> allRoleMenus)
    {
        return roleName switch
        {
            PlatformRoleNames.Admin => allRoleMenus.Select(x => x.MenuId).Distinct().ToList(),
            PlatformRoleNames.Pmo =>
            [
                PlatformSeedIds.PlatformRoot,
                PlatformSeedIds.PlatformWorkspace,
                PlatformSeedIds.PlatformProfile,
                PlatformSeedIds.PlatformScheduler,
                PlatformSeedIds.ProjectRoot,
                PlatformSeedIds.ProjectList,
                PlatformSeedIds.ProjectDetail,
                PlatformSeedIds.ProjectMyRelated,
                PlatformSeedIds.ProjectPmoOverview,
                PlatformSeedIds.ProjectCreate,
                PlatformSeedIds.BusinessRoot,
                PlatformSeedIds.BusinessOverview,
                PlatformSeedIds.BusinessProjects,
                PlatformSeedIds.BusinessReports,
                PlatformSeedIds.BusinessBudgetSensitive
            ],
            PlatformRoleNames.Pm =>
            [
                PlatformSeedIds.PlatformRoot,
                PlatformSeedIds.PlatformWorkspace,
                PlatformSeedIds.PlatformProfile,
                PlatformSeedIds.ProjectRoot,
                PlatformSeedIds.ProjectList,
                PlatformSeedIds.ProjectDetail,
                PlatformSeedIds.ProjectMyRelated,
                PlatformSeedIds.ProjectPmDashboard,
                PlatformSeedIds.ProjectCreate,
                PlatformSeedIds.ProjectTaskManage,
                PlatformSeedIds.BusinessRoot,
                PlatformSeedIds.BusinessOverview,
                PlatformSeedIds.BusinessProjects
            ],
            PlatformRoleNames.Member =>
            [
                PlatformSeedIds.PlatformRoot,
                PlatformSeedIds.PlatformWorkspace,
                PlatformSeedIds.PlatformProfile,
                PlatformSeedIds.ProjectRoot,
                PlatformSeedIds.ProjectList,
                PlatformSeedIds.ProjectDetail,
                PlatformSeedIds.ProjectMyRelated
            ],
            PlatformRoleNames.Viewer =>
            [
                PlatformSeedIds.PlatformRoot,
                PlatformSeedIds.PlatformWorkspace,
                PlatformSeedIds.PlatformProfile,
                PlatformSeedIds.ProjectRoot,
                PlatformSeedIds.ProjectList,
                PlatformSeedIds.ProjectDetail,
                PlatformSeedIds.ProjectPmoOverview,
                PlatformSeedIds.BusinessRoot,
                PlatformSeedIds.BusinessOverview,
                PlatformSeedIds.BusinessProjects,
                PlatformSeedIds.BusinessReports
            ],
            _ => []
        };
    }

    public async Task SaveAuthorizationAsync(Guid roleId, PlatformRoleAuthorizationSaveInput input)
    {
        var role = await _roleManager.GetByIdAsync(roleId);
        var authorization = await _roleAuthorizationRepository.FindAsync(x => x.RoleId == role.Id);
        var customOrganizationUnitIds = PlatformSerializationHelper.SerializeGuids(input.CustomOrganizationUnitIds);
        var accountUserIds = PlatformSerializationHelper.SerializeGuids(input.AccountUserIds);
        var apiBlacklist = PlatformSerializationHelper.SerializeStrings(input.ApiBlacklist);

        if (authorization == null)
        {
            authorization = new AppRoleAuthorization(
                Guid.NewGuid(),
                roleId,
                input.DataScopeMode,
                input.AccountScopeMode,
                customOrganizationUnitIds,
                accountUserIds,
                apiBlacklist);
            await _roleAuthorizationRepository.InsertAsync(authorization, autoSave: true);
            await _platformCacheService.InvalidateAsync("role");
            return;
        }

        authorization.Update(
            input.DataScopeMode,
            input.AccountScopeMode,
            customOrganizationUnitIds,
            accountUserIds,
            apiBlacklist);
        await _roleAuthorizationRepository.UpdateAsync(authorization, autoSave: true);
        await _platformCacheService.InvalidateAsync("role");
    }

    private static string GetDescription(string roleName)
    {
        return roleName switch
        {
            PlatformRoleNames.Admin => "系统管理员，可维护平台与全量权限。",
            PlatformRoleNames.Pmo => "PMO 角色，可查看项目汇总与经营预警。",
            PlatformRoleNames.Pm => "项目经理角色，可负责项目执行闭环。",
            PlatformRoleNames.Member => "项目成员角色，可处理任务与协作事项。",
            PlatformRoleNames.Viewer => "经营查看角色，仅查看汇总与只读经营信息。",
            _ => "自定义角色，可按菜单授权灵活分配。"
        };
    }

    private static int GetOrder(string roleName)
    {
        var index = BuiltInRoleNames
            .Select((name, order) => new { name, order })
            .FirstOrDefault(x => string.Equals(x.name, roleName, StringComparison.OrdinalIgnoreCase));
        return index?.order ?? 1000;
    }

    private static string NormalizeRoleName(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            throw new InvalidOperationException("角色名称不能为空。");
        }

        return roleName.Trim().ToLowerInvariant();
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
