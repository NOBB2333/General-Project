using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.TenantManagement;
using Volo.Abp.Uow;

namespace General.Admin.PhaseOne;

public class PhaseOneDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private const string DefaultPassword = "1q2w3E*";

    private readonly IGuidGenerator _guidGenerator;
    private readonly IRepository<AppMenu, Guid> _menuRepository;
    private readonly IRepository<AppRoleMenu, Guid> _roleMenuRepository;
    private readonly IRepository<IdentityRole, Guid> _roleRepository;
    private readonly IRepository<OrganizationUnit, Guid> _organizationUnitRepository;
    private readonly IRepository<PhaseOneProject, Guid> _projectRepository;
    private readonly IRepository<PhaseOneProjectCycle, Guid> _projectCycleRepository;
    private readonly IRepository<PhaseOneProjectDocument, Guid> _projectDocumentRepository;
    private readonly IRepository<PhaseOneProjectIssue, Guid> _projectIssueRepository;
    private readonly IRepository<PhaseOneProjectMember, Guid> _projectMemberRepository;
    private readonly IRepository<PhaseOneProjectMilestone, Guid> _projectMilestoneRepository;
    private readonly IRepository<PhaseOneProjectRaidItem, Guid> _projectRaidItemRepository;
    private readonly IRepository<PhaseOneProjectTask, Guid> _projectTaskRepository;
    private readonly IRepository<PhaseOneProjectWorklog, Guid> _projectWorklogRepository;
    private readonly IRepository<PhaseOneBusinessBudgetExecution, Guid> _businessBudgetExecutionRepository;
    private readonly IRepository<PhaseOneBusinessChain, Guid> _businessChainRepository;
    private readonly IRepository<PhaseOneBusinessContract, Guid> _businessContractRepository;
    private readonly IRepository<PhaseOneBusinessForecastHistory, Guid> _businessForecastHistoryRepository;
    private readonly IRepository<PhaseOneBusinessProcurement, Guid> _businessProcurementRepository;
    private readonly IRepository<PhaseOneBusinessReceivable, Guid> _businessReceivableRepository;
    private readonly IRepository<PlatformScheduledJob, Guid> _platformScheduledJobRepository;
    private readonly IdentityRoleManager _roleManager;
    private readonly IdentityUserManager _userManager;
    private readonly OrganizationUnitManager _organizationUnitManager;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly TenantManager _tenantManager;
    private readonly ITenantRepository _tenantRepository;

    public PhaseOneDataSeedContributor(
        IGuidGenerator guidGenerator,
        IRepository<AppMenu, Guid> menuRepository,
        IRepository<AppRoleMenu, Guid> roleMenuRepository,
        IRepository<IdentityRole, Guid> roleRepository,
        IRepository<OrganizationUnit, Guid> organizationUnitRepository,
        IRepository<PhaseOneProject, Guid> projectRepository,
        IRepository<PhaseOneProjectCycle, Guid> projectCycleRepository,
        IRepository<PhaseOneProjectDocument, Guid> projectDocumentRepository,
        IRepository<PhaseOneProjectIssue, Guid> projectIssueRepository,
        IRepository<PhaseOneProjectMember, Guid> projectMemberRepository,
        IRepository<PhaseOneProjectMilestone, Guid> projectMilestoneRepository,
        IRepository<PhaseOneProjectRaidItem, Guid> projectRaidItemRepository,
        IRepository<PhaseOneProjectTask, Guid> projectTaskRepository,
        IRepository<PhaseOneProjectWorklog, Guid> projectWorklogRepository,
        IRepository<PhaseOneBusinessBudgetExecution, Guid> businessBudgetExecutionRepository,
        IRepository<PhaseOneBusinessChain, Guid> businessChainRepository,
        IRepository<PhaseOneBusinessContract, Guid> businessContractRepository,
        IRepository<PhaseOneBusinessForecastHistory, Guid> businessForecastHistoryRepository,
        IRepository<PhaseOneBusinessProcurement, Guid> businessProcurementRepository,
        IRepository<PhaseOneBusinessReceivable, Guid> businessReceivableRepository,
        IRepository<PlatformScheduledJob, Guid> platformScheduledJobRepository,
        IdentityRoleManager roleManager,
        IdentityUserManager userManager,
        OrganizationUnitManager organizationUnitManager,
        IUnitOfWorkManager unitOfWorkManager,
        TenantManager tenantManager,
        ITenantRepository tenantRepository)
    {
        _guidGenerator = guidGenerator;
        _menuRepository = menuRepository;
        _roleMenuRepository = roleMenuRepository;
        _roleRepository = roleRepository;
        _organizationUnitRepository = organizationUnitRepository;
        _projectRepository = projectRepository;
        _projectCycleRepository = projectCycleRepository;
        _projectDocumentRepository = projectDocumentRepository;
        _projectIssueRepository = projectIssueRepository;
        _projectMemberRepository = projectMemberRepository;
        _projectMilestoneRepository = projectMilestoneRepository;
        _projectRaidItemRepository = projectRaidItemRepository;
        _projectTaskRepository = projectTaskRepository;
        _projectWorklogRepository = projectWorklogRepository;
        _businessBudgetExecutionRepository = businessBudgetExecutionRepository;
        _businessChainRepository = businessChainRepository;
        _businessContractRepository = businessContractRepository;
        _businessForecastHistoryRepository = businessForecastHistoryRepository;
        _businessProcurementRepository = businessProcurementRepository;
        _businessReceivableRepository = businessReceivableRepository;
        _platformScheduledJobRepository = platformScheduledJobRepository;
        _roleManager = roleManager;
        _userManager = userManager;
        _organizationUnitManager = organizationUnitManager;
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

        await SeedDefaultTenantAsync();
        await SeedRolesAsync();
        await SeedOrganizationUnitsAsync();
        await SeedUsersAsync();
        await SeedProjectExecutionDataAsync();
        await SeedBusinessManagementDataAsync();
        await SeedPlatformScheduledJobsAsync();
        await SeedMenusAsync();
        await SeedRoleMenusAsync();
    }

    private async Task SeedDefaultTenantAsync()
    {
        var existingTenant = (await _tenantRepository.GetListAsync())
            .FirstOrDefault(x => x.Name.Equals(PhaseOneSeedIds.DefaultTenantName, StringComparison.OrdinalIgnoreCase));

        if (existingTenant != null)
        {
            return;
        }

        var tenant = await _tenantManager.CreateAsync(PhaseOneSeedIds.DefaultTenantName);
        await _tenantRepository.InsertAsync(tenant, autoSave: true);
    }

    private async Task SeedRolesAsync()
    {
        foreach (var roleName in PhaseOneRoleNames.All)
        {
            if (await _roleManager.FindByNameAsync(roleName) != null)
            {
                continue;
            }

            var role = new IdentityRole(_guidGenerator.Create(), roleName);
            EnsureSucceeded(await _roleManager.CreateAsync(role));
        }
    }

    private async Task SeedOrganizationUnitsAsync()
    {
        if (await _organizationUnitRepository.AnyAsync())
        {
            return;
        }

        var company = new OrganizationUnit(PhaseOneSeedIds.CompanyRoot, "总公司", null);
        await _organizationUnitManager.CreateAsync(company);
        await SaveSeedChangesAsync();

        var pmoCenter = new OrganizationUnit(PhaseOneSeedIds.PmoCenter, "PMO 中心", company.Id);
        await _organizationUnitManager.CreateAsync(pmoCenter);
        await SaveSeedChangesAsync();

        var rndCenter = new OrganizationUnit(PhaseOneSeedIds.RndCenter, "研发中心", company.Id);
        await _organizationUnitManager.CreateAsync(rndCenter);
        await SaveSeedChangesAsync();

        var deliveryDept = new OrganizationUnit(PhaseOneSeedIds.DeliveryDept, "交付一部", rndCenter.Id);
        await _organizationUnitManager.CreateAsync(deliveryDept);
        await SaveSeedChangesAsync();

        var financeDept = new OrganizationUnit(PhaseOneSeedIds.FinanceDept, "财务部", company.Id);
        await _organizationUnitManager.CreateAsync(financeDept);
        await SaveSeedChangesAsync();
    }

    private async Task SeedUsersAsync()
    {
        await SeedUserAsync("admin.demo", "系统管理员", PhaseOneRoleNames.Admin, PhaseOneSeedIds.CompanyRoot);
        await SeedUserAsync("pmo.demo", "PMO 负责人", PhaseOneRoleNames.Pmo, PhaseOneSeedIds.PmoCenter);
        await SeedUserAsync("pm.demo", "项目经理", PhaseOneRoleNames.Pm, PhaseOneSeedIds.DeliveryDept);
        await SeedUserAsync("member.demo", "项目成员", PhaseOneRoleNames.Member, PhaseOneSeedIds.DeliveryDept);
        await SeedUserAsync("viewer.demo", "经营查看", PhaseOneRoleNames.Viewer, PhaseOneSeedIds.FinanceDept);
    }

    private async Task SeedUserAsync(string userName, string displayName, string roleName, Guid organizationUnitId)
    {
        var existingUser = await _userManager.FindByNameAsync(userName);
        if (existingUser != null)
        {
            return;
        }

        var user = new IdentityUser(_guidGenerator.Create(), userName, $"{userName}@general.local")
        {
            Name = displayName,
            Surname = string.Empty
        };

        EnsureSucceeded(await _userManager.CreateAsync(user, DefaultPassword));
        EnsureSucceeded(await _userManager.AddToRoleAsync(user, roleName));
        await _userManager.AddToOrganizationUnitAsync(user.Id, organizationUnitId);
    }

    private async Task SeedProjectExecutionDataAsync()
    {
        var adminUser = await _userManager.FindByNameAsync("admin.demo");
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
                    PhaseOneSeedIds.DeliveryDept,
                    pmUser.Id,
                    pmoUser.Id,
                    "高",
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
                    PhaseOneSeedIds.RndCenter,
                    pmUser.Id,
                    adminUser.Id,
                    "高",
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
                    PhaseOneSeedIds.FinanceDept,
                    pmoUser.Id,
                    viewerUser.Id,
                    "中",
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
                new(Guid.Parse("52000000-0000-0000-0000-000000000001"), projects[0].Id, PhaseOneSeedIds.DeliveryDept, "TASK-001", "梳理现场交付清单", pmUser.Id, "进行中", "高", new DateTime(2026, 4, 11), new DateTime(2026, 4, 15), estimatedWorkHours: 12, contractClause: "合同 3.2 现场联调条款", productOwnerName: "PMO 负责人", developerOwnerName: "项目成员", testerOwnerName: "项目成员"),
                new(Guid.Parse("52000000-0000-0000-0000-000000000002"), projects[0].Id, PhaseOneSeedIds.DeliveryDept, "TASK-002", "输出验收前检查项", memberUser.Id, "阻塞", "高", new DateTime(2026, 4, 12), new DateTime(2026, 4, 14), true, "客户环境账号未开通", 8, contractClause: "合同 4.1 验收准备条款", productOwnerName: "项目经理", developerOwnerName: "项目成员", testerOwnerName: "项目成员"),
                new(Guid.Parse("52000000-0000-0000-0000-000000000003"), projects[0].Id, PhaseOneSeedIds.DeliveryDept, "TASK-003", "回填项目风险基线", pmUser.Id, "已完成", "中", new DateTime(2026, 4, 9), new DateTime(2026, 4, 12), estimatedWorkHours: 6, actualWorkHours: 6, actualStartTime: new DateTime(2026, 4, 9), actualEndTime: new DateTime(2026, 4, 11), contractClause: "合同 2.4 项目治理条款", productOwnerName: "PMO 负责人", developerOwnerName: "项目经理", testerOwnerName: "项目成员"),
                new(Guid.Parse("52000000-0000-0000-0000-000000000004"), projects[1].Id, PhaseOneSeedIds.RndCenter, "TASK-004", "补齐租户与审计接口", adminUser.Id, "进行中", "高", new DateTime(2026, 4, 10), new DateTime(2026, 4, 17), estimatedWorkHours: 16, contractClause: "内部目标 1.1 平台治理", productOwnerName: "项目经理", developerOwnerName: "系统管理员", testerOwnerName: "项目成员"),
                new(Guid.Parse("52000000-0000-0000-0000-000000000005"), projects[1].Id, PhaseOneSeedIds.RndCenter, "TASK-005", "替换项目工作台占位页", pmUser.Id, "未开始", "中", new DateTime(2026, 4, 14), new DateTime(2026, 4, 19), estimatedWorkHours: 10, contractClause: "内部目标 1.3 项目经营驾驶舱", productOwnerName: "项目经理", developerOwnerName: "系统管理员", testerOwnerName: "项目成员"),
                new(Guid.Parse("52000000-0000-0000-0000-000000000006"), projects[1].Id, PhaseOneSeedIds.RndCenter, "TASK-006", "处理文件上传异常", adminUser.Id, "已完成", "高", new DateTime(2026, 4, 10), new DateTime(2026, 4, 13), estimatedWorkHours: 4, actualWorkHours: 3.5, actualStartTime: new DateTime(2026, 4, 10), actualEndTime: new DateTime(2026, 4, 13), contractClause: "内部目标 1.2 基础能力修复", productOwnerName: "项目经理", developerOwnerName: "系统管理员", testerOwnerName: "项目成员"),
                new(Guid.Parse("52000000-0000-0000-0000-000000000007"), projects[2].Id, PhaseOneSeedIds.FinanceDept, "TASK-007", "整理回款计划节点", viewerUser.Id, "未开始", "中", new DateTime(2026, 4, 15), new DateTime(2026, 4, 21), estimatedWorkHours: 6, contractClause: "补充协议 2.1 回款计划", productOwnerName: "PMO 负责人", developerOwnerName: "经营查看", testerOwnerName: "经营查看"),
                new(Guid.Parse("52000000-0000-0000-0000-000000000008"), projects[2].Id, PhaseOneSeedIds.FinanceDept, "TASK-008", "输出联调接口说明", pmoUser.Id, "进行中", "中", new DateTime(2026, 4, 12), new DateTime(2026, 4, 18), estimatedWorkHours: 8, contractClause: "补充协议 3.5 系统联调", productOwnerName: "PMO 负责人", developerOwnerName: "经营查看", testerOwnerName: "经营查看")
            ], autoSave: true);
        }

        if (!await _projectRaidItemRepository.AnyAsync())
        {
            await _projectRaidItemRepository.InsertManyAsync(
            [
                new(Guid.Parse("53000000-0000-0000-0000-000000000001"), projects[0].Id, "风险", "客户现场账号开通滞后", "高", pmUser.Id, "跟踪中", new DateTime(2026, 4, 15)),
                new(Guid.Parse("53000000-0000-0000-0000-000000000002"), projects[0].Id, "问题", "验收模板尚未确认", "中", memberUser.Id, "处理中", new DateTime(2026, 4, 17)),
                new(Guid.Parse("53000000-0000-0000-0000-000000000003"), projects[1].Id, "风险", "前后端权限路由存在残留写死逻辑", "高", adminUser.Id, "跟踪中", new DateTime(2026, 4, 16)),
                new(Guid.Parse("53000000-0000-0000-0000-000000000004"), projects[1].Id, "依赖", "项目立项页等待真实接口模型", "中", pmUser.Id, "已识别", new DateTime(2026, 4, 20)),
                new(Guid.Parse("53000000-0000-0000-0000-000000000005"), projects[2].Id, "假设", "财务系统将在本月开放测试环境", "中", viewerUser.Id, "已登记", new DateTime(2026, 4, 22))
            ], autoSave: true);
        }

        if (!await _projectCycleRepository.AnyAsync())
        {
            await _projectCycleRepository.InsertManyAsync(
            [
                new(Guid.Parse("54000000-0000-0000-0000-000000000001"), projects[0].Id, "阶段", "蓝图设计", pmoUser.Id, new DateTime(2026, 4, 1), new DateTime(2026, 4, 20), "进行中", 65, "完成蓝图评审后转现场联调"),
                new(Guid.Parse("54000000-0000-0000-0000-000000000002"), projects[0].Id, "发布", "一期联调发布", pmUser.Id, new DateTime(2026, 4, 21), new DateTime(2026, 4, 30), "未开始", 0, "交付环境联调与验收准备"),
                new(Guid.Parse("54000000-0000-0000-0000-000000000003"), projects[1].Id, "冲刺", "权限模型 Sprint 3", adminUser.Id, new DateTime(2026, 4, 8), new DateTime(2026, 4, 19), "进行中", 58, "聚焦菜单鉴权、项目详情联调"),
                new(Guid.Parse("54000000-0000-0000-0000-000000000004"), projects[1].Id, "发布", "一期平台发布", pmUser.Id, new DateTime(2026, 4, 20), new DateTime(2026, 4, 28), "未开始", 0, "发布一期平台与项目执行区"),
                new(Guid.Parse("54000000-0000-0000-0000-000000000005"), projects[2].Id, "阶段", "接口梳理", viewerUser.Id, new DateTime(2026, 4, 10), new DateTime(2026, 4, 24), "进行中", 35, "财务共享平台联调前置准备")
            ], autoSave: true);
        }

        if (!await _projectIssueRepository.AnyAsync())
        {
            await _projectIssueRepository.InsertManyAsync(
            [
                new(Guid.Parse("55000000-0000-0000-0000-000000000001"), projects[0].Id, "缺陷", "现场接口鉴权失败", "高", memberUser.Id, "处理中", new DateTime(2026, 4, 15), "接口权限配置说明", "PMO 负责人", "项目成员", "项目成员"),
                new(Guid.Parse("55000000-0000-0000-0000-000000000002"), projects[0].Id, "问题", "客户验收模板未最终确认", "中", pmUser.Id, "跟踪中", new DateTime(2026, 4, 17), "一期验收范围说明", "项目经理", "项目成员", "项目成员"),
                new(Guid.Parse("55000000-0000-0000-0000-000000000003"), projects[1].Id, "缺陷", "暗色主题卡片背景残留白底", "高", adminUser.Id, "处理中", new DateTime(2026, 4, 16), "主题切换验收项", "项目经理", "系统管理员", "项目成员"),
                new(Guid.Parse("55000000-0000-0000-0000-000000000004"), projects[1].Id, "问题", "项目详情聚合字段待补齐", "中", pmUser.Id, "已识别", new DateTime(2026, 4, 19), "项目详情字段矩阵", "项目经理", "系统管理员", "项目成员"),
                new(Guid.Parse("55000000-0000-0000-0000-000000000005"), projects[2].Id, "问题", "财务测试环境开放时间未确认", "中", viewerUser.Id, "跟踪中", new DateTime(2026, 4, 22), "接口联调计划", "PMO 负责人", "经营查看", "经营查看")
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
                new(Guid.Parse("57000000-0000-0000-0000-000000000001"), projects[0].Id, pmUser.Id, PhaseOneSeedIds.DeliveryDept, ["PM", "实施"], new DateTime(2026, 4, 1)),
                new(Guid.Parse("57000000-0000-0000-0000-000000000002"), projects[0].Id, memberUser.Id, PhaseOneSeedIds.DeliveryDept, ["开发", "测试"], new DateTime(2026, 4, 2)),
                new(Guid.Parse("57000000-0000-0000-0000-000000000003"), projects[0].Id, viewerUser.Id, PhaseOneSeedIds.FinanceDept, ["运维"], new DateTime(2026, 4, 8), new DateTime(2026, 4, 18), true),
                new(Guid.Parse("57000000-0000-0000-0000-000000000004"), projects[1].Id, adminUser.Id, PhaseOneSeedIds.RndCenter, ["架构", "开发"], new DateTime(2026, 3, 20)),
                new(Guid.Parse("57000000-0000-0000-0000-000000000005"), projects[1].Id, pmUser.Id, PhaseOneSeedIds.DeliveryDept, ["PM"], new DateTime(2026, 3, 20)),
                new(Guid.Parse("57000000-0000-0000-0000-000000000006"), projects[1].Id, memberUser.Id, PhaseOneSeedIds.DeliveryDept, ["测试", "运维"], new DateTime(2026, 4, 6)),
                new(Guid.Parse("57000000-0000-0000-0000-000000000007"), projects[2].Id, pmoUser.Id, PhaseOneSeedIds.PmoCenter, ["PMO"], new DateTime(2026, 4, 10)),
                new(Guid.Parse("57000000-0000-0000-0000-000000000008"), projects[2].Id, viewerUser.Id, PhaseOneSeedIds.FinanceDept, ["业务", "运维"], new DateTime(2026, 4, 10))
            ], autoSave: true);
        }

        if (!await _projectWorklogRepository.AnyAsync())
        {
            var weekStart = GetWeekStart(new DateTime(2026, 4, 14));
            await _projectWorklogRepository.InsertManyAsync(
            [
                new(Guid.Parse("58000000-0000-0000-0000-000000000001"), projects[0].Id, pmUser.Id, weekStart, new DateTime(2026, 4, 14), 1, "跟进现场交付清单和客户接口权限", Guid.Parse("52000000-0000-0000-0000-000000000001")),
                new(Guid.Parse("58000000-0000-0000-0000-000000000002"), projects[0].Id, memberUser.Id, weekStart, new DateTime(2026, 4, 14), 1, "整理验收前检查项与联调问题", Guid.Parse("52000000-0000-0000-0000-000000000002")),
                new(Guid.Parse("58000000-0000-0000-0000-000000000003"), projects[1].Id, adminUser.Id, weekStart, new DateTime(2026, 4, 14), 1, "补齐项目详情聚合接口", Guid.Parse("52000000-0000-0000-0000-000000000004")),
                new(Guid.Parse("58000000-0000-0000-0000-000000000004"), projects[1].Id, pmUser.Id, weekStart, new DateTime(2026, 4, 15), 0.5, "梳理 PM 工作台与详情页结构", Guid.Parse("52000000-0000-0000-0000-000000000005")),
                new(Guid.Parse("58000000-0000-0000-0000-000000000005"), projects[2].Id, viewerUser.Id, weekStart, new DateTime(2026, 4, 15), 1, "整理财务接口口径与回款节点", Guid.Parse("52000000-0000-0000-0000-000000000007"))
            ], autoSave: true);
        }
    }

    private async Task SeedPlatformScheduledJobsAsync()
    {
        if (await _platformScheduledJobRepository.AnyAsync())
        {
            return;
        }

        await _platformScheduledJobRepository.InsertManyAsync(
        [
            new(Guid.Parse("59000000-0000-0000-0000-000000000001"), "log-cleanup", "清理老旧日志", "0 0 2 * * ?", "每日凌晨清理过期系统日志、操作日志与临时文件。", true, DateTime.Today.AddDays(1).AddHours(2), DateTime.Today.AddHours(-6), "最近一次执行成功"),
            new(Guid.Parse("59000000-0000-0000-0000-000000000002"), "weekly-report-reminder", "周报填写通知", "0 0 17 ? * FRI", "每周五下午提醒项目成员回填周报与工时。", true, DateTime.Today.AddDays(4).AddHours(17), DateTime.Today.AddDays(-3).AddHours(17), "最近一次发送 18 条提醒"),
            new(Guid.Parse("59000000-0000-0000-0000-000000000003"), "tenant-health-check", "租户健康巡检", "0 0/30 * * * ?", "巡检数据库连通性、后台任务积压和磁盘占用。", false, DateTime.Today.AddMinutes(30), DateTime.Today.AddHours(-1), "当前暂停")
        ], autoSave: true);
    }

    private async Task SeedBusinessManagementDataAsync()
    {
        var pmoUser = await _userManager.FindByNameAsync("pmo.demo");
        var pmUser = await _userManager.FindByNameAsync("pm.demo");
        var viewerUser = await _userManager.FindByNameAsync("viewer.demo");

        if (pmoUser == null || pmUser == null || viewerUser == null)
        {
            return;
        }

        var projects = (await _projectRepository.GetListAsync())
            .OrderBy(x => x.ProjectCode)
            .ToList();

        if (projects.Count < 3)
        {
            return;
        }

        if (!await _businessBudgetExecutionRepository.AnyAsync())
        {
            await _businessBudgetExecutionRepository.InsertManyAsync(
            [
                new(Guid.Parse("61000000-0000-0000-0000-000000000001"), projects[0].Id, "BGT-PRJ-2026-001-01", "软件开发", 620000m, 580000m, -40000m, 10),
                new(Guid.Parse("61000000-0000-0000-0000-000000000002"), projects[0].Id, "BGT-PRJ-2026-001-02", "现场实施", 340000m, 410000m, 70000m, 20),
                new(Guid.Parse("61000000-0000-0000-0000-000000000003"), projects[0].Id, "BGT-PRJ-2026-001-03", "分包采购", 240000m, 360000m, 120000m, 30),
                new(Guid.Parse("61000000-0000-0000-0000-000000000004"), projects[0].Id, "BGT-PRJ-2026-001-04", "项目管理", 300000m, 290000m, -10000m, 40),
                new(Guid.Parse("61000000-0000-0000-0000-000000000005"), projects[1].Id, "BGT-PRJ-2026-002-01", "平台开发", 280000m, 320000m, 40000m, 10),
                new(Guid.Parse("61000000-0000-0000-0000-000000000006"), projects[1].Id, "BGT-PRJ-2026-002-02", "接口联调", 220000m, 210000m, -10000m, 20),
                new(Guid.Parse("61000000-0000-0000-0000-000000000007"), projects[1].Id, "BGT-PRJ-2026-002-03", "分包采购", 180000m, 180000m, 0m, 30),
                new(Guid.Parse("61000000-0000-0000-0000-000000000008"), projects[1].Id, "BGT-PRJ-2026-002-04", "测试发布", 220000m, 300000m, 80000m, 40),
                new(Guid.Parse("61000000-0000-0000-0000-000000000009"), projects[2].Id, "BGT-PRJ-2026-003-01", "接口联调", 160000m, 110000m, -50000m, 10),
                new(Guid.Parse("61000000-0000-0000-0000-000000000010"), projects[2].Id, "BGT-PRJ-2026-003-02", "商务支持", 140000m, 120000m, -20000m, 20),
                new(Guid.Parse("61000000-0000-0000-0000-000000000011"), projects[2].Id, "BGT-PRJ-2026-003-03", "分包采购", 90000m, 90000m, 0m, 30),
                new(Guid.Parse("61000000-0000-0000-0000-000000000012"), projects[2].Id, "BGT-PRJ-2026-003-04", "交付支持", 210000m, 200000m, -10000m, 40)
            ], autoSave: true);
        }

        if (!await _businessContractRepository.AnyAsync())
        {
            await _businessContractRepository.InsertManyAsync(
            [
                new(Guid.Parse("62000000-0000-0000-0000-000000000001"), projects[0].Id, "REV-PRJ-2026-001-001", "智慧园区一期主合同", "主合同", "华东智慧园区集团", 1800000m, new DateTime(2026, 3, 30), "执行中", true),
                new(Guid.Parse("62000000-0000-0000-0000-000000000002"), projects[0].Id, "REV-PRJ-2026-001-002", "智慧园区一期补充合同 A", "补充合同", "华东智慧园区集团", 260000m, new DateTime(2026, 5, 18), "拟签订", true, "REV-PRJ-2026-001-001", "CHG-PRJ-2026-001-01"),
                new(Guid.Parse("62000000-0000-0000-0000-000000000003"), projects[1].Id, "REV-PRJ-2026-002-001", "运营中台主合同", "主合同", "远航运营科技", 1180000m, new DateTime(2026, 2, 15), "执行中", true),
                new(Guid.Parse("62000000-0000-0000-0000-000000000004"), projects[1].Id, "REV-PRJ-2026-002-002", "运营中台补充合同 B", "补充合同", "远航运营科技", 140000m, new DateTime(2026, 4, 20), "已签订", true, "REV-PRJ-2026-002-001", "CHG-PRJ-2026-002-01"),
                new(Guid.Parse("62000000-0000-0000-0000-000000000005"), projects[2].Id, "REV-PRJ-2026-003-001", "财务共享平台主合同", "主合同", "财务共享中心", 820000m, new DateTime(2026, 4, 1), "执行中", true),
                new(Guid.Parse("62000000-0000-0000-0000-000000000006"), projects[0].Id, "PROC-PRJ-2026-001-001", "弱电集成分包合同", "分包合同", "申城弱电科技", 240000m, new DateTime(2026, 4, 12), "执行中", false, "REV-PRJ-2026-001-001"),
                new(Guid.Parse("62000000-0000-0000-0000-000000000007"), projects[0].Id, "PROC-PRJ-2026-001-002", "视频巡检外采协议", "采购合同", "沪上视频巡检服务商", 120000m, new DateTime(2026, 5, 21), "待签订", false, "REV-PRJ-2026-001-002", "CHG-PRJ-2026-001-01"),
                new(Guid.Parse("62000000-0000-0000-0000-000000000008"), projects[1].Id, "PROC-PRJ-2026-002-001", "报表开发分包合同", "分包合同", "浦江数据服务", 180000m, new DateTime(2026, 3, 19), "已签订", false, "REV-PRJ-2026-002-001"),
                new(Guid.Parse("62000000-0000-0000-0000-000000000009"), projects[2].Id, "PROC-PRJ-2026-003-001", "接口治理服务预留", "采购合同", "共享接口服务商", 90000m, new DateTime(2026, 4, 22), "待确认", false, "REV-PRJ-2026-003-001")
            ], autoSave: true);
        }

        if (!await _businessChainRepository.AnyAsync())
        {
            await _businessChainRepository.InsertManyAsync(
            [
                new(Guid.Parse("63000000-0000-0000-0000-000000000001"), projects[0].Id, "BIZ-PRJ-2026-001-001", "投标链", "对外投标", "园区一期招标应答", "已中标", pmUser.Id, "已完成应答材料、现场答疑和中标通知归档。", "REV-PRJ-2026-001-001"),
                new(Guid.Parse("63000000-0000-0000-0000-000000000002"), projects[0].Id, "BIZ-PRJ-2026-001-002", "分包链", "外采分包", "弱电集成分包", "执行中", pmUser.Id, "弱电集成能力由第三方分包执行，金额按主合同 4.2 条拆分。", "REV-PRJ-2026-001-001"),
                new(Guid.Parse("63000000-0000-0000-0000-000000000003"), projects[0].Id, "BIZ-PRJ-2026-001-003", "变更链", "合同变更", "新增视频巡检范围", "评审中", pmoUser.Id, "客户新增视频巡检范围，准备形成补充合同并同步调整分包范围。", "REV-PRJ-2026-001-002", "CHG-PRJ-2026-001-01"),
                new(Guid.Parse("63000000-0000-0000-0000-000000000004"), projects[1].Id, "BIZ-PRJ-2026-002-001", "投标链", "对外投标", "运营中台采购应答", "已中标", pmUser.Id, "通过客户统一采购流程中标，已补齐投标应答与澄清纪要。", "REV-PRJ-2026-002-001"),
                new(Guid.Parse("63000000-0000-0000-0000-000000000005"), projects[1].Id, "BIZ-PRJ-2026-002-002", "分包链", "外采分包", "报表开发分包", "已签订", pmUser.Id, "报表开发工作拆分给合作方执行，并保留二次验收条款。", "PROC-PRJ-2026-002-001"),
                new(Guid.Parse("63000000-0000-0000-0000-000000000006"), projects[2].Id, "BIZ-PRJ-2026-003-001", "投标链", "对外投标", "财务共享平台采购应答", "已中标", viewerUser.Id, "客户统一采购流程已结束，中标通知书已挂接至项目。", "REV-PRJ-2026-003-001"),
                new(Guid.Parse("63000000-0000-0000-0000-000000000007"), projects[2].Id, "BIZ-PRJ-2026-003-002", "分包链", "外采预留", "接口治理外采预留", "待确认", viewerUser.Id, "预留与外部厂商的接口治理外采，不在当前周期签订。", "PROC-PRJ-2026-003-001")
            ], autoSave: true);
        }

        if (!await _businessProcurementRepository.AnyAsync())
        {
            await _businessProcurementRepository.InsertManyAsync(
            [
                new(Guid.Parse("64000000-0000-0000-0000-000000000001"), projects[0].Id, "PRC-PRJ-2026-001-001", "弱电集成分包合同", "申城弱电科技", 240000m, new DateTime(2026, 4, 12), "分包执行", "执行中", "REV-PRJ-2026-001-001"),
                new(Guid.Parse("64000000-0000-0000-0000-000000000002"), projects[0].Id, "PRC-PRJ-2026-001-002", "视频巡检外采协议", "沪上视频巡检服务商", 120000m, new DateTime(2026, 5, 21), "拟采购", "待签订", "REV-PRJ-2026-001-002", "CHG-PRJ-2026-001-01"),
                new(Guid.Parse("64000000-0000-0000-0000-000000000003"), projects[1].Id, "PRC-PRJ-2026-002-001", "报表开发分包合同", "浦江数据服务", 180000m, new DateTime(2026, 3, 19), "分包执行", "已签订", "REV-PRJ-2026-002-001"),
                new(Guid.Parse("64000000-0000-0000-0000-000000000004"), projects[2].Id, "PRC-PRJ-2026-003-001", "接口治理服务预留", "共享接口服务商", 90000m, new DateTime(2026, 4, 22), "预留分包", "待确认", "REV-PRJ-2026-003-001")
            ], autoSave: true);
        }

        if (!await _businessReceivableRepository.AnyAsync())
        {
            await _businessReceivableRepository.InsertManyAsync(
            [
                new(Guid.Parse("65000000-0000-0000-0000-000000000001"), projects[0].Id, "REC-PRJ-2026-001-001", "首期回款", new DateTime(2026, 5, 15), 900000m, 720000m, "部分到账", "REV-PRJ-2026-001-001", "INV-PRJ-2026-001-001"),
                new(Guid.Parse("65000000-0000-0000-0000-000000000002"), projects[0].Id, "REC-PRJ-2026-001-002", "二期验收回款", new DateTime(2026, 8, 30), 620000m, 0m, "待回款", "REV-PRJ-2026-001-002", "INV-PRJ-2026-001-002"),
                new(Guid.Parse("65000000-0000-0000-0000-000000000003"), projects[1].Id, "REC-PRJ-2026-002-001", "一期上线回款", new DateTime(2026, 4, 28), 840000m, 760000m, "部分到账", "REV-PRJ-2026-002-001", "INV-PRJ-2026-002-001"),
                new(Guid.Parse("65000000-0000-0000-0000-000000000004"), projects[1].Id, "REC-PRJ-2026-002-002", "补充合同回款", new DateTime(2026, 9, 15), 340000m, 0m, "待回款", "REV-PRJ-2026-002-002", "INV-PRJ-2026-002-002"),
                new(Guid.Parse("65000000-0000-0000-0000-000000000005"), projects[2].Id, "REC-PRJ-2026-003-001", "启动款", new DateTime(2026, 5, 30), 320000m, 280000m, "部分到账", "REV-PRJ-2026-003-001", "INV-PRJ-2026-003-001"),
                new(Guid.Parse("65000000-0000-0000-0000-000000000006"), projects[2].Id, "REC-PRJ-2026-003-002", "收尾尾款", new DateTime(2026, 12, 15), 400000m, 0m, "待回款", "REV-PRJ-2026-003-001", "INV-PRJ-2026-003-002")
            ], autoSave: true);
        }

        if (!await _businessForecastHistoryRepository.AnyAsync())
        {
            await _businessForecastHistoryRepository.InsertManyAsync(
            [
                new(Guid.Parse("66000000-0000-0000-0000-000000000001"), projects[0].Id, "年底预计回款", "138.00 万", "152.00 万", viewerUser.Id, "客户确认首期验收后回款时间前置", "回款预测", "REC-PRJ-2026-001-001"),
                new(Guid.Parse("66000000-0000-0000-0000-000000000002"), projects[0].Id, "毛利率预测", "32.4%", "28.9%", pmoUser.Id, "新增弱电分包导致外采成本上升", "利润预测", "PRC-PRJ-2026-001-001"),
                new(Guid.Parse("66000000-0000-0000-0000-000000000003"), projects[1].Id, "年底预计回款", "105.00 万", "118.00 万", pmUser.Id, "客户确认补充合同将于 Q3 一并回款", "回款预测", "REC-PRJ-2026-002-002"),
                new(Guid.Parse("66000000-0000-0000-0000-000000000004"), projects[1].Id, "利润预测", "36.00 万", "31.00 万", pmoUser.Id, "测试发布成本超出预算", "利润预测", "BGT-PRJ-2026-002-04"),
                new(Guid.Parse("66000000-0000-0000-0000-000000000005"), projects[2].Id, "年底预计回款", "60.00 万", "72.00 万", viewerUser.Id, "客户确认联调结束后可释放第二笔回款", "回款预测", "REC-PRJ-2026-003-002")
            ], autoSave: true);
        }
    }

    private async Task SeedMenusAsync()
    {
        var seedMenus = BuildSeedMenus();
        var existingMenus = (await _menuRepository.GetListAsync())
            .ToDictionary(x => x.Id, x => x);

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
                continue;
            }

            await _menuRepository.InsertAsync(seedMenu, autoSave: true);
        }

        await RemoveDeprecatedMenusAsync();
        await RemovePlatformTodoMenuAsync();
        await EnsureMenuAppCodesAsync();
    }

    private async Task SeedRoleMenusAsync()
    {
        var roles = (await _roleRepository.GetListAsync())
            .Where(x => PhaseOneRoleNames.All.Contains(x.Name))
            .ToDictionary(x => x.Name, x => x.Id);

        var allMenuIds = (await _menuRepository.GetListAsync())
            .Select(x => x.Id)
            .ToList();

        var mappings = new Dictionary<string, IReadOnlyCollection<Guid>>
        {
            [PhaseOneRoleNames.Admin] = allMenuIds,
            [PhaseOneRoleNames.Pmo] =
            [
                PhaseOneSeedIds.PlatformRoot,
                PhaseOneSeedIds.PlatformWorkspace,
                PhaseOneSeedIds.PlatformProfile,
                PhaseOneSeedIds.PlatformScheduler,
                PhaseOneSeedIds.ProjectRoot,
                PhaseOneSeedIds.ProjectList,
                PhaseOneSeedIds.ProjectDetail,
                PhaseOneSeedIds.ProjectMyRelated,
                PhaseOneSeedIds.ProjectPmoOverview,
                PhaseOneSeedIds.ProjectCreate,
                PhaseOneSeedIds.BusinessRoot,
                PhaseOneSeedIds.BusinessOverview,
                PhaseOneSeedIds.BusinessProjects,
                PhaseOneSeedIds.BusinessReports,
                PhaseOneSeedIds.BusinessBudgetSensitive
            ],
            [PhaseOneRoleNames.Pm] =
            [
                PhaseOneSeedIds.PlatformRoot,
                PhaseOneSeedIds.PlatformWorkspace,
                PhaseOneSeedIds.PlatformProfile,
                PhaseOneSeedIds.ProjectRoot,
                PhaseOneSeedIds.ProjectList,
                PhaseOneSeedIds.ProjectDetail,
                PhaseOneSeedIds.ProjectMyRelated,
                PhaseOneSeedIds.ProjectPmDashboard,
                PhaseOneSeedIds.ProjectCreate,
                PhaseOneSeedIds.ProjectTaskManage,
                PhaseOneSeedIds.BusinessRoot,
                PhaseOneSeedIds.BusinessOverview,
                PhaseOneSeedIds.BusinessProjects
            ],
            [PhaseOneRoleNames.Member] =
            [
                PhaseOneSeedIds.PlatformRoot,
                PhaseOneSeedIds.PlatformWorkspace,
                PhaseOneSeedIds.PlatformProfile,
                PhaseOneSeedIds.ProjectRoot,
                PhaseOneSeedIds.ProjectList,
                PhaseOneSeedIds.ProjectDetail,
                PhaseOneSeedIds.ProjectMyRelated
            ],
            [PhaseOneRoleNames.Viewer] =
            [
                PhaseOneSeedIds.PlatformRoot,
                PhaseOneSeedIds.PlatformWorkspace,
                PhaseOneSeedIds.PlatformProfile,
                PhaseOneSeedIds.ProjectRoot,
                PhaseOneSeedIds.ProjectList,
                PhaseOneSeedIds.ProjectDetail,
                PhaseOneSeedIds.ProjectPmoOverview,
                PhaseOneSeedIds.BusinessRoot,
                PhaseOneSeedIds.BusinessOverview,
                PhaseOneSeedIds.BusinessProjects,
                PhaseOneSeedIds.BusinessReports
            ]
        };

        foreach (var (roleName, menuIds) in mappings)
        {
            if (!roles.TryGetValue(roleName, out var roleId))
            {
                continue;
            }

            foreach (var menuId in menuIds.Distinct())
            {
                var exists = await _roleMenuRepository.AnyAsync(x => x.RoleId == roleId && x.MenuId == menuId);
                if (!exists)
                {
                    await _roleMenuRepository.InsertAsync(new AppRoleMenu(_guidGenerator.Create(), roleId, menuId));
                }
            }
        }
    }

    private static List<AppMenu> BuildSeedMenus()
    {
        return
        [
            Catalog(PhaseOneAppCodes.Platform, PhaseOneSeedIds.PlatformRoot, null, "PlatformCenter", "/platform", "平台中心", "lucide:settings-2"),
            Menu(PhaseOneAppCodes.Platform, PhaseOneSeedIds.PlatformWorkspace, PhaseOneSeedIds.PlatformRoot, "PlatformWorkspace", "/platform/workspace", "/platform/workspace/index", "平台总览", "lucide:layout-dashboard", 10, affixTab: true),
            Menu(PhaseOneAppCodes.Platform, PhaseOneSeedIds.PlatformOrganization, PhaseOneSeedIds.PlatformRoot, "PlatformOrganization", "/platform/organization", "/platform/organization/index", "组织架构", "lucide:building-2", 20),
            Menu(PhaseOneAppCodes.Platform, PhaseOneSeedIds.PlatformUsers, PhaseOneSeedIds.PlatformRoot, "PlatformUsers", "/platform/users", "/platform/users/index", "用户管理", "lucide:users", 30),
            Menu(PhaseOneAppCodes.Platform, PhaseOneSeedIds.PlatformRoles, PhaseOneSeedIds.PlatformRoot, "PlatformRoles", "/platform/roles", "/platform/roles/index", "角色权限", "lucide:key-round", 40),
            Menu(PhaseOneAppCodes.Platform, PhaseOneSeedIds.PlatformTenants, PhaseOneSeedIds.PlatformRoot, "PlatformTenants", "/platform/tenants", "/platform/tenants/index", "租户管理", "lucide:building", 50),
            Menu(PhaseOneAppCodes.Platform, PhaseOneSeedIds.PlatformMenus, PhaseOneSeedIds.PlatformRoot, "PlatformMenus", "/platform/menus", "/platform/menus/index", "菜单管理", "lucide:waypoints", 60),
            Menu(PhaseOneAppCodes.Platform, PhaseOneSeedIds.PlatformFiles, PhaseOneSeedIds.PlatformRoot, "PlatformFiles", "/platform/files", "/platform/files/index", "文件管理", "lucide:files", 70),
            Menu(PhaseOneAppCodes.Platform, PhaseOneSeedIds.PlatformAuditLogs, PhaseOneSeedIds.PlatformRoot, "PlatformAuditLogs", "/platform/audit-logs", "/platform/audit-logs/index", "审计日志", "lucide:shield-check", 80),
            Menu(PhaseOneAppCodes.Platform, PhaseOneSeedIds.PlatformSystemMonitor, PhaseOneSeedIds.PlatformRoot, "PlatformSystemMonitor", "/platform/system-monitor", "/platform/system-monitor/index", "系统监控", "lucide:monitor", 90),
            Menu(PhaseOneAppCodes.Platform, PhaseOneSeedIds.PlatformScheduler, PhaseOneSeedIds.PlatformRoot, "PlatformScheduler", "/platform/scheduler", "/platform/scheduler/index", "定时任务", "lucide:clock-3", 100),
            Menu(PhaseOneAppCodes.Platform, PhaseOneSeedIds.PlatformProfile, PhaseOneSeedIds.PlatformRoot, "PlatformProfile", "/platform/profile", "/platform/profile/index", "个人中心", "lucide:user-circle-2", 110),
            Button(PhaseOneAppCodes.Platform, PhaseOneSeedIds.PlatformOrganizationManage, PhaseOneSeedIds.PlatformOrganization, "PlatformOrganizationManage", "Platform.Organization.Manage"),
            Button(PhaseOneAppCodes.Platform, PhaseOneSeedIds.PlatformUsersManage, PhaseOneSeedIds.PlatformUsers, "PlatformUsersManage", "Platform.User.Manage"),
            Button(PhaseOneAppCodes.Platform, PhaseOneSeedIds.PlatformRolesManage, PhaseOneSeedIds.PlatformRoles, "PlatformRoleManage", "Platform.Role.Manage"),
            Button(PhaseOneAppCodes.Platform, PhaseOneSeedIds.PlatformTenantsManage, PhaseOneSeedIds.PlatformTenants, "PlatformTenantManage", "Platform.Tenant.Manage"),
            Button(PhaseOneAppCodes.Platform, PhaseOneSeedIds.PlatformMenusManage, PhaseOneSeedIds.PlatformMenus, "PlatformMenuManage", "Platform.Menu.Manage"),
            Button(PhaseOneAppCodes.Platform, PhaseOneSeedIds.PlatformFilesManage, PhaseOneSeedIds.PlatformFiles, "PlatformFileManage", "Platform.File.Manage"),

            Catalog(PhaseOneAppCodes.Project, PhaseOneSeedIds.ProjectRoot, null, "ProjectCenter", "/project", "项目执行区", "lucide:folder-kanban", 20),
            Menu(PhaseOneAppCodes.Project, PhaseOneSeedIds.ProjectList, PhaseOneSeedIds.ProjectRoot, "ProjectList", "/project/projects", "/project/projects/index", "项目列表", "lucide:folder-open-dot", 20),
            Menu(PhaseOneAppCodes.Project, PhaseOneSeedIds.ProjectDetail, PhaseOneSeedIds.ProjectRoot, "ProjectDetail", "/project/detail", "/project/detail/index", "项目详情", "lucide:panel-right-open", 30),
            Menu(PhaseOneAppCodes.Project, PhaseOneSeedIds.ProjectMyRelated, PhaseOneSeedIds.ProjectRoot, "ProjectMyRelated", "/project/my-related", "/project/my-related/index", "与我相关", "lucide:user-round-check", 40),
            Menu(PhaseOneAppCodes.Project, PhaseOneSeedIds.ProjectPmDashboard, PhaseOneSeedIds.ProjectRoot, "ProjectPmDashboard", "/project/pm-dashboard", "/project/pm-dashboard/index", "PM 工作台", "lucide:monitor-cog", 60),
            Menu(PhaseOneAppCodes.Project, PhaseOneSeedIds.ProjectPmoOverview, PhaseOneSeedIds.ProjectRoot, "ProjectPmoOverview", "/project/pmo-overview", "/project/pmo-overview/index", "PMO 执行总览", "lucide:layout-dashboard", 70),
            Button(PhaseOneAppCodes.Project, PhaseOneSeedIds.ProjectCreate, PhaseOneSeedIds.ProjectList, "ProjectCreate", "Project.Project.Create"),
            Button(PhaseOneAppCodes.Project, PhaseOneSeedIds.ProjectTaskManage, PhaseOneSeedIds.ProjectDetail, "ProjectTaskManage", "Project.Task.Manage"),

            Catalog(PhaseOneAppCodes.Business, PhaseOneSeedIds.BusinessRoot, null, "BusinessCenter", "/business", "经营管理区", "lucide:briefcase-business", 30),
            Menu(PhaseOneAppCodes.Business, PhaseOneSeedIds.BusinessOverview, PhaseOneSeedIds.BusinessRoot, "BusinessOverview", "/business/overview", "/business/overview/index", "经营总览", "lucide:chart-column-big", 10),
            Menu(PhaseOneAppCodes.Business, PhaseOneSeedIds.BusinessProjects, PhaseOneSeedIds.BusinessRoot, "BusinessProjects", "/business/projects", "/business/projects/index", "项目经营", "lucide:folder-output", 20),
            Menu(PhaseOneAppCodes.Business, PhaseOneSeedIds.BusinessReports, PhaseOneSeedIds.BusinessRoot, "BusinessReports", "/business/reports", "/business/reports/index", "报表中心", "lucide:sheet", 30),
            Button(PhaseOneAppCodes.Business, PhaseOneSeedIds.BusinessBudgetSensitive, PhaseOneSeedIds.BusinessProjects, "BusinessBudgetSensitive", "Business.Budget.Sensitive")
        ];
    }

    private async Task RemoveDeprecatedMenusAsync()
    {
        var deprecatedMenuIds = new HashSet<Guid>
        {
            PhaseOneSeedIds.ProjectWorkspace,
            PhaseOneSeedIds.ProjectInitiation,
            PhaseOneSeedIds.BusinessAcceptances,
            PhaseOneSeedIds.BusinessInvoices,
            PhaseOneSeedIds.BusinessReceivables
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
            .Where(x => x.MenuId == PhaseOneSeedIds.PlatformTodo)
            .ToList();
        if (roleMappings.Count > 0)
        {
            await _roleMenuRepository.DeleteManyAsync(roleMappings, autoSave: true);
        }

        var menu = await _menuRepository.FindAsync(PhaseOneSeedIds.PlatformTodo);
        if (menu != null)
        {
            await _menuRepository.DeleteAsync(menu, autoSave: true);
        }
    }

    private async Task SaveSeedChangesAsync()
    {
        if (_unitOfWorkManager.Current != null)
        {
            await _unitOfWorkManager.Current.SaveChangesAsync();
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

    private static AppMenu Button(string appCode, Guid id, Guid parentId, string name, string permissionCode)
    {
        return new AppMenu(
            id,
            appCode,
            parentId,
            name,
            permissionCode,
            null,
            null,
            PhaseOneMenuType.Button,
            permissionCode,
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
            PhaseOneMenuType.Catalog,
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
        bool affixTab = false)
    {
        return new AppMenu(
            id,
            appCode,
            parentId,
            name,
            path,
            component,
            null,
            PhaseOneMenuType.Menu,
            title,
            icon,
            null,
            null,
            affixTab,
            true,
            false,
            false,
            false,
            false,
            order);
    }

    private static string ResolveAppCode(Guid menuId)
    {
        return menuId.ToString()[0] switch
        {
            '2' => PhaseOneAppCodes.Platform,
            '3' => PhaseOneAppCodes.Project,
            _ => PhaseOneAppCodes.Business
        };
    }

    private static void EnsureSucceeded(Microsoft.AspNetCore.Identity.IdentityResult result)
    {
        if (result.Succeeded)
        {
            return;
        }

        throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
    }

    private static DateTime GetWeekStart(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.Date.AddDays(-diff);
    }
}
