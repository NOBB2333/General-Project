namespace General.Admin.Platform;

public interface IPlatformNotificationRecipientResolver
{
    Task<List<Guid>> GetUserIdsInRolesAsync(IReadOnlyCollection<Guid> roleIds);
}
