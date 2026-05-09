using System.ComponentModel.DataAnnotations;

namespace General.Admin.Platform;

public class PlatformNotificationDto
{
    public string Avatar { get; set; } = string.Empty;

    public string AvatarText { get; set; } = string.Empty;

    public DateTime CreationTime { get; set; }

    public string Date { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public bool IsRead { get; set; }

    public string Level { get; set; } = PlatformNotificationLevels.Info;

    public string Link { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Type { get; set; } = PlatformNotificationTypes.System;
}

public class PlatformNotificationRecipientInput
{
    [Required]
    [MaxLength(64)]
    public string Mode { get; set; } = PlatformNotificationRecipientModes.Users;

    public List<Guid> OrganizationUnitIds { get; set; } = [];

    public List<string> RoleNames { get; set; } = [];

    public List<Guid> UserIds { get; set; } = [];
}

public class PlatformNotificationSendInput
{
    [MaxLength(512)]
    public string Avatar { get; set; } = string.Empty;

    [MaxLength(1024)]
    public string Content { get; set; } = string.Empty;

    [MaxLength(512)]
    public string Link { get; set; } = string.Empty;

    [MaxLength(32)]
    public string Level { get; set; } = PlatformNotificationLevels.Info;

    [Required]
    public PlatformNotificationRecipientInput Recipient { get; set; } = new();

    [Required]
    [MaxLength(128)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(64)]
    public string Type { get; set; } = PlatformNotificationTypes.System;
}

public class PlatformNotificationUnreadCountDto
{
    public int Count { get; set; }
}

public class PlatformNotificationListInput
{
    public int MaxResultCount { get; set; } = 20;

    public bool OnlyUnread { get; set; }

    public int SkipCount { get; set; }
}
