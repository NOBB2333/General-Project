using System.ComponentModel.DataAnnotations;

namespace General.Admin.Integration;

public class IntegrationMessageTemplateDto
{
    public string BodyTemplate { get; set; } = string.Empty;

    public string CardJson { get; set; } = "{}";

    public DateTime CreationTime { get; set; }

    public Guid Id { get; set; }

    public string MessageType { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string PreviewVariablesJson { get; set; } = "{}";

    public string Remark { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string TemplateCode { get; set; } = string.Empty;

    public string TitleTemplate { get; set; } = string.Empty;

    public string VariableSchemaJson { get; set; } = "{}";
}

public class IntegrationMessageTemplateQueryInput
{
    public string Keyword { get; set; } = string.Empty;

    public int MaxResultCount { get; set; } = 20;

    public string MessageType { get; set; } = string.Empty;

    public int SkipCount { get; set; }

    public string Status { get; set; } = "All";
}

public class IntegrationMessageTemplateSaveInput
{
    [MaxLength(16_000)]
    public string BodyTemplate { get; set; } = string.Empty;

    [MaxLength(16_000)]
    public string CardJson { get; set; } = "{}";

    public Guid? Id { get; set; }

    [MaxLength(32)]
    public string MessageType { get; set; } = IntegrationMessageTemplateTypes.Text;

    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(16_000)]
    public string PreviewVariablesJson { get; set; } = "{}";

    [MaxLength(512)]
    public string Remark { get; set; } = string.Empty;

    [MaxLength(32)]
    public string Status { get; set; } = IntegrationMessageTemplateStatus.Draft;

    [Required]
    [MaxLength(64)]
    public string TemplateCode { get; set; } = string.Empty;

    [MaxLength(256)]
    public string TitleTemplate { get; set; } = string.Empty;

    [MaxLength(16_000)]
    public string VariableSchemaJson { get; set; } = "{}";
}

public class IntegrationTemplateRenderInput
{
    [MaxLength(16_000)]
    public string BodyTemplate { get; set; } = string.Empty;

    [MaxLength(16_000)]
    public string CardJson { get; set; } = "{}";

    public Guid? TemplateId { get; set; }

    [MaxLength(256)]
    public string TitleTemplate { get; set; } = string.Empty;

    [MaxLength(16_000)]
    public string VariablesJson { get; set; } = "{}";
}

public class IntegrationTemplateRenderResultDto
{
    public string RenderedBody { get; set; } = string.Empty;

    public string RenderedCardJson { get; set; } = "{}";

    public string RenderedTitle { get; set; } = string.Empty;

    public IReadOnlyList<string> MissingVariables { get; set; } = [];
}

public class IntegrationNotificationRuleDto
{
    public string ChannelType { get; set; } = string.Empty;

    public string ConditionJson { get; set; } = "{}";

    public string ConnectorCode { get; set; } = string.Empty;

    public Guid? ConnectorId { get; set; }

    public DateTime CreationTime { get; set; }

    public string EventCode { get; set; } = string.Empty;

