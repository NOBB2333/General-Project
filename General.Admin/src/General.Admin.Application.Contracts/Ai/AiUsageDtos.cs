using System.ComponentModel.DataAnnotations;

namespace General.Admin.Ai;

public class AiUsageRecordDto
{
    public string AuditSummary { get; set; } = string.Empty;

    public Guid? ConversationId { get; set; }

    public decimal CostAmount { get; set; }

    public string Currency { get; set; } = string.Empty;

    public int CompletionTokens { get; set; }

    public DateTime CreationTime { get; set; }

    public long ElapsedMilliseconds { get; set; }

    public string ErrorMessage { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public Guid? JobId { get; set; }

    public string ModelCode { get; set; } = string.Empty;

    public Guid? ModelId { get; set; }

    public string ModelName { get; set; } = string.Empty;

    public Guid? ProjectId { get; set; }

    public Guid? ProviderId { get; set; }

    public string ProviderName { get; set; } = string.Empty;

    public int PromptInjectionRiskScore { get; set; }

    public string PromptInjectionSignalsJson { get; set; } = "[]";

    public int PromptTokens { get; set; }

    public string PromptTemplateCode { get; set; } = string.Empty;

    public string RequestPreview { get; set; } = string.Empty;

    public string ResponsePreview { get; set; } = string.Empty;

    public string Scenario { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string ToolCode { get; set; } = string.Empty;

    public int TotalTokens { get; set; }

    public Guid? UserId { get; set; }

    public string UserName { get; set; } = string.Empty;
}

public class AiUsageQueryInput
{
    public Guid? ConversationId { get; set; }

    public DateTime? CreatedFrom { get; set; }

    public DateTime? CreatedTo { get; set; }

    [MaxLength(128)]
    public string? Keyword { get; set; }

    public int MaxResultCount { get; set; } = 200;

    public Guid? ModelId { get; set; }

    public Guid? ProjectId { get; set; }

    public Guid? ProviderId { get; set; }

    [MaxLength(128)]
    public string? PromptTemplateCode { get; set; }

    [MaxLength(64)]
    public string? Scenario { get; set; }

    [MaxLength(32)]
    public string? Source { get; set; }

    [MaxLength(32)]
    public string? Status { get; set; }

    [MaxLength(128)]
    public string? ToolCode { get; set; }

    public Guid? UserId { get; set; }
}

public class AiUsageBucketDto
{
    public decimal CostAmount { get; set; }

    public int Count { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public int TotalTokens { get; set; }
}

public class AiUsageDashboardDto
{
    public int CallCount { get; set; }

    public decimal CostAmount { get; set; }

    public string Currency { get; set; } = "USD";

    public int FailedCount { get; set; }

    public int HighRiskPromptCount { get; set; }

    public List<AiUsageBucketDto> ModelBuckets { get; set; } = [];

    public List<AiUsageBucketDto> ProjectBuckets { get; set; } = [];

    public List<AiUsageBucketDto> ScenarioBuckets { get; set; } = [];

    public List<AiUsageBucketDto> TemplateBuckets { get; set; } = [];

    public int ToolCallCount { get; set; }

    public int TotalTokens { get; set; }

    public List<AiUsageBucketDto> UserBuckets { get; set; } = [];

    public List<string> Warnings { get; set; } = [];
}

public class AiUsageRecordInput
{
    public string AuditSummary { get; set; } = string.Empty;

    public Guid? ConversationId { get; set; }

    public int CompletionTokens { get; set; }

    public long ElapsedMilliseconds { get; set; }

    public string ErrorMessage { get; set; } = string.Empty;

    public Guid? JobId { get; set; }

    public Guid? ModelId { get; set; }

    public Guid? ProjectId { get; set; }

    public Guid? ProviderId { get; set; }

    public int PromptTokens { get; set; }

    public string PromptTemplateCode { get; set; } = string.Empty;

    public string RequestPreview { get; set; } = string.Empty;

    public string ResponsePreview { get; set; } = string.Empty;

    public string Scenario { get; set; } = string.Empty;

    public string Source { get; set; } = "ModelChat";

    public string Status { get; set; } = "Success";

    public string ToolCode { get; set; } = string.Empty;

    public Guid? UserId { get; set; }
}

public class AiUsageQuotaCheckInput
{
    public Guid? ModelId { get; set; }

    public Guid? ProjectId { get; set; }

    public string PromptTemplateCode { get; set; } = string.Empty;

    public string RequestPreview { get; set; } = string.Empty;

    public string Scenario { get; set; } = string.Empty;

    public string Source { get; set; } = "ModelChat";

    public Guid? UserId { get; set; }
}

public class AiUsageQuotaCheckResultDto
{
    public bool Allowed { get; set; } = true;

    public List<string> PromptInjectionSignals { get; set; } = [];

    public int PromptInjectionRiskScore { get; set; }

    public List<string> Warnings { get; set; } = [];
}
