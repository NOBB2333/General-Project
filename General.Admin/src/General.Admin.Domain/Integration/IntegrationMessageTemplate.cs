using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Integration;

/// <summary>
/// 外部系统消息模板。用于统一维护文本、Markdown 和互动卡片的变量模板。
/// </summary>
public class IntegrationMessageTemplate : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public string BodyTemplate { get; private set; } = string.Empty;

    public string CardJson { get; private set; } = "{}";

    public string MessageType { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string PreviewVariablesJson { get; private set; } = "{}";

    public string Remark { get; private set; } = string.Empty;

    public string Status { get; private set; } = string.Empty;

    public string TemplateCode { get; private set; } = string.Empty;

    public Guid? TenantId { get; private set; }

    public string TitleTemplate { get; private set; } = string.Empty;

    public string VariableSchemaJson { get; private set; } = "{}";

    protected IntegrationMessageTemplate()
    {
    }

    public IntegrationMessageTemplate(
        Guid id,
        string templateCode,
        string name,
        string messageType,
        string titleTemplate,
        string bodyTemplate,
        string cardJson,
        string variableSchemaJson,
        string previewVariablesJson,
        string status,
        Guid? tenantId,
        string? remark = null) : base(id)
    {
        TenantId = tenantId;
        SetCore(
            templateCode,
            name,
            messageType,
            titleTemplate,
            bodyTemplate,
            cardJson,
            variableSchemaJson,
            previewVariablesJson,
            status,
            remark);
    }

    public void Update(
        string templateCode,
        string name,
        string messageType,
        string titleTemplate,
        string bodyTemplate,
        string cardJson,
        string variableSchemaJson,
        string previewVariablesJson,
        string status,
        string? remark)
    {
        SetCore(
            templateCode,
            name,
            messageType,
            titleTemplate,
            bodyTemplate,
            cardJson,
            variableSchemaJson,
            previewVariablesJson,
            status,
            remark);
    }

    public void Toggle(string status)
    {
        Status = NormalizeStatus(status);
    }

    private void SetCore(
        string templateCode,
        string name,
        string messageType,
        string titleTemplate,
        string bodyTemplate,
        string cardJson,
        string variableSchemaJson,
        string previewVariablesJson,
        string status,
        string? remark)
    {
        TemplateCode = Check.NotNullOrWhiteSpace(templateCode, nameof(templateCode), 64).Trim();
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 128).Trim();
        MessageType = NormalizeMessageType(messageType);
        TitleTemplate = Normalize(titleTemplate, 256);
        BodyTemplate = Normalize(bodyTemplate, 16_000);
        CardJson = NormalizeJson(cardJson, "{}");
        VariableSchemaJson = NormalizeJson(variableSchemaJson, "{}");
        PreviewVariablesJson = NormalizeJson(previewVariablesJson, "{}");
        Status = NormalizeStatus(status);
        Remark = Normalize(remark, 512);
    }

    private static string NormalizeMessageType(string value)
    {
        var normalized = Check.NotNullOrWhiteSpace(value, nameof(value), 32).Trim();
        if (!IntegrationMessageTemplateTypes.All.Contains(normalized))
        {
            throw new UserFriendlyException("消息模板类型不正确。");
        }

        return normalized;
    }

    private static string NormalizeStatus(string value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? IntegrationMessageTemplateStatus.Draft : value.Trim();
        if (!IntegrationMessageTemplateStatus.All.Contains(normalized))
        {
            throw new UserFriendlyException("消息模板状态不正确。");
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
