using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.Ai;

/// <summary>
/// AI 会话。负责保存用户上下文、模型选择、会话状态和统计口径。
/// </summary>
public class AiConversation : FullAuditedAggregateRoot<Guid>
{
    public DateTime? ArchivedTime { get; private set; }

    public int CompletionTokens { get; private set; }

    public string ContextId { get; private set; }

    public string ContextSnapshotJson { get; private set; }

    public string ContextType { get; private set; }

    public string LastMessageSummary { get; private set; }

    public DateTime? LastMessageTime { get; private set; }

    public int MessageCount { get; private set; }

    public Guid? ModelId { get; private set; }

    public string Mode { get; private set; }

    public int PromptTokens { get; private set; }

    public string Status { get; private set; }

    public string Title { get; private set; }

    public int TotalTokens { get; private set; }

    protected AiConversation()
    {
        ContextId = string.Empty;
        ContextSnapshotJson = "{}";
        ContextType = "None";
        LastMessageSummary = string.Empty;
        Mode = "General";
        Status = "Active";
        Title = string.Empty;
    }

    public AiConversation(
        Guid id,
        string title,
        Guid? modelId = null,
        string? mode = null,
        string? contextType = null,
        string? contextId = null,
        string? contextSnapshotJson = null) : base(id)
    {
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), 128).Trim();
        ModelId = modelId;
        Mode = AiConversationModeNames.Normalize(mode);
        ContextType = Normalize(contextType, 32, "None");
        ContextId = Normalize(contextId, 128);
        ContextSnapshotJson = NormalizeJson(contextSnapshotJson);
        Status = "Active";
        LastMessageSummary = string.Empty;
    }

    public void Rename(string title)
    {
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), 128).Trim();
    }

    public void UpdateModel(Guid? modelId)
    {
        ModelId = modelId;
    }

    public void UpdateContext(string? contextType, string? contextId, string? contextSnapshotJson)
    {
        ContextType = Normalize(contextType, 32, "None");
        ContextId = Normalize(contextId, 128);
        ContextSnapshotJson = NormalizeJson(contextSnapshotJson);
    }

    public void TouchMessage(string summary, int promptTokens, int completionTokens)
    {
        LastMessageSummary = Normalize(summary, 256);
        LastMessageTime = DateTime.UtcNow;
        MessageCount++;
        PromptTokens += Math.Max(0, promptTokens);
        CompletionTokens += Math.Max(0, completionTokens);
        TotalTokens = PromptTokens + CompletionTokens;
    }

    public void Archive()
    {
        Status = "Archived";
        ArchivedTime = DateTime.UtcNow;
    }

    public void Restore()
    {
        Status = "Active";
        ArchivedTime = null;
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
}
