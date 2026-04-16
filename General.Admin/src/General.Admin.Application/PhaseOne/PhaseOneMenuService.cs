using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace General.Admin.PhaseOne;

public class PhaseOneMenuService : ITransientDependency
{
    private readonly ICurrentUser _currentUser;
    private readonly IRepository<AppMenu, Guid> _menuRepository;
    private readonly IRepository<AppRoleMenu, Guid> _roleMenuRepository;
    private readonly IRepository<IdentityRole, Guid> _roleRepository;

    public PhaseOneMenuService(
        ICurrentUser currentUser,
        IRepository<AppMenu, Guid> menuRepository,
        IRepository<AppRoleMenu, Guid> roleMenuRepository,
        IRepository<IdentityRole, Guid> roleRepository)
    {
        _currentUser = currentUser;
        _menuRepository = menuRepository;
        _roleMenuRepository = roleMenuRepository;
        _roleRepository = roleRepository;
    }

    public async Task<List<string>> GetCurrentAccessCodesAsync()
    {
        var menus = await GetGrantedMenusForCurrentUserAsync();
        return menus
            .Where(x => x.IsEnabled && !string.IsNullOrWhiteSpace(x.PermissionCode))
            .Select(x => x.PermissionCode!)
            .Distinct()
            .OrderBy(x => x)
            .ToList();
    }

    public async Task<List<BackendRouteDto>> GetCurrentMenusAsync(IReadOnlyCollection<string>? appCodes = null)
    {
        var grantedMenus = FilterMenusByAppCodes(await GetGrantedMenusForCurrentUserAsync(), appCodes);
        var visibleMenus = grantedMenus
            .Where(x => x.IsEnabled && x.Type != PhaseOneMenuType.Button)
            .OrderBy(x => x.Order)
            .ThenBy(x => x.CreationTime)
            .ToList();

        return BuildRouteTree(visibleMenus, null);
    }

    public async Task<List<Guid>> GetRoleMenuIdsAsync(Guid roleId)
    {
        return (await _roleMenuRepository.GetListAsync())
            .Where(x => x.RoleId == roleId)
            .Select(x => x.MenuId)
            .ToList();
    }

    public async Task<List<MenuPermissionTreeDto>> GetPermissionTreeAsync(IReadOnlyCollection<string>? appCodes = null)
    {
        var menus = FilterMenusByAppCodes((await _menuRepository.GetListAsync())
            .OrderBy(x => x.Order)
            .ThenBy(x => x.CreationTime)
            .ToList(), appCodes);

        return BuildPermissionTree(menus, null);
    }

    public async Task CreateAsync(PhaseOneMenuSaveInput input)
    {
        var menu = CreateMenu(Guid.NewGuid(), input);
        await _menuRepository.InsertAsync(menu, autoSave: true);
    }

    public async Task UpdateAsync(Guid id, PhaseOneMenuSaveInput input)
    {
        var menu = await _menuRepository.GetAsync(id);
        menu.Update(
            input.AppCode,
            input.ParentId,
            input.Name,
            input.Path,
            ResolveComponent(input),
            null,
            input.Type,
            input.Title,
            input.Icon,
            ResolvePermissionCode(input),
            null,
            false,
            input.Type != PhaseOneMenuType.Button,
            input.Type == PhaseOneMenuType.Button,
            input.Type == PhaseOneMenuType.Button,
            input.Type == PhaseOneMenuType.Button,
            false,
            input.Order,
            input.IsEnabled);
        await _menuRepository.UpdateAsync(menu, autoSave: true);
    }

    public async Task DeleteAsync(Guid id)
    {
        var menus = await _menuRepository.GetListAsync();
        var descendants = GetDescendantIds(menus, id);
        descendants.Add(id);

        var roleMappings = (await _roleMenuRepository.GetListAsync())
            .Where(x => descendants.Contains(x.MenuId))
            .ToList();
        if (roleMappings.Count > 0)
        {
            await _roleMenuRepository.DeleteManyAsync(roleMappings, autoSave: true);
        }

        var menusToDelete = menus.Where(x => descendants.Contains(x.Id)).ToList();
        if (menusToDelete.Count > 0)
        {
            await _menuRepository.DeleteManyAsync(menusToDelete, autoSave: true);
        }
    }

    public async Task GrantRoleMenusAsync(Guid roleId, IReadOnlyCollection<Guid> menuIds)
    {
        var allMenus = await _menuRepository.GetListAsync();
        var normalizedMenuIds = NormalizeMenuIds(menuIds, allMenus);

        var existingMappings = (await _roleMenuRepository.GetListAsync())
            .Where(x => x.RoleId == roleId)
            .ToList();

        var existingIds = existingMappings.Select(x => x.MenuId).ToHashSet();
        var toDelete = existingMappings.Where(x => !normalizedMenuIds.Contains(x.MenuId)).ToList();
        var toAdd = normalizedMenuIds
            .Where(menuId => !existingIds.Contains(menuId))
            .Select(menuId => new AppRoleMenu(Guid.NewGuid(), roleId, menuId))
            .ToList();

        if (toDelete.Count > 0)
        {
            await _roleMenuRepository.DeleteManyAsync(toDelete);
        }

        if (toAdd.Count > 0)
        {
            await _roleMenuRepository.InsertManyAsync(toAdd, autoSave: true);
        }
    }

