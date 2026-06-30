using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Ai;

/// <summary>
/// AI 助手角色。定义角色定位、默认模式、默认工具和系统提示词。
/// </summary>
public class AiAssistant : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public string AssistantCode { get; private set; }

    public string ConfigJson { get; private set; }

    public string DefaultMode { get; private set; }

    public Guid? DefaultModelId { get; private set; }

    public string Description { get; private set; }

    public bool IsBuiltin { get; private set; }

    public bool IsEnabled { get; private set; }

    public string Name { get; private set; }

    public string RoleType { get; private set; }

    public int Sort { get; private set; }

    public string SystemPrompt { get; private set; }

    public Guid? TenantId { get; private set; }

    public string ToolCodesJson { get; private set; }

    protected AiAssistant()
    {
        AssistantCode = string.Empty;
        ConfigJson = "{}";
        DefaultMode = "General";
        Description = string.Empty;
        Name = string.Empty;
        RoleType = string.Empty;
        SystemPrompt = string.Empty;
        ToolCodesJson = "[]";
    }

    public AiAssistant(
        Guid id,
        string assistantCode,
        string name,
        string roleType,
        string systemPrompt,
        Guid? tenantId,
        string defaultMode = "General",
        Guid? defaultModelId = null,
        string? description = null,
        string? toolCodesJson = null,
        string? configJson = null,
        bool isEnabled = true,
        bool isBuiltin = false,
        int sort = 0) : base(id)
    {
        AssistantCode = Check.NotNullOrWhiteSpace(assistantCode, nameof(assistantCode), 64).Trim();
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 128).Trim();
        RoleType = Normalize(roleType, 32, "General");
        SystemPrompt = Check.NotNullOrWhiteSpace(systemPrompt, nameof(systemPrompt), 16_000).Trim();
        TenantId = tenantId;
        DefaultMode = AiConversationModeNames.Normalize(defaultMode);
        DefaultModelId = defaultModelId;
        Description = Normalize(description, 512);
        ToolCodesJson = NormalizeJsonArray(toolCodesJson);
        ConfigJson = NormalizeJson(configJson);
        IsEnabled = isEnabled;
        IsBuiltin = isBuiltin;
        Sort = sort;
    }

    public void Update(
        string name,
        string roleType,
        string systemPrompt,
        string defaultMode,
        Guid? defaultModelId,
        string? description,
        string? toolCodesJson,
        string? configJson,
        bool isEnabled,
        int sort)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 128).Trim();
        RoleType = Normalize(roleType, 32, "General");
        SystemPrompt = Check.NotNullOrWhiteSpace(systemPrompt, nameof(systemPrompt), 16_000).Trim();
        DefaultMode = AiConversationModeNames.Normalize(defaultMode);
        DefaultModelId = defaultModelId;
        Description = Normalize(description, 512);
        ToolCodesJson = NormalizeJsonArray(toolCodesJson);
        ConfigJson = NormalizeJson(configJson);
        IsEnabled = isEnabled;
        Sort = sort;
    }

    private static string Normalize(string? value, int maxLength, string defaultValue = "")
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private static string NormalizeJson(string? value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? "{}" : value.Trim();
        return normalized.Length <= 8000 ? normalized : normalized[..8000];
    }

    private static string NormalizeJsonArray(string? value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? "[]" : value.Trim();
        return normalized.Length <= 8000 ? normalized : normalized[..8000];
    }
}
