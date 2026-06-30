using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Ai;

/// <summary>
/// AI 工具调用记录。保存输入、输出、耗时、错误和引用来源。
/// </summary>
public class AiToolInvocation : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? ConversationId { get; private set; }

    public long ElapsedMilliseconds { get; private set; }

    public string ErrorMessage { get; private set; }

    public string InputJson { get; private set; }

    public Guid? MessageId { get; private set; }

    public string OutputJson { get; private set; }

    public string SourceReferencesJson { get; private set; }

    public string Status { get; private set; }

    public Guid? TenantId { get; private set; }

    public int TimeoutSeconds { get; private set; }

    public string ToolCode { get; private set; }

    protected AiToolInvocation()
    {
        ErrorMessage = string.Empty;
        InputJson = "{}";
        OutputJson = "{}";
        SourceReferencesJson = "[]";
        Status = string.Empty;
        ToolCode = string.Empty;
    }

    public AiToolInvocation(
        Guid id,
        string toolCode,
        string inputJson,
        int timeoutSeconds,
        Guid? tenantId,
        Guid? conversationId = null,
        Guid? messageId = null) : base(id)
    {
        ToolCode = Check.NotNullOrWhiteSpace(toolCode, nameof(toolCode), 128).Trim();
        InputJson = NormalizeJson(inputJson, 8000, "{}");
        TimeoutSeconds = Math.Clamp(timeoutSeconds, 1, 120);
        TenantId = tenantId;
        ConversationId = conversationId;
        MessageId = messageId;
        OutputJson = "{}";
        SourceReferencesJson = "[]";
        ErrorMessage = string.Empty;
        Status = "Running";
    }

    public void Complete(string outputJson, string sourceReferencesJson, long elapsedMilliseconds)
    {
        OutputJson = NormalizeJson(outputJson, 64_000, "{}");
        SourceReferencesJson = NormalizeJson(sourceReferencesJson, 16_000, "[]");
        ElapsedMilliseconds = Math.Max(0, elapsedMilliseconds);
        ErrorMessage = string.Empty;
        Status = "Completed";
    }

    public void Fail(string status, string errorMessage, long elapsedMilliseconds)
    {
        Status = Normalize(status, 32, "Failed");
        ErrorMessage = Normalize(errorMessage, 2000);
        ElapsedMilliseconds = Math.Max(0, elapsedMilliseconds);
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
