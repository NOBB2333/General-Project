using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Ai;

/// <summary>
/// AI 用量、成本与审计记录。覆盖模型调用、内置工具、MCP 工具和自动化任务。
/// </summary>
public class AiUsageRecord : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public string AuditSummary { get; private set; }

    public Guid? ConversationId { get; private set; }

    public decimal CostAmount { get; private set; }

    public string Currency { get; private set; }

    public int CompletionTokens { get; private set; }

    public long ElapsedMilliseconds { get; private set; }

    public string ErrorMessage { get; private set; }

    public Guid? JobId { get; private set; }

    public string ModelCode { get; private set; }

    public Guid? ModelId { get; private set; }

    public Guid? ProjectId { get; private set; }

    public Guid? ProviderId { get; private set; }

    public int PromptInjectionRiskScore { get; private set; }

    public string PromptInjectionSignalsJson { get; private set; }

    public int PromptTokens { get; private set; }

    public string PromptTemplateCode { get; private set; }

    public string RequestPreview { get; private set; }

    public string ResponsePreview { get; private set; }

    public string Scenario { get; private set; }

    public string Source { get; private set; }

    public string Status { get; private set; }

    public Guid? TenantId { get; private set; }

    public string ToolCode { get; private set; }

    public int TotalTokens { get; private set; }

    public Guid? UserId { get; private set; }

    protected AiUsageRecord()
    {
        AuditSummary = string.Empty;
        Currency = "USD";
        ErrorMessage = string.Empty;
        ModelCode = string.Empty;
        PromptInjectionSignalsJson = "[]";
        PromptTemplateCode = string.Empty;
        RequestPreview = string.Empty;
        ResponsePreview = string.Empty;
        Scenario = string.Empty;
        Source = string.Empty;
        Status = string.Empty;
        ToolCode = string.Empty;
    }

    public AiUsageRecord(
        Guid id,
        string source,
        string status,
        Guid? tenantId,
        Guid? userId = null,
        Guid? projectId = null,
        Guid? conversationId = null,
        Guid? jobId = null,
        Guid? providerId = null,
        Guid? modelId = null,
        string? modelCode = null,
        string? toolCode = null,
        string? scenario = null,
        string? promptTemplateCode = null,
        int promptTokens = 0,
        int completionTokens = 0,
        decimal costAmount = 0,
        string? currency = null,
        long elapsedMilliseconds = 0,
        string? requestPreview = null,
        string? responsePreview = null,
        string? auditSummary = null,
        string? errorMessage = null,
        int promptInjectionRiskScore = 0,
        string? promptInjectionSignalsJson = null) : base(id)
    {
        Source = Normalize(source, 32, "ModelChat");
        Status = Normalize(status, 32, "Success");
        TenantId = tenantId;
        UserId = userId;
        ProjectId = projectId;
        ConversationId = conversationId;
        JobId = jobId;
        ProviderId = providerId;
        ModelId = modelId;
        ModelCode = Normalize(modelCode, 128);
        ToolCode = Normalize(toolCode, 128);
        Scenario = Normalize(scenario, 64);
        PromptTemplateCode = Normalize(promptTemplateCode, 128);
        PromptTokens = Math.Max(0, promptTokens);
        CompletionTokens = Math.Max(0, completionTokens);
        TotalTokens = PromptTokens + CompletionTokens;
        CostAmount = Math.Max(0, costAmount);
        Currency = Normalize(currency, 16, "USD").ToUpperInvariant();
        ElapsedMilliseconds = Math.Max(0, elapsedMilliseconds);
        RequestPreview = Normalize(requestPreview, 4000);
        ResponsePreview = Normalize(responsePreview, 4000);
        AuditSummary = Normalize(auditSummary, 1000);
        ErrorMessage = Normalize(errorMessage, 2000);
        PromptInjectionRiskScore = Math.Clamp(promptInjectionRiskScore, 0, 100);
        PromptInjectionSignalsJson = NormalizeJsonArray(promptInjectionSignalsJson);
    }

    private static string Normalize(string? value, int maxLength, string defaultValue = "")
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private static string NormalizeJsonArray(string? value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? "[]" : value.Trim();
        return normalized.Length <= 8000 ? normalized : normalized[..8000];
    }
}
