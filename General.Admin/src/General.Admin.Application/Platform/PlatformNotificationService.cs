using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
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
    private readonly IRepository<IdentityUser, Guid> _userRepository;

    public PlatformNotificationService(
        IAsyncQueryableExecuter asyncQueryableExecuter,
        IPlatformNotificationBulkOperator bulkOperator,
        ICurrentUser currentUser,
        IRepository<AppNotification, Guid> notificationRepository,
        PlatformNotificationPublisher publisher,
        IRepository<AppUserNotification, Guid> userNotificationRepository,
        IRepository<IdentityUser, Guid> userRepository)
    {
        _asyncQueryableExecuter = asyncQueryableExecuter;
        _bulkOperator = bulkOperator;
        _currentUser = currentUser;
        _notificationRepository = notificationRepository;
        _publisher = publisher;
        _userNotificationRepository = userNotificationRepository;
        _userRepository = userRepository;
    }

    public async Task<List<PlatformNotificationDto>> GetListAsync(PlatformNotificationListInput input)
    {
        var userId = GetCurrentUserId();
        var maxResultCount = Math.Clamp(input.MaxResultCount, 1, 100);
        var skipCount = Math.Max(0, input.SkipCount);

        var userNotificationQueryable = await _userNotificationRepository.GetQueryableAsync();
        var notificationQueryable = await _notificationRepository.GetQueryableAsync();
        var query = userNotificationQueryable
            .Where(x => x.UserId == userId && !x.IsRemoved);
        if (input.OnlyUnread)
        {
            query = query.Where(x => !x.IsRead);
        }

        var joinedQuery =
            from userNotification in query
            join notification in notificationQueryable
                on userNotification.NotificationId equals notification.Id
            orderby userNotification.CreationTime descending
            select new
            {
                Notification = notification,
                UserNotification = userNotification
            };
        var items = await _asyncQueryableExecuter.ToListAsync(
            joinedQuery
                .Skip(skipCount)
                .Take(maxResultCount));
        if (items.Count == 0)
        {
            return [];
        }

        var senderIds = items
            .Select(x => x.Notification)
            .Where(x => x.SenderUserId.HasValue)
            .Select(x => x.SenderUserId!.Value)
            .Distinct()
            .ToList();
        var senderNames = senderIds.Count == 0
            ? new Dictionary<Guid, string>()
            : (await _asyncQueryableExecuter.ToListAsync(
                    (await _userRepository.GetQueryableAsync()).Where(x => senderIds.Contains(x.Id))))
                .ToDictionary(x => x.Id, x => string.IsNullOrWhiteSpace(x.UserName) ? x.Name : x.UserName);

        return items
            .Select(x => MapToDto(x.Notification, x.UserNotification, senderNames))
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

    private static PlatformNotificationDto MapToDto(
        AppNotification notification,
        AppUserNotification userNotification,
        IReadOnlyDictionary<Guid, string> senderNames)
    {
        var senderName = notification.SenderUserId.HasValue &&
                         senderNames.TryGetValue(notification.SenderUserId.Value, out var resolvedSenderName)
            ? resolvedSenderName
            : null;
        return new PlatformNotificationDto
        {
            Avatar = notification.Avatar,
            AvatarText = ResolveAvatarText(senderName, notification.Title),
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

    private static string ResolveAvatarText(string? senderName, string title)
    {
        var source = string.IsNullOrWhiteSpace(senderName) ? title : senderName;
        source = source.Trim();
        return string.IsNullOrWhiteSpace(source)
            ? "G"
            : source[..1].ToUpperInvariant();
    }
}
