using System.ComponentModel.DataAnnotations;

namespace General.Admin.Platform;

public class PlatformScheduledJobBatchInput
{
    [Required]
    public List<string> JobKeys { get; set; } = [];
}

public class PlatformScheduledJobBatchToggleInput : PlatformScheduledJobBatchInput
{
    public bool IsEnabled { get; set; }
}

public class PlatformScheduledJobBatchClearRecordsInput : PlatformScheduledJobBatchInput
{
    public int KeepLastN { get; set; } = 100;
}

public class PlatformScheduledJobDashboardDto
{
    public int EnabledCount { get; set; }

    public int FailedLast24Hours { get; set; }

    public int RunningCount { get; set; }

    public int SlowLast24Hours { get; set; }

    public int TotalCount { get; set; }
}

public class PlatformScheduledJobOperationResultDto
{
    public string JobKey { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public bool Success { get; set; }
}
