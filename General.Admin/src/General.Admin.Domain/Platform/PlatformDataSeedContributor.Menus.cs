using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.TenantManagement;

namespace General.Admin.Platform;

public partial class PlatformDataSeedContributor
{
    private async Task SeedMenusAsync()
    {
        var seedMenus = BuildSeedMenus();
        var existingMenus = (await _menuRepository.GetListAsync())
            .ToDictionary(x => x.Id, x => x);
        var menusToUpdate = new List<AppMenu>();

        foreach (var seedMenu in seedMenus)
        {
            if (existingMenus.TryGetValue(seedMenu.Id, out var existingMenu))
            {
                existingMenu.Update(
                    seedMenu.AppCode,
                    seedMenu.ParentId,
                    seedMenu.Name,
                    seedMenu.Path,
                    seedMenu.Component,
                    seedMenu.Redirect,
                    seedMenu.Type,
                    seedMenu.Title,
                    seedMenu.Icon,
                    seedMenu.PermissionCode,
                    seedMenu.Link,
                    seedMenu.AffixTab,
                    seedMenu.KeepAlive,
                    seedMenu.HideInBreadcrumb,
                    seedMenu.HideInMenu,
                    seedMenu.HideInTab,
                    seedMenu.MenuVisibleWithForbidden,
                    seedMenu.Order,
                    seedMenu.IsEnabled);
                menusToUpdate.Add(existingMenu);
                continue;
            }

            await _menuRepository.InsertAsync(seedMenu, autoSave: true);
        }

        if (menusToUpdate.Count > 0)
        {
            await _menuRepository.UpdateManyAsync(menusToUpdate, autoSave: true);
        }

        await RemoveDeprecatedMenusAsync();
        await RemovePlatformTodoMenuAsync();
        await EnsureMenuAppCodesAsync();
    }

    private async Task SeedRoleMenusAsync()
    {
        var currentTenantId = _currentTenant.Id;
        var roles = (await _roleRepository.GetListAsync())
            .Where(x => x.TenantId == currentTenantId && PlatformRoleNames.All.Contains(x.Name))
            .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                x => x.Key,
                x => x.Select(role => role.Id).Distinct().ToList(),
                StringComparer.OrdinalIgnoreCase);

        var allMenus = await _menuRepository.GetListAsync();
        var allMenuIds = allMenus
            .Select(x => x.Id)
            .ToList();

        var mappings = new Dictionary<string, IReadOnlyCollection<Guid>>
        {
            [PlatformRoleNames.Admin] = allMenuIds,
            [PlatformRoleNames.Pmo] =
            [
                PlatformSeedIds.DashboardRoot,
                PlatformSeedIds.DashboardWorkspace,
                PlatformSeedIds.DashboardAnalytics,
                PlatformSeedIds.PlatformRoot,
                PlatformSeedIds.PlatformWorkspace,
                PlatformSeedIds.PlatformProfile,
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
                PlatformSeedIds.BusinessBudgetSensitive,
                PlatformSeedIds.BusinessReportExport
            ],
            [PlatformRoleNames.Pm] =
            [
                PlatformSeedIds.DashboardRoot,
                PlatformSeedIds.DashboardWorkspace,
                PlatformSeedIds.DashboardAnalytics,
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
            [PlatformRoleNames.Member] =
            [
                PlatformSeedIds.DashboardRoot,
                PlatformSeedIds.DashboardWorkspace,
                PlatformSeedIds.DashboardAnalytics,
                PlatformSeedIds.PlatformRoot,
                PlatformSeedIds.PlatformWorkspace,
                PlatformSeedIds.PlatformProfile,
                PlatformSeedIds.ProjectRoot,
                PlatformSeedIds.ProjectList,
                PlatformSeedIds.ProjectDetail,
                PlatformSeedIds.ProjectMyRelated
            ],
            [PlatformRoleNames.Viewer] =
            [
                PlatformSeedIds.DashboardRoot,
                PlatformSeedIds.DashboardWorkspace,
                PlatformSeedIds.DashboardAnalytics,
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
            ]
        };

        foreach (var (roleName, menuIds) in mappings)
        {
            if (!roles.TryGetValue(roleName, out var roleIds) || roleIds.Count == 0)
            {
                continue;
            }

            foreach (var roleId in roleIds)
            {
                var existingMenuIds = (await _roleMenuRepository.GetListAsync())
                    .Where(x => x.RoleId == roleId)
                    .Select(x => x.MenuId)
                    .ToHashSet();

                foreach (var menuId in menuIds.Distinct())
                {
                    if (existingMenuIds.Add(menuId))
                    {
                        await _roleMenuRepository.InsertAsync(new AppRoleMenu(_guidGenerator.Create(), roleId, menuId));
                    }
                }
            }

            var role = await _roleRepository.GetAsync(roleIds[0]);
            var effectiveMenuIds = (await _roleMenuRepository.GetListAsync())
                .Where(x => roleIds.Contains(x.RoleId))
                .Select(x => x.MenuId)
                .Concat(menuIds)
                .Distinct()
                .ToList();
            await GrantRolePermissionsForMenusAsync(role, allMenus, effectiveMenuIds);
        }

        await RemovePmoSchedulerSeedPermissionsAsync(roles);

        if (_unitOfWorkManager.Current != null)
        {
            await _unitOfWorkManager.Current.SaveChangesAsync();
        }
    }

