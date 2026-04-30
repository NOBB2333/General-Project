using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;

namespace General.Admin.EntityFrameworkCore.Platform;

public class EfCorePlatformNotificationRecipientResolver :
    IPlatformNotificationRecipientResolver,
    ITransientDependency
{
    private readonly IDbContextProvider<AdminDbContext> _dbContextProvider;

    public EfCorePlatformNotificationRecipientResolver(IDbContextProvider<AdminDbContext> dbContextProvider)
    {
        _dbContextProvider = dbContextProvider;
    }

    public async Task<List<Guid>> GetUserIdsInRolesAsync(IReadOnlyCollection<Guid> roleIds)
    {
        if (roleIds.Count == 0)
        {
            return [];
        }

        var dbContext = await _dbContextProvider.GetDbContextAsync();
        return await dbContext.Users
            .Where(user => user.Roles.Any(role => roleIds.Contains(role.RoleId)))
            .Select(user => user.Id)
            .Distinct()
            .ToListAsync();
    }
}
