using System.ComponentModel.DataAnnotations;

namespace General.Admin.Ai;

public class AiAgentDesignQueryInput
{
    [MaxLength(64)]
    public string? Category { get; set; }

    public bool IncludeDisabled { get; set; } = true;

    [MaxLength(64)]
    public string? Keyword { get; set; }

    [MaxLength(32)]
    public string? RoleType { get; set; }
}

public class AiSkillDto
{
    public string Category { get; set; } = string.Empty;

    public DateTime CreationTime { get; set; }

    public string DefaultMode { get; set; } = "General";

    public string Description { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public string InputSchemaJson { get; set; } = "{}";

    public string Instruction { get; set; } = string.Empty;

    public bool IsBuiltin { get; set; }

    public bool IsEnabled { get; set; }

    public string Name { get; set; } = string.Empty;

    public string OutputSchemaJson { get; set; } = "{}";

    public string SkillCode { get; set; } = string.Empty;

    public int Sort { get; set; }

    public List<string> ToolCodes { get; set; } = [];
}

public class AiSkillSaveInput
{
    [MaxLength(64)]
    public string Category { get; set; } = "项目管理";

    [MaxLength(32)]
    public string DefaultMode { get; set; } = "General";

    [MaxLength(512)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(8000)]
    public string InputSchemaJson { get; set; } = "{}";

    [Required]
    [MaxLength(16_000)]
    public string Instruction { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(8000)]
    public string OutputSchemaJson { get; set; } = "{}";

    [Required]
    [MaxLength(128)]
    public string SkillCode { get; set; } = string.Empty;

    public int Sort { get; set; }

    public List<string> ToolCodes { get; set; } = [];
}

public class AiPersonaDto
{
    public string BoundaryPrompt { get; set; } = string.Empty;

    public string ConfigJson { get; set; } = "{}";

    public DateTime CreationTime { get; set; }

    public string Description { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public bool IsBuiltin { get; set; }

    public bool IsEnabled { get; set; }

    public string Name { get; set; } = string.Empty;

    public string PersonaCode { get; set; } = string.Empty;

    public string PersonalityPrompt { get; set; } = string.Empty;

    public string RoleType { get; set; } = string.Empty;

    public int Sort { get; set; }

    public string StylePrompt { get; set; } = string.Empty;
}

public class AiPersonaSaveInput
{
    [MaxLength(8000)]
    public string BoundaryPrompt { get; set; } = string.Empty;

    [MaxLength(8000)]
    public string ConfigJson { get; set; } = "{}";

    [MaxLength(512)]
    public string Description { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string PersonaCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(8000)]
    public string PersonalityPrompt { get; set; } = string.Empty;

    [MaxLength(32)]
    public string RoleType { get; set; } = "General";

    public int Sort { get; set; }

    [MaxLength(4000)]
    public string StylePrompt { get; set; } = string.Empty;
}

public class AiCommandDto
{
    public string AgentCode { get; set; } = string.Empty;

    public string CommandCode { get; set; } = string.Empty;

    public string ConfigJson { get; set; } = "{}";

    public DateTime CreationTime { get; set; }

    public string Description { get; set; } = string.Empty;

    public string ExampleInput { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public string InputPlaceholder { get; set; } = string.Empty;

    public bool IsBuiltin { get; set; }

    public bool IsEnabled { get; set; }

    public string Mode { get; set; } = "General";

    public string Name { get; set; } = string.Empty;

    public string PromptTemplateCode { get; set; } = string.Empty;

    public string SkillCode { get; set; } = string.Empty;

    public int Sort { get; set; }

    public string SlashName { get; set; } = string.Empty;
}

public class AiCommandSaveInput
{
    [MaxLength(128)]
    public string AgentCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string CommandCode { get; set; } = string.Empty;

    [MaxLength(8000)]
    public string ConfigJson { get; set; } = "{}";

    [MaxLength(512)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(512)]
    public string ExampleInput { get; set; } = string.Empty;

    [MaxLength(256)]
    public string InputPlaceholder { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    [MaxLength(32)]
    public string Mode { get; set; } = "General";

    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(128)]
    public string PromptTemplateCode { get; set; } = string.Empty;

    [MaxLength(128)]
    public string SkillCode { get; set; } = string.Empty;

    public int Sort { get; set; }

    [Required]
    [MaxLength(64)]
    public string SlashName { get; set; } = string.Empty;
}

public class AiAgentDefinitionDto
{
    public string AgentCode { get; set; } = string.Empty;

    public DateTime CreationTime { get; set; }

    public Guid? DefaultModelId { get; set; }

    public string DefaultModelName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public bool IsBuiltin { get; set; }

    public bool IsEnabled { get; set; }

    public string Name { get; set; } = string.Empty;

    public string PersonaCode { get; set; } = string.Empty;

    public string RoleType { get; set; } = string.Empty;

    public string RuntimeConfigJson { get; set; } = "{}";

    public List<string> SkillCodes { get; set; } = [];

    public int Sort { get; set; }

    public string SystemPrompt { get; set; } = string.Empty;

    public List<string> ToolCodes { get; set; } = [];
}

public class AiAgentDefinitionSaveInput
{
    [Required]
    [MaxLength(128)]
    public string AgentCode { get; set; } = string.Empty;

    public Guid? DefaultModelId { get; set; }

    [MaxLength(512)]
    public string Description { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(128)]
    public string PersonaCode { get; set; } = string.Empty;

    [MaxLength(32)]
    public string RoleType { get; set; } = "General";

    [MaxLength(8000)]
    public string RuntimeConfigJson { get; set; } = "{}";

    public List<string> SkillCodes { get; set; } = [];

    public int Sort { get; set; }

    [Required]
    [MaxLength(16_000)]
    public string SystemPrompt { get; set; } = string.Empty;

    public List<string> ToolCodes { get; set; } = [];
}

public class AiAgentRunPlanInput
{
    [MaxLength(8000)]
    public string InputJson { get; set; } = "{}";
}

public class AiAgentRunPlanDto
{
    public string AgentCode { get; set; } = string.Empty;

    public string AgentName { get; set; } = string.Empty;

    public string ModelName { get; set; } = string.Empty;

    public string PersonaCode { get; set; } = string.Empty;

    public List<string> SkillCodes { get; set; } = [];

    public string SystemPrompt { get; set; } = string.Empty;

    public List<string> ToolCodes { get; set; } = [];
}
