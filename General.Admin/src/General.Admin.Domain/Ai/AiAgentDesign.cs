using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Ai;

/// <summary>
/// AI 技能。描述可被会话、Agent 和快捷命令复用的管理工作流。
/// </summary>
public class AiSkill : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public string Category { get; private set; }

    public string DefaultMode { get; private set; }

    public string Description { get; private set; }

    public string InputSchemaJson { get; private set; }

    public string Instruction { get; private set; }

    public bool IsBuiltin { get; private set; }

    public bool IsEnabled { get; private set; }

    public string Name { get; private set; }

    public string OutputSchemaJson { get; private set; }

    public string SkillCode { get; private set; }

    public int Sort { get; private set; }

    public Guid? TenantId { get; private set; }

    public string ToolCodesJson { get; private set; }

    protected AiSkill()
    {
        Category = string.Empty;
        DefaultMode = "General";
        Description = string.Empty;
        InputSchemaJson = "{}";
        Instruction = string.Empty;
        Name = string.Empty;
        OutputSchemaJson = "{}";
        SkillCode = string.Empty;
        ToolCodesJson = "[]";
    }

    public AiSkill(
        Guid id,
        string skillCode,
        string name,
        string category,
        string instruction,
        Guid? tenantId,
        string defaultMode = "General",
        string? description = null,
        string? inputSchemaJson = null,
        string? outputSchemaJson = null,
        string? toolCodesJson = null,
        bool isEnabled = true,
        bool isBuiltin = false,
        int sort = 0) : base(id)
    {
        SkillCode = Check.NotNullOrWhiteSpace(skillCode, nameof(skillCode), 128).Trim();
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 128).Trim();
        Category = Normalize(category, 64, "项目管理");
        Instruction = Check.NotNullOrWhiteSpace(instruction, nameof(instruction), 16_000).Trim();
        TenantId = tenantId;
        DefaultMode = AiConversationModeNames.Normalize(defaultMode);
        Description = Normalize(description, 512);
        InputSchemaJson = NormalizeJson(inputSchemaJson, 8000, "{}");
        OutputSchemaJson = NormalizeJson(outputSchemaJson, 8000, "{}");
        ToolCodesJson = NormalizeJson(toolCodesJson, 8000, "[]");
        IsEnabled = isEnabled;
        IsBuiltin = isBuiltin;
        Sort = sort;
    }

    public void Update(
        string name,
        string category,
        string instruction,
        string defaultMode,
        string? description,
        string? inputSchemaJson,
        string? outputSchemaJson,
        string? toolCodesJson,
        bool isEnabled,
        int sort)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 128).Trim();
        Category = Normalize(category, 64, "项目管理");
        Instruction = Check.NotNullOrWhiteSpace(instruction, nameof(instruction), 16_000).Trim();
        DefaultMode = AiConversationModeNames.Normalize(defaultMode);
        Description = Normalize(description, 512);
        InputSchemaJson = NormalizeJson(inputSchemaJson, 8000, "{}");
        OutputSchemaJson = NormalizeJson(outputSchemaJson, 8000, "{}");
        ToolCodesJson = NormalizeJson(toolCodesJson, 8000, "[]");
        IsEnabled = isEnabled;
        Sort = sort;
    }

    private static string Normalize(string? value, int maxLength, string defaultValue = "")
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private static string NormalizeJson(string? value, int maxLength, string defaultValue)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }
}

