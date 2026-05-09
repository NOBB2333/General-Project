using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;

namespace General.Admin.EntityFrameworkCore.Platform;

public class EfCorePlatformIdentityLookupService :
    IPlatformIdentityLookupService,
    ITransientDependency
{
    private readonly IDbContextProvider<AdminDbContext> _dbContextProvider;

    public EfCorePlatformIdentityLookupService(IDbContextProvider<AdminDbContext> dbContextProvider)
    {
        _dbContextProvider = dbContextProvider;
    }

    public async Task<Dictionary<Guid, List<string>>> GetRoleNamesByUserIdsAsync(IReadOnlyCollection<Guid> userIds)
    {
        if (userIds.Count == 0)
        {
            return [];
        }

        var userIdSet = userIds.ToHashSet();
        var dbContext = await _dbContextProvider.GetDbContextAsync();
        var roleRows = await dbContext.Users
            .Where(user => userIdSet.Contains(user.Id))
            .SelectMany(
                user => user.Roles,
                (user, userRole) => new
                {
                    user.Id,
                    userRole.RoleId
                })
            .Join(
                dbContext.Roles,
                userRole => userRole.RoleId,
                role => role.Id,
                (userRole, role) => new
                {
                    UserId = userRole.Id,
                    RoleName = role.Name
                })
            .ToListAsync();

        return roleRows
            .GroupBy(x => x.UserId)
            .ToDictionary(
                x => x.Key,
                x => x.Select(item => item.RoleName)
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(name => name)
                    .ToList());
    }

    public async Task<HashSet<Guid>> GetUserIdsByRoleNameAsync(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return [];
        }

        var normalizedRoleName = roleName.Trim().ToUpperInvariant();
        var dbContext = await _dbContextProvider.GetDbContextAsync();
        var userIds = await dbContext.Users
            .Where(user => user.Roles.Any(userRole =>
                dbContext.Roles.Any(role =>
                    role.Id == userRole.RoleId &&
                    role.NormalizedName == normalizedRoleName)))
            .Select(user => user.Id)
            .ToListAsync();

        return userIds.ToHashSet();
    }

    public async Task<Dictionary<Guid, int>> GetUserCountsByRoleIdsAsync(IReadOnlyCollection<Guid> roleIds)
    {
        if (roleIds.Count == 0)
        {
            return [];
        }

        var roleIdSet = roleIds.ToHashSet();
        var dbContext = await _dbContextProvider.GetDbContextAsync();
        var countRows = await dbContext.Users
            .SelectMany(user => user.Roles)
            .Where(userRole => roleIdSet.Contains(userRole.RoleId))
            .GroupBy(userRole => userRole.RoleId)
            .Select(group => new
            {
                RoleId = group.Key,
                Count = group.Select(item => item.UserId).Distinct().Count()
            })
            .ToListAsync();

        return countRows.ToDictionary(x => x.RoleId, x => x.Count);
    }
}