    private List<BackendRouteDto> BuildRouteTree(IReadOnlyCollection<AppMenu> menus, Guid? parentId)
    {
        return menus
            .Where(x => x.ParentId == parentId)
            .OrderBy(x => x.Order)
            .ThenBy(x => x.CreationTime)
            .Select(x => new BackendRouteDto
            {
                Name = x.Name,
                Path = x.Path,
                Component = x.Component,
                Redirect = x.Redirect,
                Meta = new BackendRouteMetaDto
                {
                    AffixTab = x.AffixTab ? true : null,
                    HideInBreadcrumb = x.HideInBreadcrumb ? true : null,
                    HideInMenu = x.HideInMenu ? true : null,
                    HideInTab = x.HideInTab ? true : null,
                    Icon = x.Icon,
                    KeepAlive = x.KeepAlive ? true : null,
                    Link = x.Link,
                    MenuVisibleWithForbidden = x.MenuVisibleWithForbidden ? true : null,
                    Order = x.Order,
                    Title = x.Title
                },
                Children = BuildRouteTree(menus, x.Id)
            })
            .ToList();
    }

    private List<MenuPermissionTreeDto> BuildPermissionTree(IReadOnlyCollection<AppMenu> menus, Guid? parentId)
    {
        return menus
            .Where(x => x.ParentId == parentId)
            .OrderBy(x => x.Order)
            .ThenBy(x => x.CreationTime)
            .Select(x => new MenuPermissionTreeDto
            {
                AppCode = x.AppCode,
                Children = BuildPermissionTree(menus, x.Id),
                Component = x.Component,
                Icon = x.Icon,
                Id = x.Id,
                IsEnabled = x.IsEnabled,
                Name = x.Name,
                Order = x.Order,
                ParentId = x.ParentId,
                Path = x.Path,
                PermissionCode = x.PermissionCode,
                Title = x.Title,
                Type = x.Type
            })
            .ToList();
    }

    private async Task<List<AppMenu>> GetGrantedMenusForCurrentUserAsync()
    {
        if (_currentUser.IsInRole(PhaseOneRoleNames.Admin))
        {
            return (await _menuRepository.GetListAsync())
                .Where(x => x.IsEnabled)
                .OrderBy(x => x.Order)
                .ToList();
        }

        var currentRoleNames = _currentUser.Roles?.Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? [];
        if (currentRoleNames.Count == 0)
        {
            return [];
        }

        var roleIds = (await _roleRepository.GetListAsync())
            .Where(x => currentRoleNames.Contains(x.Name))
            .Select(x => x.Id)
            .ToList();

        if (roleIds.Count == 0)
        {
            return [];
        }

        var menuIds = (await _roleMenuRepository.GetListAsync())
            .Where(x => roleIds.Contains(x.RoleId))
            .Select(x => x.MenuId)
            .Distinct()
            .ToList();

        if (menuIds.Count == 0)
        {
            return [];
        }

        return (await _menuRepository.GetListAsync())
            .Where(x => menuIds.Contains(x.Id) && x.IsEnabled)
            .OrderBy(x => x.Order)
            .ToList();
    }

    private static HashSet<Guid> NormalizeMenuIds(IEnumerable<Guid> menuIds, IReadOnlyCollection<AppMenu> allMenus)
    {
        var allMenusById = allMenus.ToDictionary(x => x.Id);
        var normalized = new HashSet<Guid>(menuIds);
        foreach (var menuId in menuIds)
        {
            var currentMenuId = menuId;
            while (allMenusById.TryGetValue(currentMenuId, out var menu) && menu.ParentId.HasValue)
            {
                currentMenuId = menu.ParentId.Value;
                if (!normalized.Add(currentMenuId))
                {
                    break;
                }
            }
        }

        return normalized;
    }

    private static List<AppMenu> FilterMenusByAppCodes(IReadOnlyCollection<AppMenu> menus, IReadOnlyCollection<string>? appCodes)
    {
        if (appCodes == null || appCodes.Count == 0)
        {
            return menus.ToList();
        }

        var normalizedCodes = appCodes
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().ToLowerInvariant())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (normalizedCodes.Count == 0)
        {
            return menus.ToList();
        }

        return menus
            .Where(x => normalizedCodes.Contains(x.AppCode))
            .ToList();
    }

    private static AppMenu CreateMenu(Guid id, PhaseOneMenuSaveInput input)
    {
        return new AppMenu(
            id,
            input.AppCode,
            input.ParentId,
            input.Name,
            input.Path,
            ResolveComponent(input),
            null,
            input.Type,
            input.Title,
            input.Icon,
            ResolvePermissionCode(input),
            null,
            false,
            input.Type != PhaseOneMenuType.Button,
            input.Type == PhaseOneMenuType.Button,
            input.Type == PhaseOneMenuType.Button,
            input.Type == PhaseOneMenuType.Button,
            false,
            input.Order,
            input.IsEnabled);
    }

    private static string? ResolveComponent(PhaseOneMenuSaveInput input)
    {
        return input.Type switch
        {
            PhaseOneMenuType.Catalog => "BasicLayout",
            PhaseOneMenuType.Button => null,
            _ => string.IsNullOrWhiteSpace(input.Component) ? null : input.Component.Trim()
        };
    }

    private static string? ResolvePermissionCode(PhaseOneMenuSaveInput input)
    {
        if (input.Type != PhaseOneMenuType.Button)
        {
            return string.IsNullOrWhiteSpace(input.PermissionCode) ? null : input.PermissionCode.Trim();
        }

        return string.IsNullOrWhiteSpace(input.PermissionCode)
            ? input.Path.Trim()
            : input.PermissionCode.Trim();
    }

    private static HashSet<Guid> GetDescendantIds(IReadOnlyCollection<AppMenu> menus, Guid parentId)
    {
        var ids = new HashSet<Guid>();
        foreach (var child in menus.Where(x => x.ParentId == parentId))
        {
            ids.Add(child.Id);
            ids.UnionWith(GetDescendantIds(menus, child.Id));
        }

        return ids;
    }
}
