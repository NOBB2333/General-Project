using System.IO;
using System.Text.RegularExpressions;
using General.Admin.Permissions;
using Volo.Abp.Authorization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement;
using Volo.Abp.TenantManagement;

namespace General.Admin.Platform;

public class PlatformTenantService : ITransientDependency
{
    private readonly ICurrentTenant _currentTenant;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IRepository<AppMenu, Guid> _menuRepository;
    private readonly IPermissionManager _permissionManager;
    private readonly IRepository<AppRoleMenu, Guid> _roleMenuRepository;
    private readonly IRepository<AppTenantAuthorization, Guid> _tenantAuthorizationRepository;
    private readonly IRepository<OrganizationUnit, Guid> _organizationUnitRepository;
    private readonly IdentityRoleManager _roleManager;
    private readonly IdentityUserManager _userManager;
    private readonly IRepository<IdentityUser, Guid> _userRepository;
    private readonly OrganizationUnitManager _organizationUnitManager;
    private readonly PlatformCacheService _platformCacheService;
    private readonly TenantManager _tenantManager;
    private readonly ITenantRepository _tenantRepository;

    public PlatformTenantService(
        ICurrentTenant currentTenant,
        IGuidGenerator guidGenerator,
        IRepository<AppMenu, Guid> menuRepository,
        IPermissionManager permissionManager,
        IRepository<AppRoleMenu, Guid> roleMenuRepository,
        IRepository<AppTenantAuthorization, Guid> tenantAuthorizationRepository,
        IRepository<OrganizationUnit, Guid> organizationUnitRepository,
        IdentityRoleManager roleManager,
        IdentityUserManager userManager,
        IRepository<IdentityUser, Guid> userRepository,
        OrganizationUnitManager organizationUnitManager,
        PlatformCacheService platformCacheService,
        TenantManager tenantManager,
        ITenantRepository tenantRepository)
    {
        _currentTenant = currentTenant;
        _guidGenerator = guidGenerator;
        _menuRepository = menuRepository;
        _permissionManager = permissionManager;
        _roleMenuRepository = roleMenuRepository;
        _tenantAuthorizationRepository = tenantAuthorizationRepository;
        _organizationUnitRepository = organizationUnitRepository;
        _roleManager = roleManager;
        _userManager = userManager;
        _userRepository = userRepository;
        _organizationUnitManager = organizationUnitManager;
        _platformCacheService = platformCacheService;
        _tenantManager = tenantManager;
        _tenantRepository = tenantRepository;
    }

    public async Task<List<PlatformTenantListItemDto>> GetListAsync()
    {
        EnsureHostTenantManagement();
        var tenants = (await _tenantRepository.GetListAsync(includeDetails: true))
            .OrderBy(x => x.Name)
            .ToList();
        var authorizations = (await _tenantAuthorizationRepository.GetListAsync())
            .ToDictionary(x => x.TenantId);
        var adminUsers = await ResolveTenantAdminUsersAsync(authorizations);

        return tenants
            .Select(x => new PlatformTenantListItemDto
            {
                AdminEmail = authorizations.GetValueOrDefault(x.Id) is { AdminUserId: { } adminUserId } authorizationForAdmin &&
                             adminUsers.TryGetValue(adminUserId, out var adminUser)
                    ? adminUser.Email
                    : null,
                AdminUserId = authorizations.GetValueOrDefault(x.Id)?.AdminUserId,
                AdminUserName = authorizations.GetValueOrDefault(x.Id) is { AdminUserId: { } adminUserId2 } authorizationForName &&
                                 adminUsers.TryGetValue(adminUserId2, out var adminUserForName)
                    ? adminUserForName.UserName
                    : null,
                ApiBlacklist = authorizations.GetValueOrDefault(x.Id) is { } authorization
                    ? PlatformSerializationHelper.DeserializeStringList(authorization.ApiBlacklist)
                    : [],
                CreationTime = x.CreationTime,
                DefaultConnectionStringDisplay = BuildConnectionStringDisplay(
                    x.ConnectionStrings
                        .FirstOrDefault(item => item.Name == ConnectionStrings.DefaultConnectionStringName)
                        ?.Value),
                HasDefaultConnectionString = x.ConnectionStrings
                    .Any(item =>
                        item.Name == ConnectionStrings.DefaultConnectionStringName &&
                        !string.IsNullOrWhiteSpace(item.Value)),
                Id = x.Id,
                HasExplicitAuthorization = authorizations.ContainsKey(x.Id),
                IsActive = authorizations.GetValueOrDefault(x.Id)?.IsActive ?? true,
                Name = x.Name,
                Remark = authorizations.GetValueOrDefault(x.Id)?.Remark
            })
            .ToList();
    }

