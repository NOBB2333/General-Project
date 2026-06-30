using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Ai;

/// <summary>
/// AI 场景提示词模板。承接周报、会议纪要、风险分析、经营解释等管理场景。
/// </summary>
public class AiPromptTemplate : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public string AssistantCode { get; private set; }

    public string Category { get; private set; }

    public string Description { get; private set; }

    public string InputSchemaJson { get; private set; }

    public bool IsBuiltin { get; private set; }

    public bool IsEnabled { get; private set; }

    public string Mode { get; private set; }

    public string Name { get; private set; }

    public string OutputSchemaJson { get; private set; }

    public string PromptContent { get; private set; }

    public string Scenario { get; private set; }

    public int Sort { get; private set; }

    public string TemplateCode { get; private set; }

    public Guid? TenantId { get; private set; }

    public string ToolCodesJson { get; private set; }

    public int Version { get; private set; }

    protected AiPromptTemplate()
    {
        AssistantCode = string.Empty;
        Category = string.Empty;
        Description = string.Empty;
        InputSchemaJson = "{}";
        Mode = "General";
        Name = string.Empty;
        OutputSchemaJson = "{}";
        PromptContent = string.Empty;
        Scenario = string.Empty;
        TemplateCode = string.Empty;
        ToolCodesJson = "[]";
    }

    public AiPromptTemplate(
        Guid id,
        string templateCode,
        string name,
        string category,
        string scenario,
        string mode,
        string assistantCode,
        string promptContent,
        Guid? tenantId,
        string? description = null,
        string? inputSchemaJson = null,
        string? outputSchemaJson = null,
        string? toolCodesJson = null,
        int version = 1,
        bool isEnabled = true,
        bool isBuiltin = false,
        int sort = 0) : base(id)
    {
        TemplateCode = Check.NotNullOrWhiteSpace(templateCode, nameof(templateCode), 128).Trim();
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 128).Trim();
        Category = Normalize(category, 64, "项目管理");
        Scenario = Normalize(scenario, 64, "General");
        Mode = AiConversationModeNames.Normalize(mode);
        AssistantCode = Normalize(assistantCode, 64);
        PromptContent = Check.NotNullOrWhiteSpace(promptContent, nameof(promptContent), 16_000).Trim();
        TenantId = tenantId;
        Description = Normalize(description, 512);
        InputSchemaJson = NormalizeJson(inputSchemaJson);
        OutputSchemaJson = NormalizeJson(outputSchemaJson);
        ToolCodesJson = NormalizeJsonArray(toolCodesJson);
        Version = Math.Max(1, version);
        IsEnabled = isEnabled;
        IsBuiltin = isBuiltin;
        Sort = sort;
    }

    public void Update(
        string name,
        string category,
        string scenario,
        string mode,
        string assistantCode,
        string promptContent,
        string? description,
        string? inputSchemaJson,
        string? outputSchemaJson,
        string? toolCodesJson,
        int version,
        bool isEnabled,
        int sort)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 128).Trim();
        Category = Normalize(category, 64, "项目管理");
        Scenario = Normalize(scenario, 64, "General");
        Mode = AiConversationModeNames.Normalize(mode);
        AssistantCode = Normalize(assistantCode, 64);
        PromptContent = Check.NotNullOrWhiteSpace(promptContent, nameof(promptContent), 16_000).Trim();
        Description = Normalize(description, 512);
        InputSchemaJson = NormalizeJson(inputSchemaJson);
        OutputSchemaJson = NormalizeJson(outputSchemaJson);
        ToolCodesJson = NormalizeJsonArray(toolCodesJson);
        Version = Math.Max(1, version);
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