/// <summary>
/// AI 角色画像。替代 OpenCowork 的 SOUL 命名，用企业系统可理解的角色边界表达。
/// </summary>
public class AiPersona : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public string BoundaryPrompt { get; private set; }

    public string ConfigJson { get; private set; }

    public string Description { get; private set; }

    public bool IsBuiltin { get; private set; }

    public bool IsEnabled { get; private set; }

    public string Name { get; private set; }

    public string PersonaCode { get; private set; }

    public string PersonalityPrompt { get; private set; }

    public string RoleType { get; private set; }

    public int Sort { get; private set; }

    public string StylePrompt { get; private set; }

    public Guid? TenantId { get; private set; }

    protected AiPersona()
    {
        BoundaryPrompt = string.Empty;
        ConfigJson = "{}";
        Description = string.Empty;
        Name = string.Empty;
        PersonaCode = string.Empty;
        PersonalityPrompt = string.Empty;
        RoleType = "General";
        StylePrompt = string.Empty;
    }

    public AiPersona(
        Guid id,
        string personaCode,
        string name,
        string roleType,
        string personalityPrompt,
        string boundaryPrompt,
        string stylePrompt,
        Guid? tenantId,
        string? description = null,
        string? configJson = null,
        bool isEnabled = true,
        bool isBuiltin = false,
        int sort = 0) : base(id)
    {
        PersonaCode = Check.NotNullOrWhiteSpace(personaCode, nameof(personaCode), 128).Trim();
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 128).Trim();
        RoleType = Normalize(roleType, 32, "General");
        PersonalityPrompt = Check.NotNullOrWhiteSpace(personalityPrompt, nameof(personalityPrompt), 8000).Trim();
        BoundaryPrompt = Normalize(boundaryPrompt, 8000);
        StylePrompt = Normalize(stylePrompt, 4000);
        TenantId = tenantId;
        Description = Normalize(description, 512);
        ConfigJson = NormalizeJson(configJson, 8000, "{}");
        IsEnabled = isEnabled;
        IsBuiltin = isBuiltin;
        Sort = sort;
    }

    public void Update(
        string name,
        string roleType,
        string personalityPrompt,
        string boundaryPrompt,
        string stylePrompt,
        string? description,
        string? configJson,
        bool isEnabled,
        int sort)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 128).Trim();
        RoleType = Normalize(roleType, 32, "General");
        PersonalityPrompt = Check.NotNullOrWhiteSpace(personalityPrompt, nameof(personalityPrompt), 8000).Trim();
        BoundaryPrompt = Normalize(boundaryPrompt, 8000);
        StylePrompt = Normalize(stylePrompt, 4000);
        Description = Normalize(description, 512);
        ConfigJson = NormalizeJson(configJson, 8000, "{}");
        IsEnabled = isEnabled;
        Sort = sort;
    }

    private static string Normalize(string? value, int maxLength, string defaultValue = "")
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private static string NormalizeJson(string? value, int maxLength, string defaultValue)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }
}

/// <summary>
/// AI 快捷命令。用于会话输入区的 /周报、/风险分析 等命令。
/// </summary>
public class AiCommand : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public string AgentCode { get; private set; }

    public string CommandCode { get; private set; }

    public string ConfigJson { get; private set; }

    public string Description { get; private set; }

    public string ExampleInput { get; private set; }

    public string InputPlaceholder { get; private set; }

    public bool IsBuiltin { get; private set; }

    public bool IsEnabled { get; private set; }

    public string Mode { get; private set; }

    public string Name { get; private set; }

    public string PromptTemplateCode { get; private set; }

    public string SkillCode { get; private set; }

    public int Sort { get; private set; }

    public string SlashName { get; private set; }

    public Guid? TenantId { get; private set; }

    protected AiCommand()
    {
        AgentCode = string.Empty;
        CommandCode = string.Empty;
        ConfigJson = "{}";
        Description = string.Empty;
        ExampleInput = string.Empty;
        InputPlaceholder = string.Empty;
        Mode = "General";
        Name = string.Empty;
        PromptTemplateCode = string.Empty;
        SkillCode = string.Empty;
        SlashName = string.Empty;
    }

    public AiCommand(
        Guid id,
        string commandCode,
        string slashName,
        string name,
        Guid? tenantId,
        string mode = "General",
        string? skillCode = null,
        string? promptTemplateCode = null,
        string? agentCode = null,
        string? inputPlaceholder = null,
        string? exampleInput = null,
        string? description = null,
        string? configJson = null,
        bool isEnabled = true,
        bool isBuiltin = false,
        int sort = 0) : base(id)
    {
        CommandCode = Check.NotNullOrWhiteSpace(commandCode, nameof(commandCode), 128).Trim();
        SlashName = NormalizeSlashName(slashName);
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 128).Trim();
        TenantId = tenantId;
        Mode = AiConversationModeNames.Normalize(mode);
        SkillCode = Normalize(skillCode, 128);
        PromptTemplateCode = Normalize(promptTemplateCode, 128);
        AgentCode = Normalize(agentCode, 128);
        InputPlaceholder = Normalize(inputPlaceholder, 256);
        ExampleInput = Normalize(exampleInput, 512);
        Description = Normalize(description, 512);
        ConfigJson = NormalizeJson(configJson, 8000, "{}");
        IsEnabled = isEnabled;
        IsBuiltin = isBuiltin;
        Sort = sort;
    }

    public void Update(
        string slashName,
        string name,
        string mode,
        string? skillCode,
        string? promptTemplateCode,
        string? agentCode,
        string? inputPlaceholder,
        string? exampleInput,
        string? description,
        string? configJson,
        bool isEnabled,
        int sort)
    {
        SlashName = NormalizeSlashName(slashName);
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 128).Trim();
        Mode = AiConversationModeNames.Normalize(mode);
        SkillCode = Normalize(skillCode, 128);
        PromptTemplateCode = Normalize(promptTemplateCode, 128);
        AgentCode = Normalize(agentCode, 128);
        InputPlaceholder = Normalize(inputPlaceholder, 256);
        ExampleInput = Normalize(exampleInput, 512);
        Description = Normalize(description, 512);
        ConfigJson = NormalizeJson(configJson, 8000, "{}");
        IsEnabled = isEnabled;
        Sort = sort;
    }

    private static string NormalizeSlashName(string value)
    {
        var normalized = Check.NotNullOrWhiteSpace(value, nameof(value), 64).Trim();
        return normalized.StartsWith('/') ? normalized : $"/{normalized}";
    }

    private static string Normalize(string? value, int maxLength, string defaultValue = "")
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private static string NormalizeJson(string? value, int maxLength, string defaultValue)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }
}

