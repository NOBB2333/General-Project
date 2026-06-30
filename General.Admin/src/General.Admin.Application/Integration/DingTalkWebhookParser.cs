using System.Text.Json;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Integration;

[ExposeServices(typeof(IIntegrationWebhookParser))]
public class DingTalkWebhookParser : IIntegrationWebhookParser, ITransientDependency
{
    private readonly IDataFilter<IMultiTenant> _multiTenantFilter;
    private readonly IRepository<IntegrationTenantConfig, Guid> _configRepository;

    public DingTalkWebhookParser(
        IDataFilter<IMultiTenant> multiTenantFilter,
        IRepository<IntegrationTenantConfig, Guid> configRepository)
    {
        _multiTenantFilter = multiTenantFilter;
        _configRepository = configRepository;
    }

    public string ConnectorCode => "dingtalk";

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
            var eventType = IntegrationWebhookParserUtilities.ReadString(
                root,
                "eventType",
                "event_type",
                "EventType",
                "type",
                "event.type");
            var eventId = IntegrationWebhookParserUtilities.ReadString(root, "eventId", "event_id", "EventId", "uuid", "msgId");
            var actionCode = IntegrationWebhookParserUtilities.ReadString(
                root,
                "action",
                "actionCode",
                "action_code",
                "event.action",
                "event.actionCode",
                "event.action_code");
            var businessKey = IntegrationWebhookParserUtilities.ReadString(
                root,
                "businessKey",
                "business_key",
                "projectId",
                "project_id",
                "event.businessKey",
                "event.business_key",
                "event.projectId",
                "event.project_id");
            var signatureStatus = ValidateSignature(config?.ConfigJson ?? "{}", input.HeadersJson, input.RawBody);
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
            return BuildFailedResult(input.RawBody, $"钉钉 Webhook 解析失败：{ex.Message}");
        }
    }

    private static string ValidateSignature(string configJson, string headersJson, string rawBody)
    {
        var token = IntegrationWebhookParserUtilities.ReadConfig(configJson, "callbackToken", "token");
        if (string.IsNullOrWhiteSpace(token))
        {
            return "Unchecked";
        }

        var signature = IntegrationWebhookParserUtilities.ReadHeader(headersJson, "signature", "x-dingtalk-signature");
        var timestamp = IntegrationWebhookParserUtilities.ReadHeader(headersJson, "timestamp", "x-dingtalk-timestamp");
        var nonce = IntegrationWebhookParserUtilities.ReadHeader(headersJson, "nonce", "x-dingtalk-nonce");
        if (string.IsNullOrWhiteSpace(signature) ||
            string.IsNullOrWhiteSpace(timestamp) ||
            string.IsNullOrWhiteSpace(nonce))
        {
            return "Invalid";
        }

        var expected = IntegrationWebhookParserUtilities.ComputeSortedSha1(token, timestamp, nonce, rawBody);
        return signature.Equals(expected, StringComparison.OrdinalIgnoreCase) ? "Valid" : "Invalid";
    }

    private static IntegrationWebhookParseResult BuildFailedResult(string rawBody, string message)
    {
        return new IntegrationWebhookParseResult
        {
            ErrorMessage = message,
            EventType = "Unknown",
            IdempotencyKey = IntegrationWebhookParserUtilities.BuildDedupeKey("dingtalk", string.Empty, rawBody),
            ParsedJson = "{}",
            SignatureStatus = "Unchecked",
            SignatureValid = false
        };
    }
}
