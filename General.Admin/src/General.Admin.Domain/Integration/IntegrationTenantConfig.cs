using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Integration;

/// <summary>
/// 租户级外部系统连接配置。AppSecret 和访问 Token 只保存加密值。
/// </summary>
public class IntegrationTenantConfig : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public string AccessTokenCipherText { get; private set; }

    public string CallbackPath { get; private set; }

    public string ConfigJson { get; private set; }

    public Guid ConnectorId { get; private set; }

    public bool IsEnabled { get; private set; }

    public string LastTestMessage { get; private set; }

    public string LastTestStatus { get; private set; }

    public DateTime? LastTestTime { get; private set; }

    public string Remark { get; private set; }

    public string SecretCipherText { get; private set; }

    public Guid? TenantId { get; private set; }

    public DateTime? TokenExpiresAt { get; private set; }

    protected IntegrationTenantConfig()
    {
        AccessTokenCipherText = string.Empty;
        CallbackPath = string.Empty;
        ConfigJson = "{}";
        LastTestMessage = string.Empty;
        LastTestStatus = "未测试";
        Remark = string.Empty;
        SecretCipherText = string.Empty;
    }

    public IntegrationTenantConfig(
        Guid id,
        Guid connectorId,
        Guid? tenantId,
        string configJson,
        string secretCipherText,
        bool isEnabled = true,
        string? callbackPath = null,
        string? remark = null) : base(id)
    {
        ConnectorId = connectorId;
        TenantId = tenantId;
        ConfigJson = NormalizeJson(configJson);
        SecretCipherText = Normalize(secretCipherText, 4096);
        IsEnabled = isEnabled;
        CallbackPath = Normalize(callbackPath, 512);
        Remark = Normalize(remark, 512);
        AccessTokenCipherText = string.Empty;
        LastTestMessage = string.Empty;
        LastTestStatus = "未测试";
    }

    public void Update(
        string configJson,
        bool isEnabled,
        string? callbackPath,
        string? remark)
    {
        ConfigJson = NormalizeJson(configJson);
        IsEnabled = isEnabled;
        CallbackPath = Normalize(callbackPath, 512);
        Remark = Normalize(remark, 512);
    }

    public void ResetSecret(string secretCipherText)
    {
        SecretCipherText = Normalize(secretCipherText, 4096);
        AccessTokenCipherText = string.Empty;
        TokenExpiresAt = null;
        LastTestStatus = "待重测";
        LastTestMessage = "连接器密钥已轮换，请重新测试连接。";
        LastTestTime = null;
    }

    public void ResetAccessToken(string accessTokenCipherText, DateTime? tokenExpiresAt)
    {
        AccessTokenCipherText = Normalize(accessTokenCipherText, 4096);
        TokenExpiresAt = tokenExpiresAt;
    }

    public void MarkTestResult(bool success, string? message)
    {
        LastTestStatus = success ? "成功" : "失败";
        LastTestMessage = Normalize(message, 1000);
        LastTestTime = DateTime.UtcNow;
    }

    public void Toggle(bool isEnabled)
    {
        IsEnabled = isEnabled;
    }

    public bool HasSecret()
    {
        return !string.IsNullOrWhiteSpace(SecretCipherText);
    }

    private static string Normalize(string? value, int maxLength)
    {
        var normalized = value?.Trim() ?? string.Empty;
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private static string NormalizeJson(string? value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? "{}" : value.Trim();
        return normalized.Length <= 16_000 ? normalized : normalized[..16_000];
    }
}
