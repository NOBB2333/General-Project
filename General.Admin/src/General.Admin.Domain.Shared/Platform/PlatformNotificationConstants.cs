namespace General.Admin.Platform;

public static class PlatformNotificationLevels
{
    public const string Error = "error";
    public const string Info = "info";
    public const string Success = "success";
    public const string Warning = "warning";

    public static string Normalize(string? value)
    {
        var normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
        return normalized switch
        {
            Error => Error,
            Success => Success,
            Warning => Warning,
            _ => Info
        };
    }
}

public static class PlatformNotificationRecipientModes
{
    public const string AllUsers = "all_users";
    public const string Organizations = "organizations";
    public const string OrganizationsAndDescendants = "organizations_and_descendants";
    public const string Roles = "roles";
    public const string Users = "users";

    public static string Normalize(string? value)
    {
        var normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
        return normalized switch
        {
            AllUsers => AllUsers,
            Organizations => Organizations,
            OrganizationsAndDescendants => OrganizationsAndDescendants,
            Roles => Roles,
            _ => Users
        };
    }
}

public static class PlatformNotificationTypes
{
    public const string Message = "message";
    public const string System = "system";
    public const string Task = "task";
    public const string UpdateLog = "update_log";
    public const string Warning = "warning";

    public static string Normalize(string? value)
    {
        var normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
        return normalized switch
        {
            Message => Message,
            Task => Task,
            UpdateLog => UpdateLog,
            Warning => Warning,
            _ => System
        };
    }
}
