using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.Ai;

/// <summary>
/// AI 会话消息。保存输入、输出、错误和引用来源，后续流式输出可持续更新同一条助手消息。
/// </summary>
public class AiMessage : FullAuditedAggregateRoot<Guid>
{
    public string CitationsJson { get; private set; }

    public int CompletionTokens { get; private set; }

    public string Content { get; private set; }

    public Guid ConversationId { get; private set; }

    public string ErrorMessage { get; private set; }

    public string MetadataJson { get; private set; }

    public Guid? ModelId { get; private set; }

    public int PromptTokens { get; private set; }

    public Guid? ProviderId { get; private set; }

    public string Role { get; private set; }

    public int SequenceNo { get; private set; }

    public string Status { get; private set; }

    public int TotalTokens { get; private set; }

    protected AiMessage()
    {
        CitationsJson = "[]";
        Content = string.Empty;
        ErrorMessage = string.Empty;
        MetadataJson = "{}";
        Role = string.Empty;
        Status = string.Empty;
    }

    public AiMessage(
        Guid id,
        Guid conversationId,
        string role,
        string content,
        int sequenceNo,
        string status = "Completed",
        Guid? providerId = null,
        Guid? modelId = null,
        int promptTokens = 0,
        int completionTokens = 0,
        string? citationsJson = null,
        string? metadataJson = null,
        string? errorMessage = null) : base(id)
    {
        ConversationId = conversationId;
        Role = Check.NotNullOrWhiteSpace(role, nameof(role), 32).Trim();
        Content = Normalize(content, 16_000);
        SequenceNo = sequenceNo;
        Status = Check.NotNullOrWhiteSpace(status, nameof(status), 32).Trim();
        ProviderId = providerId;
        ModelId = modelId;
        PromptTokens = Math.Max(0, promptTokens);
        CompletionTokens = Math.Max(0, completionTokens);
        TotalTokens = PromptTokens + CompletionTokens;
        CitationsJson = NormalizeJsonArray(citationsJson);
        MetadataJson = NormalizeJson(metadataJson);
        ErrorMessage = Normalize(errorMessage, 2000);
    }

    public void Complete(string content, int promptTokens, int completionTokens, string? citationsJson = null, string? metadataJson = null)
    {
        Content = Normalize(content, 16_000);
        PromptTokens = Math.Max(0, promptTokens);
        CompletionTokens = Math.Max(0, completionTokens);
        TotalTokens = PromptTokens + CompletionTokens;
        CitationsJson = NormalizeJsonArray(citationsJson);
        MetadataJson = NormalizeJson(metadataJson);
        ErrorMessage = string.Empty;
        Status = "Completed";
    }

    public void Fail(string errorMessage)
    {
        ErrorMessage = Normalize(errorMessage, 2000);
        Status = "Failed";
    }

    public void UpdateStructuredContent(string content, string? metadataJson)
    {
        Content = Normalize(content, 16_000);
        MetadataJson = NormalizeJson(metadataJson);
    }

    private static string Normalize(string? value, int maxLength)
    {
        var normalized = value?.Trim() ?? string.Empty;
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
