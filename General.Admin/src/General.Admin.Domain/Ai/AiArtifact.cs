using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Ai;

/// <summary>
/// AI 安全输出构件。用白名单 JSON 协议承接图表、指标卡和行动队列，替代任意 HTML Widget。
/// </summary>
public class AiArtifact : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public string ArtifactCode { get; private set; }

    public string Category { get; private set; }

    public string Description { get; private set; }

    public string ExamplePayloadJson { get; private set; }

    public bool IsBuiltin { get; private set; }

    public bool IsEnabled { get; private set; }

    public string JsonSchema { get; private set; }

    public string Name { get; private set; }

    public string RendererType { get; private set; }

    public int Sort { get; private set; }

    public string SystemHint { get; private set; }

    public Guid? TenantId { get; private set; }

    protected AiArtifact()
    {
        ArtifactCode = string.Empty;
        Category = string.Empty;
        Description = string.Empty;
        ExamplePayloadJson = "{}";
        JsonSchema = "{}";
        Name = string.Empty;
        RendererType = string.Empty;
        SystemHint = string.Empty;
    }

    public AiArtifact(
        Guid id,
        string artifactCode,
        string name,
        string category,
        string rendererType,
        string jsonSchema,
        Guid? tenantId,
        string? examplePayloadJson = null,
        string? systemHint = null,
        string? description = null,
        bool isEnabled = true,
        bool isBuiltin = false,
        int sort = 0) : base(id)
    {
        ArtifactCode = Check.NotNullOrWhiteSpace(artifactCode, nameof(artifactCode), 128).Trim();
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 128).Trim();
        Category = Normalize(category, 64, "Chart");
        RendererType = Normalize(rendererType, 64, "metric-card");
        JsonSchema = NormalizeJson(jsonSchema, 16_000, "{}");
        TenantId = tenantId;
        ExamplePayloadJson = NormalizeJson(examplePayloadJson, 16_000, "{}");
        SystemHint = Normalize(systemHint, 4000);
        Description = Normalize(description, 512);
        IsEnabled = isEnabled;
        IsBuiltin = isBuiltin;
        Sort = sort;
    }

    public void Update(
        string name,
        string category,
        string rendererType,
        string jsonSchema,
        string? examplePayloadJson,
        string? systemHint,
        string? description,
        bool isEnabled,
        int sort)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 128).Trim();
        Category = Normalize(category, 64, "Chart");
        RendererType = Normalize(rendererType, 64, "metric-card");
        JsonSchema = NormalizeJson(jsonSchema, 16_000, "{}");
        ExamplePayloadJson = NormalizeJson(examplePayloadJson, 16_000, "{}");
        SystemHint = Normalize(systemHint, 4000);
        Description = Normalize(description, 512);
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
