using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using General.Admin.Platform;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace General.Admin.EntityFrameworkCore.Platform;

[Collection(AdminTestConsts.CollectionDefinitionName)]
public class PlatformDataSeedContributorTests : AdminEntityFrameworkCoreTestBase
{
    private readonly PlatformDataSeedContributor _seedContributor;
    private readonly ICurrentTenant _currentTenant;

    public PlatformDataSeedContributorTests()
    {
        _seedContributor = GetRequiredService<PlatformDataSeedContributor>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
    }

    [Fact]
    public async Task SeedAsync_Should_Create_Default_Tenant_User_Organization_Links()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            await _seedContributor.SeedAsync(new DataSeedContext());
        });

        await WithUnitOfWorkAsync(async () =>
        {
            await _seedContributor.SeedAsync(new DataSeedContext());
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var dbContext = GetRequiredService<AdminDbContext>();
            var defaultTenant = await dbContext.Tenants.SingleAsync(x => x.Name == PlatformSeedIds.DefaultTenantName);
            using var tenantChange = _currentTenant.Change(defaultTenant.Id, defaultTenant.Name);
            var seedUsers = await dbContext.Users
                .Where(x => PlatformSeedUserNames.All.Contains(x.UserName!))
                .ToListAsync();
            var seedUserIds = seedUsers.Select(user => user.Id).ToList();
            var seedUserOrganizationUnitCount = await dbContext.UserOrganizationUnits
                .Where(x => seedUserIds.Contains(x.UserId))
                .CountAsync();
            var pmoRoleIds = await dbContext.Roles
                .Where(x => x.TenantId == defaultTenant.Id && x.Name == PlatformRoleNames.Pmo)
                .Select(x => x.Id)
                .ToListAsync();
            var pmoAuthorization = await dbContext.Set<AppRoleAuthorization>()
                .Where(x => pmoRoleIds.Contains(x.RoleId))
                .ToListAsync();

            seedUsers.Count.ShouldBe(PlatformSeedUserNames.All.Length);
            seedUserOrganizationUnitCount.ShouldBe(PlatformSeedUserNames.All.Length);
            pmoAuthorization.ShouldContain(x => x.DataScopeMode == PlatformAuthorizationDefaults.DataScopeAll);
        });
    }

    [Fact]
    public async Task SeedAsync_Should_Remove_Deprecated_Business_Ledger_Menus()
    {
        var deprecatedMenuIds = new[]
        {
            Guid.Parse("40000000-0000-0000-0000-000000000004"),
            Guid.Parse("40000000-0000-0000-0000-000000000005"),
            Guid.Parse("40000000-0000-0000-0000-000000000006"),
            Guid.Parse("40000000-0000-0000-0000-000000000010"),
            Guid.Parse("40000000-0000-0000-0000-000000000011"),
            Guid.Parse("40000000-0000-0000-0000-000000000012"),
            Guid.Parse("40000000-0000-0000-0000-000000000013")
        };

        await WithUnitOfWorkAsync(async () =>
        {
            await _seedContributor.SeedAsync(new DataSeedContext());
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var dbContext = GetRequiredService<AdminDbContext>();
            var pmoRoleId = await dbContext.Roles
                .Where(x => x.TenantId == null && x.Name == PlatformRoleNames.Pmo)
                .Select(x => x.Id)
                .SingleAsync();

            var deprecatedMenus = deprecatedMenuIds
                .Select((menuId, index) => new AppMenu(
                    menuId,
                    PlatformAppCodes.Business,
                    PlatformSeedIds.BusinessRoot,
                    $"BusinessLegacyLedger{index}",
                    $"/business/legacy-ledger-{index}",
                    $"/business/legacy-ledger-{index}/index",
                    null,
                    PlatformMenuType.Menu,
                    $"旧台账入口{index}",
                    "lucide:sheet",
                    "Business.Report.View",
                    null,
                    false,
                    true,
                    false,
                    false,
                    false,
                    false,
                    100 + index))
                .ToList();
            await dbContext.AppMenus.AddRangeAsync(deprecatedMenus);

            var deprecatedRoleMenus = deprecatedMenuIds
                .Select(menuId => new AppRoleMenu(Guid.NewGuid(), pmoRoleId, menuId))
                .ToList();
            await dbContext.AppRoleMenus.AddRangeAsync(deprecatedRoleMenus);
            await dbContext.SaveChangesAsync();

            (await dbContext.AppMenus.CountAsync(x => deprecatedMenuIds.Contains(x.Id))).ShouldBe(deprecatedMenuIds.Length);
            (await dbContext.AppRoleMenus.CountAsync(x => deprecatedMenuIds.Contains(x.MenuId))).ShouldBe(deprecatedMenuIds.Length);
        });

        await WithUnitOfWorkAsync(async () =>
        {
            await _seedContributor.SeedAsync(new DataSeedContext());
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var dbContext = GetRequiredService<AdminDbContext>();
            (await dbContext.AppMenus.CountAsync(x => deprecatedMenuIds.Contains(x.Id))).ShouldBe(0);
            (await dbContext.AppRoleMenus.CountAsync(x => deprecatedMenuIds.Contains(x.MenuId))).ShouldBe(0);

            var businessMenuIds = await dbContext.AppMenus
                .Where(x => x.ParentId == PlatformSeedIds.BusinessRoot && x.Type == PlatformMenuType.Menu)
                .OrderBy(x => x.Order)
                .Select(x => x.Id)
                .ToListAsync();
            businessMenuIds.ShouldBe(new List<Guid>
            {
                PlatformSeedIds.BusinessOverview,
                PlatformSeedIds.BusinessProjects,
                PlatformSeedIds.BusinessReports
            });
        });
    }

    [Fact]
    public async Task SeedAsync_Should_Create_Project_Schedule_Sprints_And_Assign_Tasks()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            await _seedContributor.SeedAsync(new DataSeedContext());
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var dbContext = GetRequiredService<AdminDbContext>();
            var scheduleSprints = await dbContext.ProjectSprints
                .ToListAsync();
            var scheduledTaskCount = await dbContext.ProjectTasks
                .CountAsync(x => x.SprintId != null);

            scheduleSprints.Count.ShouldBeGreaterThanOrEqualTo(5);
            scheduleSprints.ShouldContain(x => x.SprintCode == "SPR-001" && x.Name == "第 1 迭代 · 现场联调");
            scheduleSprints.ShouldContain(x => x.SprintCode == "SPR-002" && x.Name == "第 2 迭代 · 验收准备");
            scheduledTaskCount.ShouldBeGreaterThanOrEqualTo(8);
        });
    }

    private static class PlatformSeedUserNames
    {
        public static readonly string[] All =
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
}
