using System.ComponentModel.DataAnnotations;

namespace General.Admin.Ai;

public class AiMemoryDto
{
    public string Content { get; set; } = string.Empty;

    public string ContextId { get; set; } = string.Empty;

    public string ContextType { get; set; } = string.Empty;

    public DateTime CreationTime { get; set; }

    public Guid Id { get; set; }

    public int Importance { get; set; }

    public bool IsEnabled { get; set; }

    public Guid? KnowledgeSourceId { get; set; }

    public string KnowledgeSourceTitle { get; set; } = string.Empty;

    public DateTime? LastUsedTime { get; set; }

    public string MemoryType { get; set; } = string.Empty;

    public Guid? OwnerUserId { get; set; }

    public string SourceReferencesJson { get; set; } = "[]";

    public string Summary { get; set; } = string.Empty;

    public string TagsJson { get; set; } = "[]";

    public string Title { get; set; } = string.Empty;

    public int UseCount { get; set; }
}

public class AiMemorySaveInput
{
    [MaxLength(8000)]
    public string Content { get; set; } = string.Empty;

    [MaxLength(128)]
    public string? ContextId { get; set; }

    [MaxLength(64)]
    public string? ContextType { get; set; }

    public int Importance { get; set; } = 50;

    public bool IsEnabled { get; set; } = true;

    public Guid? KnowledgeSourceId { get; set; }

    [Required]
    [MaxLength(32)]
    public string MemoryType { get; set; } = "Project";

    public Guid? OwnerUserId { get; set; }

    [MaxLength(16000)]
    public string SourceReferencesJson { get; set; } = "[]";

    [MaxLength(2000)]
    public string Summary { get; set; } = string.Empty;

    [MaxLength(16000)]
    public string TagsJson { get; set; } = "[]";

    [Required]
    [MaxLength(256)]
    public string Title { get; set; } = string.Empty;
}

public class AiMemoryQueryInput
{
    [MaxLength(128)]
    public string ContextId { get; set; } = string.Empty;

    [MaxLength(64)]
    public string ContextType { get; set; } = string.Empty;

    public bool IncludeDisabled { get; set; } = true;

    public Guid? KnowledgeSourceId { get; set; }

    [MaxLength(128)]
    public string? Keyword { get; set; }

    public int MaxResultCount { get; set; } = 200;

    [MaxLength(32)]
    public string? MemoryType { get; set; }

    public Guid? OwnerUserId { get; set; }
}

public class AiMemoryToggleInput
{
    public bool IsEnabled { get; set; }
}

public class AiKnowledgeSourceDto
{
    public string ContextId { get; set; } = string.Empty;

    public string ContextType { get; set; } = string.Empty;

    public string ContentText { get; set; } = string.Empty;

    public DateTime CreationTime { get; set; }

    public Guid Id { get; set; }

    public DateTime LastIndexedTime { get; set; }

    public string MetadataJson { get; set; } = "{}";

    public Guid? ProjectId { get; set; }

    public string SourceId { get; set; } = string.Empty;

    public string SourceType { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
}

public class AiKnowledgeSourceQueryInput
{
    [MaxLength(128)]
    public string? ContextId { get; set; }

    [MaxLength(64)]
    public string? ContextType { get; set; }

    [MaxLength(128)]
    public string? Keyword { get; set; }

    public int MaxResultCount { get; set; } = 200;

    public Guid? ProjectId { get; set; }

    [MaxLength(64)]
    public string? SourceType { get; set; }
}

public class AiMemoryImportResultDto
{
    public int ImportedCount { get; set; }

    public Guid ProjectId { get; set; }

    public int SkippedCount { get; set; }

    public string SourceType { get; set; } = string.Empty;
}

public class AiConversationMemoryContextDto
{
    public List<string> Citations { get; set; } = [];

    public string ContextPrompt { get; set; } = string.Empty;
}
