using System.Collections.Generic;

namespace General.Admin.Integration;

public static class IntegrationSyncJobStatus
{
    public const string SchedulerJobKey = "integration-sync-dispatcher";

    public const string Active = "Active";
    public const string Disabled = "Disabled";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Active,
        Disabled
    };
}

public static class IntegrationSyncModes
{
    public const string Full = "Full";
    public const string Incremental = "Incremental";
    public const string Manual = "Manual";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Full,
        Incremental,
        Manual
    };
}

public static class IntegrationSyncDirections
{
    public const string Inbound = "Inbound";
    public const string Outbound = "Outbound";
    public const string Bidirectional = "Bidirectional";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Inbound,
        Outbound,
        Bidirectional
    };
}

public static class IntegrationSyncTypes
{
    public const string Department = "Department";
    public const string Group = "Group";
    public const string Meeting = "Meeting";
    public const string User = "User";
    public const string Custom = "Custom";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Department,
        Group,
        Meeting,
        User,
        Custom
    };
}

public static class IntegrationSyncRecordStatus
{
    public const string Failed = "Failed";
    public const string Pending = "Pending";
    public const string Running = "Running";
    public const string Skipped = "Skipped";
    public const string Succeeded = "Succeeded";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Failed,
        Pending,
        Running,
        Skipped,
        Succeeded
    };
}
