using System.ComponentModel.DataAnnotations;

namespace General.Admin.Ai;

public class AiPagedResultDto<T>
{
    public IReadOnlyList<T> Items { get; set; } = [];

    public int TotalCount { get; set; }
}

public class AiWritebackRequestDto
{
    public string ActionCode { get; set; } = string.Empty;

    public DateTime? ApprovedTime { get; set; }

    public Guid? ApproverUserId { get; set; }

    public string BusinessModule { get; set; } = string.Empty;

    public Guid? ConversationId { get; set; }

    public DateTime CreationTime { get; set; }

    public string DiffPreviewJson { get; set; } = "{}";

    public string ErrorMessage { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public Guid? MessageId { get; set; }

    public string PayloadJson { get; set; } = "{}";

    public Guid? ProjectId { get; set; }

    public string RejectReason { get; set; } = string.Empty;

    public string SourceType { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string TargetEntityId { get; set; } = string.Empty;

    public string TargetEntityType { get; set; } = string.Empty;
}

public class AiWritebackRequestQueryInput
{
    [MaxLength(128)]
    public string? ActionCode { get; set; }

    [MaxLength(64)]
    public string? BusinessModule { get; set; }

    public Guid? ConversationId { get; set; }

    public DateTime? CreatedFrom { get; set; }

    public DateTime? CreatedTo { get; set; }

    [MaxLength(128)]
    public string? Keyword { get; set; }

    public int MaxResultCount { get; set; } = 200;

    public Guid? ProjectId { get; set; }

    public int SkipCount { get; set; }

    [MaxLength(32)]
    public string? Status { get; set; } = "Pending";

    [MaxLength(128)]
    public string? TargetEntityType { get; set; }
}

public class AiWritebackRequestCreateInput
{
    [Required]
    [MaxLength(128)]
    public string ActionCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string BusinessModule { get; set; } = "Project";

    public Guid? ConversationId { get; set; }

    [MaxLength(16_000)]
    public string DiffPreviewJson { get; set; } = "{}";

    public Guid? MessageId { get; set; }

    [MaxLength(16_000)]
    public string PayloadJson { get; set; } = "{}";

    public Guid? ProjectId { get; set; }

    [MaxLength(64)]
    public string SourceType { get; set; } = "Ai";

    [Required]
    [MaxLength(512)]
    public string Summary { get; set; } = string.Empty;

    [MaxLength(128)]
    public string TargetEntityId { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string TargetEntityType { get; set; } = string.Empty;
}

public class AiWritebackDecisionInput
{
    [MaxLength(512)]
    public string Reason { get; set; } = string.Empty;
}
