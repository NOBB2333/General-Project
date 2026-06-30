using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Integration;

/// <summary>
/// 外部系统通知规则。定义业务事件触发、收件人规则、通道、静默期和频控策略。
/// </summary>
public class IntegrationNotificationRule : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public string ChannelType { get; private set; } = string.Empty;

    public string ConditionJson { get; private set; } = "{}";

    public string ConnectorCode { get; private set; } = string.Empty;

    public Guid? ConnectorId { get; private set; }

    public string EventCode { get; private set; } = string.Empty;

    public string EventName { get; private set; } = string.Empty;

    public DateTime? LastTriggeredTime { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string ProviderType { get; private set; } = string.Empty;

    public string QuietEndTime { get; private set; } = string.Empty;

    public string QuietStartTime { get; private set; } = string.Empty;

    public int RateLimitMaxCount { get; private set; }

    public int RateLimitWindowMinutes { get; private set; }

    public string RecipientRuleJson { get; private set; } = "{}";

    public string Remark { get; private set; } = string.Empty;

    public string RuleCode { get; private set; } = string.Empty;

    public string Status { get; private set; } = string.Empty;

    public string TargetType { get; private set; } = string.Empty;

    public Guid TemplateId { get; private set; }

    public Guid? TenantId { get; private set; }

    protected IntegrationNotificationRule()
    {
    }

    public IntegrationNotificationRule(
        Guid id,
        string ruleCode,
        string name,
        string eventCode,
        string eventName,
        Guid templateId,
        Guid? connectorId,
        string connectorCode,
        string providerType,
        string channelType,
        string targetType,
        string recipientRuleJson,
        string conditionJson,
        string quietStartTime,
        string quietEndTime,
        int rateLimitWindowMinutes,
        int rateLimitMaxCount,
        string status,
        Guid? tenantId,
        string? remark = null) : base(id)
    {
        TenantId = tenantId;
        SetCore(
            ruleCode,
            name,
            eventCode,
            eventName,
            templateId,
            connectorId,
            connectorCode,
            providerType,
            channelType,
            targetType,
            recipientRuleJson,
            conditionJson,
            quietStartTime,
            quietEndTime,
            rateLimitWindowMinutes,
            rateLimitMaxCount,
            status,
            remark);
    }

    public void Update(
        string ruleCode,
        string name,
        string eventCode,
        string eventName,
        Guid templateId,
        Guid? connectorId,
        string connectorCode,
        string providerType,
        string channelType,
        string targetType,
        string recipientRuleJson,
        string conditionJson,
        string quietStartTime,
        string quietEndTime,
        int rateLimitWindowMinutes,
        int rateLimitMaxCount,
        string status,
        string? remark)
    {
        SetCore(
            ruleCode,
            name,
            eventCode,
            eventName,
            templateId,
            connectorId,
            connectorCode,
            providerType,
            channelType,
            targetType,
            recipientRuleJson,
            conditionJson,
            quietStartTime,
            quietEndTime,
            rateLimitWindowMinutes,
            rateLimitMaxCount,
            status,
            remark);
    }

    public void MarkTriggered(DateTime triggeredTime)
    {
        LastTriggeredTime = triggeredTime;
    }

    public void Toggle(string status)
    {
        Status = NormalizeStatus(status);
    }

    private void SetCore(
        string ruleCode,
        string name,
        string eventCode,
        string eventName,
        Guid templateId,
        Guid? connectorId,
        string connectorCode,
        string providerType,
        string channelType,
        string targetType,
        string recipientRuleJson,
        string conditionJson,
        string quietStartTime,
        string quietEndTime,
        int rateLimitWindowMinutes,
        int rateLimitMaxCount,
        string status,
        string? remark)
    {
        RuleCode = Check.NotNullOrWhiteSpace(ruleCode, nameof(ruleCode), 64).Trim();
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 128).Trim();
        EventCode = Check.NotNullOrWhiteSpace(eventCode, nameof(eventCode), 128).Trim();
        EventName = Normalize(eventName, 128);
        TemplateId = templateId == Guid.Empty ? throw new UserFriendlyException("通知规则必须选择消息模板。") : templateId;
        ConnectorId = connectorId;
        ConnectorCode = Normalize(connectorCode, 64);
        ProviderType = Normalize(providerType, 64);
        ChannelType = NormalizeChannelType(channelType);
        TargetType = Normalize(targetType, 32);
        RecipientRuleJson = NormalizeJson(recipientRuleJson, "{}");
        ConditionJson = NormalizeJson(conditionJson, "{}");
        QuietStartTime = Normalize(quietStartTime, 16);
        QuietEndTime = Normalize(quietEndTime, 16);
        RateLimitWindowMinutes = Math.Clamp(rateLimitWindowMinutes, 0, 24 * 60);
        RateLimitMaxCount = Math.Clamp(rateLimitMaxCount, 0, 100_000);
        Status = NormalizeStatus(status);
        Remark = Normalize(remark, 512);
    }

    private static string NormalizeChannelType(string value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? IntegrationNotificationChannelTypes.Personal : value.Trim();
        if (!IntegrationNotificationChannelTypes.All.Contains(normalized))
        {
            throw new UserFriendlyException("通知通道类型不正确。");
        }

        return normalized;
    }

    private static string NormalizeStatus(string value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? IntegrationNotificationRuleStatus.Active : value.Trim();
        if (!IntegrationNotificationRuleStatus.All.Contains(normalized))
        {
            throw new UserFriendlyException("通知规则状态不正确。");
        }

        return normalized;
    }

    private static string Normalize(string? value, int maxLength)
    {
        var normalized = value?.Trim() ?? string.Empty;
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private static string NormalizeJson(string? value, string defaultValue)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
        return normalized.Length <= 16_000 ? normalized : normalized[..16_000];
    }
}