    private async Task RemovePmoSchedulerSeedPermissionsAsync(IReadOnlyDictionary<string, List<Guid>> roles)
    {
        if (!roles.TryGetValue(PlatformRoleNames.Pmo, out var roleIds) || roleIds.Count == 0)
        {
            return;
        }

        var schedulerMenuIds = new[]
        {
            PlatformSeedIds.PlatformScheduler,
            PlatformSeedIds.PlatformSchedulerManage,
            PlatformSeedIds.PlatformSchedulerExecute
        }.ToHashSet();

        var roleMenus = (await _roleMenuRepository.GetListAsync())
            .Where(x => roleIds.Contains(x.RoleId) && schedulerMenuIds.Contains(x.MenuId))
            .ToList();
        if (roleMenus.Count > 0)
        {
            await _roleMenuRepository.DeleteManyAsync(roleMenus);
        }

        var schedulerPermissionCodes = new[]
        {
            "Platform.Scheduler.View",
            "Platform.Scheduler.Manage",
            "Platform.Scheduler.Execute"
        }.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var permissionGrants = (await _permissionGrantRepository.GetListAsync())
            .Where(x =>
                schedulerPermissionCodes.Contains(x.Name) &&
                x.ProviderName == RolePermissionValueProvider.ProviderName &&
                x.ProviderKey == PlatformRoleNames.Pmo &&
                x.TenantId == _currentTenant.Id)
            .ToList();
        if (permissionGrants.Count > 0)
        {
            await _permissionGrantRepository.DeleteManyAsync(permissionGrants);
        }
    }

    private async Task SeedDefaultTenantAdminRoleMenusAsync(Tenant defaultTenant, IReadOnlyCollection<AppMenu> allMenus)
    {
        using (_currentTenant.Change(defaultTenant.Id, defaultTenant.Name))
        {
            var role = await _roleManager.FindByNameAsync(PlatformRoleNames.Admin);
            if (role == null)
            {
                return;
            }

            var menuIds = ResolveTenantAdminMenuIds(allMenus);
            var excludedRoleMenus = (await _roleMenuRepository.GetListAsync())
                .Where(x => x.RoleId == role.Id && IsTenantManagementMenu(x.MenuId))
                .ToList();
            if (excludedRoleMenus.Count > 0)
            {
                await _roleMenuRepository.DeleteManyAsync(excludedRoleMenus, autoSave: true);
            }

            foreach (var menuId in menuIds)
            {
                var exists = await _roleMenuRepository.AnyAsync(x => x.RoleId == role.Id && x.MenuId == menuId);
                if (!exists)
                {
                    await _roleMenuRepository.InsertAsync(new AppRoleMenu(_guidGenerator.Create(), role.Id, menuId));
                }
            }

            await GrantRolePermissionsForMenusAsync(role, allMenus, menuIds);
            await RevokeRolePermissionAsync(role, "Platform.Tenant.View");
            await RevokeRolePermissionAsync(role, "Platform.Tenant.Manage");
        }
    }

    private async Task GrantRolePermissionsForMenusAsync(
        IdentityRole role,
        IReadOnlyCollection<AppMenu> allMenus,
        IReadOnlyCollection<Guid> menuIds)
    {
        var menuIdSet = menuIds.ToHashSet();
        var permissionCodes = allMenus
            .Where(x => menuIdSet.Contains(x.Id) && x.IsEnabled && !string.IsNullOrWhiteSpace(x.PermissionCode))
            .Select(x => x.PermissionCode!.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var permissionCode in permissionCodes)
        {
            var exists = await _permissionGrantRepository.AnyAsync(x =>
                x.Name == permissionCode &&
                x.ProviderName == RolePermissionValueProvider.ProviderName &&
                x.ProviderKey == role.Name &&
                x.TenantId == _currentTenant.Id);
            if (!exists)
            {
                await _permissionGrantRepository.InsertAsync(new PermissionGrant(
                    _guidGenerator.Create(),
                    permissionCode,
                    RolePermissionValueProvider.ProviderName,
                    role.Name,
                    _currentTenant.Id));
            }
        }
    }

