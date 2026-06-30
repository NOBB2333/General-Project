using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Ai;

/// <summary>
/// AI 模型配置，挂接到供应商并声明能力、上下文和成本口径。
/// </summary>
public class AiModel : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public string CapabilitiesJson { get; private set; }

    public int ContextWindow { get; private set; }

    public string Currency { get; private set; }

    public string DisplayName { get; private set; }

    public Guid? FallbackModelId { get; private set; }

    public bool IsDefault { get; private set; }

    public bool IsEnabled { get; private set; }

    public int MaxOutputTokens { get; private set; }

    public string ModelCode { get; private set; }

    public string ModelType { get; private set; }

    public decimal OutputTokenPrice { get; private set; }

    public decimal InputTokenPrice { get; private set; }

    public Guid ProviderId { get; private set; }

    public string Remark { get; private set; }

    public int Sort { get; private set; }

    public Guid? TenantId { get; private set; }

    protected AiModel()
    {
        CapabilitiesJson = "[]";
        Currency = "USD";
        DisplayName = string.Empty;
        ModelCode = string.Empty;
        ModelType = string.Empty;
        Remark = string.Empty;
    }

    public AiModel(
        Guid id,
        Guid providerId,
        Guid? tenantId,
        string modelCode,
        string displayName,
        string modelType,
        bool isEnabled = true,
        bool isDefault = false,
        int contextWindow = 0,
        int maxOutputTokens = 0,
        Guid? fallbackModelId = null,
        string? capabilitiesJson = null,
        decimal inputTokenPrice = 0,
        decimal outputTokenPrice = 0,
        string? currency = null,
        int sort = 0,
        string? remark = null) : base(id)
    {
        ProviderId = providerId;
        TenantId = tenantId;
        ModelCode = Check.NotNullOrWhiteSpace(modelCode, nameof(modelCode), 128).Trim();
        DisplayName = Check.NotNullOrWhiteSpace(displayName, nameof(displayName), 128).Trim();
        ModelType = Check.NotNullOrWhiteSpace(modelType, nameof(modelType), 32).Trim();
        IsEnabled = isEnabled;
        IsDefault = isDefault;
        ContextWindow = Math.Max(0, contextWindow);
        MaxOutputTokens = Math.Max(0, maxOutputTokens);
        FallbackModelId = fallbackModelId == id ? null : fallbackModelId;
        CapabilitiesJson = NormalizeJsonArray(capabilitiesJson);
        InputTokenPrice = Math.Max(0, inputTokenPrice);
        OutputTokenPrice = Math.Max(0, outputTokenPrice);
        Currency = Normalize(currency, 16, "USD").ToUpperInvariant();
        Sort = sort;
        Remark = Normalize(remark, 512);
    }

    public void Update(
        string modelCode,
        string displayName,
        string modelType,
        bool isEnabled,
        bool isDefault,
        int contextWindow,
        int maxOutputTokens,
        Guid? fallbackModelId,
        string? capabilitiesJson,
        decimal inputTokenPrice,
        decimal outputTokenPrice,
        string? currency,
        int sort,
        string? remark)
    {
        ModelCode = Check.NotNullOrWhiteSpace(modelCode, nameof(modelCode), 128).Trim();
        DisplayName = Check.NotNullOrWhiteSpace(displayName, nameof(displayName), 128).Trim();
        ModelType = Check.NotNullOrWhiteSpace(modelType, nameof(modelType), 32).Trim();
        IsEnabled = isEnabled;
        IsDefault = isDefault;
        ContextWindow = Math.Max(0, contextWindow);
        MaxOutputTokens = Math.Max(0, maxOutputTokens);
        FallbackModelId = fallbackModelId == Id ? null : fallbackModelId;
        CapabilitiesJson = NormalizeJsonArray(capabilitiesJson);
        InputTokenPrice = Math.Max(0, inputTokenPrice);
        OutputTokenPrice = Math.Max(0, outputTokenPrice);
        Currency = Normalize(currency, 16, "USD").ToUpperInvariant();
        Sort = sort;
        Remark = Normalize(remark, 512);
    }

    public void ClearDefault()
    {
        IsDefault = false;
    }

    private static string Normalize(string? value, int maxLength, string defaultValue = "")
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private static string NormalizeJsonArray(string? value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? "[]" : value.Trim();
        return normalized.Length <= 4000 ? normalized : normalized[..4000];
    }
}
