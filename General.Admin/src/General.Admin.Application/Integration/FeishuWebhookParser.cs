using System.Text.Json;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Integration;

[ExposeServices(typeof(IIntegrationWebhookParser))]
public class FeishuWebhookParser : IIntegrationWebhookParser, ITransientDependency
{
    private readonly IDataFilter<IMultiTenant> _multiTenantFilter;
    private readonly IRepository<IntegrationTenantConfig, Guid> _configRepository;

    public FeishuWebhookParser(
        IDataFilter<IMultiTenant> multiTenantFilter,
        IRepository<IntegrationTenantConfig, Guid> configRepository)
    {
        _multiTenantFilter = multiTenantFilter;
        _configRepository = configRepository;
    }

    public string ConnectorCode => "feishu";

    public async Task<IntegrationWebhookParseResult> ParseAsync(Guid connectorId, IntegrationWebhookParseInput input)
    {
        try
        {
            IntegrationTenantConfig? config;
            using (_multiTenantFilter.Disable())
            {
                config = await _configRepository.FindAsync(x => x.ConnectorId == connectorId);
            }
            var root = IntegrationWebhookParserUtilities.ParseRawBody(input.RawBody, input.RawBody);
            var eventType = IntegrationWebhookParserUtilities.ReadString(root, "header.event_type", "type", "event.type");
            if (string.IsNullOrWhiteSpace(eventType) &&
                !string.IsNullOrWhiteSpace(IntegrationWebhookParserUtilities.ReadString(root, "challenge")))
            {
                eventType = "url_verification";
            }

            var eventId = IntegrationWebhookParserUtilities.ReadString(root, "header.event_id", "uuid", "event.event_id", "challenge");
            var actionCode = IntegrationWebhookParserUtilities.ReadString(
                root,
                "event.action.value.action",
                "event.action.value.actionCode",
                "event.action.action",
                "event.action_code",
                "action",
                "actionCode");
            var businessKey = IntegrationWebhookParserUtilities.ReadString(
                root,
                "event.action.value.businessKey",
                "event.action.value.business_key",
                "event.action.value.projectId",
                "event.action.value.project_id",
                "event.business_key",
                "event.project_id",
                "businessKey",
                "business_key");
            var signatureStatus = ValidateToken(config?.ConfigJson ?? "{}", root);
            return new IntegrationWebhookParseResult
            {
                EventType = string.IsNullOrWhiteSpace(eventType) ? "Unknown" : eventType,
                IdempotencyKey = IntegrationWebhookParserUtilities.BuildDedupeKey(ConnectorCode, eventId, input.RawBody),
                ParsedJson = IntegrationWebhookParserUtilities.BuildNormalizedPayload(
                    ConnectorCode,
                    string.IsNullOrWhiteSpace(eventType) ? "Unknown" : eventType,
                    actionCode,
                    businessKey,
                    root),
                SignatureStatus = signatureStatus,
                SignatureValid = signatureStatus is "Valid" or "Unchecked"
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return BuildFailedResult(input.RawBody, $"飞书 Webhook 解析失败：{ex.Message}");
        }
    }

    private static string ValidateToken(string configJson, JsonElement root)
    {
        var configuredToken = IntegrationWebhookParserUtilities.ReadConfig(configJson, "verificationToken", "token");
        if (string.IsNullOrWhiteSpace(configuredToken))
        {
            return "Unchecked";
        }

        var incomingToken = IntegrationWebhookParserUtilities.ReadString(root, "header.token", "token");
        return configuredToken.Equals(incomingToken, StringComparison.Ordinal)
            ? "Valid"
            : "Invalid";
    }

    private static IntegrationWebhookParseResult BuildFailedResult(string rawBody, string message)
    {
        return new IntegrationWebhookParseResult
        {
            ErrorMessage = message,
            EventType = "Unknown",
            IdempotencyKey = IntegrationWebhookParserUtilities.BuildDedupeKey("feishu", string.Empty, rawBody),
            ParsedJson = "{}",
            SignatureStatus = "Unchecked",
            SignatureValid = false
        };
    }
}
