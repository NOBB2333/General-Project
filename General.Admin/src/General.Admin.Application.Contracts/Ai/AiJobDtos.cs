using System.ComponentModel.DataAnnotations;

namespace General.Admin.Ai;

public class AiJobDto
{
    public string AssistantCode { get; set; } = string.Empty;

    public string ContextScopeJson { get; set; } = "{}";

    public DateTime CreationTime { get; set; }

    public string Description { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public bool IsEnabled { get; set; }

    public string JobCode { get; set; } = string.Empty;

    public string JobType { get; set; } = string.Empty;

    public DateTime? LastRunTime { get; set; }

    public string LastRunStatus { get; set; } = string.Empty;

    public string LastRunSummary { get; set; } = string.Empty;

    public Guid? ModelId { get; set; }

    public string ModelName { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string OutputTargetJson { get; set; } = "{}";

    public string PromptTemplateCode { get; set; } = string.Empty;

    public string SchedulerJobKey { get; set; } = string.Empty;
}

public class AiJobSaveInput
{
    [MaxLength(64)]
    public string AssistantCode { get; set; } = string.Empty;

    [MaxLength(32000)]
    public string ContextScopeJson { get; set; } = "{}";

    [MaxLength(512)]
    public string Description { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    [Required]
    [MaxLength(64)]
    public string JobCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string JobType { get; set; } = "ProjectWeeklySummary";

    public Guid? ModelId { get; set; }

    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(32000)]
    public string OutputTargetJson { get; set; } = "{}";

    [MaxLength(128)]
    public string PromptTemplateCode { get; set; } = string.Empty;

    [MaxLength(64)]
    public string SchedulerJobKey { get; set; } = AiJobConstants.SchedulerJobKey;
}

public class AiJobQueryInput
{
    public bool IncludeDisabled { get; set; } = true;

    [MaxLength(64)]
    public string? JobType { get; set; }

    [MaxLength(128)]
    public string? Keyword { get; set; }

    public int MaxResultCount { get; set; } = 500;

    [MaxLength(64)]
    public string? SchedulerJobKey { get; set; }
}

public class AiJobToggleInput
{
    public bool IsEnabled { get; set; }
}

public class AiJobRunDto
{
    public Guid AiJobId { get; set; }

    public int DraftItemCount { get; set; }

    public long ElapsedMilliseconds { get; set; }

    public string ErrorMessage { get; set; } = string.Empty;

    public DateTime? FinishedTime { get; set; }

    public Guid Id { get; set; }

    public string InputSnapshotJson { get; set; } = "{}";

    public string JobCode { get; set; } = string.Empty;

    public string JobName { get; set; } = string.Empty;

    public string JobType { get; set; } = string.Empty;

    public string OutputDraftJson { get; set; } = "{}";

    public string RunMode { get; set; } = string.Empty;

    public DateTime StartedTime { get; set; }

    public string Status { get; set; } = string.Empty;
}

public class AiJobRunQueryInput
{
    public Guid? AiJobId { get; set; }

    [MaxLength(64)]
    public string? JobCode { get; set; }

    [MaxLength(64)]
    public string? JobType { get; set; }

    public int MaxResultCount { get; set; } = 200;

    [MaxLength(32)]
    public string? RunMode { get; set; }

    public DateTime? StartedFrom { get; set; }

    public DateTime? StartedTo { get; set; }

    [MaxLength(32)]
    public string? Status { get; set; }
}

public class AiJobRunResultDto
{
    public AiJobDto Job { get; set; } = new();

    public AiJobRunDto Run { get; set; } = new();

    public string Summary { get; set; } = string.Empty;
}
