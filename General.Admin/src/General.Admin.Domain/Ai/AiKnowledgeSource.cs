using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Ai;

/// <summary>
/// AI 知识源。记录记忆来自哪个项目文档、会议纪要、风险复盘或外部资料。
/// </summary>
public class AiKnowledgeSource : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public string ContextId { get; private set; }

    public string ContextType { get; private set; }

    public string ContentText { get; private set; }

    public DateTime LastIndexedTime { get; private set; }

    public string MetadataJson { get; private set; }

    public Guid? ProjectId { get; private set; }

    public string SourceId { get; private set; }

    public string SourceType { get; private set; }

    public string Status { get; private set; }

    public string Summary { get; private set; }

    public Guid? TenantId { get; private set; }

    public string Title { get; private set; }

    protected AiKnowledgeSource()
    {
        ContextId = string.Empty;
        ContextType = string.Empty;
        ContentText = string.Empty;
        MetadataJson = "{}";
        SourceId = string.Empty;
        SourceType = string.Empty;
        Status = "Active";
        Summary = string.Empty;
        Title = string.Empty;
    }

    public AiKnowledgeSource(
        Guid id,
        string sourceType,
        string sourceId,
        string title,
        string summary,
        string contentText,
        Guid? tenantId,
        string contextType = "",
        string contextId = "",
        Guid? projectId = null,
        string? metadataJson = null,
        string status = "Active") : base(id)
    {
        SourceType = Check.NotNullOrWhiteSpace(sourceType, nameof(sourceType), 64).Trim();
        SourceId = Check.NotNullOrWhiteSpace(sourceId, nameof(sourceId), 128).Trim();
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), 256).Trim();
        Summary = Normalize(summary, 4000);
        ContentText = Normalize(contentText, 16_000);
        TenantId = tenantId;
        ContextType = Normalize(contextType, 64);
        ContextId = Normalize(contextId, 128);
        ProjectId = projectId;
        MetadataJson = NormalizeJson(metadataJson);
        Status = Normalize(status, 32, "Active");
        LastIndexedTime = DateTime.UtcNow;
    }

    public void Refresh(
        string title,
        string summary,
        string contentText,
        string contextType,
        string contextId,
        Guid? projectId,
        string? metadataJson,
        string status = "Active")
    {
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), 256).Trim();
        Summary = Normalize(summary, 4000);
        ContentText = Normalize(contentText, 16_000);
        ContextType = Normalize(contextType, 64);
        ContextId = Normalize(contextId, 128);
        ProjectId = projectId;
        MetadataJson = NormalizeJson(metadataJson);
        Status = Normalize(status, 32, "Active");
        LastIndexedTime = DateTime.UtcNow;
    }

    private static string Normalize(string? value, int maxLength, string defaultValue = "")
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private static string NormalizeJson(string? value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? "{}" : value.Trim();
        return normalized.Length <= 16_000 ? normalized : normalized[..16_000];
    }
}