    private async Task RevokeRolePermissionAsync(IdentityRole role, string permissionCode)
    {
        var grant = await _permissionGrantRepository.FindAsync(x =>
            x.Name == permissionCode &&
            x.ProviderName == RolePermissionValueProvider.ProviderName &&
            x.ProviderKey == role.Name &&
            x.TenantId == _currentTenant.Id);
        if (grant != null)
        {
            await _permissionGrantRepository.DeleteAsync(grant, autoSave: true);
        }
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

    private async Task RemoveDeprecatedMenusAsync()
    {
        var deprecatedMenuIds = new HashSet<Guid>
        {
            PlatformSeedIds.PlatformStats,
            PlatformSeedIds.PlatformOnlineUsers,
            PlatformSeedIds.ProjectWorkspace,
            PlatformSeedIds.ProjectInitiation,
            Guid.Parse("40000000-0000-0000-0000-000000000004"),
            Guid.Parse("40000000-0000-0000-0000-000000000005"),
            Guid.Parse("40000000-0000-0000-0000-000000000006"),
            Guid.Parse("40000000-0000-0000-0000-000000000010"),
            Guid.Parse("40000000-0000-0000-0000-000000000011"),
            Guid.Parse("40000000-0000-0000-0000-000000000012"),
            Guid.Parse("40000000-0000-0000-0000-000000000013")
        };

        var roleMappings = (await _roleMenuRepository.GetListAsync())
            .Where(x => deprecatedMenuIds.Contains(x.MenuId))
            .ToList();
        if (roleMappings.Count > 0)
        {
            await _roleMenuRepository.DeleteManyAsync(roleMappings, autoSave: true);
        }

        var menus = (await _menuRepository.GetListAsync())
            .Where(x => deprecatedMenuIds.Contains(x.Id))
            .ToList();
        if (menus.Count > 0)
        {
            await _menuRepository.DeleteManyAsync(menus, autoSave: true);
        }
    }

    private async Task RemovePlatformTodoMenuAsync()
    {
        var roleMappings = (await _roleMenuRepository.GetListAsync())
            .Where(x => x.MenuId == PlatformSeedIds.PlatformTodo)
            .ToList();
        if (roleMappings.Count > 0)
        {
            await _roleMenuRepository.DeleteManyAsync(roleMappings, autoSave: true);
        }

        var menu = await _menuRepository.FindAsync(PlatformSeedIds.PlatformTodo);
        if (menu != null)
        {
            await _menuRepository.DeleteAsync(menu, autoSave: true);
        }
    }

    private async Task EnsureMenuAppCodesAsync()
    {
        var existingMenus = await _menuRepository.GetListAsync();
        foreach (var menu in existingMenus)
        {
            menu.SetAppCode(ResolveAppCode(menu.Id));
        }

        await _menuRepository.UpdateManyAsync(existingMenus, autoSave: true);
    }

    private static string ResolveAppCode(Guid menuId)
    {
        return menuId.ToString()[0] switch
        {
            '2' => PlatformAppCodes.Platform,
            '3' => PlatformAppCodes.Project,
            '7' => PlatformAppCodes.Ai,
            '8' => PlatformAppCodes.Integration,
            _ => PlatformAppCodes.Business
        };
    }

    private static List<AppMenu> BuildSeedMenus()
    {
        return
        [
            Catalog(PlatformAppCodes.Platform, PlatformSeedIds.DashboardRoot, null, "DashboardCenter", "/dashboard", "综合看板", "lucide:layout-dashboard", 1),
            Menu(PlatformAppCodes.Platform, PlatformSeedIds.DashboardWorkspace, PlatformSeedIds.DashboardRoot, "DashboardWorkspace", "/dashboard/workspace", "/dashboard/workspace/index", "工作台", "carbon:workspace", 10),
            Menu(PlatformAppCodes.Platform, PlatformSeedIds.DashboardAnalytics, PlatformSeedIds.DashboardRoot, "DashboardAnalytics", "/dashboard/analytics", "/dashboard/analytics/index", "分析页", "lucide:area-chart", 20),
            Catalog(PlatformAppCodes.Platform, PlatformSeedIds.PlatformRoot, null, "PlatformCenter", "/platform", "平台中心", "lucide:settings-2"),
            Menu(PlatformAppCodes.Platform, PlatformSeedIds.PlatformWorkspace, PlatformSeedIds.PlatformRoot, "PlatformWorkspace", "/platform/workspace", "/platform/workspace/index", "平台总览", "lucide:layout-dashboard", 10, affixTab: true),
            Menu(PlatformAppCodes.Platform, PlatformSeedIds.PlatformOrganization, PlatformSeedIds.PlatformRoot, "PlatformOrganization", "/platform/organization", "/platform/organization/index", "组织架构", "lucide:building-2", 20),
            Menu(PlatformAppCodes.Platform, PlatformSeedIds.PlatformTenants, PlatformSeedIds.PlatformRoot, "PlatformTenants", "/platform/tenants", "/platform/tenants/index", "租户管理", "lucide:building", 50, permissionCode: "Platform.Tenant.View"),
            Menu(PlatformAppCodes.Platform, PlatformSeedIds.PlatformUsers, PlatformSeedIds.PlatformRoot, "PlatformUsers", "/platform/users", "/platform/users/index", "用户管理", "lucide:users", 30),
            Menu(PlatformAppCodes.Platform, PlatformSeedIds.PlatformRoles, PlatformSeedIds.PlatformRoot, "PlatformRoles", "/platform/roles", "/platform/roles/index", "角色权限", "lucide:key-round", 40, permissionCode: "Platform.Role.Manage"),
            Menu(PlatformAppCodes.Platform, PlatformSeedIds.PlatformMenus, PlatformSeedIds.PlatformRoot, "PlatformMenus", "/platform/menus", "/platform/menus/index", "菜单管理", "lucide:waypoints", 60, permissionCode: "Platform.Menu.Manage"),
            Menu(PlatformAppCodes.Platform, PlatformSeedIds.PlatformConfigs, PlatformSeedIds.PlatformRoot, "PlatformConfigs", "/platform/configs", "/platform/configs/index", "配置参数", "lucide:sliders-horizontal", 65, permissionCode: "Platform.Config.Manage"),
            Menu(PlatformAppCodes.Platform, PlatformSeedIds.PlatformCache, PlatformSeedIds.PlatformRoot, "PlatformCache", "/platform/cache", "/platform/cache/index", "缓存管理", "lucide:database-zap", 66, permissionCode: "Platform.Cache.Manage"),
            Menu(PlatformAppCodes.Platform, PlatformSeedIds.PlatformDicts, PlatformSeedIds.PlatformRoot, "PlatformDicts", "/platform/dicts", "/platform/dicts/index", "字典管理", "lucide:list-tree", 68, permissionCode: "Platform.Dict.Manage"),
            Menu(PlatformAppCodes.Platform, PlatformSeedIds.PlatformOpenApi, PlatformSeedIds.PlatformRoot, "PlatformOpenApi", "/platform/open-api", "/platform/open-api/index", "开放接口", "lucide:plug-zap", 69, permissionCode: "Platform.OpenApi.Manage"),
            Menu(PlatformAppCodes.Platform, PlatformSeedIds.PlatformFiles, PlatformSeedIds.PlatformRoot, "PlatformFiles", "/platform/files", "/platform/files/index", "文件管理", "lucide:files", 70),
            Menu(PlatformAppCodes.Platform, PlatformSeedIds.PlatformRecycleBin, PlatformSeedIds.PlatformRoot, "PlatformRecycleBin", "/platform/recycle-bin", "/platform/recycle-bin/index", "回收中心", "lucide:archive-restore", 73, permissionCode: "Platform.RecycleBin.Manage"),
            Menu(PlatformAppCodes.Platform, PlatformSeedIds.PlatformSystemMonitor, PlatformSeedIds.PlatformRoot, "PlatformSystemMonitor", "/platform/system-monitor", "/platform/system-monitor/index", "系统监控", "lucide:monitor", 90, permissionCode: "Platform.SystemMonitor.View"),
            Menu(PlatformAppCodes.Platform, PlatformSeedIds.PlatformScheduler, PlatformSeedIds.PlatformRoot, "PlatformScheduler", "/platform/scheduler", "/platform/scheduler/index", "定时任务", "lucide:clock-3", 100, permissionCode: "Platform.Scheduler.View"),
            Menu(PlatformAppCodes.Platform, PlatformSeedIds.PlatformAuditLogs, PlatformSeedIds.PlatformRoot, "PlatformAuditLogs", "/platform/audit-logs", "/platform/audit-logs/index", "日志中心", "lucide:shield-check", 75, permissionCode: "Platform.AuditLog.View"),
            Menu(PlatformAppCodes.Platform, PlatformSeedIds.PlatformUpdateLogs, PlatformSeedIds.PlatformRoot, "PlatformUpdateLogs", "/platform/update-logs", "/platform/update-logs/index", "更新日志", "lucide:scroll-text", 110),
            Menu(PlatformAppCodes.Platform, PlatformSeedIds.PlatformProfile, PlatformSeedIds.PlatformRoot, "PlatformProfile", "/platform/profile", "/platform/profile/index", "个人中心", "lucide:user-circle-2", 120),
            Button(PlatformAppCodes.Platform, PlatformSeedIds.PlatformOrganizationManage, PlatformSeedIds.PlatformOrganization, "PlatformOrganizationManage", "Platform.Organization.Manage", "管理组织"),
            Button(PlatformAppCodes.Platform, PlatformSeedIds.PlatformUsersManage, PlatformSeedIds.PlatformUsers, "PlatformUsersManage", "Platform.User.Manage", "管理用户"),
            Button(PlatformAppCodes.Platform, PlatformSeedIds.PlatformRolesManage, PlatformSeedIds.PlatformRoles, "PlatformRoleManage", "Platform.Role.Manage", "管理角色"),
            Button(PlatformAppCodes.Platform, PlatformSeedIds.PlatformTenantsManage, PlatformSeedIds.PlatformTenants, "PlatformTenantManage", "Platform.Tenant.Manage", "管理租户"),
            Button(PlatformAppCodes.Platform, PlatformSeedIds.PlatformMenusManage, PlatformSeedIds.PlatformMenus, "PlatformMenuManage", "Platform.Menu.Manage", "管理菜单"),
            Button(PlatformAppCodes.Platform, PlatformSeedIds.PlatformConfigsManage, PlatformSeedIds.PlatformConfigs, "PlatformConfigManage", "Platform.Config.Manage", "管理配置"),
            Button(PlatformAppCodes.Platform, PlatformSeedIds.PlatformCacheManage, PlatformSeedIds.PlatformCache, "PlatformCacheManage", "Platform.Cache.Manage", "管理缓存"),
            Button(PlatformAppCodes.Platform, PlatformSeedIds.PlatformDictsManage, PlatformSeedIds.PlatformDicts, "PlatformDictManage", "Platform.Dict.Manage", "管理字典"),
            Button(PlatformAppCodes.Platform, PlatformSeedIds.PlatformOpenApiManage, PlatformSeedIds.PlatformOpenApi, "PlatformOpenApiManage", "Platform.OpenApi.Manage", "管理开放接口"),
            Button(PlatformAppCodes.Platform, PlatformSeedIds.PlatformRecycleBinManage, PlatformSeedIds.PlatformRecycleBin, "PlatformRecycleBinManage", "Platform.RecycleBin.Manage", "管理回收中心"),
            Button(PlatformAppCodes.Platform, PlatformSeedIds.PlatformFilesManage, PlatformSeedIds.PlatformFiles, "PlatformFileManage", "Platform.File.Manage", "管理文件"),
            Button(PlatformAppCodes.Platform, PlatformSeedIds.PlatformSchedulerManage, PlatformSeedIds.PlatformScheduler, "PlatformSchedulerManage", "Platform.Scheduler.Manage", "管理任务"),
            Button(PlatformAppCodes.Platform, PlatformSeedIds.PlatformSchedulerExecute, PlatformSeedIds.PlatformScheduler, "PlatformSchedulerExecute", "Platform.Scheduler.Execute", "执行任务"),

            Catalog(PlatformAppCodes.Project, PlatformSeedIds.ProjectRoot, null, "ProjectCenter", "/project", "项目执行", "lucide:folder-kanban", 20),
            Menu(PlatformAppCodes.Project, PlatformSeedIds.ProjectList, PlatformSeedIds.ProjectRoot, "ProjectList", "/project/projects", "/project/projects/index", "项目列表", "lucide:folder-open-dot", 20),
            Menu(PlatformAppCodes.Project, PlatformSeedIds.ProjectDetail, PlatformSeedIds.ProjectRoot, "ProjectDetail", "/project/detail", "/project/detail/index", "项目详情", "lucide:panel-right-open", 30),
            Menu(PlatformAppCodes.Project, PlatformSeedIds.ProjectMyRelated, PlatformSeedIds.ProjectRoot, "ProjectMyRelated", "/project/my-related", "/project/my-related/index", "与我相关", "lucide:user-round-check", 40),
            Menu(PlatformAppCodes.Project, PlatformSeedIds.ProjectPmDashboard, PlatformSeedIds.ProjectRoot, "ProjectPmDashboard", "/project/pm-dashboard", "/project/pm-dashboard/index", "PM 工作台", "lucide:monitor-cog", 60),
            Menu(PlatformAppCodes.Project, PlatformSeedIds.ProjectPmoOverview, PlatformSeedIds.ProjectRoot, "ProjectPmoOverview", "/project/pmo-overview", "/project/pmo-overview/index", "PMO 总览", "lucide:layout-dashboard", 70),
            Button(PlatformAppCodes.Project, PlatformSeedIds.ProjectCreate, PlatformSeedIds.ProjectList, "ProjectCreate", "Project.Project.Create", "新建项目"),
            Button(PlatformAppCodes.Project, PlatformSeedIds.ProjectTaskManage, PlatformSeedIds.ProjectDetail, "ProjectTaskManage", "Project.Task.Manage", "管理任务"),

            Catalog(PlatformAppCodes.Business, PlatformSeedIds.BusinessRoot, null, "BusinessCenter", "/business", "经营管理", "lucide:briefcase-business", 30),
            Menu(PlatformAppCodes.Business, PlatformSeedIds.BusinessOverview, PlatformSeedIds.BusinessRoot, "BusinessOverview", "/business/overview", "/business/overview/index", "经营总览", "lucide:chart-column-big", 10, permissionCode: "Business.Report.View"),
            Menu(PlatformAppCodes.Business, PlatformSeedIds.BusinessProjects, PlatformSeedIds.BusinessRoot, "BusinessProjects", "/business/projects", "/business/projects/index", "项目经营", "lucide:folder-output", 20),
            Menu(PlatformAppCodes.Business, PlatformSeedIds.BusinessReports, PlatformSeedIds.BusinessRoot, "BusinessReports", "/business/reports", "/business/reports/index", "报表中心", "lucide:sheet", 30, permissionCode: "Business.Report.View"),
            Button(PlatformAppCodes.Business, PlatformSeedIds.BusinessBudgetSensitive, PlatformSeedIds.BusinessProjects, "BusinessBudgetSensitive", "Business.Budget.Sensitive", "查看敏感预算"),
            Button(PlatformAppCodes.Business, PlatformSeedIds.BusinessReportExport, PlatformSeedIds.BusinessReports, "BusinessReportExport", "Business.Report.Export", "导出报表"),

            Catalog(PlatformAppCodes.Ai, PlatformSeedIds.AiRoot, null, "AiCenter", "/ai", "AI 智能能力", "lucide:bot", 40),
            Menu(PlatformAppCodes.Ai, PlatformSeedIds.AiConversations, PlatformSeedIds.AiRoot, "AiConversations", "/ai/conversations", "/ai/conversations/index", "AI 对话", "lucide:messages-square", 10, permissionCode: "Ai.Conversation.Use"),
            Menu(PlatformAppCodes.Ai, PlatformSeedIds.AiConversationManagement, PlatformSeedIds.AiRoot, "AiConversationManagement", "/ai/conversation-management", "/ai/conversation-management/index", "会话管理", "lucide:message-square-more", 20, permissionCode: "Ai.Conversation.Use"),
            Menu(PlatformAppCodes.Ai, PlatformSeedIds.AiProviders, PlatformSeedIds.AiRoot, "AiProviders", "/ai/providers", "/ai/providers/index", "模型供应商", "lucide:brain-circuit", 30, permissionCode: "Ai.Provider.Manage"),
            Menu(PlatformAppCodes.Ai, PlatformSeedIds.AiAssistants, PlatformSeedIds.AiRoot, "AiAssistants", "/ai/assistants", "/ai/assistants/index", "助手角色", "lucide:bot-message-square", 40, permissionCode: "Ai.Assistant.Manage"),
            Menu(PlatformAppCodes.Ai, PlatformSeedIds.AiPrompts, PlatformSeedIds.AiRoot, "AiPrompts", "/ai/prompts", "/ai/prompts/index", "场景模板", "lucide:library-big", 50, permissionCode: "Ai.Prompt.Manage"),
            Menu(PlatformAppCodes.Ai, PlatformSeedIds.AiAgentDesign, PlatformSeedIds.AiRoot, "AiAgentDesign", "/ai/agent-design", "/ai/agent-design/index", "Agent 编排", "lucide:network", 60, permissionCode: "Ai.Agent.Manage"),
            Menu(PlatformAppCodes.Ai, PlatformSeedIds.AiArtifacts, PlatformSeedIds.AiRoot, "AiArtifacts", "/ai/artifacts", "/ai/artifacts/index", "安全构件", "lucide:blocks", 70, permissionCode: "Ai.Artifact.Manage"),
            Menu(PlatformAppCodes.Ai, PlatformSeedIds.AiWebSearch, PlatformSeedIds.AiRoot, "AiWebSearch", "/ai/web-search", "/ai/web-search/index", "联网搜索", "lucide:globe-2", 80, permissionCode: "Ai.WebSearch.Manage"),
            Menu(PlatformAppCodes.Ai, PlatformSeedIds.AiMcp, PlatformSeedIds.AiRoot, "AiMcp", "/ai/mcp", "/ai/mcp/index", "MCP 配置", "lucide:plug-zap", 90, permissionCode: "Ai.Mcp.Manage"),
            Menu(PlatformAppCodes.Ai, PlatformSeedIds.AiMemory, PlatformSeedIds.AiRoot, "AiMemory", "/ai/memory", "/ai/memory/index", "记忆库", "lucide:database-zap", 100, permissionCode: "Ai.Memory.Manage"),
            Menu(PlatformAppCodes.Ai, PlatformSeedIds.AiJobs, PlatformSeedIds.AiRoot, "AiJobs", "/ai/jobs", "/ai/jobs/index", "AI 自动化任务", "lucide:calendar-clock", 110, permissionCode: "Ai.Job.Manage"),
            Menu(PlatformAppCodes.Ai, PlatformSeedIds.AiUsage, PlatformSeedIds.AiRoot, "AiUsage", "/ai/usage", "/ai/usage/index", "用量与成本", "lucide:chart-no-axes-combined", 120, permissionCode: "Ai.Usage.View"),
            Menu(PlatformAppCodes.Ai, PlatformSeedIds.AiWritebacks, PlatformSeedIds.AiRoot, "AiWritebacks", "/ai/writebacks", "/ai/writebacks/index", "写回确认", "lucide:clipboard-check", 130, permissionCode: "Ai.Writeback.View"),
            Button(PlatformAppCodes.Ai, PlatformSeedIds.AiProviderManage, PlatformSeedIds.AiProviders, "AiProviderManage", "Ai.Provider.Manage", "管理模型供应商"),
            Button(PlatformAppCodes.Ai, PlatformSeedIds.AiAgentManage, PlatformSeedIds.AiAgentDesign, "AiAgentManage", "Ai.Agent.Manage", "管理 Agent 编排"),
            Button(PlatformAppCodes.Ai, PlatformSeedIds.AiArtifactManage, PlatformSeedIds.AiArtifacts, "AiArtifactManage", "Ai.Artifact.Manage", "管理安全构件"),
            Button(PlatformAppCodes.Ai, PlatformSeedIds.AiWebSearchManage, PlatformSeedIds.AiWebSearch, "AiWebSearchManage", "Ai.WebSearch.Manage", "管理联网搜索"),
            Button(PlatformAppCodes.Ai, PlatformSeedIds.AiWritebackApprove, PlatformSeedIds.AiWritebacks, "AiWritebackApprove", "Ai.Writeback.Approve", "审批写回"),

            Catalog(PlatformAppCodes.Integration, PlatformSeedIds.IntegrationRoot, null, "IntegrationCenter", "/integration", "外部集成", "lucide:plug", 50),
            Menu(PlatformAppCodes.Integration, PlatformSeedIds.IntegrationConnectors, PlatformSeedIds.IntegrationRoot, "IntegrationConnectors", "/integration/connectors", "/integration/connectors/index", "连接器配置", "lucide:unplug", 10, permissionCode: "Integration.Connector.View"),
            Menu(PlatformAppCodes.Integration, PlatformSeedIds.IntegrationBindings, PlatformSeedIds.IntegrationRoot, "IntegrationBindings", "/integration/bindings", "/integration/bindings/index", "账号绑定", "lucide:link-2", 20, permissionCode: "Integration.Binding.View"),
            Menu(PlatformAppCodes.Integration, PlatformSeedIds.IntegrationGroups, PlatformSeedIds.IntegrationRoot, "IntegrationGroups", "/integration/groups", "/integration/groups/index", "群与机器人", "lucide:message-circle-code", 30, permissionCode: "Integration.Binding.View"),
            Menu(PlatformAppCodes.Integration, PlatformSeedIds.IntegrationTemplates, PlatformSeedIds.IntegrationRoot, "IntegrationTemplates", "/integration/templates", "/integration/templates/index", "消息模板", "lucide:message-square-code", 40, permissionCode: "Integration.Template.View"),
            Menu(PlatformAppCodes.Integration, PlatformSeedIds.IntegrationRules, PlatformSeedIds.IntegrationRoot, "IntegrationRules", "/integration/rules", "/integration/rules/index", "通知规则", "lucide:workflow", 50, permissionCode: "Integration.Rule.View"),
            Menu(PlatformAppCodes.Integration, PlatformSeedIds.IntegrationSyncJobs, PlatformSeedIds.IntegrationRoot, "IntegrationSyncJobs", "/integration/sync-jobs", "/integration/sync-jobs/index", "同步任务", "lucide:refresh-cw", 60, permissionCode: "Integration.Sync.View"),
            Menu(PlatformAppCodes.Integration, PlatformSeedIds.IntegrationWebhookEvents, PlatformSeedIds.IntegrationRoot, "IntegrationWebhookEvents", "/integration/webhook-events", "/integration/webhook-events/index", "Webhook 事件", "lucide:webhook", 70, permissionCode: "Integration.Webhook.View"),
            Menu(PlatformAppCodes.Integration, PlatformSeedIds.IntegrationDeliveryRecords, PlatformSeedIds.IntegrationRoot, "IntegrationDeliveryRecords", "/integration/delivery-records", "/integration/delivery-records/index", "投递日志", "lucide:send-horizontal", 80, permissionCode: "Integration.Delivery.View"),
            Button(PlatformAppCodes.Integration, PlatformSeedIds.IntegrationConnectorManage, PlatformSeedIds.IntegrationConnectors, "IntegrationConnectorManage", "Integration.Connector.Manage", "管理连接器"),
            Button(PlatformAppCodes.Integration, PlatformSeedIds.IntegrationConnectorRotateSecret, PlatformSeedIds.IntegrationConnectors, "IntegrationConnectorRotateSecret", "Integration.Connector.RotateSecret", "轮换连接器密钥"),
            Button(PlatformAppCodes.Integration, PlatformSeedIds.IntegrationConnectorTest, PlatformSeedIds.IntegrationConnectors, "IntegrationConnectorTest", "Integration.Connector.Test", "测试连接器"),
            Button(PlatformAppCodes.Integration, PlatformSeedIds.IntegrationBindingManage, PlatformSeedIds.IntegrationBindings, "IntegrationBindingManage", "Integration.Binding.Manage", "管理账号绑定"),
            Button(PlatformAppCodes.Integration, PlatformSeedIds.IntegrationBindingImport, PlatformSeedIds.IntegrationBindings, "IntegrationBindingImport", "Integration.Binding.Import", "导入外部账号"),
            Button(PlatformAppCodes.Integration, PlatformSeedIds.IntegrationGroupManage, PlatformSeedIds.IntegrationGroups, "IntegrationGroupManage", "Integration.Binding.Manage", "管理群与机器人"),
            Button(PlatformAppCodes.Integration, PlatformSeedIds.IntegrationTemplateManage, PlatformSeedIds.IntegrationTemplates, "IntegrationTemplateManage", "Integration.Template.Manage", "管理消息模板"),
            Button(PlatformAppCodes.Integration, PlatformSeedIds.IntegrationTemplateTest, PlatformSeedIds.IntegrationTemplates, "IntegrationTemplateTest", "Integration.Template.Test", "预览消息模板"),
            Button(PlatformAppCodes.Integration, PlatformSeedIds.IntegrationRuleManage, PlatformSeedIds.IntegrationRules, "IntegrationRuleManage", "Integration.Rule.Manage", "管理通知规则"),
            Button(PlatformAppCodes.Integration, PlatformSeedIds.IntegrationSyncManage, PlatformSeedIds.IntegrationSyncJobs, "IntegrationSyncManage", "Integration.Sync.Manage", "管理同步任务"),
            Button(PlatformAppCodes.Integration, PlatformSeedIds.IntegrationSyncRun, PlatformSeedIds.IntegrationSyncJobs, "IntegrationSyncRun", "Integration.Sync.Run", "执行同步任务"),
            Button(PlatformAppCodes.Integration, PlatformSeedIds.IntegrationWebhookManage, PlatformSeedIds.IntegrationWebhookEvents, "IntegrationWebhookManage", "Integration.Webhook.Manage", "管理 Webhook 事件"),
            Button(PlatformAppCodes.Integration, PlatformSeedIds.IntegrationDeliveryManage, PlatformSeedIds.IntegrationDeliveryRecords, "IntegrationDeliveryManage", "Integration.Delivery.Manage", "管理失败投递"),
            Button(PlatformAppCodes.Integration, PlatformSeedIds.IntegrationDeliveryExport, PlatformSeedIds.IntegrationDeliveryRecords, "IntegrationDeliveryExport", "Integration.Delivery.Export", "导出投递审计")
        ];
    }

    private static AppMenu Button(string appCode, Guid id, Guid parentId, string name, string permissionCode, string title)
    {
        return new AppMenu(
            id,
            appCode,
            parentId,
            name,
            permissionCode,
            null,
            null,
            PlatformMenuType.Button,
            title,
            null,
            permissionCode,
            null,
            false,
            false,
            true,
            true,
            true,
            false,
            999);
    }

    private static AppMenu Catalog(string appCode, Guid id, Guid? parentId, string name, string path, string title, string icon, int order = 10)
    {
        return new AppMenu(
            id,
            appCode,
            parentId,
            name,
            path,
            "BasicLayout",
            null,
            PlatformMenuType.Catalog,
            title,
            icon,
            null,
            null,
            false,
            true,
            false,
            false,
            false,
            false,
            order);
    }

    private static AppMenu Menu(
        string appCode,
        Guid id,
        Guid parentId,
        string name,
        string path,
        string component,
        string title,
        string icon,
        int order,
        bool affixTab = false,
        string? permissionCode = null)
    {
        return new AppMenu(
            id,
            appCode,
            parentId,
            name,
            path,
            component,
            null,
            PlatformMenuType.Menu,
            title,
            icon,
            permissionCode,
            null,
            affixTab,
            true,
            false,
            false,
            false,
            false,
            order);
    }
}
