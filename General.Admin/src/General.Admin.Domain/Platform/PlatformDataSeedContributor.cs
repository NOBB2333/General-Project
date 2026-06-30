using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement;
using Volo.Abp.TenantManagement;
using Volo.Abp.Uow;
using System.Text.Json;
using ProjectEntity = General.Admin.Project.Project;

namespace General.Admin.Platform;

public partial class PlatformDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private const string DefaultPassword = "1q2w3E*";

    private readonly IGuidGenerator _guidGenerator;
    private readonly IRepository<AppMenu, Guid> _menuRepository;
    private readonly IRepository<AppRoleAuthorization, Guid> _roleAuthorizationRepository;
    private readonly IRepository<AppRoleMenu, Guid> _roleMenuRepository;
    private readonly IRepository<AppTenantAuthorization, Guid> _tenantAuthorizationRepository;
    private readonly IRepository<AppUserProfile, Guid> _userProfileRepository;
    private readonly IRepository<IdentityRole, Guid> _roleRepository;
    private readonly IRepository<OrganizationUnit, Guid> _organizationUnitRepository;
    private readonly IRepository<IdentityUserOrganizationUnit> _userOrganizationUnitRepository;
    private readonly IRepository<ProjectEntity, Guid> _projectRepository;
    private readonly IRepository<ProjectSprint, Guid> _projectSprintRepository;
    private readonly IRepository<ProjectDocument, Guid> _projectDocumentRepository;
    private readonly IRepository<ProjectIssue, Guid> _projectIssueRepository;
    private readonly IRepository<ProjectMember, Guid> _projectMemberRepository;
    private readonly IRepository<ProjectMemberRole, Guid> _projectMemberRoleRepository;
    private readonly IRepository<ProjectMilestone, Guid> _projectMilestoneRepository;
    private readonly IRepository<ProjectRaidItem, Guid> _projectRaidItemRepository;
    private readonly IRepository<ProjectTask, Guid> _projectTaskRepository;
    private readonly IRepository<ProjectTaskParticipant, Guid> _projectTaskParticipantRepository;
    private readonly IRepository<ProjectWorklog, Guid> _projectWorklogRepository;
    private readonly IRepository<BusinessBudgetExecution, Guid> _businessBudgetExecutionRepository;
    private readonly IRepository<BusinessChain, Guid> _businessChainRepository;
    private readonly IRepository<BusinessContract, Guid> _businessContractRepository;
    private readonly IRepository<BusinessForecastHistory, Guid> _businessForecastHistoryRepository;
    private readonly IRepository<BusinessProcurement, Guid> _businessProcurementRepository;
    private readonly IRepository<BusinessReceivable, Guid> _businessReceivableRepository;
    private readonly IRepository<AppDictData, Guid> _dictDataRepository;
    private readonly IRepository<AppDictType, Guid> _dictTypeRepository;
    private readonly IRepository<AppFileStorageSource, Guid> _fileStorageSourceRepository;
    private readonly IRepository<PlatformScheduledJob, Guid> _platformScheduledJobRepository;
    private readonly IRepository<PlatformScheduledJobTrigger, Guid> _platformScheduledJobTriggerRepository;
    private readonly IRepository<AiJob, Guid> _aiJobRepository;
    private readonly IRepository<PermissionGrant, Guid> _permissionGrantRepository;
    private readonly IPlatformSeedTenantRepairService _seedTenantRepairService;
    private readonly IdentityRoleManager _roleManager;
    private readonly IdentityUserManager _userManager;
    private readonly OrganizationUnitManager _organizationUnitManager;
    private readonly ICurrentTenant _currentTenant;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly TenantManager _tenantManager;
    private readonly ITenantRepository _tenantRepository;

    public PlatformDataSeedContributor(
        IGuidGenerator guidGenerator,
        IRepository<AppMenu, Guid> menuRepository,
        IRepository<AppRoleAuthorization, Guid> roleAuthorizationRepository,
        IRepository<AppRoleMenu, Guid> roleMenuRepository,
        IRepository<AppTenantAuthorization, Guid> tenantAuthorizationRepository,
        IRepository<AppUserProfile, Guid> userProfileRepository,
        IRepository<IdentityRole, Guid> roleRepository,
        IRepository<OrganizationUnit, Guid> organizationUnitRepository,
        IRepository<IdentityUserOrganizationUnit> userOrganizationUnitRepository,
        IRepository<ProjectEntity, Guid> projectRepository,
        IRepository<ProjectSprint, Guid> projectSprintRepository,
        IRepository<ProjectDocument, Guid> projectDocumentRepository,
        IRepository<ProjectIssue, Guid> projectIssueRepository,
        IRepository<ProjectMember, Guid> projectMemberRepository,
        IRepository<ProjectMemberRole, Guid> projectMemberRoleRepository,
        IRepository<ProjectMilestone, Guid> projectMilestoneRepository,
        IRepository<ProjectRaidItem, Guid> projectRaidItemRepository,
        IRepository<ProjectTask, Guid> projectTaskRepository,
        IRepository<ProjectTaskParticipant, Guid> projectTaskParticipantRepository,
        IRepository<ProjectWorklog, Guid> projectWorklogRepository,
        IRepository<BusinessBudgetExecution, Guid> businessBudgetExecutionRepository,
        IRepository<BusinessChain, Guid> businessChainRepository,
        IRepository<BusinessContract, Guid> businessContractRepository,
        IRepository<BusinessForecastHistory, Guid> businessForecastHistoryRepository,
        IRepository<BusinessProcurement, Guid> businessProcurementRepository,
        IRepository<BusinessReceivable, Guid> businessReceivableRepository,
        IRepository<AppDictData, Guid> dictDataRepository,
        IRepository<AppDictType, Guid> dictTypeRepository,
        IRepository<AppFileStorageSource, Guid> fileStorageSourceRepository,
        IRepository<PlatformScheduledJob, Guid> platformScheduledJobRepository,
        IRepository<PlatformScheduledJobTrigger, Guid> platformScheduledJobTriggerRepository,
        IRepository<AiJob, Guid> aiJobRepository,
        IRepository<PermissionGrant, Guid> permissionGrantRepository,
        IPlatformSeedTenantRepairService seedTenantRepairService,
        IdentityRoleManager roleManager,
        IdentityUserManager userManager,
        OrganizationUnitManager organizationUnitManager,
        ICurrentTenant currentTenant,
        IUnitOfWorkManager unitOfWorkManager,
        TenantManager tenantManager,
        ITenantRepository tenantRepository)
    {
        _guidGenerator = guidGenerator;
        _menuRepository = menuRepository;
        _roleAuthorizationRepository = roleAuthorizationRepository;
        _roleMenuRepository = roleMenuRepository;
        _tenantAuthorizationRepository = tenantAuthorizationRepository;
        _userProfileRepository = userProfileRepository;
        _roleRepository = roleRepository;
        _organizationUnitRepository = organizationUnitRepository;
        _userOrganizationUnitRepository = userOrganizationUnitRepository;
        _projectRepository = projectRepository;
        _projectSprintRepository = projectSprintRepository;
        _projectDocumentRepository = projectDocumentRepository;
        _projectIssueRepository = projectIssueRepository;
        _projectMemberRepository = projectMemberRepository;
        _projectMemberRoleRepository = projectMemberRoleRepository;
        _projectMilestoneRepository = projectMilestoneRepository;
        _projectRaidItemRepository = projectRaidItemRepository;
        _projectTaskRepository = projectTaskRepository;
        _projectTaskParticipantRepository = projectTaskParticipantRepository;
        _projectWorklogRepository = projectWorklogRepository;
        _businessBudgetExecutionRepository = businessBudgetExecutionRepository;
        _businessChainRepository = businessChainRepository;
        _businessContractRepository = businessContractRepository;
        _businessForecastHistoryRepository = businessForecastHistoryRepository;
        _businessProcurementRepository = businessProcurementRepository;
        _businessReceivableRepository = businessReceivableRepository;
        _dictDataRepository = dictDataRepository;
        _dictTypeRepository = dictTypeRepository;
        _fileStorageSourceRepository = fileStorageSourceRepository;
        _platformScheduledJobRepository = platformScheduledJobRepository;
        _platformScheduledJobTriggerRepository = platformScheduledJobTriggerRepository;
        _aiJobRepository = aiJobRepository;
        _permissionGrantRepository = permissionGrantRepository;
        _seedTenantRepairService = seedTenantRepairService;
        _roleManager = roleManager;
        _userManager = userManager;
        _organizationUnitManager = organizationUnitManager;
        _currentTenant = currentTenant;
        _unitOfWorkManager = unitOfWorkManager;
        _tenantManager = tenantManager;
        _tenantRepository = tenantRepository;
    }

    [UnitOfWork]
    public async Task SeedAsync(DataSeedContext context)
    {
        if (context.TenantId.HasValue)
        {
            return;
        }

        var defaultTenant = await SeedDefaultTenantAsync();
        await SeedRolesAsync();
        await SeedDictionariesAsync();
        await SeedFileStorageSourcesAsync();
        await SeedPlatformScheduledJobsAsync();
        await SeedMenusAsync();
        await SeedRoleMenusAsync();
        await RepairDefaultTenantSeedDataAsync(defaultTenant);
        Guid defaultTenantAdminId;
        using (_currentTenant.Change(defaultTenant.Id, defaultTenant.Name))
        {
            await SeedRolesAsync();
            defaultTenantAdminId = await SeedDefaultTenantUserAsync(defaultTenant);
            await SeedRoleAuthorizationsAsync();
            await SeedRoleMenusAsync();
            await SeedOrganizationUnitsAsync();
            await SeedUsersAsync();
            await SeedProjectExecutionDataAsync();
            await SeedBusinessManagementDataAsync();
            await SeedAiAutomationJobsAsync();
        }

        await SeedDefaultTenantAuthorizationAsync(defaultTenant, defaultTenantAdminId);
        await SeedDefaultTenantAdminRoleMenusAsync(defaultTenant, await _menuRepository.GetListAsync());
        await SeedRolesAsync();
        await SeedHostAdminUserAsync();
        await SeedRoleMenusAsync();
    }

    private async Task<Tenant> SeedDefaultTenantAsync()
    {
        var existingTenant = (await _tenantRepository.GetListAsync())
            .FirstOrDefault(x => x.Name.Equals(PlatformSeedIds.DefaultTenantName, StringComparison.OrdinalIgnoreCase));

        if (existingTenant != null)
        {
            return existingTenant;
        }

        var tenant = await _tenantManager.CreateAsync(PlatformSeedIds.DefaultTenantName);
        await _tenantRepository.InsertAsync(tenant, autoSave: true);
        return tenant;
    }

    private async Task SeedRolesAsync()
    {
        foreach (var roleName in PlatformRoleNames.All)
        {
            if (await _roleManager.FindByNameAsync(roleName) != null)
            {
                continue;
            }

            var role = new IdentityRole(_guidGenerator.Create(), roleName, _currentTenant.Id);
            EnsureSucceeded(await _roleManager.CreateAsync(role));
        }
    }

    private async Task SeedOrganizationUnitsAsync()
    {
        var seedOrganizationUnits = new (Guid Id, string DisplayName, Guid? ParentId)[]
        {
            (PlatformSeedIds.CompanyRoot, "公司总部", null),
            (PlatformSeedIds.PmoCenter, "PMO 中心", PlatformSeedIds.CompanyRoot),
            (PlatformSeedIds.RndCenter, "研发中心", PlatformSeedIds.CompanyRoot),
            (PlatformSeedIds.SoftwareDept, "软件一部", PlatformSeedIds.RndCenter),
            (PlatformSeedIds.HardwareDept, "硬件一部", PlatformSeedIds.RndCenter),
            (PlatformSeedIds.DeliveryCenter, "交付中心", PlatformSeedIds.CompanyRoot),
            (PlatformSeedIds.PreSalesDept, "售前部", PlatformSeedIds.DeliveryCenter),
            (PlatformSeedIds.BusinessDept, "商务部", PlatformSeedIds.DeliveryCenter),
            (PlatformSeedIds.AdminCenter, "行政中心", PlatformSeedIds.CompanyRoot),
            (PlatformSeedIds.HrDept, "人力资源部", PlatformSeedIds.AdminCenter),
            (PlatformSeedIds.LegalDept, "法务部", PlatformSeedIds.AdminCenter),
            (PlatformSeedIds.FinanceDept, "财务部", PlatformSeedIds.AdminCenter)
        };

        foreach (var item in seedOrganizationUnits)
        {
            await EnsureOrganizationUnitAsync(item.Id, item.DisplayName, item.ParentId);
        }
    }

    private async Task SeedUsersAsync()
    {
        await SeedUserAsync(Guid.Parse("AB6C716B-905E-48B6-A23D-982846A13BF5"), "CEO", "CEO", "CEO@qq.com", PlatformRoleNames.Pmo, PlatformSeedIds.PmoCenter, "16621778888");
        await SeedUserAsync(Guid.Parse("3A20D747-55D4-78F6-7109-F9242277138C"), "pmo.demo", "PMO 负责人", "pmo.demo@general.local", PlatformRoleNames.Pmo, PlatformSeedIds.PmoCenter);
        await SeedUserAsync(Guid.Parse("913ED331-C5B2-48E3-9FAC-798741FC4474"), "yanfazongjian", "研发总监", "yanfazongjian@qq.com", PlatformRoleNames.Pm, PlatformSeedIds.RndCenter, "18821775555");
        await SeedUserAsync(Guid.Parse("3A20D747-5621-1DC5-01DE-7CE5B4648787"), "pm.demo", "项目经理", "pm.demo@general.local", PlatformRoleNames.Pm, PlatformSeedIds.SoftwareDept);
        await SeedUserAsync(Guid.Parse("2F7BBD26-5484-4AF5-874D-7A23D25D2FF0"), "ruanjian_chanpin1", "软件-产品1", "ruanjian_chanpin1@qq.com", PlatformRoleNames.Member, PlatformSeedIds.SoftwareDept, "18821775099");
        await SeedUserAsync(Guid.Parse("F42FAFAE-178A-4753-BFE1-79FF9CB70EC9"), "ruanjian_chanpin2", "软件-产品2", "ruanjian_chanpin2@qq.com", PlatformRoleNames.Member, PlatformSeedIds.SoftwareDept, "17721880000");
        await SeedUserAsync(Guid.Parse("26D40296-53FA-4FA5-8321-E2069CB3462D"), "ruanjian-suanfa1", "软件-算法1", "ruanjian-suanfa1@qq.com", PlatformRoleNames.Member, PlatformSeedIds.SoftwareDept, "18821667777");
        await SeedUserAsync(Guid.Parse("28057D1B-513F-4A2F-92F1-5BE88948EAE2"), "ruanjian-yanfa1", "软件-研发1", "ruanjian-yanfa2@qq.com", PlatformRoleNames.Member, PlatformSeedIds.SoftwareDept, "18821776666");
        await SeedUserAsync(Guid.Parse("44BB3449-9374-477E-B5F4-0C7170FEC1D6"), "ruanjian-yanfa2", "软件-研发2", "ruanjian-channpin2@qq.com", PlatformRoleNames.Member, PlatformSeedIds.SoftwareDept, "17721889999");
        await SeedUserAsync(Guid.Parse("B6CC364B-2251-4867-A64D-0AF40CF31CB0"), "ruanjian-ceshi1", "软件-测试1", "ruanjian-ceshi1@qq.com", PlatformRoleNames.Member, PlatformSeedIds.SoftwareDept, "17721889999");
        await SeedUserAsync(Guid.Parse("5F7B6BA9-84A5-4E32-9538-1DD7B7B5E1F6"), "ruanjian-yunwei1", "软件-运维1", "ruanjian-yunwei1@qq.com", PlatformRoleNames.Member, PlatformSeedIds.SoftwareDept, "18821776666");
        await SeedUserAsync(Guid.Parse("B0BF606F-4EBB-4984-90DB-B152A0BDA3FD"), "yingjian-jiegou1", "硬件-结构1", "yingjian-jiegou1@qq.com", PlatformRoleNames.Member, PlatformSeedIds.HardwareDept, "18821776666");
        await SeedUserAsync(Guid.Parse("3A20D747-5654-BABA-C2BF-504368B3652C"), "shouqian1", "售前1", "shouqian1@general.local", PlatformRoleNames.Member, PlatformSeedIds.PreSalesDept, "19921776666");
        await SeedUserAsync(Guid.Parse("3A20DB5A-77E9-5B3D-3005-B5FFF8F5A793"), "member.demo", "项目成员", "member.demo@general.local", PlatformRoleNames.Member, PlatformSeedIds.PreSalesDept);
        await SeedUserAsync(Guid.Parse("3A20D747-5689-9C72-3A86-5F1BAC103070"), "viewer.demo", "经营查看", "viewer.demo@general.local", PlatformRoleNames.Viewer, PlatformSeedIds.FinanceDept);
    }

    private async Task RepairDefaultTenantSeedDataAsync(Tenant defaultTenant)
    {
        await _seedTenantRepairService.RepairDefaultTenantSeedDataAsync(
            defaultTenant.Id,
            GetSeedUserIds(),
            GetSeedUserNames(),
            GetSeedOrganizationUnitIds(),
            GetSeedOrganizationUnitNames(),
            GetDefaultTenantSeedRoleNames());
    }

    private async Task SeedRoleAuthorizationsAsync()
    {
        var currentTenantId = _currentTenant.Id;
        var roles = (await _roleRepository.GetListAsync())
            .Where(x => x.TenantId == currentTenantId && PlatformRoleNames.All.Contains(x.Name))
            .ToList();

        foreach (var role in roles)
        {
            var defaults = ResolveDefaultRoleAuthorization(role.Name);
            var authorization = await _roleAuthorizationRepository.FindAsync(x => x.RoleId == role.Id);
            if (authorization != null)
            {
                continue;
            }

            await _roleAuthorizationRepository.InsertAsync(
                new AppRoleAuthorization(
                    _guidGenerator.Create(),
                    role.Id,
                    defaults.DataScopeMode,
                    defaults.AccountScopeMode,
                    "[]",
                    "[]",
                    "[]"),
                autoSave: true);
        }
    }

    private static (string AccountScopeMode, string DataScopeMode) ResolveDefaultRoleAuthorization(string roleName)
    {
        return roleName switch
        {
            PlatformRoleNames.Admin => (
                PlatformAuthorizationDefaults.AccountScopeAll,
                PlatformAuthorizationDefaults.DataScopeAll),
            PlatformRoleNames.Pmo => (
                PlatformAuthorizationDefaults.AccountScopeAll,
                PlatformAuthorizationDefaults.DataScopeAll),
            PlatformRoleNames.Pm => (
                PlatformAuthorizationDefaults.AccountScopeDataAndUsers,
                PlatformAuthorizationDefaults.DataScopeCurrentOrganizationAndDescendants),
            PlatformRoleNames.Member => (
                PlatformAuthorizationDefaults.AccountScopeOnlyUsers,
                PlatformAuthorizationDefaults.DataScopeSelf),
            PlatformRoleNames.Viewer => (
                PlatformAuthorizationDefaults.AccountScopeData,
                PlatformAuthorizationDefaults.DataScopeCurrentOrganizationAndDescendants),
            _ => (
                PlatformAuthorizationDefaults.AccountScopeDataAndUsers,
                PlatformAuthorizationDefaults.DataScopeCurrentOrganizationAndDescendants)
        };
    }

    private static Guid[] GetSeedOrganizationUnitIds()
    {
        return
        [
            PlatformSeedIds.CompanyRoot,
            PlatformSeedIds.PmoCenter,
            PlatformSeedIds.RndCenter,
            PlatformSeedIds.SoftwareDept,
            PlatformSeedIds.HardwareDept,
            PlatformSeedIds.DeliveryCenter,
            PlatformSeedIds.PreSalesDept,
            PlatformSeedIds.BusinessDept,
            PlatformSeedIds.AdminCenter,
            PlatformSeedIds.HrDept,
            PlatformSeedIds.LegalDept,
            PlatformSeedIds.FinanceDept
        ];
    }

    private static string[] GetSeedOrganizationUnitNames()
    {
        return
        [
            "公司总部",
            "PMO 中心",
            "研发中心",
            "软件一部",
            "硬件一部",
            "交付中心",
            "售前部",
            "商务部",
            "行政中心",
            "人力资源部",
            "法务部",
            "财务部"
        ];
    }

    private static Guid[] GetSeedUserIds()
    {
        return
        [
            Guid.Parse("AB6C716B-905E-48B6-A23D-982846A13BF5"),
            Guid.Parse("3A20D747-55D4-78F6-7109-F9242277138C"),
            Guid.Parse("913ED331-C5B2-48E3-9FAC-798741FC4474"),
            Guid.Parse("3A20D747-5621-1DC5-01DE-7CE5B4648787"),
            Guid.Parse("2F7BBD26-5484-4AF5-874D-7A23D25D2FF0"),
            Guid.Parse("F42FAFAE-178A-4753-BFE1-79FF9CB70EC9"),
            Guid.Parse("26D40296-53FA-4FA5-8321-E2069CB3462D"),
            Guid.Parse("28057D1B-513F-4A2F-92F1-5BE88948EAE2"),
            Guid.Parse("44BB3449-9374-477E-B5F4-0C7170FEC1D6"),
            Guid.Parse("B6CC364B-2251-4867-A64D-0AF40CF31CB0"),
            Guid.Parse("5F7B6BA9-84A5-4E32-9538-1DD7B7B5E1F6"),
            Guid.Parse("B0BF606F-4EBB-4984-90DB-B152A0BDA3FD"),
            Guid.Parse("3A20D747-5654-BABA-C2BF-504368B3652C"),
            Guid.Parse("3A20DB5A-77E9-5B3D-3005-B5FFF8F5A793"),
            Guid.Parse("3A20D747-5689-9C72-3A86-5F1BAC103070")
        ];
    }

    private static string[] GetSeedUserNames()
    {
        return
        [
            "CEO",
            "pmo.demo",
            "yanfazongjian",
            "pm.demo",
            "ruanjian_chanpin1",
            "ruanjian_chanpin2",
            "ruanjian-suanfa1",
            "ruanjian-yanfa1",
            "ruanjian-yanfa2",
            "ruanjian-ceshi1",
            "ruanjian-yunwei1",
            "yingjian-jiegou1",
            "shouqian1",
            "member.demo",
            "viewer.demo"
        ];
    }

    private static string[] GetDefaultTenantSeedRoleNames()
    {
        return
        [
            PlatformRoleNames.Pmo,
            PlatformRoleNames.Pm,
            PlatformRoleNames.Member,
            PlatformRoleNames.Viewer
        ];
    }

    private async Task<Guid> SeedDefaultTenantUserAsync(Tenant tenant)
    {
        using (_currentTenant.Change(tenant.Id, tenant.Name))
        {
            var role = await _roleManager.FindByNameAsync(PlatformRoleNames.Admin);
            if (role == null)
            {
                role = new IdentityRole(_guidGenerator.Create(), PlatformRoleNames.Admin, tenant.Id);
                EnsureSucceeded(await _roleManager.CreateAsync(role));
                await SaveSeedChangesAsync();
            }

            var existingUser = await _userManager.FindByNameAsync("tenant.admin");
            if (existingUser != null)
            {
                return existingUser.Id;
            }

            var user = new IdentityUser(_guidGenerator.Create(), "tenant.admin", "tenant.admin@general.local", tenant.Id)
            {
                Name = "默认租户管理员",
                Surname = string.Empty
            };
            user.AddRole(role.Id);
            EnsureSucceeded(await _userManager.CreateAsync(user, DefaultPassword));
            return user.Id;
        }
    }

    private async Task SeedHostAdminUserAsync()
    {
        var role = await _roleManager.FindByNameAsync(PlatformRoleNames.Admin);
        if (role == null)
        {
            role = new IdentityRole(_guidGenerator.Create(), PlatformRoleNames.Admin);
            EnsureSucceeded(await _roleManager.CreateAsync(role));
            await SaveSeedChangesAsync();
        }

        var existingUser = await _userManager.FindByNameAsync("host.admin");
        if (existingUser != null)
        {
            if (!await _userManager.IsInRoleAsync(existingUser, PlatformRoleNames.Admin))
            {
                EnsureSucceeded(await _userManager.AddToRoleAsync(existingUser, PlatformRoleNames.Admin));
            }

            return;
        }

        var user = new IdentityUser(_guidGenerator.Create(), "host.admin", "host.admin@general.local")
        {
            Name = "超级管理员",
            Surname = string.Empty
        };
        user.AddRole(role.Id);
        EnsureSucceeded(await _userManager.CreateAsync(user, DefaultPassword));
    }

    private async Task SeedDefaultTenantAuthorizationAsync(Tenant tenant, Guid adminUserId)
    {
        var menuIds = (await _menuRepository.GetListAsync())
            .Where(x => x.IsEnabled)
            .Select(x => x.Id)
            .Distinct()
            .OrderBy(x => x)
            .ToList();
        var serializedMenuIds = JsonSerializer.Serialize(menuIds);
        var authorization = await _tenantAuthorizationRepository.FindAsync(x => x.TenantId == tenant.Id);

        if (authorization == null)
        {
            await _tenantAuthorizationRepository.InsertAsync(
                new AppTenantAuthorization(
                    _guidGenerator.Create(),
                    tenant.Id,
                    serializedMenuIds,
                    "[]",
                    true,
                    adminUserId,
                    "默认租户初始化授权"),
                autoSave: true);
            return;
        }

        authorization.Update(
            string.IsNullOrWhiteSpace(authorization.MenuIds) || authorization.MenuIds == "[]" ? serializedMenuIds : authorization.MenuIds,
            authorization.ApiBlacklist,
            authorization.IsActive,
            authorization.AdminUserId ?? adminUserId,
            authorization.Remark);
        await _tenantAuthorizationRepository.UpdateAsync(authorization, autoSave: true);
    }

    private async Task EnsureOrganizationUnitAsync(Guid id, string displayName, Guid? parentId)
    {
        var organizationUnit = await _organizationUnitRepository.FindAsync(id);
        if (organizationUnit == null)
        {
            organizationUnit = new OrganizationUnit(id, displayName, parentId, _currentTenant.Id);
            await _organizationUnitManager.CreateAsync(organizationUnit);
            await SaveSeedChangesAsync();
            return;
        }

        var changed = false;
        if (!string.Equals(organizationUnit.DisplayName, displayName, StringComparison.Ordinal))
        {
            organizationUnit.DisplayName = displayName;
            changed = true;
        }

        if (organizationUnit.ParentId != parentId)
        {
            await _organizationUnitManager.MoveAsync(id, parentId);
            changed = false;
        }

        if (changed)
        {
            await _organizationUnitManager.UpdateAsync(organizationUnit);
            await SaveSeedChangesAsync();
        }
    }

    private async Task SeedUserAsync(
        Guid id,
        string userName,
        string displayName,
        string email,
        string roleName,
        Guid organizationUnitId,
        string? phoneNumber = null,
        string? employeeNo = null)
    {
        var existingUser = await _userManager.FindByNameAsync(userName);
        if (existingUser == null)
        {
            existingUser = new IdentityUser(id, userName, email, _currentTenant.Id)
            {
                Name = displayName,
                Surname = string.Empty
            };

            existingUser.SetIsActive(true);
            existingUser.SetPhoneNumber(phoneNumber, false);
            EnsureSucceeded(await _userManager.CreateAsync(existingUser, DefaultPassword));
        }
        else
        {
            existingUser.Name = displayName;
            existingUser.Surname = string.Empty;
            existingUser.SetEmailWithoutValidation(email, email.ToUpperInvariant());
            existingUser.SetPhoneNumber(phoneNumber, false);
            existingUser.SetIsActive(true);
            EnsureSucceeded(await _userManager.UpdateAsync(existingUser));
        }

        if (!await _userManager.IsInRoleAsync(existingUser, roleName))
        {
            EnsureSucceeded(await _userManager.AddToRoleAsync(existingUser, roleName));
        }

        await SaveSeedChangesAsync();
        await EnsureUserOrganizationUnitAsync(existingUser.Id, organizationUnitId);
        await UpsertSeedUserProfileAsync(existingUser.Id, employeeNo, phoneNumber);
    }

    private async Task EnsureUserOrganizationUnitAsync(Guid userId, Guid organizationUnitId)
    {
        if (!await _organizationUnitRepository.AnyAsync(x => x.Id == organizationUnitId))
        {
            throw new InvalidOperationException($"OrganizationUnit {organizationUnitId} not found in database.");
        }

        var exists = await _userOrganizationUnitRepository.AnyAsync(x =>
            x.UserId == userId &&
            x.OrganizationUnitId == organizationUnitId);
        if (exists)
        {
            return;
        }

        await _userOrganizationUnitRepository.InsertAsync(
            new IdentityUserOrganizationUnit(userId, organizationUnitId, _currentTenant.Id),
            autoSave: true);
    }

    private async Task UpsertSeedUserProfileAsync(Guid userId, string? employeeNo, string? phoneNumber)
    {
        var profile = await _userProfileRepository.FindAsync(x => x.UserId == userId);
        if (profile == null)
        {
            await _userProfileRepository.InsertAsync(
                new AppUserProfile(
                    _guidGenerator.Create(),
                    userId,
                    employeeNo,
                    phoneNumber,
                    null,
                    null,
                    null),
                autoSave: true);
            return;
        }

        profile.Update(employeeNo, phoneNumber, null, null);
        await _userProfileRepository.UpdateAsync(profile, autoSave: true);
    }

    private async Task SeedProjectExecutionDataAsync()
    {
        var adminUser = await _userManager.FindByNameAsync("tenant.admin");
        var pmoUser = await _userManager.FindByNameAsync("pmo.demo");
        var pmUser = await _userManager.FindByNameAsync("pm.demo");
        var memberUser = await _userManager.FindByNameAsync("member.demo");
        var viewerUser = await _userManager.FindByNameAsync("viewer.demo");

        if (adminUser == null || pmoUser == null || pmUser == null || memberUser == null || viewerUser == null)
        {
            return;
        }

        var projects = (await _projectRepository.GetListAsync())
            .OrderBy(x => x.ProjectCode)
            .ToList();

        if (projects.Count == 0)
        {
            projects =
            [
                new(
                    Guid.Parse("50000000-0000-0000-0000-000000000001"),
                    "PRJ-2026-001",
                    "智慧园区一期建设",
                    "园区一期",
                    "交付类",
                    "市场签约",
                    PlatformSeedIds.PreSalesDept,
                    pmUser.Id,
                    pmoUser.Id,
                    ProjectPriorities.P0,
                    "进行中",
                    new DateTime(2026, 4, 1),
                    new DateTime(2026, 6, 30),
                    true,
                    "一期聚焦现场交付、平台联调和验收准备。",
                    1500000m,
                    1800000m,
                    450000m),
                new(
                    Guid.Parse("50000000-0000-0000-0000-000000000002"),
                    "PRJ-2026-002",
                    "企业运营中台升级",
                    "运营中台",
                    "研发类",
                    "内部立项",
                    PlatformSeedIds.RndCenter,
                    pmUser.Id,
                    adminUser.Id,
                    ProjectPriorities.P0,
                    "进行中",
                    new DateTime(2026, 3, 20),
                    new DateTime(2026, 7, 15),
                    true,
                    "涉及组织权限、菜单鉴权和审计能力补齐。",
                    900000m,
                    0m,
                    0m),
                new(
                    Guid.Parse("50000000-0000-0000-0000-000000000003"),
                    "PRJ-2026-003",
                    "财务共享平台联调",
                    "财务联调",
                    "协同类",
                    "客户增补",
                    PlatformSeedIds.FinanceDept,
                    pmoUser.Id,
                    viewerUser.Id,
                    ProjectPriorities.P1,
                    "待规划",
                    new DateTime(2026, 4, 10),
                    new DateTime(2026, 8, 15),
                    false,
                    "一期先完成接口梳理与验收计划编排。",
                    600000m,
                    820000m,
                    120000m)
            ];

            await _projectRepository.InsertManyAsync(projects, autoSave: true);
        }

        if (!await _projectMilestoneRepository.AnyAsync())
        {
            await _projectMilestoneRepository.InsertManyAsync(
            [
                new(Guid.Parse("51000000-0000-0000-0000-000000000001"), projects[0].Id, "完成蓝图评审", pmoUser.Id, new DateTime(2026, 4, 18), "进行中"),
                new(Guid.Parse("51000000-0000-0000-0000-000000000002"), projects[0].Id, "完成现场联调", pmUser.Id, new DateTime(2026, 4, 25), "进行中"),
                new(Guid.Parse("51000000-0000-0000-0000-000000000003"), projects[1].Id, "完成权限模型切换", adminUser.Id, new DateTime(2026, 4, 16), "进行中"),
                new(Guid.Parse("51000000-0000-0000-0000-000000000004"), projects[1].Id, "完成项目页联调", pmUser.Id, new DateTime(2026, 4, 22), "未开始"),
                new(Guid.Parse("51000000-0000-0000-0000-000000000005"), projects[2].Id, "完成接口清单确认", viewerUser.Id, new DateTime(2026, 4, 20), "未开始")
            ], autoSave: true);
        }

        if (!await _projectTaskRepository.AnyAsync())
        {
            await _projectTaskRepository.InsertManyAsync(
            [
                new(Guid.Parse("52000000-0000-0000-0000-000000000001"), projects[0].Id, PlatformSeedIds.PreSalesDept, "TASK-001", "梳理现场交付清单", pmUser.Id, "进行中", "P0", new DateTime(2026, 4, 11), new DateTime(2026, 4, 15), estimatedWorkHours: 12, contractClause: "合同 3.2 现场联调条款", productOwnerName: "PMO 负责人", developerOwnerName: "项目成员", testerOwnerName: "项目成员", productOwnerUserId: pmoUser.Id, developerOwnerUserId: memberUser.Id, testerOwnerUserId: memberUser.Id, description: "梳理现场联调、设备清单、验收前置条件和责任分工，形成可执行的交付清单。"),
                new(Guid.Parse("52000000-0000-0000-0000-000000000002"), projects[0].Id, PlatformSeedIds.PreSalesDept, "TASK-002", "输出验收前检查项", memberUser.Id, "阻塞", "P0", new DateTime(2026, 4, 12), new DateTime(2026, 4, 14), true, "客户环境账号未开通", 8, contractClause: "合同 4.1 验收准备条款", productOwnerName: "项目经理", developerOwnerName: "项目成员", testerOwnerName: "项目成员", productOwnerUserId: pmUser.Id, developerOwnerUserId: memberUser.Id, testerOwnerUserId: memberUser.Id, description: "按验收模板整理检查项，并标记依赖客户环境账号的阻塞点。"),
                new(Guid.Parse("52000000-0000-0000-0000-000000000003"), projects[0].Id, PlatformSeedIds.PreSalesDept, "TASK-003", "回填项目风险基线", pmUser.Id, "已完成", "P1", new DateTime(2026, 4, 9), new DateTime(2026, 4, 12), estimatedWorkHours: 6, actualWorkHours: 6, actualStartTime: new DateTime(2026, 4, 9), actualEndTime: new DateTime(2026, 4, 11), contractClause: "合同 2.4 项目治理条款", productOwnerName: "PMO 负责人", developerOwnerName: "项目经理", testerOwnerName: "项目成员", productOwnerUserId: pmoUser.Id, developerOwnerUserId: pmUser.Id, testerOwnerUserId: memberUser.Id, description: "将已识别风险回填到项目风险基线，作为后续周报和验收沟通依据。"),
                new(Guid.Parse("52000000-0000-0000-0000-000000000004"), projects[1].Id, PlatformSeedIds.RndCenter, "TASK-004", "补齐租户与审计接口", adminUser.Id, "进行中", "P0", new DateTime(2026, 4, 10), new DateTime(2026, 4, 17), estimatedWorkHours: 16, contractClause: "内部目标 1.1 平台治理", productOwnerName: "项目经理", developerOwnerName: "系统管理员", testerOwnerName: "项目成员", productOwnerUserId: pmUser.Id, developerOwnerUserId: adminUser.Id, testerOwnerUserId: memberUser.Id, description: "补齐租户、审计和数据范围接口，支撑项目详情页权限闭环。"),
                new(Guid.Parse("52000000-0000-0000-0000-000000000005"), projects[1].Id, PlatformSeedIds.RndCenter, "TASK-005", "替换项目工作台占位页", pmUser.Id, "未开始", "P1", new DateTime(2026, 4, 14), new DateTime(2026, 4, 19), estimatedWorkHours: 10, contractClause: "内部目标 1.3 项目经营驾驶舱", productOwnerName: "项目经理", developerOwnerName: "系统管理员", testerOwnerName: "项目成员", productOwnerUserId: pmUser.Id, developerOwnerUserId: adminUser.Id, testerOwnerUserId: memberUser.Id, description: "将历史占位页替换为真实项目经营和执行入口。"),
                new(Guid.Parse("52000000-0000-0000-0000-000000000006"), projects[1].Id, PlatformSeedIds.RndCenter, "TASK-006", "处理文件上传异常", adminUser.Id, "已完成", "P0", new DateTime(2026, 4, 10), new DateTime(2026, 4, 13), estimatedWorkHours: 4, actualWorkHours: 3.5, actualStartTime: new DateTime(2026, 4, 10), actualEndTime: new DateTime(2026, 4, 13), contractClause: "内部目标 1.2 基础能力修复", productOwnerName: "项目经理", developerOwnerName: "系统管理员", testerOwnerName: "项目成员", productOwnerUserId: pmUser.Id, developerOwnerUserId: adminUser.Id, testerOwnerUserId: memberUser.Id, description: "修复业务页面文件上传权限和预览链路，确保章程、验收、报告附件可用。"),
                new(Guid.Parse("52000000-0000-0000-0000-000000000007"), projects[2].Id, PlatformSeedIds.FinanceDept, "TASK-007", "整理回款计划节点", viewerUser.Id, "未开始", "P1", new DateTime(2026, 4, 15), new DateTime(2026, 4, 21), estimatedWorkHours: 6, contractClause: "补充协议 2.1 回款计划", productOwnerName: "PMO 负责人", developerOwnerName: "经营查看", testerOwnerName: "经营查看", productOwnerUserId: pmoUser.Id, developerOwnerUserId: viewerUser.Id, testerOwnerUserId: viewerUser.Id, description: "整理合同回款计划节点，并与项目交付节点建立对应关系。"),
                new(Guid.Parse("52000000-0000-0000-0000-000000000008"), projects[2].Id, PlatformSeedIds.FinanceDept, "TASK-008", "输出联调接口说明", pmoUser.Id, "进行中", "P1", new DateTime(2026, 4, 12), new DateTime(2026, 4, 18), estimatedWorkHours: 8, contractClause: "补充协议 3.5 系统联调", productOwnerName: "PMO 负责人", developerOwnerName: "经营查看", testerOwnerName: "经营查看", productOwnerUserId: pmoUser.Id, developerOwnerUserId: viewerUser.Id, testerOwnerUserId: viewerUser.Id, description: "输出财务共享平台联调接口说明，明确字段、鉴权和验收样例。", isOutOfContract: true)
            ], autoSave: true);
        }

        var taskParticipants = new List<ProjectTaskParticipant>
        {
            new(Guid.Parse("52800000-0000-0000-0000-000000000001"), projects[0].Id, Guid.Parse("52000000-0000-0000-0000-000000000001"), pmoUser.Id, "产品"),
            new(Guid.Parse("52800000-0000-0000-0000-000000000002"), projects[0].Id, Guid.Parse("52000000-0000-0000-0000-000000000001"), pmUser.Id, "产品"),
            new(Guid.Parse("52800000-0000-0000-0000-000000000003"), projects[0].Id, Guid.Parse("52000000-0000-0000-0000-000000000001"), memberUser.Id, "开发"),
            new(Guid.Parse("52800000-0000-0000-0000-000000000004"), projects[0].Id, Guid.Parse("52000000-0000-0000-0000-000000000001"), adminUser.Id, "开发"),
            new(Guid.Parse("52800000-0000-0000-0000-000000000005"), projects[0].Id, Guid.Parse("52000000-0000-0000-0000-000000000001"), memberUser.Id, "测试"),
            new(Guid.Parse("52800000-0000-0000-0000-000000000006"), projects[0].Id, Guid.Parse("52000000-0000-0000-0000-000000000002"), pmUser.Id, "产品"),
            new(Guid.Parse("52800000-0000-0000-0000-000000000007"), projects[0].Id, Guid.Parse("52000000-0000-0000-0000-000000000002"), memberUser.Id, "开发"),
            new(Guid.Parse("52800000-0000-0000-0000-000000000008"), projects[0].Id, Guid.Parse("52000000-0000-0000-0000-000000000002"), viewerUser.Id, "测试"),
            new(Guid.Parse("52800000-0000-0000-0000-000000000013"), projects[0].Id, Guid.Parse("52000000-0000-0000-0000-000000000003"), pmoUser.Id, "产品"),
            new(Guid.Parse("52800000-0000-0000-0000-000000000014"), projects[0].Id, Guid.Parse("52000000-0000-0000-0000-000000000003"), pmUser.Id, "开发"),
            new(Guid.Parse("52800000-0000-0000-0000-000000000015"), projects[0].Id, Guid.Parse("52000000-0000-0000-0000-000000000003"), memberUser.Id, "测试"),
            new(Guid.Parse("52800000-0000-0000-0000-000000000009"), projects[1].Id, Guid.Parse("52000000-0000-0000-0000-000000000004"), pmUser.Id, "产品"),
            new(Guid.Parse("52800000-0000-0000-0000-000000000010"), projects[1].Id, Guid.Parse("52000000-0000-0000-0000-000000000004"), adminUser.Id, "开发"),
            new(Guid.Parse("52800000-0000-0000-0000-000000000011"), projects[1].Id, Guid.Parse("52000000-0000-0000-0000-000000000004"), memberUser.Id, "开发"),
            new(Guid.Parse("52800000-0000-0000-0000-000000000012"), projects[1].Id, Guid.Parse("52000000-0000-0000-0000-000000000004"), memberUser.Id, "测试"),
            new(Guid.Parse("52800000-0000-0000-0000-000000000016"), projects[1].Id, Guid.Parse("52000000-0000-0000-0000-000000000005"), pmUser.Id, "产品"),
            new(Guid.Parse("52800000-0000-0000-0000-000000000017"), projects[1].Id, Guid.Parse("52000000-0000-0000-0000-000000000005"), adminUser.Id, "开发"),
            new(Guid.Parse("52800000-0000-0000-0000-000000000018"), projects[1].Id, Guid.Parse("52000000-0000-0000-0000-000000000005"), memberUser.Id, "测试"),
            new(Guid.Parse("52800000-0000-0000-0000-000000000019"), projects[1].Id, Guid.Parse("52000000-0000-0000-0000-000000000006"), pmUser.Id, "产品"),
            new(Guid.Parse("52800000-0000-0000-0000-000000000020"), projects[1].Id, Guid.Parse("52000000-0000-0000-0000-000000000006"), adminUser.Id, "开发"),
            new(Guid.Parse("52800000-0000-0000-0000-000000000021"), projects[1].Id, Guid.Parse("52000000-0000-0000-0000-000000000006"), memberUser.Id, "测试"),
            new(Guid.Parse("52800000-0000-0000-0000-000000000022"), projects[2].Id, Guid.Parse("52000000-0000-0000-0000-000000000007"), pmoUser.Id, "产品"),
            new(Guid.Parse("52800000-0000-0000-0000-000000000023"), projects[2].Id, Guid.Parse("52000000-0000-0000-0000-000000000007"), viewerUser.Id, "开发"),
            new(Guid.Parse("52800000-0000-0000-0000-000000000024"), projects[2].Id, Guid.Parse("52000000-0000-0000-0000-000000000007"), viewerUser.Id, "测试"),
            new(Guid.Parse("52800000-0000-0000-0000-000000000025"), projects[2].Id, Guid.Parse("52000000-0000-0000-0000-000000000008"), pmoUser.Id, "产品"),
            new(Guid.Parse("52800000-0000-0000-0000-000000000026"), projects[2].Id, Guid.Parse("52000000-0000-0000-0000-000000000008"), viewerUser.Id, "开发"),
            new(Guid.Parse("52800000-0000-0000-0000-000000000027"), projects[2].Id, Guid.Parse("52000000-0000-0000-0000-000000000008"), viewerUser.Id, "测试")
        };
        var existingTaskParticipantKeys = new HashSet<string>(
            (await _projectTaskParticipantRepository.GetListAsync())
                .Select(x => $"{x.TaskId:N}:{x.RoleName}:{x.UserId:N}"),
            StringComparer.OrdinalIgnoreCase);
        var missingTaskParticipants = taskParticipants
            .Where(x => !existingTaskParticipantKeys.Contains($"{x.TaskId:N}:{x.RoleName}:{x.UserId:N}"))
            .ToList();
        if (missingTaskParticipants.Count > 0)
        {
            await _projectTaskParticipantRepository.InsertManyAsync(missingTaskParticipants, autoSave: true);
        }

        if (!await _projectRaidItemRepository.AnyAsync())
        {
            await _projectRaidItemRepository.InsertManyAsync(
            [
                new(Guid.Parse("53000000-0000-0000-0000-000000000001"), projects[0].Id, "风险", "客户现场账号开通滞后", ProjectPriorities.P0, pmUser.Id, "跟踪中", new DateTime(2026, 4, 15)),
                new(Guid.Parse("53000000-0000-0000-0000-000000000002"), projects[0].Id, "问题", "验收模板尚未确认", ProjectPriorities.P1, memberUser.Id, "处理中", new DateTime(2026, 4, 17)),
                new(Guid.Parse("53000000-0000-0000-0000-000000000003"), projects[1].Id, "风险", "前后端权限路由存在残留写死逻辑", ProjectPriorities.P0, adminUser.Id, "跟踪中", new DateTime(2026, 4, 16)),
                new(Guid.Parse("53000000-0000-0000-0000-000000000004"), projects[1].Id, "依赖", "项目立项页等待真实接口模型", ProjectPriorities.P1, pmUser.Id, "已识别", new DateTime(2026, 4, 20)),
                new(Guid.Parse("53000000-0000-0000-0000-000000000005"), projects[2].Id, "假设", "财务系统将在本月开放测试环境", ProjectPriorities.P1, viewerUser.Id, "已登记", new DateTime(2026, 4, 22))
            ], autoSave: true);
        }

        if (!await _projectSprintRepository.AnyAsync())
        {
            await _projectSprintRepository.InsertManyAsync(
            [
                new(Guid.Parse("54000000-0000-0000-0000-000000000001"), projects[0].Id, "SPR-001", "第 1 迭代 · 现场联调", pmUser.Id, new DateTime(2026, 4, 11), new DateTime(2026, 4, 25), "进行中", 55, "现场交付清单、联调和验收准备统一放入本次迭代。"),
                new(Guid.Parse("54000000-0000-0000-0000-000000000002"), projects[0].Id, "SPR-002", "第 2 迭代 · 验收准备", memberUser.Id, new DateTime(2026, 4, 26), new DateTime(2026, 5, 8), "未开始", 0, "集中处理验收标准、检查项和现场阻塞。"),
                new(Guid.Parse("54000000-0000-0000-0000-000000000003"), projects[1].Id, "SPR-001", "第 1 迭代 · 权限模型", adminUser.Id, new DateTime(2026, 4, 8), new DateTime(2026, 4, 19), "进行中", 58, "聚焦菜单鉴权、项目详情联调"),
                new(Guid.Parse("54000000-0000-0000-0000-000000000004"), projects[1].Id, "SPR-002", "第 2 迭代 · 项目详情体验", pmUser.Id, new DateTime(2026, 4, 20), new DateTime(2026, 4, 28), "未开始", 0, "发布一期平台与项目执行区"),
                new(Guid.Parse("54000000-0000-0000-0000-000000000005"), projects[2].Id, "SPR-001", "第 1 迭代 · 财务接口联调", viewerUser.Id, new DateTime(2026, 4, 10), new DateTime(2026, 4, 24), "进行中", 35, "财务共享平台联调前置准备")
            ], autoSave: true);
        }

        await EnsureProjectSprintSeedDataAsync(projects, adminUser.Id, pmoUser.Id, pmUser.Id, memberUser.Id);

        if (!await _projectIssueRepository.AnyAsync())
        {
            await _projectIssueRepository.InsertManyAsync(
            [
                new(Guid.Parse("55000000-0000-0000-0000-000000000001"), projects[0].Id, "缺陷", "现场接口鉴权失败", ProjectPriorities.P0, memberUser.Id, "处理中", new DateTime(2026, 4, 15), "接口权限配置说明", "PMO 负责人", "项目成员", "项目成员", pmoUser.Id, memberUser.Id, memberUser.Id),
                new(Guid.Parse("55000000-0000-0000-0000-000000000002"), projects[0].Id, "问题", "客户验收模板未最终确认", ProjectPriorities.P1, pmUser.Id, "跟踪中", new DateTime(2026, 4, 17), "一期验收范围说明", "项目经理", "项目成员", "项目成员", pmUser.Id, memberUser.Id, memberUser.Id),
                new(Guid.Parse("55000000-0000-0000-0000-000000000003"), projects[1].Id, "缺陷", "暗色主题卡片背景残留白底", ProjectPriorities.P0, adminUser.Id, "处理中", new DateTime(2026, 4, 16), "主题切换验收项", "项目经理", "系统管理员", "项目成员", pmUser.Id, adminUser.Id, memberUser.Id),
                new(Guid.Parse("55000000-0000-0000-0000-000000000004"), projects[1].Id, "问题", "项目详情聚合字段待补齐", ProjectPriorities.P1, pmUser.Id, "已识别", new DateTime(2026, 4, 19), "项目详情字段矩阵", "项目经理", "系统管理员", "项目成员", pmUser.Id, adminUser.Id, memberUser.Id),
                new(Guid.Parse("55000000-0000-0000-0000-000000000005"), projects[2].Id, "问题", "财务测试环境开放时间未确认", ProjectPriorities.P1, viewerUser.Id, "跟踪中", new DateTime(2026, 4, 22), "接口联调计划", "PMO 负责人", "经营查看", "经营查看", pmoUser.Id, viewerUser.Id, viewerUser.Id)
            ], autoSave: true);
        }

        if (!await _projectDocumentRepository.AnyAsync())
        {
            await _projectDocumentRepository.InsertManyAsync(
            [
                new(Guid.Parse("56000000-0000-0000-0000-000000000001"), projects[0].Id, "需求", "一期蓝图说明书", "v1.2", pmoUser.Id, "已发布", "用于交付与验收统一口径"),
                new(Guid.Parse("56000000-0000-0000-0000-000000000002"), projects[0].Id, "会议纪要", "现场联调例会纪要", "v1.0", memberUser.Id, "草稿", "记录每日联调阻塞和推进项"),
                new(Guid.Parse("56000000-0000-0000-0000-000000000003"), projects[1].Id, "方案", "项目权限重构方案", "v1.1", adminUser.Id, "已发布", "菜单鉴权与组织范围设计"),
                new(Guid.Parse("56000000-0000-0000-0000-000000000004"), projects[1].Id, "周报", "一期联调周报", "v1.0", pmUser.Id, "已发布", "对外同步一期进度"),
                new(Guid.Parse("56000000-0000-0000-0000-000000000005"), projects[2].Id, "接口", "财务平台接口清单", "v0.9", viewerUser.Id, "评审中", "对接接口与口径确认")
            ], autoSave: true);
        }

        if (!await _projectMemberRepository.AnyAsync())
        {
            await _projectMemberRepository.InsertManyAsync(
            [
                new(Guid.Parse("57000000-0000-0000-0000-000000000001"), projects[0].Id, pmUser.Id, PlatformSeedIds.PreSalesDept, ["产品"], new DateTime(2026, 4, 1)),
                new(Guid.Parse("57000000-0000-0000-0000-000000000002"), projects[0].Id, memberUser.Id, PlatformSeedIds.PreSalesDept, ["研发"], new DateTime(2026, 4, 2)),
                new(Guid.Parse("57000000-0000-0000-0000-000000000003"), projects[0].Id, viewerUser.Id, PlatformSeedIds.FinanceDept, ["研发"], new DateTime(2026, 4, 8), new DateTime(2026, 4, 18), true),
                new(Guid.Parse("57000000-0000-0000-0000-000000000004"), projects[1].Id, adminUser.Id, PlatformSeedIds.RndCenter, ["研发"], new DateTime(2026, 3, 20)),
                new(Guid.Parse("57000000-0000-0000-0000-000000000005"), projects[1].Id, pmUser.Id, PlatformSeedIds.PreSalesDept, ["产品"], new DateTime(2026, 3, 20)),
                new(Guid.Parse("57000000-0000-0000-0000-000000000006"), projects[1].Id, memberUser.Id, PlatformSeedIds.PreSalesDept, ["研发"], new DateTime(2026, 4, 6)),
                new(Guid.Parse("57000000-0000-0000-0000-000000000007"), projects[2].Id, pmoUser.Id, PlatformSeedIds.PmoCenter, ["产品"], new DateTime(2026, 4, 10)),
                new(Guid.Parse("57000000-0000-0000-0000-000000000008"), projects[2].Id, viewerUser.Id, PlatformSeedIds.FinanceDept, ["研发"], new DateTime(2026, 4, 10))
            ], autoSave: true);
        }

        var week1 = GetWeekStart(new DateTime(2026, 4, 14));
        var week2 = GetWeekStart(new DateTime(2026, 4, 21));
        var week3 = GetWeekStart(new DateTime(2026, 4, 28));
        var week4 = GetWeekStart(new DateTime(2026, 5, 5));
        var allWorklogs = new List<ProjectWorklog>
        {
            // 第1周 4/14 — 产品角色
            new(Guid.Parse("58000000-0000-0000-0000-000000000001"), projects[0].Id, pmUser.Id, week1, new DateTime(2026, 4, 14), 2, "跟进现场交付清单和客户接口权限", Guid.Parse("52000000-0000-0000-0000-000000000001")),
            new(Guid.Parse("58000000-0000-0000-0000-000000000002"), projects[0].Id, memberUser.Id, week1, new DateTime(2026, 4, 14), 4, "联调客户接口并排查鉴权问题", Guid.Parse("52000000-0000-0000-0000-000000000001")),
            new(Guid.Parse("58000000-0000-0000-0000-000000000003"), projects[0].Id, pmUser.Id, week1, new DateTime(2026, 4, 15), 1.5, "与客户确认验收标准细节", Guid.Parse("52000000-0000-0000-0000-000000000002")),
            new(Guid.Parse("58000000-0000-0000-0000-000000000004"), projects[0].Id, memberUser.Id, week1, new DateTime(2026, 4, 15), 6, "修复交付清单导出乱码", Guid.Parse("52000000-0000-0000-0000-000000000002")),
            new(Guid.Parse("58000000-0000-0000-0000-000000000005"), projects[0].Id, pmUser.Id, week1, new DateTime(2026, 4, 16), 1, "编写项目周报并同步干系人", null),
            new(Guid.Parse("58000000-0000-0000-0000-000000000006"), projects[0].Id, memberUser.Id, week1, new DateTime(2026, 4, 17), 3, "补充验收检查项自动化脚本", Guid.Parse("52000000-0000-0000-0000-000000000003")),
            new(Guid.Parse("58000000-0000-0000-0000-000000000007"), projects[1].Id, adminUser.Id, week1, new DateTime(2026, 4, 14), 8, "补齐租户审计接口与权限校验", Guid.Parse("52000000-0000-0000-0000-000000000004")),
            new(Guid.Parse("58000000-0000-0000-0000-000000000008"), projects[1].Id, adminUser.Id, week1, new DateTime(2026, 4, 15), 6, "修复文件上传大小限制异常", Guid.Parse("52000000-0000-0000-0000-000000000006")),
            new(Guid.Parse("58000000-0000-0000-0000-000000000009"), projects[1].Id, pmUser.Id, week1, new DateTime(2026, 4, 15), 2, "梳理 PM 工作台需求清单", Guid.Parse("52000000-0000-0000-0000-000000000005")),
            new(Guid.Parse("58000000-0000-0000-0000-00000000000A"), projects[1].Id, pmUser.Id, week1, new DateTime(2026, 4, 17), 1, "评审研发方案并确认 Sprint", null),
            new(Guid.Parse("58000000-0000-0000-0000-00000000000B"), projects[2].Id, viewerUser.Id, week1, new DateTime(2026, 4, 15), 3, "整理财务接口口径与回款节点", Guid.Parse("52000000-0000-0000-0000-000000000007")),
            new(Guid.Parse("58000000-0000-0000-0000-00000000000C"), projects[2].Id, pmoUser.Id, week1, new DateTime(2026, 4, 16), 2, "协调财务与研发联调 Sprint", Guid.Parse("52000000-0000-0000-0000-000000000008")),
            // 第2周 4/21
            new(Guid.Parse("58000000-0000-0000-0000-000000000010"), projects[0].Id, pmUser.Id, week2, new DateTime(2026, 4, 21), 1.5, "准备验收汇报材料", null),
            new(Guid.Parse("58000000-0000-0000-0000-000000000011"), projects[0].Id, pmUser.Id, week2, new DateTime(2026, 4, 23), 1, "跟进客户反馈问题闭环", Guid.Parse("52000000-0000-0000-0000-000000000002")),
            new(Guid.Parse("58000000-0000-0000-0000-000000000012"), projects[0].Id, memberUser.Id, week2, new DateTime(2026, 4, 21), 8, "完成接口联调并通过冒烟测试", Guid.Parse("52000000-0000-0000-0000-000000000001")),
            new(Guid.Parse("58000000-0000-0000-0000-000000000013"), projects[0].Id, memberUser.Id, week2, new DateTime(2026, 4, 22), 6, "修复数据同步延迟问题", Guid.Parse("52000000-0000-0000-0000-000000000003")),
            new(Guid.Parse("58000000-0000-0000-0000-000000000014"), projects[0].Id, memberUser.Id, week2, new DateTime(2026, 4, 24), 4, "编写单元测试覆盖核心流程", Guid.Parse("52000000-0000-0000-0000-000000000002")),
            new(Guid.Parse("58000000-0000-0000-0000-000000000015"), projects[1].Id, adminUser.Id, week2, new DateTime(2026, 4, 21), 4, "优化查询性能并添加缓存", Guid.Parse("52000000-0000-0000-0000-000000000004")),
            new(Guid.Parse("58000000-0000-0000-0000-000000000016"), projects[1].Id, adminUser.Id, week2, new DateTime(2026, 4, 23), 3, "对接组织架构同步接口", null),
            new(Guid.Parse("58000000-0000-0000-0000-000000000017"), projects[1].Id, pmUser.Id, week2, new DateTime(2026, 4, 22), 2, "验收第一阶段交付物", Guid.Parse("52000000-0000-0000-0000-000000000005")),
            new(Guid.Parse("58000000-0000-0000-0000-000000000018"), projects[2].Id, pmoUser.Id, week2, new DateTime(2026, 4, 21), 3, "输出联调接口文档", Guid.Parse("52000000-0000-0000-0000-000000000008")),
            new(Guid.Parse("58000000-0000-0000-0000-000000000019"), projects[2].Id, viewerUser.Id, week2, new DateTime(2026, 4, 22), 2, "核对回款计划与发票信息", Guid.Parse("52000000-0000-0000-0000-000000000007")),
            // 第3周 4/28
            new(Guid.Parse("58000000-0000-0000-0000-00000000001A"), projects[0].Id, pmUser.Id, week3, new DateTime(2026, 4, 28), 2, "组织验收评审会议", null),
            new(Guid.Parse("58000000-0000-0000-0000-00000000001B"), projects[0].Id, pmUser.Id, week3, new DateTime(2026, 4, 30), 1, "整理验收遗留问题清单", Guid.Parse("52000000-0000-0000-0000-000000000002")),
            new(Guid.Parse("58000000-0000-0000-0000-00000000001C"), projects[0].Id, memberUser.Id, week3, new DateTime(2026, 4, 28), 6, "修复验收评审发现的缺陷", Guid.Parse("52000000-0000-0000-0000-000000000002")),
            new(Guid.Parse("58000000-0000-0000-0000-00000000001D"), projects[0].Id, memberUser.Id, week3, new DateTime(2026, 4, 29), 4, "补充操作手册与部署文档", null),
            new(Guid.Parse("58000000-0000-0000-0000-00000000001E"), projects[1].Id, adminUser.Id, week3, new DateTime(2026, 4, 28), 5, "实现数据导出 Excel 功能", Guid.Parse("52000000-0000-0000-0000-000000000004")),
            new(Guid.Parse("58000000-0000-0000-0000-00000000001F"), projects[1].Id, adminUser.Id, week3, new DateTime(2026, 4, 30), 3, "修复权限越级访问漏洞", Guid.Parse("52000000-0000-0000-0000-000000000006")),
            new(Guid.Parse("58000000-0000-0000-0000-000000000020"), projects[1].Id, pmUser.Id, week3, new DateTime(2026, 4, 29), 1.5, "编写第二阶段需求说明书", Guid.Parse("52000000-0000-0000-0000-000000000005")),
            new(Guid.Parse("58000000-0000-0000-0000-000000000021"), projects[2].Id, pmoUser.Id, week3, new DateTime(2026, 4, 29), 4, "推动财务系统联调", Guid.Parse("52000000-0000-0000-0000-000000000008")),
            new(Guid.Parse("58000000-0000-0000-0000-000000000022"), projects[2].Id, viewerUser.Id, week3, new DateTime(2026, 4, 30), 2, "确认回款到账并更新台账", Guid.Parse("52000000-0000-0000-0000-000000000007")),
            // 第4周 5/5
            new(Guid.Parse("58000000-0000-0000-0000-000000000023"), projects[0].Id, pmUser.Id, week4, new DateTime(2026, 5, 5), 1, "项目结项总结与复盘", null),
            new(Guid.Parse("58000000-0000-0000-0000-000000000024"), projects[0].Id, memberUser.Id, week4, new DateTime(2026, 5, 5), 3, "归档代码与配置到制品库", null),
            new(Guid.Parse("58000000-0000-0000-0000-000000000025"), projects[0].Id, memberUser.Id, week4, new DateTime(2026, 5, 6), 2, "协助运维完成生产部署", null),
            new(Guid.Parse("58000000-0000-0000-0000-000000000026"), projects[1].Id, adminUser.Id, week4, new DateTime(2026, 5, 5), 6, "开发消息通知模块", Guid.Parse("52000000-0000-0000-0000-000000000005")),
            new(Guid.Parse("58000000-0000-0000-0000-000000000027"), projects[1].Id, adminUser.Id, week4, new DateTime(2026, 5, 7), 4, "编写 API 接口文档", null),
            new(Guid.Parse("58000000-0000-0000-0000-000000000028"), projects[1].Id, pmUser.Id, week4, new DateTime(2026, 5, 6), 2, "验收消息通知功能", Guid.Parse("52000000-0000-0000-0000-000000000005")),
        };

        var existingIds = new HashSet<Guid>(
            (await _projectWorklogRepository.GetListAsync()).Select(x => x.Id));
        var toInsert = allWorklogs.Where(x => !existingIds.Contains(x.Id)).ToList();
        if (toInsert.Count > 0)
        {
            await _projectWorklogRepository.InsertManyAsync(toInsert, autoSave: true);
        }
    }

    private async Task SaveSeedChangesAsync()
    {
        if (_unitOfWorkManager.Current != null)
        {
            await _unitOfWorkManager.Current.SaveChangesAsync();
        }
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
