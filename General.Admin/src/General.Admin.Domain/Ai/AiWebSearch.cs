using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Ai;

/// <summary>
/// 联网搜索配置。企业环境必须显式配置 Provider、域名策略和调用上限。
/// </summary>
public class AiWebSearchConfig : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public string AllowDomainsJson { get; private set; }

    public string ApiKeyCipherText { get; private set; }

    public string ConfigJson { get; private set; }

    public string DenyDomainsJson { get; private set; }

    public string Endpoint { get; private set; }

    public bool IsDefault { get; private set; }

    public bool IsEnabled { get; private set; }

    public int MaxResults { get; private set; }

    public string Name { get; private set; }

    public string ProviderType { get; private set; }

    public string Remark { get; private set; }

    public Guid? TenantId { get; private set; }

    public int TimeoutSeconds { get; private set; }

    protected AiWebSearchConfig()
    {
        AllowDomainsJson = "[]";
        ApiKeyCipherText = string.Empty;
        ConfigJson = "{}";
        DenyDomainsJson = "[]";
        Endpoint = string.Empty;
        Name = string.Empty;
        ProviderType = string.Empty;
        Remark = string.Empty;
    }

    public AiWebSearchConfig(
        Guid id,
        string name,
        string providerType,
        string endpoint,
        string? apiKeyCipherText,
        Guid? tenantId,
        string? allowDomainsJson = null,
        string? denyDomainsJson = null,
        int maxResults = 5,
        int timeoutSeconds = 15,
        string? configJson = null,
        bool isEnabled = true,
        bool isDefault = false,
        string? remark = null) : base(id)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 128).Trim();
        ProviderType = Normalize(providerType, 64, "Custom");
        Endpoint = Normalize(endpoint, 512);
        ApiKeyCipherText = Normalize(apiKeyCipherText, 2048);
        TenantId = tenantId;
        AllowDomainsJson = NormalizeJson(allowDomainsJson, 8000, "[]");
        DenyDomainsJson = NormalizeJson(denyDomainsJson, 8000, "[]");
        MaxResults = Math.Clamp(maxResults, 1, 20);
        TimeoutSeconds = Math.Clamp(timeoutSeconds, 1, 60);
        ConfigJson = NormalizeJson(configJson, 8000, "{}");
        IsEnabled = isEnabled;
        IsDefault = isDefault;
        Remark = Normalize(remark, 512);
    }

    public void Update(
        string name,
        string providerType,
        string endpoint,
        string? allowDomainsJson,
        string? denyDomainsJson,
        int maxResults,
        int timeoutSeconds,
        string? configJson,
        bool isEnabled,
        bool isDefault,
        string? remark)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 128).Trim();
        ProviderType = Normalize(providerType, 64, "Custom");
        Endpoint = Normalize(endpoint, 512);
        AllowDomainsJson = NormalizeJson(allowDomainsJson, 8000, "[]");
        DenyDomainsJson = NormalizeJson(denyDomainsJson, 8000, "[]");
        MaxResults = Math.Clamp(maxResults, 1, 20);
        TimeoutSeconds = Math.Clamp(timeoutSeconds, 1, 60);
        ConfigJson = NormalizeJson(configJson, 8000, "{}");
        IsEnabled = isEnabled;
        IsDefault = isDefault;
        Remark = Normalize(remark, 512);
    }

    public void ResetApiKey(string? apiKeyCipherText)
    {
        ApiKeyCipherText = Normalize(apiKeyCipherText, 2048);
    }

    public void ClearDefault()
    {
        IsDefault = false;
    }

    public bool HasApiKey()
    {
        return !string.IsNullOrWhiteSpace(ApiKeyCipherText);
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

/// <summary>
/// 联网搜索调用记录。保存查询、结果摘要、耗时和错误，便于审计和成本回溯。
/// </summary>
public class AiWebSearchRecord : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? ConfigId { get; private set; }

    public Guid? ConversationId { get; private set; }

    public long ElapsedMilliseconds { get; private set; }

    public string ErrorMessage { get; private set; }

    public Guid? MessageId { get; private set; }

    public string ProviderName { get; private set; }

    public string Query { get; private set; }

    public string RequestJson { get; private set; }

    public string ResponseSummaryJson { get; private set; }

    public int ResultCount { get; private set; }

    public string Status { get; private set; }

    public Guid? TenantId { get; private set; }

    protected AiWebSearchRecord()
    {
        ErrorMessage = string.Empty;
        ProviderName = string.Empty;
        Query = string.Empty;
        RequestJson = "{}";
        ResponseSummaryJson = "{}";
        Status = string.Empty;
    }

    public AiWebSearchRecord(
        Guid id,
        string query,
        string providerName,
        string requestJson,
        Guid? tenantId,
        Guid? configId = null,
        Guid? conversationId = null,
        Guid? messageId = null) : base(id)
    {
        Query = Check.NotNullOrWhiteSpace(query, nameof(query), 512).Trim();
        ProviderName = Normalize(providerName, 128);
        RequestJson = NormalizeJson(requestJson, 8000, "{}");
        TenantId = tenantId;
        ConfigId = configId;
        ConversationId = conversationId;
        MessageId = messageId;
        ResponseSummaryJson = "{}";
        ErrorMessage = string.Empty;
        Status = "Running";
    }

    public void Complete(string responseSummaryJson, int resultCount, long elapsedMilliseconds)
    {
        ResponseSummaryJson = NormalizeJson(responseSummaryJson, 64_000, "{}");
        ResultCount = Math.Max(0, resultCount);
        ElapsedMilliseconds = Math.Max(0, elapsedMilliseconds);
        ErrorMessage = string.Empty;
        Status = "Completed";
    }

    public void Fail(string errorMessage, long elapsedMilliseconds)
    {
        ErrorMessage = Normalize(errorMessage, 2000);
        ElapsedMilliseconds = Math.Max(0, elapsedMilliseconds);
        Status = "Failed";
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
