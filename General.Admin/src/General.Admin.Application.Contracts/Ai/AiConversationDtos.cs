using System.ComponentModel.DataAnnotations;

namespace General.Admin.Ai;

public class AiConversationDto
{
    public DateTime? ArchivedTime { get; set; }

    public int CompletionTokens { get; set; }

    public string ContextId { get; set; } = string.Empty;

    public string ContextSnapshotJson { get; set; } = "{}";

    public string ContextType { get; set; } = "None";

    public DateTime CreationTime { get; set; }

    public Guid Id { get; set; }

    public string LastMessageSummary { get; set; } = string.Empty;

    public DateTime? LastMessageTime { get; set; }

    public int MessageCount { get; set; }

    public Guid? ModelId { get; set; }

    public string ModelName { get; set; } = string.Empty;

    public string Mode { get; set; } = "General";

    public int PromptTokens { get; set; }

    public string Status { get; set; } = "Active";

    public string Title { get; set; } = string.Empty;

    public int TotalTokens { get; set; }
}

public class AiConversationDetailDto : AiConversationDto
{
    public List<AiMessageDto> Messages { get; set; } = [];
}

public class AiConversationCreateInput
{
    [MaxLength(128)]
    public string ContextId { get; set; } = string.Empty;

    [MaxLength(8000)]
    public string ContextSnapshotJson { get; set; } = "{}";

    [MaxLength(32)]
    public string ContextType { get; set; } = "None";

    public Guid? ModelId { get; set; }

    [MaxLength(32)]
    public string Mode { get; set; } = "General";

    [Required]
    [MaxLength(128)]
    public string Title { get; set; } = string.Empty;
}

public class AiConversationUpdateInput
{
    [MaxLength(128)]
    public string ContextId { get; set; } = string.Empty;

    [MaxLength(8000)]
    public string ContextSnapshotJson { get; set; } = "{}";

    [MaxLength(32)]
    public string ContextType { get; set; } = "None";

    public Guid? ModelId { get; set; }

    [Required]
    [MaxLength(128)]
    public string Title { get; set; } = string.Empty;
}

public class AiConversationQueryInput
{
    [MaxLength(64)]
    public string? Keyword { get; set; }

    [MaxLength(32)]
    public string? Status { get; set; } = "Active";
}

public class AiConversationSendInput
{
    [MaxLength(128)]
    public string AgentCode { get; set; } = string.Empty;

    [MaxLength(128)]
    public string CommandCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(16_000)]
    public string Content { get; set; } = string.Empty;

    public Guid? ModelId { get; set; }

    [MaxLength(128)]
    public string SkillCode { get; set; } = string.Empty;
}

public class AiConversationExportDto
{
    public string Content { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;
}

public class AiMessageDto
{
    public List<string> Citations { get; set; } = [];

    public int CompletionTokens { get; set; }

    public string Content { get; set; } = string.Empty;

    public Guid ConversationId { get; set; }

    public DateTime CreationTime { get; set; }

