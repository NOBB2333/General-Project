namespace General.Admin.Platform;

public interface IPlatformIdentityLookupService
{
    Task<Dictionary<Guid, List<string>>> GetRoleNamesByUserIdsAsync(IReadOnlyCollection<Guid> userIds);

    Task<HashSet<Guid>> GetUserIdsByRoleNameAsync(string roleName);

    Task<Dictionary<Guid, int>> GetUserCountsByRoleIdsAsync(IReadOnlyCollection<Guid> roleIds);
}
