using System.Text.Json;
using System.Text.Json.Nodes;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Linq;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Integration;

public class IntegrationWebhookService : ITransientDependency
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IAsyncQueryableExecuter _asyncQueryableExecuter;
    private readonly IRepository<IntegrationConnector, Guid> _connectorRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IntegrationRateLimitService _rateLimitService;
    private readonly IntegrationSecurityPolicyService _securityPolicyService;
    private readonly IEnumerable<IIntegrationWebhookParser> _webhookParsers;
    private readonly IRepository<IntegrationWebhookEvent, Guid> _webhookEventRepository;

    public IntegrationWebhookService(
        IAsyncQueryableExecuter asyncQueryableExecuter,
        IRepository<IntegrationConnector, Guid> connectorRepository,
        ICurrentTenant currentTenant,
        IGuidGenerator guidGenerator,
        IntegrationRateLimitService rateLimitService,
        IntegrationSecurityPolicyService securityPolicyService,
        IEnumerable<IIntegrationWebhookParser> webhookParsers,
        IRepository<IntegrationWebhookEvent, Guid> webhookEventRepository)
    {
        _asyncQueryableExecuter = asyncQueryableExecuter;
        _connectorRepository = connectorRepository;
        _currentTenant = currentTenant;
        _guidGenerator = guidGenerator;
        _rateLimitService = rateLimitService;
        _securityPolicyService = securityPolicyService;
        _webhookParsers = webhookParsers;
        _webhookEventRepository = webhookEventRepository;
    }

    public async Task<PlatformPagedResultDto<IntegrationWebhookEventDto>> GetEventsAsync(IntegrationWebhookEventQueryInput input)
    {
        var maxResultCount = Math.Clamp(input.MaxResultCount, 1, 200);
        var skipCount = Math.Max(input.SkipCount, 0);
        var queryable = await BuildFilteredQueryAsync(input);
        var totalCount = await _asyncQueryableExecuter.CountAsync(queryable);
        var events = await _asyncQueryableExecuter.ToListAsync(queryable
            .OrderByDescending(x => x.CreationTime)
            .Skip(skipCount)
            .Take(maxResultCount));

        return new PlatformPagedResultDto<IntegrationWebhookEventDto>
        {
            Items = events.Select(MapEvent).ToList(),
            TotalCount = totalCount
        };
    }

    public async Task<IntegrationWebhookEventDto> ReceiveAsync(IntegrationWebhookReceiveInput input)
    {
        await _securityPolicyService.CheckWebhookSourceAsync(input);
        await _rateLimitService.CheckWebhookRequestAsync(input);
        var connector = await ResolveConnectorAsync(input);
        var rawPayloadJson = NormalizeJson(input.RawPayloadJson, input.RawBody);
        var eventType = input.EventType;
        var eventId = input.EventId;
        var explicitDedupeKey = input.DedupeKey;
        var signatureStatus = NormalizeSignatureStatus(input.SignatureStatus);
        var actionCode = input.ActionCode;
        var businessKey = input.BusinessKey;
        var parseError = string.Empty;

        if (connector != null && !string.IsNullOrWhiteSpace(input.RawBody))
        {
            var parser = _webhookParsers.FirstOrDefault(x =>
                x.ConnectorCode.Equals(connector.ConnectorCode, StringComparison.OrdinalIgnoreCase));
            if (parser != null)
            {
                var parseResult = await parser.ParseAsync(connector.Id, new IntegrationWebhookParseInput
                {
                    HeadersJson = NormalizeJson(input.HeadersJson, "{}"),
                    RawBody = string.IsNullOrWhiteSpace(input.RawBody) ? rawPayloadJson : input.RawBody
                });
                rawPayloadJson = string.IsNullOrWhiteSpace(parseResult.ParsedJson) ? rawPayloadJson : parseResult.ParsedJson;
                eventType = string.IsNullOrWhiteSpace(parseResult.EventType) || parseResult.EventType == "Unknown"
                    ? eventType
                    : parseResult.EventType;
                eventId = string.IsNullOrWhiteSpace(eventId) ? parseResult.IdempotencyKey : eventId;
                explicitDedupeKey = string.IsNullOrWhiteSpace(explicitDedupeKey) ? parseResult.IdempotencyKey : explicitDedupeKey;
                signatureStatus = string.IsNullOrWhiteSpace(parseResult.SignatureStatus)
                    ? parseResult.SignatureValid ? "Valid" : "Invalid"
                    : parseResult.SignatureStatus;
                parseError = parseResult.ErrorMessage;
            }
        }

        var dedupeKey = ResolveDedupeKey(input, rawPayloadJson);
        if (!string.IsNullOrWhiteSpace(explicitDedupeKey))
        {
            dedupeKey = explicitDedupeKey.Trim();
        }

        var duplicate = await FindDuplicateAsync(dedupeKey);
        if (duplicate != null)
        {
            return MapEvent(duplicate);
        }

        var webhookEvent = new IntegrationWebhookEvent(
            _guidGenerator.Create(),
            input.ProviderType,
            connector?.ConnectorCode ?? input.ConnectorCode,
            eventType,
            eventId,
            dedupeKey,
            rawPayloadJson,
            input.RawBody,
            NormalizeJson(input.HeadersJson, "{}"),
            _currentTenant.Id,
            connector?.Id,
            input.SourceIp,
            signatureStatus,
            actionCode,
            businessKey);
        TryParsePayload(webhookEvent, rawPayloadJson);
        if (!string.IsNullOrWhiteSpace(parseError))
        {
            webhookEvent.MarkFailed(parseError);
        }
        else if (signatureStatus.Equals("Invalid", StringComparison.OrdinalIgnoreCase))
        {
            webhookEvent.MarkFailed("Webhook 签名校验失败。");
        }

        await _webhookEventRepository.InsertAsync(webhookEvent, autoSave: true);
        return MapEvent(webhookEvent);
    }

    public async Task<IntegrationWebhookEventDto> RetryAsync(Guid id)
    {
        var webhookEvent = await _webhookEventRepository.GetAsync(id);
        webhookEvent.ResetForRetry();
        TryParsePayload(webhookEvent, webhookEvent.RawPayloadJson);
        await _webhookEventRepository.UpdateAsync(webhookEvent, autoSave: true);
        return MapEvent(webhookEvent);
    }

    public async Task<IntegrationWebhookEventDto> SkipAsync(Guid id, IntegrationWebhookSkipInput input)
    {
        var webhookEvent = await _webhookEventRepository.GetAsync(id);
        webhookEvent.MarkSkipped(input.Reason);
        await _webhookEventRepository.UpdateAsync(webhookEvent, autoSave: true);
        return MapEvent(webhookEvent);
    }

    private async Task<IQueryable<IntegrationWebhookEvent>> BuildFilteredQueryAsync(IntegrationWebhookEventQueryInput input)
    {
        var queryable = await _webhookEventRepository.GetQueryableAsync();
        if (!string.IsNullOrWhiteSpace(input.Status) && !input.Status.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            var status = input.Status.Trim();
            queryable = queryable.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(input.ProviderType))
        {
            var providerType = input.ProviderType.Trim();
            queryable = queryable.Where(x => x.ProviderType == providerType);
        }

        if (!string.IsNullOrWhiteSpace(input.EventType))
        {
            var eventType = input.EventType.Trim();
            queryable = queryable.Where(x => x.EventType == eventType);
        }

        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            var keyword = input.Keyword.Trim();
            queryable = queryable.Where(x =>
                x.EventId.Contains(keyword) ||
                x.DedupeKey.Contains(keyword) ||
                x.BusinessKey.Contains(keyword) ||
                x.ActionCode.Contains(keyword) ||
                x.ErrorMessage.Contains(keyword));
        }

        return queryable;
    }

    private async Task<IntegrationConnector?> ResolveConnectorAsync(IntegrationWebhookReceiveInput input)
    {
        var connectors = await _connectorRepository.GetListAsync();
        if (!string.IsNullOrWhiteSpace(input.ConnectorCode))
        {
            var connectorCode = input.ConnectorCode.Trim();
            var matchedByCode = connectors.FirstOrDefault(x => x.ConnectorCode.Equals(connectorCode, StringComparison.OrdinalIgnoreCase));
            if (matchedByCode != null)
            {
                return matchedByCode;
            }
        }

        if (!string.IsNullOrWhiteSpace(input.ProviderType))
        {
            var providerType = input.ProviderType.Trim();
            return connectors
                .OrderBy(x => x.Sort)
                .FirstOrDefault(x => x.ProviderType.Equals(providerType, StringComparison.OrdinalIgnoreCase));
        }

        return null;
    }

    private async Task<IntegrationWebhookEvent?> FindDuplicateAsync(string dedupeKey)
    {
        if (string.IsNullOrWhiteSpace(dedupeKey))
        {
            return null;
        }

        var existing = await _webhookEventRepository.GetListAsync(x => x.DedupeKey == dedupeKey);
        return existing.OrderByDescending(x => x.CreationTime).FirstOrDefault();
    }

    private static string ResolveDedupeKey(IntegrationWebhookReceiveInput input, string rawPayloadJson)
    {
        if (!string.IsNullOrWhiteSpace(input.DedupeKey))
        {
            return input.DedupeKey.Trim();
        }

        if (!string.IsNullOrWhiteSpace(input.EventId))
        {
            return $"{input.ProviderType}:{input.EventId}".Trim(':');
        }

        var normalized = string.Join("|", input.ProviderType, input.EventType, rawPayloadJson);
        return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(normalized)));
    }

    private static void TryParsePayload(IntegrationWebhookEvent webhookEvent, string rawPayloadJson)
    {
        try
        {
            var node = JsonNode.Parse(rawPayloadJson) as JsonObject;
            if (node == null)
            {
                webhookEvent.MarkFailed("Webhook Payload 不是 JSON 对象。");
                return;
            }

            var commandText = ResolveString(
                node,
                "command",
                "text",
                "message.text",
                "message.content",
                "event.message.text",
                "event.message.content",
                "raw.command",
                "raw.text",
                "raw.Content",
                "raw.message.text",
                "raw.message.content",
                "raw.event.message.text",
                "raw.event.message.content");
            var actionCode = ResolveString(
                node,
                "action",
                "actionCode",
                "action_code",
                "event.action",
                "event.actionCode",
                "event.action_code",
                "event.action.value.action",
                "event.action.value.actionCode",
                "event.action.value.action_code",
                "raw.EventKey",
                "raw.event.action",
                "raw.event.actionCode",
                "raw.event.action_code",
                "raw.event.action.value.action",
                "raw.event.action.value.actionCode",
                "raw.event.action.value.action_code");
            if (string.IsNullOrWhiteSpace(actionCode))
            {
                actionCode = ResolveCommandAction(commandText);
            }

            var businessKey = ResolveString(
                node,
                "businessKey",
                "business_key",
                "projectId",
                "project_id",
                "event.business_key",
                "event.project_id",
                "event.action.value.businessKey",
                "event.action.value.business_key",
                "event.action.value.projectId",
                "event.action.value.project_id",
                "raw.businessKey",
                "raw.business_key",
                "raw.projectId",
                "raw.project_id",
                "raw.event.business_key",
                "raw.event.project_id",
                "raw.event.action.value.businessKey",
                "raw.event.action.value.business_key",
                "raw.event.action.value.projectId",
                "raw.event.action.value.project_id");
            if (string.IsNullOrWhiteSpace(businessKey))
            {
                businessKey = ResolveCommandBusinessKey(commandText);
            }

            webhookEvent.MarkParsed(node.ToJsonString(JsonOptions), actionCode, businessKey);
        }
        catch (Exception ex)
        {
            webhookEvent.MarkFailed($"Webhook Payload 解析失败：{ex.Message}");
        }
    }

    private static string ResolveString(JsonObject node, params string[] paths)
    {
        foreach (var path in paths)
        {
            var value = ResolvePath(node, path);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return string.Empty;
    }

    private static string ResolvePath(JsonObject node, string path)
    {
        JsonNode? current = node;
        foreach (var segment in path.Split('.', StringSplitOptions.RemoveEmptyEntries))
        {
            if (current is not JsonObject currentObject || currentObject[segment] == null)
            {
                return string.Empty;
            }

            current = currentObject[segment];
        }

        return current?.GetValue<string>() ?? string.Empty;
    }

    private static string ResolveCommandAction(string commandText)
    {
        var command = NormalizeCommandText(commandText);
        if (string.IsNullOrWhiteSpace(command))
        {
            return string.Empty;
        }

        var firstToken = command.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault();
        return string.IsNullOrWhiteSpace(firstToken) ? string.Empty : $"command.{firstToken}";
    }

    private static string ResolveCommandBusinessKey(string commandText)
    {
        var command = NormalizeCommandText(commandText);
        if (string.IsNullOrWhiteSpace(command))
        {
            return string.Empty;
        }

        return command.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Skip(1)
            .FirstOrDefault() ?? string.Empty;
    }

    private static string NormalizeCommandText(string commandText)
    {
        var normalized = commandText.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return string.Empty;
        }

        if (normalized.StartsWith('/'))
        {
            return normalized[1..].Trim();
        }

        if (normalized.StartsWith('!') || normalized.StartsWith('！'))
        {
            return normalized[1..].Trim();
        }

        return string.Empty;
    }

    private static string NormalizeJson(string value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        return value.Trim();
    }

    private static string NormalizeSignatureStatus(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "Unchecked" : value.Trim();
    }

    private static IntegrationWebhookEventDto MapEvent(IntegrationWebhookEvent item)
    {
        return new IntegrationWebhookEventDto
        {
            ActionCode = item.ActionCode,
            BusinessKey = item.BusinessKey,
            ConnectorCode = item.ConnectorCode,
            ConnectorId = item.ConnectorId,
            CreationTime = item.CreationTime,
            DedupeKey = item.DedupeKey,
            ErrorMessage = item.ErrorMessage,
            EventId = item.EventId,
            EventType = item.EventType,
            HeadersJson = item.HeadersJson,
            Id = item.Id,
            NormalizedPayloadJson = item.NormalizedPayloadJson,
            ProviderType = item.ProviderType,
            RawBody = item.RawBody,
            RawPayloadJson = item.RawPayloadJson,
            ResolvedTime = item.ResolvedTime,
            RetryCount = item.RetryCount,
            SignatureStatus = item.SignatureStatus,
            SourceIp = item.SourceIp,
            Status = item.Status
        };
    }
}
