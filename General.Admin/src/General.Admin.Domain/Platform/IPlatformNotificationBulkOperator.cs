namespace General.Admin.Platform;

public interface IPlatformNotificationBulkOperator
{
    Task ClearAsync(Guid userId);

    Task MarkAllReadAsync(Guid userId);
}
