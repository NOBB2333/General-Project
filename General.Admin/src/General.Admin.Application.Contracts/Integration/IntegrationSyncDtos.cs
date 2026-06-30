using System.ComponentModel.DataAnnotations;

namespace General.Admin.Integration;

public class IntegrationSyncJobDto
{
    public string ConfigJson { get; set; } = "{}";

    public string ConnectorCode { get; set; } = string.Empty;

    public Guid ConnectorId { get; set; }

    public string CronExpression { get; set; } = string.Empty;

    public string Direction { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public bool IsEnabled { get; set; }

    public string JobCode { get; set; } = string.Empty;

    public string LastRunError { get; set; } = string.Empty;

    public string LastRunStatus { get; set; } = string.Empty;

    public string LastRunSummary { get; set; } = string.Empty;

    public DateTime? LastRunTime { get; set; }

    public string Mode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string ProviderType { get; set; } = string.Empty;

    public string Remark { get; set; } = string.Empty;

    public string SyncType { get; set; } = string.Empty;
}

public class IntegrationSyncJobQueryInput
{
    public string ConnectorCode { get; set; } = string.Empty;

    public string Keyword { get; set; } = string.Empty;

    public int MaxResultCount { get; set; } = 20;

    public string ProviderType { get; set; } = string.Empty;

    public int SkipCount { get; set; }

    public string Status { get; set; } = "All";

    public string SyncType { get; set; } = string.Empty;
}

public class IntegrationSyncJobSaveInput
{
    [MaxLength(16_000)]
    public string ConfigJson { get; set; } = "{}";

    [Required]
    public Guid ConnectorId { get; set; }

    [MaxLength(128)]
    public string CronExpression { get; set; } = string.Empty;

    [MaxLength(32)]
    public string Direction { get; set; } = IntegrationSyncDirections.Inbound;

    public Guid? Id { get; set; }

    public bool IsEnabled { get; set; } = true;

    [Required]
    [MaxLength(64)]
    public string JobCode { get; set; } = string.Empty;

    [MaxLength(32)]
    public string Mode { get; set; } = IntegrationSyncModes.Incremental;

    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(512)]
    public string Remark { get; set; } = string.Empty;

    [MaxLength(32)]
    public string SyncType { get; set; } = IntegrationSyncTypes.User;
}

public class IntegrationSyncJobToggleInput
{
    public bool IsEnabled { get; set; }
}

public class IntegrationSyncRunInput
{
    public bool Force { get; set; }

    [MaxLength(32)]
    public string Mode { get; set; } = string.Empty;

    [MaxLength(16_000)]
    public string RequestJson { get; set; } = "{}";
}

public class IntegrationSyncRecordDto
{
    public string ConflictJson { get; set; } = "{}";

    public int ConflictCount { get; set; }

    public string ConnectorCode { get; set; } = string.Empty;

    public Guid ConnectorId { get; set; }

    public DateTime CreationTime { get; set; }

    public int CreatedCount { get; set; }

    public int DurationMilliseconds { get; set; }

    public string ErrorMessage { get; set; } = string.Empty;

    public int FailedCount { get; set; }

    public DateTime? FinishedTime { get; set; }

    public Guid Id { get; set; }

    public string Mode { get; set; } = string.Empty;

    public string ProviderType { get; set; } = string.Empty;

    public string RequestJson { get; set; } = "{}";

    public string ResultJson { get; set; } = "{}";

    public int RetryCount { get; set; }

    public int SkippedCount { get; set; }

    public DateTime? StartedTime { get; set; }

    public string Status { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public Guid SyncJobId { get; set; }

    public string SyncType { get; set; } = string.Empty;

    public int TotalCount { get; set; }

    public int UpdatedCount { get; set; }
}

public class IntegrationSyncRecordQueryInput
{
    public string ConnectorCode { get; set; } = string.Empty;

    public string Keyword { get; set; } = string.Empty;

    public int MaxResultCount { get; set; } = 20;

    public string Mode { get; set; } = string.Empty;

    public string ProviderType { get; set; } = string.Empty;

    public int SkipCount { get; set; }

    public string Status { get; set; } = "All";

    public Guid? SyncJobId { get; set; }

    public string SyncType { get; set; } = string.Empty;
}

public class IntegrationSyncRecordSkipInput
{
    [MaxLength(1000)]
    public string Reason { get; set; } = string.Empty;
}
