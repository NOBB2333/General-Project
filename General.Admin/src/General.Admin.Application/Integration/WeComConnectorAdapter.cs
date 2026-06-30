using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Security.Encryption;

namespace General.Admin.Integration;

[ExposeServices(
    typeof(IIntegrationConnectorAdapter),
    typeof(IIntegrationMessageSender),
    typeof(IIntegrationSyncAdapter))]
public class WeComConnectorAdapter :
    IIntegrationConnectorAdapter,
    IIntegrationMessageSender,
    IIntegrationSyncAdapter,
    ITransientDependency
{
    private const string DefaultBaseUrl = "https://qyapi.weixin.qq.com/cgi-bin";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IRepository<IntegrationConnector, Guid> _connectorRepository;
    private readonly IRepository<IntegrationTenantConfig, Guid> _configRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IntegrationBindingService _bindingService;
    private readonly IntegrationSyncImportService _syncImportService;
    private readonly IStringEncryptionService _encryptionService;

    public WeComConnectorAdapter(
        IRepository<IntegrationConnector, Guid> connectorRepository,
        IRepository<IntegrationTenantConfig, Guid> configRepository,
        IHttpClientFactory httpClientFactory,
        IntegrationBindingService bindingService,
        IntegrationSyncImportService syncImportService,
        IStringEncryptionService encryptionService)
    {
        _connectorRepository = connectorRepository;
        _configRepository = configRepository;
        _httpClientFactory = httpClientFactory;
        _bindingService = bindingService;
        _syncImportService = syncImportService;
        _encryptionService = encryptionService;
    }

    public string ConnectorCode => "wecom";

    public async Task<IntegrationConnectionTestResultDto> TestConnectionAsync(
        IntegrationConnector connector,
        IntegrationTenantConfig? tenantConfig)
    {
        try
        {
            EnsureBaseEnabled(connector, tenantConfig);
            await GetAccessTokenAsync(tenantConfig!, forceRefresh: true);
            return new IntegrationConnectionTestResultDto
            {
                Success = true,
                Message = "企业微信 access_token 获取成功。"
            };
        }
        catch (UserFriendlyException ex)
        {
            return new IntegrationConnectionTestResultDto
            {
                Success = false,
                Message = ex.Message
            };
        }
        catch (Exception ex)
        {
            return new IntegrationConnectionTestResultDto
            {
                Success = false,
                Message = $"企业微信连接测试失败：{ex.Message}"
            };
        }
    }

    public async Task<IntegrationMessageSendResult> SendMessageAsync(IntegrationMessageSendInput input)
    {
        var connector = await _connectorRepository.GetAsync(input.ConnectorId);
        if (!connector.ConnectorCode.Equals(ConnectorCode, StringComparison.OrdinalIgnoreCase))
        {
            throw new UserFriendlyException("当前企业微信适配器不能处理其他连接器。");
        }

        var config = await _configRepository.FindAsync(x => x.ConnectorId == input.ConnectorId);
        EnsureBaseEnabled(connector, config);

        if (IsRobotTarget(input.TargetType))
        {
            return await SendRobotMessageAsync(config!, input);
        }

        var token = await GetAccessTokenAsync(config!, forceRefresh: false);
        var agentId = ReadRequiredConfig(config!.ConfigJson, "agentId", "企业微信 Agent ID 未配置。");
        var target = ResolveAppMessageTarget(input.TargetType, input.TargetId);
        var payload = BuildAppMessagePayload(agentId, input.MessageType, input.Body, target);
        var root = await SendWeComPostAsync(
            config,
            token,
            "/message/send",
            payload,
            "企业微信应用消息发送失败");

        var invalidUsers = ReadString(root, "invaliduser");
        var message = string.IsNullOrWhiteSpace(invalidUsers)
            ? "企业微信应用消息发送成功。"
            : $"企业微信应用消息已发送，部分用户无效：{invalidUsers}";
        return new IntegrationMessageSendResult
        {
            ExternalMessageId = ReadString(root, "msgid", "response_code"),
            Message = message,
            Success = true
        };
    }

    public async Task<IntegrationSyncResult> ExecuteSyncAsync(IntegrationSyncExecuteInput input)
    {
        var connector = await _connectorRepository.GetAsync(input.ConnectorId);
        if (!connector.ConnectorCode.Equals(ConnectorCode, StringComparison.OrdinalIgnoreCase))
        {
            throw new UserFriendlyException("当前企业微信适配器不能处理其他连接器。");
        }

        var config = await _configRepository.FindAsync(x => x.ConnectorId == input.ConnectorId);
        EnsureBaseEnabled(connector, config);

        var syncType = input.SyncType.Trim().ToLowerInvariant();
        if (syncType == "group")
        {
            return await _syncImportService.SyncGroupsAsync(connector, input.ConfigJson, input.RequestJson, "企业微信");
        }

        if (syncType == "meeting")
        {
            return await _syncImportService.SyncMeetingsAsync(connector, input.ConfigJson, input.RequestJson, "企业微信");
        }

        var token = await GetAccessTokenAsync(config!, forceRefresh: false);
        var options = BuildSyncOptions(input.ConfigJson, input.RequestJson);
        return syncType switch
        {
            "department" => await SyncDepartmentsAsync(config!, token, options),
            "user" => await SyncUsersAsync(config!, token, options),
            _ => throw new UserFriendlyException("企业微信同步暂只支持部门、用户、群和会议记录。")
        };
    }

    private async Task<IntegrationMessageSendResult> SendRobotMessageAsync(
        IntegrationTenantConfig config,
        IntegrationMessageSendInput input)
    {
        var url = ResolveRobotWebhookUrl(config.ConfigJson, input.TargetId);
        var payload = BuildRobotMessagePayload(input.MessageType, input.Body);
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        using var response = await _httpClientFactory.CreateClient().SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new UserFriendlyException($"企业微信群机器人消息发送失败：HTTP {(int)response.StatusCode}。");
        }

        using var document = JsonDocument.Parse(responseBody);
        EnsureWeComSuccess(document.RootElement, "企业微信群机器人消息发送失败");
        return new IntegrationMessageSendResult
        {
            ExternalMessageId = ReadString(document.RootElement, "msgid", "response_code"),
            Message = "企业微信群机器人消息发送成功。",
            Success = true
        };
    }

    private async Task<IntegrationSyncResult> SyncDepartmentsAsync(
        IntegrationTenantConfig config,
        string token,
        WeComSyncOptions options)
    {
        var departments = await FetchDepartmentsAsync(config, token, options);
        foreach (var department in departments)
        {
            await _bindingService.SaveDepartmentBindingAsync(new IntegrationDepartmentBindingSaveInput
            {
                ConnectorId = config.ConnectorId,
                ExternalDepartmentId = department.ExternalDepartmentId,
                ExternalDepartmentName = department.Name,
                MatchType = IntegrationBindingMatchTypes.Sync,
                ParentExternalDepartmentId = department.ParentExternalDepartmentId,
                Remark = "企业微信部门同步导入",
                Status = IntegrationBindingStatus.Pending
            });
        }

        return new IntegrationSyncResult
        {
            Message = $"企业微信部门同步完成：导入 {departments.Count} 个部门，待人工映射到内部组织。",
            ResultJson = JsonSerializer.Serialize(new
            {
                options.RootDepartmentId,
                Departments = departments.Select(item => new
                {
                    item.ExternalDepartmentId,
                    item.Name,
                    item.ParentExternalDepartmentId
                })
            }),
            SuccessCount = departments.Count,
            TotalCount = departments.Count
        };
    }

    private async Task<IntegrationSyncResult> SyncUsersAsync(
        IntegrationTenantConfig config,
        string token,
        WeComSyncOptions options)
    {
        var departments = await FetchDepartmentsAsync(config, token, options);
        var departmentIds = departments
            .Select(item => item.ExternalDepartmentId)
            .Prepend(options.RootDepartmentId)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var users = await FetchUsersAsync(config, token, options, departmentIds);
        foreach (var user in users)
        {
            await _bindingService.SaveExternalAccountAsync(new IntegrationExternalAccountSaveInput
            {
                AvatarUrl = user.AvatarUrl,
                ConnectorId = config.ConnectorId,
                DepartmentExternalId = user.DepartmentExternalId,
                DisplayName = user.DisplayName,
                Email = user.Email,
                ExternalUnionId = user.ExternalUnionId,
                ExternalUserId = user.ExternalUserId,
                Mobile = user.Mobile,
                RawJson = user.RawJson,
                Status = IntegrationBindingStatus.Pending
            });
        }

        IntegrationAutoMatchResultDto? autoMatch = null;
        if (options.AutoMatchUsers)
        {
            autoMatch = await _bindingService.AutoMatchUsersAsync(new IntegrationAutoMatchInput
            {
                ConnectorId = config.ConnectorId
            });
        }

        return new IntegrationSyncResult
        {
            ConflictCount = autoMatch?.ConflictCount ?? 0,
            Message = autoMatch == null
                ? $"企业微信用户同步完成：导入 {users.Count} 个外部账号，未执行自动匹配。"
                : $"企业微信用户同步完成：导入 {users.Count} 个外部账号，自动绑定 {autoMatch.BoundCount} 个，冲突 {autoMatch.ConflictCount} 个，待处理 {autoMatch.PendingCount} 个。",
            ResultJson = JsonSerializer.Serialize(new
            {
                options.RootDepartmentId,
                options.AutoMatchUsers,
                DepartmentCount = departments.Count,
                UserCount = users.Count,
                AutoMatch = autoMatch
            }),
            SuccessCount = users.Count,
            TotalCount = users.Count
        };
    }

    private async Task<List<WeComDepartmentSyncItem>> FetchDepartmentsAsync(
        IntegrationTenantConfig config,
        string token,
        WeComSyncOptions options)
    {
        var query = string.IsNullOrWhiteSpace(options.RootDepartmentId)
            ? string.Empty
            : $"&id={Uri.EscapeDataString(options.RootDepartmentId)}";
        var root = await SendWeComGetAsync(
            config,
            token,
            $"/department/list?access_token={Uri.EscapeDataString(token)}{query}",
            "企业微信部门同步失败");
        var departments = new Dictionary<string, WeComDepartmentSyncItem>(StringComparer.OrdinalIgnoreCase);
        if (root.TryGetProperty("department", out var items) && items.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in items.EnumerateArray())
            {
                var department = ParseDepartmentItem(item, options.RootDepartmentId);
                if (!string.IsNullOrWhiteSpace(department.ExternalDepartmentId))
                {
                    departments[department.ExternalDepartmentId] = department;
                }
            }
        }

        return departments.Values
            .OrderBy(item => item.ParentExternalDepartmentId)
            .ThenBy(item => item.Name)
            .ToList();
    }

    private async Task<List<WeComUserSyncItem>> FetchUsersAsync(
        IntegrationTenantConfig config,
        string token,
        WeComSyncOptions options,
        IReadOnlyCollection<string> departmentIds)
    {
        var users = new Dictionary<string, WeComUserSyncItem>(StringComparer.OrdinalIgnoreCase);
        foreach (var departmentId in departmentIds)
        {
            var root = await SendWeComGetAsync(
                config,
                token,
                $"/user/list?access_token={Uri.EscapeDataString(token)}&department_id={Uri.EscapeDataString(departmentId)}&fetch_child={(options.FetchChildUsers ? 1 : 0)}",
                "企业微信用户同步失败");
            if (!root.TryGetProperty("userlist", out var items) || items.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var item in items.EnumerateArray())
            {
                var user = ParseUserItem(item, departmentId);
                if (!string.IsNullOrWhiteSpace(user.ExternalUserId))
                {
                    users[user.ExternalUserId] = user;
                }
            }
        }

        return users.Values
            .OrderBy(item => item.DisplayName)
            .ThenBy(item => item.ExternalUserId)
            .ToList();
    }

    private async Task<JsonElement> SendWeComPostAsync(
        IntegrationTenantConfig config,
        string token,
        string path,
        string payload,
        string errorPrefix)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{ResolveBaseUrl(config.ConfigJson)}{path}?access_token={Uri.EscapeDataString(token)}")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await _httpClientFactory.CreateClient().SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new UserFriendlyException($"{errorPrefix}：HTTP {(int)response.StatusCode}。");
        }

        using var document = JsonDocument.Parse(responseBody);
        EnsureWeComSuccess(document.RootElement, errorPrefix);
        return document.RootElement.Clone();
    }

    private async Task<JsonElement> SendWeComGetAsync(
        IntegrationTenantConfig config,
        string token,
        string pathAndQuery,
        string errorPrefix)
    {
        var url = pathAndQuery.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? pathAndQuery
            : $"{ResolveBaseUrl(config.ConfigJson)}{pathAndQuery}";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await _httpClientFactory.CreateClient().SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new UserFriendlyException($"{errorPrefix}：HTTP {(int)response.StatusCode}。");
        }

        using var document = JsonDocument.Parse(responseBody);
        EnsureWeComSuccess(document.RootElement, errorPrefix);
        return document.RootElement.Clone();
    }

    private async Task<string> GetAccessTokenAsync(IntegrationTenantConfig config, bool forceRefresh)
    {
        if (!forceRefresh &&
            !string.IsNullOrWhiteSpace(config.AccessTokenCipherText) &&
            config.TokenExpiresAt.HasValue &&
            config.TokenExpiresAt.Value > DateTime.UtcNow.AddMinutes(5))
        {
            return Decrypt(config.AccessTokenCipherText, "企业微信访问 Token 解密失败。");
        }

        var corpId = ReadRequiredConfig(config.ConfigJson, "corpId", "企业微信 Corp ID 未配置。");
        var corpSecret = DecryptMainSecretValue(config.SecretCipherText, "corpSecret", "企业微信 Corp Secret 未配置。");
        var url = $"{ResolveBaseUrl(config.ConfigJson)}/gettoken?corpid={Uri.EscapeDataString(corpId)}&corpsecret={Uri.EscapeDataString(corpSecret)}";
        using var response = await _httpClientFactory.CreateClient().GetAsync(url);
        var responseBody = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new UserFriendlyException($"企业微信 Token 获取失败：HTTP {(int)response.StatusCode}。");
        }

        using var document = JsonDocument.Parse(responseBody);
        EnsureWeComSuccess(document.RootElement, "企业微信 Token 获取失败");
        var token = ReadString(document.RootElement, "access_token");
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UserFriendlyException("企业微信 Token 响应缺少 access_token。");
        }

        var expireSeconds = ReadInt(document.RootElement, "expires_in", 7200);
        config.ResetAccessToken(Encrypt(token), DateTime.UtcNow.AddSeconds(Math.Max(60, expireSeconds)));
        await _configRepository.UpdateAsync(config, autoSave: true);
        return token;
    }

    private static void EnsureBaseEnabled(IntegrationConnector connector, IntegrationTenantConfig? config)
    {
        if (!connector.IsEnabled)
        {
            throw new UserFriendlyException("企业微信连接器定义已停用。");
        }

        if (config == null)
        {
            throw new UserFriendlyException("尚未保存企业微信租户级连接配置。");
        }

        if (!config.IsEnabled)
        {
            throw new UserFriendlyException("企业微信租户级连接配置已停用。");
        }

        if (!config.HasSecret())
        {
            throw new UserFriendlyException("企业微信 Corp Secret 未配置。");
        }
    }

    internal static string BuildAppMessagePayload(
        string agentId,
        string messageType,
        string body,
        WeComAppMessageTarget target)
    {
        var message = BuildWeComMessage(messageType, Check.NotNullOrWhiteSpace(body, nameof(body), 64_000).Trim());
        var root = new Dictionary<string, object?>
        {
            ["touser"] = target.ToUser,
            ["toparty"] = target.ToParty,
            ["totag"] = target.ToTag,
            ["msgtype"] = message.MessageType,
            ["agentid"] = Check.NotNullOrWhiteSpace(agentId, nameof(agentId), 64).Trim(),
            [message.MessageType] = message.Content,
            ["enable_duplicate_check"] = 1
        }.Where(item => item.Value != null).ToDictionary(item => item.Key, item => item.Value);
        return JsonSerializer.Serialize(root, JsonOptions);
    }

    internal static string BuildRobotMessagePayload(string messageType, string body)
    {
        var message = BuildWeComMessage(messageType, Check.NotNullOrWhiteSpace(body, nameof(body), 64_000).Trim());
        return JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["msgtype"] = message.MessageType,
            [message.MessageType] = message.Content
        }, JsonOptions);
    }

    internal static WeComSyncOptions BuildSyncOptions(string configJson, string requestJson)
    {
        var rootDepartmentId = "1";
        var fetchChildUsers = false;
        var autoMatchUsers = true;

        ApplyOptions(configJson);
        ApplyOptions(requestJson);

        return new WeComSyncOptions(
            string.IsNullOrWhiteSpace(rootDepartmentId) ? "1" : rootDepartmentId.Trim(),
            fetchChildUsers,
            autoMatchUsers);

        void ApplyOptions(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            try
            {
                using var document = JsonDocument.Parse(json);
                if (document.RootElement.ValueKind != JsonValueKind.Object)
                {
                    return;
                }

                var root = document.RootElement;
                rootDepartmentId = ReadOptionString(root, rootDepartmentId, "rootDepartmentId", "RootDepartmentId", "departmentId", "DepartmentId");
                fetchChildUsers = ReadOptionBool(root, fetchChildUsers, "fetchChildUsers", "FetchChildUsers");
                autoMatchUsers = ReadOptionBool(root, autoMatchUsers, "autoMatchUsers", "AutoMatchUsers");

                var nestedRequestJson = ReadOptionString(root, string.Empty, "requestJson", "RequestJson");
                if (!string.IsNullOrWhiteSpace(nestedRequestJson))
                {
                    ApplyOptions(nestedRequestJson);
                }

                var nestedConfigJson = ReadOptionString(root, string.Empty, "configJson", "ConfigJson");
                if (!string.IsNullOrWhiteSpace(nestedConfigJson))
                {
                    ApplyOptions(nestedConfigJson);
                }
            }
            catch (JsonException)
            {
                throw new UserFriendlyException("企业微信同步配置 JSON 格式不正确。");
            }
        }
    }

    internal static WeComDepartmentSyncItem ParseDepartmentItem(JsonElement item, string fallbackParentDepartmentId)
    {
        var externalDepartmentId = ReadString(item, "id", "department_id");
        var name = ReadString(item, "name", "department_name");
        return new WeComDepartmentSyncItem(
            externalDepartmentId,
            string.IsNullOrWhiteSpace(name) ? externalDepartmentId : name,
            ReadString(item, "parentid", "parent_id") is { Length: > 0 } parentId ? parentId : fallbackParentDepartmentId,
            item.GetRawText());
    }

    internal static WeComUserSyncItem ParseUserItem(JsonElement item, string fallbackDepartmentId)
    {
        var externalUserId = ReadString(item, "userid", "user_id", "id");
        var displayName = ReadString(item, "name", "alias");
        var departmentId = ReadFirstStringFromArray(item, "department", "department_ids", "department_id");
        return new WeComUserSyncItem(
            externalUserId,
            string.IsNullOrWhiteSpace(displayName) ? externalUserId : displayName,
            ReadString(item, "unionid", "external_userid"),
            ReadString(item, "mobile"),
            ReadString(item, "email", "biz_mail"),
            ReadString(item, "avatar", "thumb_avatar", "avatar_url"),
            string.IsNullOrWhiteSpace(departmentId) ? fallbackDepartmentId : departmentId,
            item.GetRawText());
    }

    private static WeComMessage BuildWeComMessage(string messageType, string body)
    {
        return messageType.Trim().ToLowerInvariant() switch
        {
            "markdown" or "md" => new WeComMessage("markdown", new { content = body }),
            "card" or "templatecard" or "template_card" => new WeComMessage("template_card", ParseTemplateCard(body)),
            _ => new WeComMessage("text", new { content = body })
        };
    }

    private static JsonElement ParseTemplateCard(string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new UserFriendlyException("企业微信卡片消息内容必须是 JSON 对象。");
            }

            return document.RootElement.Clone();
        }
        catch (JsonException ex)
        {
            throw new UserFriendlyException($"企业微信卡片消息内容不是合法 JSON：{ex.Message}");
        }
    }

    private static WeComAppMessageTarget ResolveAppMessageTarget(string targetType, string targetId)
    {
        var normalizedTargetId = Check.NotNullOrWhiteSpace(targetId, nameof(targetId), 256).Trim();
        return targetType.Trim().ToLowerInvariant() switch
        {
            "all" or "alluser" or "all_user" => new WeComAppMessageTarget("@all", null, null),
            "department" or "dept" or "party" or "partyid" or "party_id" => new WeComAppMessageTarget(null, normalizedTargetId, null),
            "tag" or "tagid" or "tag_id" => new WeComAppMessageTarget(null, null, normalizedTargetId),
            "user" or "userid" or "user_id" or "openid" or "open_id" => new WeComAppMessageTarget(normalizedTargetId, null, null),
            _ => throw new UserFriendlyException("企业微信接收人类型不支持。")
        };
    }

    private static string ResolveRobotWebhookUrl(string configJson, string targetId)
    {
        var value = !string.IsNullOrWhiteSpace(targetId)
            ? targetId.Trim()
            : ReadOptionalConfig(configJson, "robotWebhook");
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new UserFriendlyException("企业微信群机器人 Webhook 或 Key 未配置。");
        }

        if (value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            value.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return value;
        }

        return $"{ResolveBaseUrl(configJson)}/webhook/send?key={Uri.EscapeDataString(value)}";
    }

    private static bool IsRobotTarget(string targetType)
    {
        return targetType.Trim().Equals("Robot", StringComparison.OrdinalIgnoreCase) ||
               targetType.Trim().Equals("Group", StringComparison.OrdinalIgnoreCase) ||
               targetType.Trim().Equals("Chat", StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveBaseUrl(string configJson)
    {
        var baseUrl = ReadOptionalConfig(configJson, "baseUrl");
        return string.IsNullOrWhiteSpace(baseUrl)
            ? DefaultBaseUrl
            : baseUrl.Trim().TrimEnd('/');
    }

    private static void EnsureWeComSuccess(JsonElement root, string errorPrefix)
    {
        var errCode = root.TryGetProperty("errcode", out var codeElement) && codeElement.TryGetInt32(out var code)
            ? code
            : -1;
        if (errCode == 0)
        {
            return;
        }

        var message = ReadString(root, "errmsg", "message", "msg");
        throw new UserFriendlyException($"{errorPrefix}：{(string.IsNullOrWhiteSpace(message) ? "未知错误" : message)}");
    }

    private string Encrypt(string value)
    {
        return _encryptionService.Encrypt(value) ?? throw new InvalidOperationException("企业微信 Token 加密失败。");
    }

    private string Decrypt(string value, string errorMessage)
    {
        var decrypted = _encryptionService.Decrypt(value);
        if (string.IsNullOrWhiteSpace(decrypted))
        {
            throw new UserFriendlyException(errorMessage);
        }

        return decrypted;
    }

    private string DecryptMainSecretValue(string value, string key, string errorMessage)
    {
        var decrypted = Decrypt(value, errorMessage);
        if (TryReadSecretJson(decrypted, key, out var secret))
        {
            return secret;
        }

        return decrypted;
    }

    private static bool TryReadSecretJson(string value, string key, out string secret)
    {
        secret = string.Empty;
        if (string.IsNullOrWhiteSpace(value) || !value.TrimStart().StartsWith('{'))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(value);
            if (document.RootElement.ValueKind != JsonValueKind.Object ||
                !document.RootElement.TryGetProperty(key, out var element) ||
                element.ValueKind != JsonValueKind.String)
            {
                return false;
            }

            secret = element.GetString()?.Trim() ?? string.Empty;
            return !string.IsNullOrWhiteSpace(secret);
        }
        catch (JsonException)
        {
            throw new UserFriendlyException("企业微信连接密钥 JSON 格式不正确。");
        }
    }

    private static string ReadRequiredConfig(string configJson, string key, string errorMessage)
    {
        var value = ReadOptionalConfig(configJson, key);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new UserFriendlyException(errorMessage);
        }

        return value.Trim();
    }

    private static string ReadOptionalConfig(string configJson, string key)
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(configJson) ? "{}" : configJson);
        return document.RootElement.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : string.Empty;
    }

    private static string ReadString(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (!element.TryGetProperty(name, out var value))
            {
                continue;
            }

            if (value.ValueKind == JsonValueKind.String)
            {
                return value.GetString()?.Trim() ?? string.Empty;
            }

            if (value.ValueKind is JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False)
            {
                return value.ToString().Trim();
            }
        }

        return string.Empty;
    }

    private static int ReadInt(JsonElement element, string name, int fallback)
    {
        if (!element.TryGetProperty(name, out var value))
        {
            return fallback;
        }

        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number))
        {
            return number;
        }

        return value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out number)
            ? number
            : fallback;
    }

    private static string ReadFirstStringFromArray(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (!element.TryGetProperty(name, out var value))
            {
                continue;
            }

            if (value.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in value.EnumerateArray())
                {
                    var text = item.ValueKind == JsonValueKind.String ? item.GetString() : item.ToString();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        return text.Trim();
                    }
                }
            }
            else if (value.ValueKind is JsonValueKind.String or JsonValueKind.Number)
            {
                return value.ToString().Trim();
            }
        }

        return string.Empty;
    }

    private static bool ReadOptionBool(JsonElement element, bool fallback, params string[] names)
    {
        foreach (var name in names)
        {
            if (!element.TryGetProperty(name, out var value))
            {
                continue;
            }

            return value.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Number => value.TryGetInt32(out var number) ? number != 0 : fallback,
                JsonValueKind.String => bool.TryParse(value.GetString(), out var parsed) ? parsed : fallback,
                _ => fallback
            };
        }

        return fallback;
    }

    private static string ReadOptionString(JsonElement element, string fallback, params string[] names)
    {
        foreach (var name in names)
        {
            if (!element.TryGetProperty(name, out var value))
            {
                continue;
            }

            if (value.ValueKind == JsonValueKind.String)
            {
                return value.GetString()?.Trim() ?? fallback;
            }

            if (value.ValueKind is JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False)
            {
                return value.ToString().Trim();
            }
        }

        return fallback;
    }

    private sealed record WeComMessage(string MessageType, object Content);

    internal sealed record WeComAppMessageTarget(string? ToUser, string? ToParty, string? ToTag);

    internal sealed record WeComSyncOptions(string RootDepartmentId, bool FetchChildUsers, bool AutoMatchUsers);

    internal sealed record WeComDepartmentSyncItem(
        string ExternalDepartmentId,
        string Name,
        string ParentExternalDepartmentId,
        string RawJson);

    internal sealed record WeComUserSyncItem(
        string ExternalUserId,
        string DisplayName,
        string ExternalUnionId,
        string Mobile,
        string Email,
        string AvatarUrl,
        string DepartmentExternalId,
        string RawJson);
}