/// <summary>
/// AI Agent 定义。组合角色画像、技能、工具、模型和运行时配置。
/// </summary>
public class AiAgentDefinition : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public string AgentCode { get; private set; }

    public Guid? DefaultModelId { get; private set; }

    public string Description { get; private set; }

    public bool IsBuiltin { get; private set; }

    public bool IsEnabled { get; private set; }

    public string Name { get; private set; }

    public string PersonaCode { get; private set; }

    public string RoleType { get; private set; }

    public string RuntimeConfigJson { get; private set; }

    public string SkillCodesJson { get; private set; }

    public int Sort { get; private set; }

    public string SystemPrompt { get; private set; }

    public Guid? TenantId { get; private set; }

    public string ToolCodesJson { get; private set; }

    protected AiAgentDefinition()
    {
        AgentCode = string.Empty;
        Description = string.Empty;
        Name = string.Empty;
        PersonaCode = string.Empty;
        RoleType = "General";
        RuntimeConfigJson = "{}";
        SkillCodesJson = "[]";
        SystemPrompt = string.Empty;
        ToolCodesJson = "[]";
    }

    public AiAgentDefinition(
        Guid id,
        string agentCode,
        string name,
        string roleType,
        string systemPrompt,
        Guid? tenantId,
        string? personaCode = null,
        Guid? defaultModelId = null,
        string? description = null,
        string? toolCodesJson = null,
        string? skillCodesJson = null,
        string? runtimeConfigJson = null,
        bool isEnabled = true,
        bool isBuiltin = false,
        int sort = 0) : base(id)
    {
        AgentCode = Check.NotNullOrWhiteSpace(agentCode, nameof(agentCode), 128).Trim();
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 128).Trim();
        RoleType = Normalize(roleType, 32, "General");
        SystemPrompt = Check.NotNullOrWhiteSpace(systemPrompt, nameof(systemPrompt), 16_000).Trim();
        TenantId = tenantId;
        PersonaCode = Normalize(personaCode, 128);
        DefaultModelId = defaultModelId;
        Description = Normalize(description, 512);
        ToolCodesJson = NormalizeJson(toolCodesJson, 8000, "[]");
        SkillCodesJson = NormalizeJson(skillCodesJson, 8000, "[]");
        RuntimeConfigJson = NormalizeJson(runtimeConfigJson, 8000, "{}");
        IsEnabled = isEnabled;
        IsBuiltin = isBuiltin;
        Sort = sort;
    }

    public void Update(
        string name,
        string roleType,
        string systemPrompt,
        string? personaCode,
        Guid? defaultModelId,
        string? description,
        string? toolCodesJson,
        string? skillCodesJson,
        string? runtimeConfigJson,
        bool isEnabled,
        int sort)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 128).Trim();
        RoleType = Normalize(roleType, 32, "General");
        SystemPrompt = Check.NotNullOrWhiteSpace(systemPrompt, nameof(systemPrompt), 16_000).Trim();
        PersonaCode = Normalize(personaCode, 128);
        DefaultModelId = defaultModelId;
        Description = Normalize(description, 512);
        ToolCodesJson = NormalizeJson(toolCodesJson, 8000, "[]");
        SkillCodesJson = NormalizeJson(skillCodesJson, 8000, "[]");
        RuntimeConfigJson = NormalizeJson(runtimeConfigJson, 8000, "{}");
        IsEnabled = isEnabled;
        Sort = sort;
    }

    private static string Normalize(string? value, int maxLength, string defaultValue = "")
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private static string NormalizeJson(string? value, int maxLength, string defaultValue)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }
}
