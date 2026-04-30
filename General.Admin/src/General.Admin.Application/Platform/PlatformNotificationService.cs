using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;
using Volo.Abp.Users;

namespace General.Admin.Platform;

public class PlatformNotificationService : ITransientDependency
{
    private readonly IAsyncQueryableExecuter _asyncQueryableExecuter;
    private readonly IPlatformNotificationBulkOperator _bulkOperator;
    private readonly ICurrentUser _currentUser;
    private readonly IRepository<AppNotification, Guid> _notificationRepository;
    private readonly PlatformNotificationPublisher _publisher;
    private readonly IRepository<AppUserNotification, Guid> _userNotificationRepository;

    public PlatformNotificationService(
        IAsyncQueryableExecuter asyncQueryableExecuter,
        IPlatformNotificationBulkOperator bulkOperator,
        ICurrentUser currentUser,
        IRepository<AppNotification, Guid> notificationRepository,
        PlatformNotificationPublisher publisher,
        IRepository<AppUserNotification, Guid> userNotificationRepository)
    {
        _asyncQueryableExecuter = asyncQueryableExecuter;
        _bulkOperator = bulkOperator;
        _currentUser = currentUser;
        _notificationRepository = notificationRepository;
        _publisher = publisher;
        _userNotificationRepository = userNotificationRepository;
    }

    public async Task<List<PlatformNotificationDto>> GetListAsync(PlatformNotificationListInput input)
    {
        var userId = GetCurrentUserId();
        var maxResultCount = Math.Clamp(input.MaxResultCount, 1, 100);
        var skipCount = Math.Max(0, input.SkipCount);

        var userNotificationQueryable = await _userNotificationRepository.GetQueryableAsync();
        var query = userNotificationQueryable
            .Where(x => x.UserId == userId && !x.IsRemoved);
        if (input.OnlyUnread)
        {
            query = query.Where(x => !x.IsRead);
        }

        var userNotifications = await _asyncQueryableExecuter.ToListAsync(
            query
                .OrderByDescending(x => x.CreationTime)
                .Skip(skipCount)
                .Take(maxResultCount));
        if (userNotifications.Count == 0)
        {
            return [];
        }

        var notificationIds = userNotifications.Select(x => x.NotificationId).Distinct().ToList();
        var notificationQueryable = await _notificationRepository.GetQueryableAsync();
        var notifications = (await _asyncQueryableExecuter.ToListAsync(
                notificationQueryable.Where(x => notificationIds.Contains(x.Id))))
            .ToDictionary(x => x.Id);

        return userNotifications
            .Where(x => notifications.ContainsKey(x.NotificationId))
            .Select(x => MapToDto(notifications[x.NotificationId], x))
            .ToList();
    }

    public async Task<PlatformNotificationUnreadCountDto> GetUnreadCountAsync()
    {
        var userId = GetCurrentUserId();
        var queryable = await _userNotificationRepository.GetQueryableAsync();
        return new PlatformNotificationUnreadCountDto
        {
            Count = await _asyncQueryableExecuter.CountAsync(queryable.Where(x =>
                x.UserId == userId &&
                !x.IsRemoved &&
                !x.IsRead))
        };
    }

    public async Task MarkReadAsync(Guid id)
    {
        var item = await GetCurrentUserNotificationAsync(id);
        item.MarkRead(DateTime.UtcNow);
        await _userNotificationRepository.UpdateAsync(item, autoSave: true);
    }

    public async Task MarkAllReadAsync()
    {
        await _bulkOperator.MarkAllReadAsync(GetCurrentUserId());
    }

    public async Task RemoveAsync(Guid id)
    {
        var item = await GetCurrentUserNotificationAsync(id);
        item.Remove(DateTime.UtcNow);
        await _userNotificationRepository.UpdateAsync(item, autoSave: true);
    }

    public async Task ClearAsync()
    {
        await _bulkOperator.ClearAsync(GetCurrentUserId());
    }

    public Task<Guid?> SendAsync(PlatformNotificationSendInput input)
    {
        return _publisher.PublishAsync(input);
    }

    private async Task<AppUserNotification> GetCurrentUserNotificationAsync(Guid id)
    {
        var userId = GetCurrentUserId();
        return await _userNotificationRepository.GetAsync(x =>
            x.NotificationId == id &&
            x.UserId == userId &&
            !x.IsRemoved);
    }

    private Guid GetCurrentUserId()
    {
        return _currentUser.Id ?? throw new InvalidOperationException("Current user is not available.");
    }

    private static PlatformNotificationDto MapToDto(AppNotification notification, AppUserNotification userNotification)
    {
        return new PlatformNotificationDto
        {
            Avatar = string.IsNullOrWhiteSpace(notification.Avatar)
                ? $"https://avatar.vercel.sh/{notification.Type}.svg?text=GA"
                : notification.Avatar,
            CreationTime = notification.CreationTime,
            Date = notification.CreationTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm"),
            Id = notification.Id,
            IsRead = userNotification.IsRead,
            Level = notification.Level,
            Link = notification.Link,
            Message = notification.Content,
            Title = notification.Title,
            Type = notification.Type
        };
    }
}