    private async Task<Dictionary<Guid, IdentityUser>> ResolveTenantAdminUsersAsync(
        IReadOnlyDictionary<Guid, AppTenantAuthorization> authorizations)
    {
        var result = new Dictionary<Guid, IdentityUser>();
        foreach (var authorization in authorizations.Values.Where(x => x.AdminUserId.HasValue))
        {
            using (_currentTenant.Change(authorization.TenantId))
            {
                var user = await _userRepository.FindAsync(authorization.AdminUserId!.Value);
                if (user != null)
                {
                    result[user.Id] = user;
                }
            }
        }

        return result;
    }

    public async Task CreateAsync(PlatformTenantSaveInput input)
    {
        EnsureHostTenantManagement();
        ValidateTenantCreateInput(input);
        var tenant = await _tenantManager.CreateAsync(input.Name.Trim());
        if (!string.IsNullOrWhiteSpace(input.DefaultConnectionString))
        {
            tenant.SetDefaultConnectionString(input.DefaultConnectionString.Trim());
        }

        await _tenantRepository.InsertAsync(tenant, autoSave: true);

        var adminUserId = input.AdminUserId;
        if (!adminUserId.HasValue)
        {
            adminUserId = await InitializeTenantAdminAsync(tenant, input);
        }
        else
        {
            await EnsureAdminUserBelongsToTenantAsync(tenant.Id, adminUserId.Value);
        }

        await SaveAuthorizationAsync(tenant.Id, new PlatformTenantAuthorizationSaveInput
        {
            AdminUserId = adminUserId,
            ApiBlacklist = [],
            IsActive = true,
            MenuIds = await ResolveDefaultMenuIdsAsync(),
            Remark = input.Remark
        });
    }

    public async Task UpdateAsync(Guid id, PlatformTenantSaveInput input)
    {
        EnsureHostTenantManagement();
        var tenant = await _tenantRepository.GetAsync(id);
        await _tenantManager.ChangeNameAsync(tenant, input.Name.Trim());
        if (!string.IsNullOrWhiteSpace(input.DefaultConnectionString))
        {
            tenant.SetDefaultConnectionString(input.DefaultConnectionString.Trim());
        }

        await _tenantRepository.UpdateAsync(tenant, autoSave: true);

        var authorization = await GetAuthorizationAsync(id);
        await SaveAuthorizationAsync(id, new PlatformTenantAuthorizationSaveInput
        {
            AdminUserId = input.AdminUserId,
            ApiBlacklist = authorization.ApiBlacklist,
            IsActive = authorization.IsActive,
            MenuIds = authorization.MenuIds,
            Remark = input.Remark
        });
    }

