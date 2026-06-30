using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Ai;

/// <summary>
/// AI 记忆。按组织、项目、用户、场景分层，支持来源引用、启停和删除审计。
/// </summary>
public class AiMemory : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public string Content { get; private set; }

    public string ContextId { get; private set; }

    public string ContextType { get; private set; }

    public int Importance { get; private set; }

    public bool IsEnabled { get; private set; }

    public Guid? KnowledgeSourceId { get; private set; }

    public DateTime? LastUsedTime { get; private set; }

    public string MemoryType { get; private set; }

    public Guid? OwnerUserId { get; private set; }

    public string SourceReferencesJson { get; private set; }

    public string Summary { get; private set; }

    public string TagsJson { get; private set; }

    public Guid? TenantId { get; private set; }

    public string Title { get; private set; }

    public int UseCount { get; private set; }

    protected AiMemory()
    {
        Content = string.Empty;
        ContextId = string.Empty;
        ContextType = string.Empty;
        MemoryType = string.Empty;
        SourceReferencesJson = "[]";
        Summary = string.Empty;
        TagsJson = "[]";
        Title = string.Empty;
    }

    public AiMemory(
        Guid id,
        string memoryType,
        string title,
        string content,
        Guid? tenantId,
        string? summary = null,
        string? contextType = null,
        string? contextId = null,
        Guid? ownerUserId = null,
        Guid? knowledgeSourceId = null,
        string? tagsJson = null,
        string? sourceReferencesJson = null,
        int importance = 50,
        bool isEnabled = true) : base(id)
    {
        MemoryType = NormalizeMemoryType(memoryType);
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), 256).Trim();
        Content = Check.NotNullOrWhiteSpace(content, nameof(content), 8000).Trim();
        TenantId = tenantId;
        Summary = Normalize(summary, 2000);
        ContextType = Normalize(contextType, 64);
        ContextId = Normalize(contextId, 128);
        OwnerUserId = ownerUserId;
        KnowledgeSourceId = knowledgeSourceId;
        TagsJson = NormalizeJsonArray(tagsJson);
        SourceReferencesJson = NormalizeJsonArray(sourceReferencesJson);
        Importance = Math.Clamp(importance, 0, 100);
        IsEnabled = isEnabled;
    }

    public void Update(
        string memoryType,
        string title,
        string content,
        string? summary,
        string? contextType,
        string? contextId,
        Guid? ownerUserId,
        Guid? knowledgeSourceId,
        string? tagsJson,
        string? sourceReferencesJson,
        int importance,
        bool isEnabled)
    {
        MemoryType = NormalizeMemoryType(memoryType);
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), 256).Trim();
        Content = Check.NotNullOrWhiteSpace(content, nameof(content), 8000).Trim();
        Summary = Normalize(summary, 2000);
        ContextType = Normalize(contextType, 64);
        ContextId = Normalize(contextId, 128);
        OwnerUserId = ownerUserId;
        KnowledgeSourceId = knowledgeSourceId;
        TagsJson = NormalizeJsonArray(tagsJson);
        SourceReferencesJson = NormalizeJsonArray(sourceReferencesJson);
        Importance = Math.Clamp(importance, 0, 100);
        IsEnabled = isEnabled;
    }

    public void Toggle(bool isEnabled)
    {
        IsEnabled = isEnabled;
    }

    public void MarkUsed(DateTime usedTime)
    {
        LastUsedTime = usedTime;
        UseCount++;
    }

    private static string NormalizeMemoryType(string? value)
    {
        var normalized = Normalize(value, 32, "Project");
        return normalized.Equals("organization", StringComparison.OrdinalIgnoreCase) ? "Organization" :
            normalized.Equals("project", StringComparison.OrdinalIgnoreCase) ? "Project" :
            normalized.Equals("user", StringComparison.OrdinalIgnoreCase) ? "User" :
            normalized.Equals("scenario", StringComparison.OrdinalIgnoreCase) ? "Scenario" :
            throw new UserFriendlyException("AI 记忆类型只支持 Organization、Project、User、Scenario。");
    }

    private static string Normalize(string? value, int maxLength, string defaultValue = "")
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private static string NormalizeJsonArray(string? value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? "[]" : value.Trim();
        return normalized.Length <= 16_000 ? normalized : normalized[..16_000];
    }
}
