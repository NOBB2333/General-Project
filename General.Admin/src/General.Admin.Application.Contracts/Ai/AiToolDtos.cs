using System.ComponentModel.DataAnnotations;

namespace General.Admin.Ai;

public class AiToolDefinitionDto
{
    public string Category { get; set; } = string.Empty;

    public DateTime CreationTime { get; set; }

    public string Description { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public string InputSchemaJson { get; set; } = "{}";

    public bool IsEnabled { get; set; }

    public string Name { get; set; } = string.Empty;

    public string OutputSchemaJson { get; set; } = "{}";

    public string PermissionCode { get; set; } = string.Empty;

    public int Sort { get; set; }

    public int TimeoutSeconds { get; set; }

    public string ToolCode { get; set; } = string.Empty;
}

public class AiSourceReferenceDto
{
    public string Path { get; set; } = string.Empty;

    public string SourceId { get; set; } = string.Empty;

    public string SourceType { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
}

public class AiToolInvokeInput
{
    public Guid? ConversationId { get; set; }

    [MaxLength(8000)]
    public string InputJson { get; set; } = "{}";

    public Guid? MessageId { get; set; }
}

public class AiToolInvokeResultDto
{
    public long ElapsedMilliseconds { get; set; }

    public string ErrorMessage { get; set; } = string.Empty;

    public Guid InvocationId { get; set; }

    public string OutputJson { get; set; } = "{}";

    public List<AiSourceReferenceDto> SourceReferences { get; set; } = [];

    public string Status { get; set; } = string.Empty;

    public string ToolCode { get; set; } = string.Empty;
}

public class AiToolInvocationDto : AiToolInvokeResultDto
{
    public Guid? ConversationId { get; set; }

    public DateTime CreationTime { get; set; }

    public string InputJson { get; set; } = "{}";

    public Guid? MessageId { get; set; }

    public int TimeoutSeconds { get; set; }
}

public class AiToolInvocationQueryInput
{
    public Guid? ConversationId { get; set; }

    public int MaxResultCount { get; set; } = 50;

    public Guid? MessageId { get; set; }

    [MaxLength(32)]
    public string? Status { get; set; }

    [MaxLength(128)]
    public string? ToolCode { get; set; }
}

public class AiConversationToolContextDto
{
    public List<string> Citations { get; set; } = [];

    public string ContextPrompt { get; set; } = string.Empty;
}