    public async Task DeleteAsync(Guid id)
    {
        EnsureHostTenantManagement();
        var tenant = await _tenantRepository.GetAsync(id);
        if (tenant.Name.Equals(PlatformSeedIds.DefaultTenantName, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("默认租户不允许删除。");
        }

        var authorization = await _tenantAuthorizationRepository.FindAsync(x => x.TenantId == id);
        if (authorization != null)
        {
            await _tenantAuthorizationRepository.DeleteAsync(authorization, autoSave: true);
        }

        await _tenantRepository.DeleteAsync(id);
    }

    public async Task<PlatformTenantAuthorizationDto> GetAuthorizationAsync(Guid tenantId)
    {
        EnsureHostTenantManagement();
        await _tenantRepository.GetAsync(tenantId);
        var authorization = await _tenantAuthorizationRepository.FindAsync(x => x.TenantId == tenantId);
        var defaultMenuIds = await ResolveDefaultMenuIdsAsync();
        return new PlatformTenantAuthorizationDto
        {
            AdminUserId = authorization?.AdminUserId,
            ApiBlacklist = authorization == null
                ? []
                : PlatformSerializationHelper.DeserializeStringList(authorization.ApiBlacklist),
            IsActive = authorization?.IsActive ?? true,
            MenuIds = authorization == null
                ? defaultMenuIds
                : PlatformSerializationHelper.DeserializeGuidList(authorization.MenuIds),
            Remark = authorization?.Remark
        };
    }

    public async Task SaveAuthorizationAsync(Guid tenantId, PlatformTenantAuthorizationSaveInput input)
    {
        EnsureHostTenantManagement();
        await _tenantRepository.GetAsync(tenantId);
        if (input.AdminUserId.HasValue)
        {
            await EnsureAdminUserBelongsToTenantAsync(tenantId, input.AdminUserId.Value);
        }

        var authorization = await _tenantAuthorizationRepository.FindAsync(x => x.TenantId == tenantId);
        var normalizedMenuIds = PlatformSerializationHelper.SerializeGuids(input.MenuIds);
        var normalizedApiBlacklist = PlatformSerializationHelper.SerializeStrings(input.ApiBlacklist);

        if (authorization == null)
        {
            authorization = new AppTenantAuthorization(
                Guid.NewGuid(),
                tenantId,
                normalizedMenuIds,
                normalizedApiBlacklist,
                input.IsActive,
                input.AdminUserId,
                input.Remark);
            await _tenantAuthorizationRepository.InsertAsync(authorization, autoSave: true);
            await _platformCacheService.InvalidateAsync("menu");
            return;
        }

        authorization.Update(normalizedMenuIds, normalizedApiBlacklist, input.IsActive, input.AdminUserId, input.Remark);
        await _tenantAuthorizationRepository.UpdateAsync(authorization, autoSave: true);
        await _platformCacheService.InvalidateAsync("menu");
    }

    private async Task EnsureAdminUserBelongsToTenantAsync(Guid tenantId, Guid adminUserId)
    {
        using (_currentTenant.Change(tenantId))
        {
            var adminUser = await _userRepository.FindAsync(adminUserId);
            if (adminUser == null)
            {
                throw new InvalidOperationException("租户管理员必须是当前租户下的用户。");
            }
        }
    }

    public async Task SetStatusAsync(Guid tenantId, bool isActive)
    {
        EnsureHostTenantManagement();
        await _tenantRepository.GetAsync(tenantId);
        var authorization = await _tenantAuthorizationRepository.FindAsync(x => x.TenantId == tenantId);
        if (authorization == null)
        {
            authorization = new AppTenantAuthorization(
                Guid.NewGuid(),
                tenantId,
                PlatformSerializationHelper.SerializeGuids(await ResolveDefaultMenuIdsAsync()),
                "[]",
                isActive);
            await _tenantAuthorizationRepository.InsertAsync(authorization, autoSave: true);
            await _platformCacheService.InvalidateAsync("menu");
            return;
        }

        authorization.UpdateStatus(isActive);
        await _tenantAuthorizationRepository.UpdateAsync(authorization, autoSave: true);
        await _platformCacheService.InvalidateAsync("menu");
    }

    private async Task<List<Guid>> ResolveDefaultMenuIdsAsync()
    {
        return (await _menuRepository.GetListAsync())
            .Where(x => x.IsEnabled)
            .Select(x => x.Id)
            .Distinct()
            .OrderBy(x => x)
            .ToList();
    }

    private async Task<Guid> InitializeTenantAdminAsync(Tenant tenant, PlatformTenantSaveInput input)
    {
        using (_currentTenant.Change(tenant.Id, tenant.Name))
        {
            var role = await _roleManager.FindByNameAsync(PlatformRoleNames.Admin);
            if (role == null)
            {
                role = new IdentityRole(_guidGenerator.Create(), PlatformRoleNames.Admin, tenant.Id);
                EnsureSucceeded(await _roleManager.CreateAsync(role));
            }

            await EnsureTenantAdminAuthorizationAsync(role);

            var adminUserName = input.AdminUserName!.Trim();
            var adminEmail = NormalizeAdminEmail(input.AdminEmail, tenant.Name, adminUserName);
            var existingUser = await _userManager.FindByNameAsync(adminUserName);
            if (existingUser != null)
            {
                if (!await _userManager.IsInRoleAsync(existingUser, PlatformRoleNames.Admin))
                {
                    EnsureSucceeded(await _userManager.AddToRoleAsync(existingUser, PlatformRoleNames.Admin));
                }

                var existingOrganizationUnitId = await EnsureTenantRootOrganizationAsync(tenant.Name);
                await _userManager.AddToOrganizationUnitAsync(existingUser.Id, existingOrganizationUnitId);
                return existingUser.Id;
            }

            var user = new IdentityUser(_guidGenerator.Create(), adminUserName, adminEmail, tenant.Id)
            {
                Name = "租户管理员",
                Surname = string.Empty
            };
            user.AddRole(role.Id);
            EnsureSucceeded(await _userManager.CreateAsync(user, input.AdminPassword!.Trim()));

            var organizationUnitId = await EnsureTenantRootOrganizationAsync(tenant.Name);
            await _userManager.AddToOrganizationUnitAsync(user.Id, organizationUnitId);
            return user.Id;
        }
    }

    private async Task<Guid> EnsureTenantRootOrganizationAsync(string tenantName)
    {
        var existing = await _organizationUnitRepository.FindAsync(x => x.ParentId == null && x.DisplayName == tenantName);
        if (existing != null)
        {
            return existing.Id;
        }

        var organizationUnit = new OrganizationUnit(_guidGenerator.Create(), tenantName, null);
        await _organizationUnitManager.CreateAsync(organizationUnit);
        return organizationUnit.Id;
    }

    private async Task EnsureTenantAdminAuthorizationAsync(IdentityRole role)
    {
        var allMenus = await _menuRepository.GetListAsync();
        var defaultAdminMenuIds = ResolveTenantAdminMenuIds(allMenus);
        var existingRoleMenus = await _roleMenuRepository.GetListAsync();
        var existingMenuIds = existingRoleMenus
            .Where(x => x.RoleId == role.Id)
            .Select(x => x.MenuId)
            .ToHashSet();
        var excludedRoleMenus = existingRoleMenus
            .Where(x => x.RoleId == role.Id && IsTenantManagementMenu(x.MenuId))
            .ToList();
        if (excludedRoleMenus.Count > 0)
        {
            await _roleMenuRepository.DeleteManyAsync(excludedRoleMenus, autoSave: true);
        }

        var roleMenusToCreate = defaultAdminMenuIds
            .Where(menuId => !existingMenuIds.Contains(menuId))
            .Select(menuId => new AppRoleMenu(_guidGenerator.Create(), role.Id, menuId))
            .ToList();
        if (roleMenusToCreate.Count > 0)
        {
            await _roleMenuRepository.InsertManyAsync(roleMenusToCreate, autoSave: true);
        }

        var grantedMenuIdSet = defaultAdminMenuIds.ToHashSet();
        var managedPermissionCodes = AdminPermissions.All.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var permissionCodes = allMenus
            .Where(x => grantedMenuIdSet.Contains(x.Id) && x.IsEnabled && !string.IsNullOrWhiteSpace(x.PermissionCode))
            .Select(x => x.PermissionCode!.Trim())
            .Where(x => managedPermissionCodes.Contains(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        foreach (var permissionCode in permissionCodes)
        {
            await _permissionManager.SetAsync(
                permissionCode,
                RolePermissionValueProvider.ProviderName,
                role.Name,
                true);
        }

        await _permissionManager.SetAsync(
            AdminPermissions.Platform.TenantManage,
            RolePermissionValueProvider.ProviderName,
            role.Name,
            false);
    }

    private static List<Guid> ResolveTenantAdminMenuIds(IReadOnlyCollection<AppMenu> allMenus)
    {
        return allMenus
            .Where(x => x.IsEnabled && !IsTenantManagementMenu(x.Id))
            .Select(x => x.Id)
            .Distinct()
            .OrderBy(x => x)
            .ToList();
    }

    private static bool IsTenantManagementMenu(Guid menuId)
    {
        return menuId == PlatformSeedIds.PlatformTenants ||
               menuId == PlatformSeedIds.PlatformTenantsManage;
    }

    private static void ValidateTenantCreateInput(PlatformTenantSaveInput input)
    {
        if (input.AdminUserId.HasValue)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(input.AdminUserName))
        {
            throw new InvalidOperationException("创建租户时必须填写租户管理员账号。");
        }

        if (string.IsNullOrWhiteSpace(input.AdminPassword))
        {
            throw new InvalidOperationException("创建租户时必须填写租户管理员初始密码。");
        }
    }

    private static string NormalizeAdminEmail(string? email, string tenantName, string userName)
    {
        if (!string.IsNullOrWhiteSpace(email))
        {
            return email.Trim();
        }

        var normalizedTenantName = Regex.Replace(tenantName.Trim().ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
        if (string.IsNullOrWhiteSpace(normalizedTenantName))
        {
            normalizedTenantName = "tenant";
        }

        return $"{userName}@{normalizedTenantName}.local";
    }

    private static void EnsureSucceeded(Microsoft.AspNetCore.Identity.IdentityResult result)
    {
        if (result.Succeeded)
        {
            return;
        }

        throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
    }

    public async Task<List<PlatformTenantUserDto>> GetUsersAsync(Guid tenantId)
    {
        EnsureHostTenantManagement();
        await _tenantRepository.GetAsync(tenantId);

        using (_currentTenant.Change(tenantId))
        {
            return (await _userRepository.GetListAsync())
                .OrderBy(x => x.UserName)
                .Select(x => new PlatformTenantUserDto
                {
                    DisplayName = string.IsNullOrWhiteSpace($"{x.Name}{x.Surname}".Trim())
                        ? x.UserName ?? string.Empty
                        : $"{x.Name}{x.Surname}".Trim(),
                    Email = x.Email ?? string.Empty,
                    Id = x.Id,
                    IsActive = x.IsActive,
                    Username = x.UserName ?? string.Empty
                })
                .ToList();
        }
    }

    private void EnsureHostTenantManagement()
    {
        if (_currentTenant.Id.HasValue)
        {
            throw new AbpAuthorizationException("租户管理只能在宿主上下文执行。");
        }
    }

    private static string? BuildConnectionStringDisplay(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return null;
        }

        var provider = ResolveProvider(connectionString);
        var database = ResolveDatabaseName(connectionString, provider);
        return string.IsNullOrWhiteSpace(database)
            ? $"已配置（{provider}）"
            : $"已配置（{provider} · {database}）";
    }

    private static string? ExtractConnectionValue(string connectionString, params string[] keys)
    {
        foreach (var key in keys)
        {
            var match = Regex.Match(
                connectionString,
                $@"(?:^|;)\s*{Regex.Escape(key)}\s*=\s*(?<value>[^;]+)",
                RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups["value"].Value.Trim();
            }
        }

        return null;
    }

    private static string? ResolveDatabaseName(string connectionString, string provider)
    {
        if (provider == "SQLite")
        {
            var dataSource = ExtractConnectionValue(connectionString, "Data Source");
            return string.IsNullOrWhiteSpace(dataSource) ? null : Path.GetFileName(dataSource);
        }

        return ExtractConnectionValue(connectionString, "Initial Catalog", "Database");
    }

    private static string ResolveProvider(string connectionString)
    {
        if (connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase))
        {
            return "PostgreSQL";
        }

        if (connectionString.Contains("Uid=", StringComparison.OrdinalIgnoreCase) ||
            connectionString.Contains("User Id=", StringComparison.OrdinalIgnoreCase))
        {
            return "MySQL";
        }

        if (connectionString.Contains("Initial Catalog=", StringComparison.OrdinalIgnoreCase) ||
            connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase) ||
            connectionString.Contains("Trusted_Connection=", StringComparison.OrdinalIgnoreCase))
        {
            return "SQL Server";
        }

        if (connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase) ||
            connectionString.Contains(".db", StringComparison.OrdinalIgnoreCase) ||
            connectionString.Contains(".sqlite", StringComparison.OrdinalIgnoreCase))
        {
            return "SQLite";
        }

        return "数据库";
    }
}
