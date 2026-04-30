using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.Platform;

public class AppUserNotification : CreationAuditedEntity<Guid>
{
    public bool IsRead { get; private set; }

    public bool IsRemoved { get; private set; }

    public Guid NotificationId { get; private set; }

    public DateTime? ReadAt { get; private set; }

    public DateTime? RemovedAt { get; private set; }

    public Guid UserId { get; private set; }

    protected AppUserNotification()
    {
    }

    public AppUserNotification(Guid id, Guid notificationId, Guid userId) : base(id)
    {
        NotificationId = notificationId;
        UserId = userId;
    }

    public void MarkRead(DateTime now)
    {
        IsRead = true;
        ReadAt ??= now;
    }

    public void Remove(DateTime now)
    {
        IsRemoved = true;
        RemovedAt ??= now;
    }
}