    public string ErrorMessage { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public string MetadataJson { get; set; } = "{}";

    public Guid? ModelId { get; set; }

    public string ModelName { get; set; } = string.Empty;

    public int PromptTokens { get; set; }

    public Guid? ProviderId { get; set; }

    public string ProviderName { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public int SequenceNo { get; set; }

    public string Status { get; set; } = string.Empty;

    public int TotalTokens { get; set; }
}

public class AiConversationSendResultDto
{
    public AiConversationDto Conversation { get; set; } = new();

    public List<AiMessageDto> Messages { get; set; } = [];
}

public class AiConversationStreamEventDto
{
    public string Delta { get; set; } = string.Empty;

    public string ErrorMessage { get; set; } = string.Empty;

    public string Event { get; set; } = string.Empty;

    public AiConversationSendResultDto? Result { get; set; }

    public AiMessageDto? UserMessage { get; set; }

    public static AiConversationStreamEventDto CompletedEvent(AiConversationSendResultDto result)
    {
        return new AiConversationStreamEventDto
        {
            Event = "completed",
            Result = result
        };
    }

    public static AiConversationStreamEventDto DeltaEvent(string delta)
    {
        return new AiConversationStreamEventDto
        {
            Delta = delta,
            Event = "delta"
        };
    }

    public static AiConversationStreamEventDto ErrorEvent(string errorMessage)
    {
        return new AiConversationStreamEventDto
        {
            ErrorMessage = errorMessage,
            Event = "error"
        };
    }

    public static AiConversationStreamEventDto UserMessageEvent(AiMessageDto userMessage)
    {
        return new AiConversationStreamEventDto
        {
            Event = "userMessage",
            UserMessage = userMessage
        };
    }
}

public class AiPlanConfirmInput
{
    public bool Confirmed { get; set; }

    public Guid MessageId { get; set; }
}

public class AiPlanStepToggleInput
{
    public bool IsDone { get; set; }

    [Required]
    [MaxLength(64)]
    public string StepId { get; set; } = string.Empty;

    public Guid MessageId { get; set; }
}

public class AiPlanDraftInput
{
    public Guid MessageId { get; set; }

    [MaxLength(128)]
    public string Title { get; set; } = string.Empty;
}

public class AiPromptTemplateConversationCreateInput
{
    [MaxLength(128)]
    public string ContextId { get; set; } = string.Empty;

    [MaxLength(8000)]
    public string ContextSnapshotJson { get; set; } = "{}";

    [MaxLength(32)]
    public string ContextType { get; set; } = "None";

    [MaxLength(8000)]
    public string InputJson { get; set; } = "{}";

    public Guid? ModelId { get; set; }

    [MaxLength(128)]
    public string Title { get; set; } = string.Empty;
}

public class AiPromptTemplateDryRunInput
{
    [MaxLength(128)]
    public string ContextId { get; set; } = string.Empty;

    [MaxLength(8000)]
    public string ContextSnapshotJson { get; set; } = "{}";

    [MaxLength(32)]
    public string ContextType { get; set; } = "None";

    [MaxLength(8000)]
    public string InputJson { get; set; } = "{}";
}

public class AiPromptTemplateDryRunBatchInput : AiPromptTemplateDryRunInput
{
    public bool IncludeDisabled { get; set; }

    public bool OnlyBuiltin { get; set; } = true;

    public List<string> TemplateCodes { get; set; } = [];
}

public class AiPromptTemplateDryRunResultDto
{
    public string AssistantCode { get; set; } = string.Empty;

    public string AssistantName { get; set; } = string.Empty;

    public string ContextId { get; set; } = string.Empty;

    public string ContextSnapshotJson { get; set; } = "{}";

    public string ContextType { get; set; } = "None";

    public string DisplayContent { get; set; } = string.Empty;

    public List<string> Errors { get; set; } = [];

    public int EstimatedCompletionTokens { get; set; }

    public int EstimatedPromptTokens { get; set; }

    public string InputJson { get; set; } = "{}";

    public bool IsModelCallSkipped { get; set; } = true;

    public List<string> MissingToolCodes { get; set; } = [];

    public string Mode { get; set; } = "General";

    public string RenderedPrompt { get; set; } = string.Empty;

    public List<string> RecommendedToolCodes { get; set; } = [];

    public string Scenario { get; set; } = string.Empty;

    public string StructuredOutputJson { get; set; } = "{}";

    public bool Success { get; set; }

    public string TemplateCode { get; set; } = string.Empty;

    public Guid TemplateId { get; set; }

    public string TemplateName { get; set; } = string.Empty;

    public List<string> Warnings { get; set; } = [];
}

public class AiPromptTemplateDryRunBatchResultDto
{
    public int FailedCount { get; set; }

    public List<AiPromptTemplateDryRunResultDto> Items { get; set; } = [];

    public List<string> MissingRequiredTemplateCodes { get; set; } = [];

    public int PassedCount { get; set; }

    public bool Success { get; set; }

    public int TotalCount { get; set; }
}

public class AiScenarioAcceptanceInput : AiPromptTemplateConversationCreateInput
{
    public List<string> TemplateCodes { get; set; } = [];
}

public class AiScenarioAcceptanceItemDto
{
    public string AcceptanceCode { get; set; } = string.Empty;

    public string AcceptanceName { get; set; } = string.Empty;

    public string AssistantMessagePreview { get; set; } = string.Empty;

    public Guid? ConversationId { get; set; }

    public string ErrorMessage { get; set; } = string.Empty;

    public string Mode { get; set; } = string.Empty;

    public string Scenario { get; set; } = string.Empty;

    public bool Success { get; set; }

    public string TemplateCode { get; set; } = string.Empty;

    public Guid? TemplateId { get; set; }

    public string TemplateName { get; set; } = string.Empty;

    public List<string> ValidationMessages { get; set; } = [];
}

public class AiScenarioAcceptanceResultDto
{
    public int FailedCount { get; set; }

    public List<AiScenarioAcceptanceItemDto> Items { get; set; } = [];

    public List<string> MissingRequiredTemplateCodes { get; set; } = [];

    public int PassedCount { get; set; }

    public bool Success { get; set; }

    public int TotalCount { get; set; }
}
