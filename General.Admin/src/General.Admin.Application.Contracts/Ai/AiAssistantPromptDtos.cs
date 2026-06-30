using System.ComponentModel.DataAnnotations;

namespace General.Admin.Ai;

public class AiAssistantDto
{
    public string AssistantCode { get; set; } = string.Empty;

    public string ConfigJson { get; set; } = "{}";

    public DateTime CreationTime { get; set; }

    public string DefaultMode { get; set; } = "General";

    public Guid? DefaultModelId { get; set; }

    public string DefaultModelName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public bool IsBuiltin { get; set; }

    public bool IsEnabled { get; set; }

    public string Name { get; set; } = string.Empty;

    public string RoleType { get; set; } = string.Empty;

    public int Sort { get; set; }

    public string SystemPrompt { get; set; } = string.Empty;

    public List<string> ToolCodes { get; set; } = [];
}

public class AiAssistantSaveInput
{
    [Required]
    [MaxLength(64)]
    public string AssistantCode { get; set; } = string.Empty;

    [MaxLength(8000)]
    public string ConfigJson { get; set; } = "{}";

    [MaxLength(32)]
    public string DefaultMode { get; set; } = "General";

    public Guid? DefaultModelId { get; set; }

    [MaxLength(512)]
    public string Description { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(32)]
    public string RoleType { get; set; } = "General";

    public int Sort { get; set; }

    [Required]
    [MaxLength(16_000)]
    public string SystemPrompt { get; set; } = string.Empty;

    public List<string> ToolCodes { get; set; } = [];
}

public class AiPromptTemplateDto
{
    public string AssistantCode { get; set; } = string.Empty;

    public string AssistantName { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public DateTime CreationTime { get; set; }

    public string Description { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public string InputSchemaJson { get; set; } = "{}";

    public bool IsBuiltin { get; set; }

    public bool IsEnabled { get; set; }

    public string Mode { get; set; } = "General";

    public string Name { get; set; } = string.Empty;

    public string OutputSchemaJson { get; set; } = "{}";

    public string PromptContent { get; set; } = string.Empty;

    public string Scenario { get; set; } = string.Empty;

    public int Sort { get; set; }

    public string TemplateCode { get; set; } = string.Empty;

    public List<string> ToolCodes { get; set; } = [];

    public int Version { get; set; }
}

public class AiPromptTemplateSaveInput
{
    [MaxLength(64)]
    public string AssistantCode { get; set; } = string.Empty;

    [MaxLength(64)]
    public string Category { get; set; } = "项目管理";

    [MaxLength(512)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(8000)]
    public string InputSchemaJson { get; set; } = "{}";

    public bool IsEnabled { get; set; } = true;

    [MaxLength(32)]
    public string Mode { get; set; } = "General";

    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(8000)]
    public string OutputSchemaJson { get; set; } = "{}";

    [Required]
    [MaxLength(16_000)]
    public string PromptContent { get; set; } = string.Empty;

    [MaxLength(64)]
    public string Scenario { get; set; } = "General";

    public int Sort { get; set; }

    [Required]
    [MaxLength(128)]
    public string TemplateCode { get; set; } = string.Empty;

    public List<string> ToolCodes { get; set; } = [];

    public int Version { get; set; } = 1;
}

public class AiPromptTemplateQueryInput
{
    [MaxLength(64)]
    public string? AssistantCode { get; set; }

    [MaxLength(64)]
    public string? Category { get; set; }

    public bool IncludeDisabled { get; set; }

    [MaxLength(64)]
    public string? Keyword { get; set; }

    [MaxLength(64)]
    public string? Scenario { get; set; }
}
