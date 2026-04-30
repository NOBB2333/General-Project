using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;

namespace General.Admin.EntityFrameworkCore.Platform;

public class EfCorePlatformNotificationBulkOperator :
    IPlatformNotificationBulkOperator,
    ITransientDependency
{
    private readonly IDbContextProvider<AdminDbContext> _dbContextProvider;

    public EfCorePlatformNotificationBulkOperator(IDbContextProvider<AdminDbContext> dbContextProvider)
    {
        _dbContextProvider = dbContextProvider;
    }

    public async Task MarkAllReadAsync(Guid userId)
    {
        var dbContext = await _dbContextProvider.GetDbContextAsync();
        var now = DateTime.UtcNow;
        await dbContext.AppUserNotifications
            .Where(x => x.UserId == userId && !x.IsRemoved && !x.IsRead)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.IsRead, true)
                .SetProperty(x => x.ReadAt, now));
    }

    public async Task ClearAsync(Guid userId)
    {
        var dbContext = await _dbContextProvider.GetDbContextAsync();
        var now = DateTime.UtcNow;
        await dbContext.AppUserNotifications
            .Where(x => x.UserId == userId && !x.IsRemoved)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.IsRemoved, true)
                .SetProperty(x => x.RemovedAt, now));
    }
}
