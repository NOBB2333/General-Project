using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.Platform;

public class AppNotification : FullAuditedAggregateRoot<Guid>
{
    public string Avatar { get; private set; }

    public string Content { get; private set; }

    public string Link { get; private set; }

    public string Level { get; private set; }

    public string RecipientMode { get; private set; }

    public string RecipientSummary { get; private set; }

    public Guid? SenderUserId { get; private set; }

    public string Title { get; private set; }

    public string Type { get; private set; }

    protected AppNotification()
    {
        Avatar = string.Empty;
        Content = string.Empty;
        Link = string.Empty;
        Level = PlatformNotificationLevels.Info;
        RecipientMode = PlatformNotificationRecipientModes.Users;
        RecipientSummary = "[]";
        Title = string.Empty;
        Type = PlatformNotificationTypes.System;
    }

    public AppNotification(
        Guid id,
        string title,
        string content,
        string type,
        string level,
        string link,
        string avatar,
        string recipientMode,
        string recipientSummary,
        Guid? senderUserId) : base(id)
    {
        Title = NormalizeRequired(title, 128);
        Content = Normalize(content, 1024) ?? string.Empty;
        Type = PlatformNotificationTypes.Normalize(type);
        Level = PlatformNotificationLevels.Normalize(level);
        Link = Normalize(link, 512) ?? string.Empty;
        Avatar = Normalize(avatar, 512) ?? string.Empty;
        RecipientMode = PlatformNotificationRecipientModes.Normalize(recipientMode);
        RecipientSummary = string.IsNullOrWhiteSpace(recipientSummary) ? "[]" : recipientSummary.Trim();
        SenderUserId = senderUserId;
    }

    private static string NormalizeRequired(string value, int maxLength)
    {
        return Check.NotNullOrWhiteSpace(value, nameof(value), maxLength).Trim();
    }

    private static string? Normalize(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }
}
