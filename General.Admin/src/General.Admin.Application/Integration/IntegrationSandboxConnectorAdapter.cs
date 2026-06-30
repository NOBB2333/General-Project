using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace General.Admin.Integration;

/// <summary>
/// Integration 沙箱连接器。用于无外部租户凭据时验证投递、同步、Webhook 和重试链路。
/// </summary>
[ExposeServices(
    typeof(IIntegrationConnectorAdapter),
    typeof(IIntegrationMessageSender),
    typeof(IIntegrationSyncAdapter),
    typeof(IIntegrationWebhookParser))]
public class IntegrationSandboxConnectorAdapter :
    IIntegrationConnectorAdapter,
    IIntegrationMessageSender,
    IIntegrationSyncAdapter,
    IIntegrationWebhookParser,
    ITransientDependency
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IRepository<IntegrationConnector, Guid> _connectorRepository;
    private readonly IRepository<IntegrationTenantConfig, Guid> _configRepository;
    private readonly IntegrationBindingService _bindingService;
    private readonly IntegrationSyncImportService _syncImportService;

    public IntegrationSandboxConnectorAdapter(
        IRepository<IntegrationConnector, Guid> connectorRepository,
        IRepository<IntegrationTenantConfig, Guid> configRepository,
        IntegrationBindingService bindingService,
        IntegrationSyncImportService syncImportService)
    {
        _connectorRepository = connectorRepository;
        _configRepository = configRepository;
        _bindingService = bindingService;
        _syncImportService = syncImportService;
    }

    public string ConnectorCode => "sandbox";

    public async Task<IntegrationConnectionTestResultDto> TestConnectionAsync(
        IntegrationConnector connector,
        IntegrationTenantConfig? tenantConfig)
    {
        try
        {
            EnsureBaseEnabled(connector, tenantConfig);
            var options = ReadOptions(tenantConfig!.ConfigJson);
            if (options.ForceFailure)
            {
                return new IntegrationConnectionTestResultDto
                {
                    Message = "集成沙箱按配置返回连接失败，用于验证异常链路。",
                    Success = false
                };
            }

            return new IntegrationConnectionTestResultDto
            {
                Message = "集成沙箱连接测试成功，可用于验证投递、同步、Webhook 和重试链路。",
                Success = true
            };
        }
        catch (UserFriendlyException ex)
        {
            return new IntegrationConnectionTestResultDto
            {
                Message = ex.Message,
                Success = false
            };
        }
    }

    public async Task<IntegrationMessageSendResult> SendMessageAsync(IntegrationMessageSendInput input)
    {
        var connector = await _connectorRepository.GetAsync(input.ConnectorId);
        EnsureConnector(connector);
        var config = await _configRepository.FindAsync(x => x.ConnectorId == input.ConnectorId);
        EnsureBaseEnabled(connector, config);

        var options = ReadOptions(config!.ConfigJson);
        if (options.ForceFailure || !options.AcceptMessage)
        {
            throw new UserFriendlyException("集成沙箱按配置拒绝消息投递。");
        }

        var externalMessageId = $"sandbox-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}"[..42];
        return new IntegrationMessageSendResult
        {
            ExternalMessageId = externalMessageId,
            Message = $"{options.EchoPrefix}沙箱投递成功：{input.TargetType}/{input.TargetId}",
            Success = true
        };
    }

    public async Task<IntegrationSyncResult> ExecuteSyncAsync(IntegrationSyncExecuteInput input)
    {
        var connector = await _connectorRepository.GetAsync(input.ConnectorId);
        EnsureConnector(connector);
        var config = await _configRepository.FindAsync(x => x.ConnectorId == input.ConnectorId);
        EnsureBaseEnabled(connector, config);

        var syncType = input.SyncType.Trim().ToLowerInvariant();
        return syncType switch
        {
            "department" => await SyncDepartmentsAsync(connector, input.ConfigJson, input.RequestJson),
            "user" => await SyncUsersAsync(connector, input.ConfigJson, input.RequestJson),
            "group" => await _syncImportService.SyncGroupsAsync(connector, input.ConfigJson, input.RequestJson, "集成沙箱"),
            "meeting" => await _syncImportService.SyncMeetingsAsync(connector, input.ConfigJson, input.RequestJson, "集成沙箱"),
            _ => throw new UserFriendlyException("集成沙箱同步仅支持部门、用户、群和会议记录。")
        };
    }

    public Task<IntegrationWebhookParseResult> ParseAsync(Guid connectorId, IntegrationWebhookParseInput input)
    {
        try
        {
            var raw = string.IsNullOrWhiteSpace(input.RawBody) ? "{}" : input.RawBody.Trim();
            using var document = JsonDocument.Parse(raw);
            var root = document.RootElement;
            var eventType = ReadString(root, "eventType", "event_type", "type");
            if (string.IsNullOrWhiteSpace(eventType))
            {
                eventType = "sandbox.event";
            }

            var eventId = ReadString(root, "eventId", "event_id", "id");
            if (string.IsNullOrWhiteSpace(eventId))
            {
                eventId = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(raw)));
            }

            var actionCode = ReadString(root, "actionCode", "action", "event.action", "event.actionCode");
            var businessKey = ReadString(root, "businessKey", "business_key", "projectId", "event.businessKey", "event.projectId");
            var normalized = new JsonObject
            {
                ["connector_code"] = ConnectorCode,
                ["event_type"] = eventType,
                ["event_id"] = eventId,
                ["actionCode"] = actionCode,
                ["businessKey"] = businessKey,
                ["raw"] = JsonNode.Parse(raw)
            };

            return Task.FromResult(new IntegrationWebhookParseResult
            {
                EventType = eventType,
                IdempotencyKey = $"sandbox:{eventId}",
                ParsedJson = normalized.ToJsonString(JsonOptions),
                SignatureStatus = "Valid",
                SignatureValid = true
            });
        }
        catch (JsonException ex)
        {
            return Task.FromResult(new IntegrationWebhookParseResult
            {
                ErrorMessage = $"集成沙箱 Webhook Payload 不是合法 JSON：{ex.Message}",
                EventType = "sandbox.invalid",
                ParsedJson = "{}",
                SignatureStatus = "Invalid",
                SignatureValid = false
            });
        }
    }

    private async Task<IntegrationSyncResult> SyncDepartmentsAsync(
        IntegrationConnector connector,
        string configJson,
        string requestJson)
    {
        var rows = ExtractRows(configJson, requestJson, "departments", "departmentItems", "deptList");
        var successCount = 0;
        var skippedCount = 0;
        var conflicts = new List<object>();

        foreach (var row in rows)
        {
            var externalDepartmentId = ReadString(row, "externalDepartmentId", "external_department_id", "deptId", "dept_id", "id");
            if (string.IsNullOrWhiteSpace(externalDepartmentId))
            {
                skippedCount++;
                conflicts.Add(new { reason = "外部部门 ID 为空。", raw = row.GetRawText() });
                continue;
            }

            await _bindingService.SaveDepartmentBindingAsync(new IntegrationDepartmentBindingSaveInput
            {
                ConnectorId = connector.Id,
                ExternalDepartmentId = externalDepartmentId,
                ExternalDepartmentName = ReadString(row, "externalDepartmentName", "external_department_name", "name", "title"),
                MatchType = IntegrationBindingMatchTypes.Sync,
                ParentExternalDepartmentId = ReadString(row, "parentExternalDepartmentId", "parent_external_department_id", "parentId", "parent_id"),
                Remark = "集成沙箱部门同步导入",
                Status = IntegrationBindingStatus.Pending
            });
            successCount++;
        }

        return BuildSyncResult("部门同步", rows.Count, successCount, skippedCount, conflicts);
    }

    private async Task<IntegrationSyncResult> SyncUsersAsync(
        IntegrationConnector connector,
        string configJson,
        string requestJson)
    {
        var rows = ExtractRows(configJson, requestJson, "users", "userItems", "externalAccounts", "accounts");
        var successCount = 0;
        var skippedCount = 0;
        var conflicts = new List<object>();

        foreach (var row in rows)
        {
            var externalUserId = ReadString(row, "externalUserId", "external_user_id", "userId", "user_id", "openId", "id");
            if (string.IsNullOrWhiteSpace(externalUserId))
            {
                skippedCount++;
                conflicts.Add(new { reason = "外部用户 ID 为空。", raw = row.GetRawText() });
                continue;
            }

            await _bindingService.SaveExternalAccountAsync(new IntegrationExternalAccountSaveInput
            {
                AvatarUrl = ReadString(row, "avatarUrl", "avatar_url"),
                ConnectorId = connector.Id,
                DepartmentExternalId = ReadString(row, "departmentExternalId", "department_external_id", "deptId", "dept_id"),
                DisplayName = ReadString(row, "displayName", "display_name", "name"),
                Email = ReadString(row, "email", "mail"),
                ExternalUnionId = ReadString(row, "externalUnionId", "external_union_id", "unionId", "union_id"),
                ExternalUserId = externalUserId,
                Mobile = ReadString(row, "mobile", "phone"),
                RawJson = row.GetRawText(),
                Status = IntegrationBindingStatus.Pending
            });
            successCount++;
        }

        return BuildSyncResult("用户同步", rows.Count, successCount, skippedCount, conflicts);
    }

    private static IntegrationSyncResult BuildSyncResult(
        string actionName,
        int totalCount,
        int successCount,
        int skippedCount,
        IReadOnlyCollection<object> conflicts)
    {
        return new IntegrationSyncResult
        {
            ConflictCount = conflicts.Count,
            ConflictJson = conflicts.Count == 0 ? "{}" : JsonSerializer.Serialize(conflicts, JsonOptions),
            CreatedCount = successCount,
            Message = $"集成沙箱{actionName}完成：处理 {totalCount} 条，成功 {successCount} 条，跳过 {skippedCount} 条，冲突 {conflicts.Count} 条。",
            ResultJson = JsonSerializer.Serialize(new { actionName, totalCount, successCount, skippedCount }, JsonOptions),
            SkippedCount = skippedCount,
            SuccessCount = successCount,
            TotalCount = totalCount
        };
    }

    private static List<JsonElement> ExtractRows(string configJson, string requestJson, params string[] arrayNames)
    {
        var rows = new List<JsonElement>();
        ExtractRows(configJson, rows, arrayNames);
        ExtractRows(requestJson, rows, arrayNames);
        return rows;
    }

    private static void ExtractRows(string json, List<JsonElement> rows, IReadOnlyCollection<string> arrayNames)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return;
        }

        using var document = JsonDocument.Parse(json);
        Visit(document.RootElement, rows, arrayNames, depth: 0);
    }

    private static void Visit(JsonElement element, List<JsonElement> rows, IReadOnlyCollection<string> arrayNames, int depth)
    {
        if (depth > 6 || element.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        foreach (var property in element.EnumerateObject())
        {
            if (arrayNames.Any(name => property.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) &&
                property.Value.ValueKind == JsonValueKind.Array)
            {
                rows.AddRange(property.Value.EnumerateArray()
                    .Where(item => item.ValueKind == JsonValueKind.Object)
                    .Select(item => item.Clone()));
                continue;
            }

            if (property.Value.ValueKind == JsonValueKind.Object)
            {
                Visit(property.Value, rows, arrayNames, depth + 1);
            }
            else if (property.Value.ValueKind == JsonValueKind.String &&
                     (property.Name.Equals("requestJson", StringComparison.OrdinalIgnoreCase) ||
                      property.Name.Equals("configJson", StringComparison.OrdinalIgnoreCase)))
            {
                ExtractRows(property.Value.GetString() ?? string.Empty, rows, arrayNames);
            }
        }
    }

    private static string ReadString(JsonElement element, params string[] paths)
    {
        foreach (var path in paths)
        {
            if (TryReadPath(element, path, out var value))
            {
                return value;
            }
        }

        return string.Empty;
    }

    private static bool TryReadPath(JsonElement root, string path, out string value)
    {
        value = string.Empty;
        if (!TryResolvePath(root, path, out var current))
        {
            return false;
        }

        value = current.ValueKind switch
        {
            JsonValueKind.String => current.GetString()?.Trim() ?? string.Empty,
            JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False => current.ToString().Trim(),
            _ => string.Empty
        };
        return !string.IsNullOrWhiteSpace(value);
    }

    private static bool TryResolvePath(JsonElement root, string path, out JsonElement value)
    {
        value = root;
        foreach (var segment in path.Split('.', StringSplitOptions.RemoveEmptyEntries))
        {
            if (value.ValueKind != JsonValueKind.Object || !TryGetProperty(value, segment, out value))
            {
                return false;
            }
        }

        return true;
    }

    private static bool TryGetProperty(JsonElement element, string name, out JsonElement value)
    {
        if (element.TryGetProperty(name, out value))
        {
            return true;
        }

        foreach (var property in element.EnumerateObject())
        {
            if (property.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    private static SandboxOptions ReadOptions(string configJson)
    {
        if (string.IsNullOrWhiteSpace(configJson))
        {
            return new SandboxOptions();
        }

        try
        {
            return JsonSerializer.Deserialize<SandboxOptions>(configJson, JsonOptions) ?? new SandboxOptions();
        }
        catch (JsonException)
        {
            throw new UserFriendlyException("集成沙箱配置 JSON 格式不正确。");
        }
    }

    private static void EnsureBaseEnabled(IntegrationConnector connector, IntegrationTenantConfig? tenantConfig)
    {
        EnsureConnector(connector);
        if (!connector.IsEnabled)
        {
            throw new UserFriendlyException("集成沙箱连接器定义已停用。");
        }

        if (tenantConfig == null)
        {
            throw new UserFriendlyException("尚未保存集成沙箱租户级连接配置。");
        }

        if (!tenantConfig.IsEnabled)
        {
            throw new UserFriendlyException("集成沙箱租户级连接配置已停用。");
        }
    }

    private static void EnsureConnector(IntegrationConnector connector)
    {
        if (!connector.ConnectorCode.Equals("sandbox", StringComparison.OrdinalIgnoreCase))
        {
            throw new UserFriendlyException("集成沙箱适配器不能处理其他连接器。");
        }
    }

    private sealed class SandboxOptions
    {
        public bool AcceptMessage { get; set; } = true;

        public string EchoPrefix { get; set; } = string.Empty;

        public bool ForceFailure { get; set; }
    }
}
