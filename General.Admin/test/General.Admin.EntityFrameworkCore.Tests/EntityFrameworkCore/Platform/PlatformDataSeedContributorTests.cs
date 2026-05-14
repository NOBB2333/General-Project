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
