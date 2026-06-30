using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Ai;

/// <summary>
/// AI 工具定义。只允许白名单工具被会话和自动化任务调用。
/// </summary>
public class AiToolDefinition : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public string Category { get; private set; }

    public string Description { get; private set; }

    public string InputSchemaJson { get; private set; }

    public bool IsEnabled { get; private set; }

    public string Name { get; private set; }

    public string OutputSchemaJson { get; private set; }

    public string PermissionCode { get; private set; }

    public int Sort { get; private set; }

    public Guid? TenantId { get; private set; }

    public int TimeoutSeconds { get; private set; }

    public string ToolCode { get; private set; }

    protected AiToolDefinition()
    {
        Category = string.Empty;
        Description = string.Empty;
        InputSchemaJson = "{}";
        Name = string.Empty;
        OutputSchemaJson = "{}";
        PermissionCode = string.Empty;
        ToolCode = string.Empty;
    }

    public AiToolDefinition(
        Guid id,
        string toolCode,
        string name,
        string description,
        string category,
        string inputSchemaJson,
        string outputSchemaJson,
        string permissionCode,
        Guid? tenantId,
        bool isEnabled = true,
        int timeoutSeconds = 15,
        int sort = 0) : base(id)
    {
        ToolCode = Check.NotNullOrWhiteSpace(toolCode, nameof(toolCode), 128).Trim();
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 128).Trim();
        Description = Normalize(description, 512);
        Category = Normalize(category, 64, "Project");
        InputSchemaJson = NormalizeJson(inputSchemaJson, 8000);
        OutputSchemaJson = NormalizeJson(outputSchemaJson, 8000);
        PermissionCode = Normalize(permissionCode, 128);
        TenantId = tenantId;
        IsEnabled = isEnabled;
        TimeoutSeconds = NormalizeTimeout(timeoutSeconds);
        Sort = sort;
    }

    public void UpdateMetadata(
        string name,
        string description,
        string category,
        string inputSchemaJson,
        string outputSchemaJson,
        string permissionCode,
        int timeoutSeconds,
        int sort)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 128).Trim();
        Description = Normalize(description, 512);
        Category = Normalize(category, 64, "Project");
        InputSchemaJson = NormalizeJson(inputSchemaJson, 8000);
        OutputSchemaJson = NormalizeJson(outputSchemaJson, 8000);
        PermissionCode = Normalize(permissionCode, 128);
        TimeoutSeconds = NormalizeTimeout(timeoutSeconds);
        Sort = sort;
    }

    public void SetEnabled(bool isEnabled)
    {
        IsEnabled = isEnabled;
    }

    private static int NormalizeTimeout(int value)
    {
        return Math.Clamp(value, 1, 120);
    }

    private static string Normalize(string? value, int maxLength, string defaultValue = "")
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private static string NormalizeJson(string? value, int maxLength)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? "{}" : value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }
}
