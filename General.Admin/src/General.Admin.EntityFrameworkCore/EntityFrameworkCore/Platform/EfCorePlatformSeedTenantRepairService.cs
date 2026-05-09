using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;

namespace General.Admin.EntityFrameworkCore.Platform;

public class EfCorePlatformSeedTenantRepairService :
    IPlatformSeedTenantRepairService,
    ITransientDependency
{
    private readonly IDbContextProvider<AdminDbContext> _dbContextProvider;

    public EfCorePlatformSeedTenantRepairService(IDbContextProvider<AdminDbContext> dbContextProvider)
    {
        _dbContextProvider = dbContextProvider;
    }

    public async Task RepairDefaultTenantSeedDataAsync(
        Guid tenantId,
        IReadOnlyCollection<Guid> userIds,
        IReadOnlyCollection<string> userNames,
        IReadOnlyCollection<Guid> organizationUnitIds,
        IReadOnlyCollection<string> organizationUnitNames,
        IReadOnlyCollection<string> roleNames)
    {
        var dbContext = await _dbContextProvider.GetDbContextAsync();
        var normalizedRoleNames = roleNames
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().ToUpperInvariant())
            .Distinct()
            .ToList();
        var tenantRoleNames = await dbContext.Roles
            .Where(x => x.TenantId == tenantId && x.NormalizedName != null && normalizedRoleNames.Contains(x.NormalizedName))
            .Select(x => x.NormalizedName)
            .Distinct()
            .ToListAsync();
        var migratableRoleNames = normalizedRoleNames
            .Except(tenantRoleNames, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var roleIds = await dbContext.Roles
            .Where(x => x.TenantId == null && x.NormalizedName != null && migratableRoleNames.Contains(x.NormalizedName))
            .Select(x => x.Id)
            .ToListAsync();

        if (roleIds.Count > 0)
        {
            await dbContext.Roles
                .Where(x => roleIds.Contains(x.Id))
                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.TenantId, tenantId));

            await dbContext.Set<Volo.Abp.Identity.IdentityUserRole>()
                .Where(x => roleIds.Contains(x.RoleId))
                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.TenantId, tenantId));

            await dbContext.Set<Volo.Abp.Identity.IdentityRoleClaim>()
                .Where(x => roleIds.Contains(x.RoleId))
                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.TenantId, tenantId));

            await dbContext.Set<Volo.Abp.Identity.OrganizationUnitRole>()
                .Where(x => roleIds.Contains(x.RoleId))
                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.TenantId, tenantId));
        }

        if (organizationUnitIds.Count > 0)
        {
            await dbContext.OrganizationUnits
                .Where(x => organizationUnitIds.Contains(x.Id) && x.TenantId == null)
                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.TenantId, tenantId));

            await dbContext.UserOrganizationUnits
                .Where(x => organizationUnitIds.Contains(x.OrganizationUnitId))
                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.TenantId, tenantId));

            await dbContext.Set<Volo.Abp.Identity.OrganizationUnitRole>()
                .Where(x => organizationUnitIds.Contains(x.OrganizationUnitId))
                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.TenantId, tenantId));
        }

        var normalizedOrganizationUnitNames = organizationUnitNames
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();
        if (normalizedOrganizationUnitNames.Count > 0)
        {
            var organizationUnitNameIds = await dbContext.OrganizationUnits
                .Where(x => x.TenantId == null && normalizedOrganizationUnitNames.Contains(x.DisplayName))
                .Select(x => x.Id)
                .ToListAsync();

            if (organizationUnitNameIds.Count > 0)
            {
                await dbContext.OrganizationUnits
                    .Where(x => organizationUnitNameIds.Contains(x.Id))
                    .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.TenantId, tenantId));

                await dbContext.UserOrganizationUnits
                    .Where(x => organizationUnitNameIds.Contains(x.OrganizationUnitId))
                    .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.TenantId, tenantId));

                await dbContext.Set<Volo.Abp.Identity.OrganizationUnitRole>()
                    .Where(x => organizationUnitNameIds.Contains(x.OrganizationUnitId))
                    .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.TenantId, tenantId));
            }
        }

        var normalizedUserNames = userNames
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().ToUpperInvariant())
            .Distinct()
            .ToList();

        if (userIds.Count > 0)
        {
            var userIdItems = await dbContext.Users
                .Where(x => userIds.Contains(x.Id) && x.TenantId == null)
                .Select(x => new { x.Id, x.NormalizedUserName })
                .ToListAsync();
            var userIdSet = userIdItems
                .Where(x => normalizedUserNames.Contains(x.NormalizedUserName))
                .Select(x => x.Id)
                .ToList();

            await dbContext.Users
                .Where(x => userIdSet.Contains(x.Id))
                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.TenantId, tenantId));

            await dbContext.Set<Volo.Abp.Identity.IdentityUserRole>()
                .Where(x => userIdSet.Contains(x.UserId))
                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.TenantId, tenantId));

            await dbContext.UserOrganizationUnits
                .Where(x => userIdSet.Contains(x.UserId))
                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.TenantId, tenantId));
        }

        if (normalizedUserNames.Count > 0)
        {
            var userNameIds = await dbContext.Users
                .Where(x => x.TenantId == null && normalizedUserNames.Contains(x.NormalizedUserName))
                .Select(x => x.Id)
                .ToListAsync();

            if (userNameIds.Count > 0)
            {
                await dbContext.Users
                    .Where(x => userNameIds.Contains(x.Id))
                    .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.TenantId, tenantId));

                await dbContext.Set<Volo.Abp.Identity.IdentityUserRole>()
                    .Where(x => userNameIds.Contains(x.UserId))
                    .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.TenantId, tenantId));

                await dbContext.UserOrganizationUnits
                    .Where(x => userNameIds.Contains(x.UserId))
                    .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.TenantId, tenantId));
            }
        }
    }
}