    public string EventName { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public DateTime? LastTriggeredTime { get; set; }

    public string Name { get; set; } = string.Empty;

    public string ProviderType { get; set; } = string.Empty;

    public string QuietEndTime { get; set; } = string.Empty;

    public string QuietStartTime { get; set; } = string.Empty;

    public int RateLimitMaxCount { get; set; }

    public int RateLimitWindowMinutes { get; set; }

    public string RecipientRuleJson { get; set; } = "{}";

    public string Remark { get; set; } = string.Empty;

    public string RuleCode { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string TargetType { get; set; } = string.Empty;

    public string TemplateCode { get; set; } = string.Empty;

    public Guid TemplateId { get; set; }

    public string TemplateName { get; set; } = string.Empty;
}

public class IntegrationNotificationRuleQueryInput
{
    public Guid? ConnectorId { get; set; }

    public string EventCode { get; set; } = string.Empty;

    public string Keyword { get; set; } = string.Empty;

    public int MaxResultCount { get; set; } = 20;

    public int SkipCount { get; set; }

    public string Status { get; set; } = "All";
}

public class IntegrationNotificationRuleSaveInput
{
    [MaxLength(32)]
    public string ChannelType { get; set; } = IntegrationNotificationChannelTypes.Personal;

    [MaxLength(16_000)]
    public string ConditionJson { get; set; } = "{}";

    public Guid? ConnectorId { get; set; }

    [Required]
    [MaxLength(128)]
    public string EventCode { get; set; } = string.Empty;

    [MaxLength(128)]
    public string EventName { get; set; } = string.Empty;

    public Guid? Id { get; set; }

    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(16)]
    public string QuietEndTime { get; set; } = string.Empty;

    [MaxLength(16)]
    public string QuietStartTime { get; set; } = string.Empty;

    public int RateLimitMaxCount { get; set; }

    public int RateLimitWindowMinutes { get; set; }

    [MaxLength(16_000)]
    public string RecipientRuleJson { get; set; } = "{}";

    [MaxLength(512)]
    public string Remark { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string RuleCode { get; set; } = string.Empty;

    [MaxLength(32)]
    public string Status { get; set; } = IntegrationNotificationRuleStatus.Active;

    [MaxLength(32)]
    public string TargetType { get; set; } = "OpenId";

    public Guid TemplateId { get; set; }
}

public class IntegrationNotificationRuleToggleInput
{
    [MaxLength(32)]
    public string Status { get; set; } = IntegrationNotificationRuleStatus.Active;
}

public class IntegrationNotificationEventInput
{
    public bool CommitTriggered { get; set; }

    [Required]
    [MaxLength(128)]
    public string EventCode { get; set; } = string.Empty;

    public DateTime? OccurredAt { get; set; }

    [MaxLength(16_000)]
    public string VariablesJson { get; set; } = "{}";
}

public class IntegrationNotificationEvaluationResultDto
{
    public int BlockedCount { get; set; }

    public string EventCode { get; set; } = string.Empty;

    public IReadOnlyList<IntegrationNotificationRuleEvaluationDto> Items { get; set; } = [];

    public int ReadyCount { get; set; }

    public int TotalRuleCount { get; set; }
}

public class IntegrationNotificationDispatchResultDto
{
    public int BlockedCount { get; set; }

    public IReadOnlyList<IntegrationNotificationDispatchItemDto> Items { get; set; } = [];

    public int FailedCount { get; set; }

    public int NoTargetCount { get; set; }

    public int SentCount { get; set; }

    public int SkippedCount { get; set; }
}

public class IntegrationNotificationDispatchItemDto
{
    public string BlockReason { get; set; } = string.Empty;

    public Guid? DeliveryRecordId { get; set; }

    public string Message { get; set; } = string.Empty;

    public string RuleCode { get; set; } = string.Empty;

    public string RuleName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string TargetId { get; set; } = string.Empty;

    public string TargetType { get; set; } = string.Empty;
}

public class IntegrationNotificationRuleEvaluationDto
{
    public string BlockReason { get; set; } = string.Empty;

    public bool CanSend { get; set; }

    public string ChannelType { get; set; } = string.Empty;

    public string ConnectorCode { get; set; } = string.Empty;

    public Guid? ConnectorId { get; set; }

    public IReadOnlyList<string> MissingVariables { get; set; } = [];

    public string RenderedBody { get; set; } = string.Empty;

    public string RenderedCardJson { get; set; } = "{}";

    public string RenderedTitle { get; set; } = string.Empty;

    public Guid RuleId { get; set; }

    public string RuleCode { get; set; } = string.Empty;

    public string RuleName { get; set; } = string.Empty;

    public string TargetType { get; set; } = string.Empty;

    public Guid TemplateId { get; set; }

    public string TemplateName { get; set; } = string.Empty;
}
