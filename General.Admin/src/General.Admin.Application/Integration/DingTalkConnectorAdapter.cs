using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
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
public class DingTalkConnectorAdapter :
    IIntegrationConnectorAdapter,
    IIntegrationMessageSender,
    IIntegrationSyncAdapter,
    ITransientDependency
{
    private const string DefaultBaseUrl = "https://oapi.dingtalk.com";

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

    public DingTalkConnectorAdapter(
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

    public string ConnectorCode => "dingtalk";

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
                Message = "钉钉 access_token 获取成功。"
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
                Message = $"钉钉连接测试失败：{ex.Message}"
            };
        }
    }

    public async Task<IntegrationMessageSendResult> SendMessageAsync(IntegrationMessageSendInput input)
    {
        var connector = await _connectorRepository.GetAsync(input.ConnectorId);
        if (!connector.ConnectorCode.Equals(ConnectorCode, StringComparison.OrdinalIgnoreCase))
        {
            throw new UserFriendlyException("当前钉钉适配器不能处理其他连接器。");
        }

        var config = await _configRepository.FindAsync(x => x.ConnectorId == input.ConnectorId);
        EnsureBaseEnabled(connector, config);

        if (IsRobotTarget(input.TargetType))
        {
            return await SendRobotMessageAsync(config!, input);
        }

        var token = await GetAccessTokenAsync(config!, forceRefresh: false);
        var agentId = ReadRequiredConfig(config!.ConfigJson, "agentId", "钉钉 Agent ID 未配置。");
        var target = ResolveWorkMessageTarget(input.TargetType, input.TargetId);
        var payload = BuildWorkMessagePayload(agentId, input.MessageType, input.Body, target);
        var root = await SendDingTalkPostAsync(
            config,
            token,
            "/topapi/message/corpconversation/asyncsend_v2",
            payload,
            "钉钉工作通知发送失败");

        return new IntegrationMessageSendResult
        {
            ExternalMessageId = ReadString(root, "task_id"),
            Message = "钉钉工作通知发送成功。",
            Success = true
        };
    }

    public async Task<IntegrationSyncResult> ExecuteSyncAsync(IntegrationSyncExecuteInput input)
    {
        var connector = await _connectorRepository.GetAsync(input.ConnectorId);
        if (!connector.ConnectorCode.Equals(ConnectorCode, StringComparison.OrdinalIgnoreCase))
        {
            throw new UserFriendlyException("当前钉钉适配器不能处理其他连接器。");
        }

        var config = await _configRepository.FindAsync(x => x.ConnectorId == input.ConnectorId);
        EnsureBaseEnabled(connector, config);

        var syncType = input.SyncType.Trim().ToLowerInvariant();
        if (syncType == "group")
        {
            return await _syncImportService.SyncGroupsAsync(connector, input.ConfigJson, input.RequestJson, "钉钉");
        }

        if (syncType == "meeting")
        {
            return await _syncImportService.SyncMeetingsAsync(connector, input.ConfigJson, input.RequestJson, "钉钉");
        }

        var token = await GetAccessTokenAsync(config!, forceRefresh: false);
        var options = BuildSyncOptions(input.ConfigJson, input.RequestJson);
        return syncType switch
        {
            "department" => await SyncDepartmentsAsync(config!, token, options),
            "user" => await SyncUsersAsync(config!, token, options),
            _ => throw new UserFriendlyException("钉钉同步暂只支持部门、用户、群和会议记录。")
        };
    }

    private async Task<IntegrationMessageSendResult> SendRobotMessageAsync(
        IntegrationTenantConfig config,
        IntegrationMessageSendInput input)
    {
        var url = ResolveRobotWebhookUrl(config.ConfigJson, input.TargetId);
        var secret = DecryptOptionalSecretValue(config.SecretCipherText, "robotSecret");
        if (!string.IsNullOrWhiteSpace(secret))
        {
            url = AppendRobotSignature(url, secret);
        }

        var payload = BuildRobotMessagePayload(input.MessageType, input.Body);
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        using var response = await _httpClientFactory.CreateClient().SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new UserFriendlyException($"钉钉群机器人消息发送失败：HTTP {(int)response.StatusCode}。");
        }

        using var document = JsonDocument.Parse(responseBody);
        EnsureDingTalkSuccess(document.RootElement, "钉钉群机器人消息发送失败");
        return new IntegrationMessageSendResult
        {
            ExternalMessageId = ReadString(document.RootElement, "processQueryKey", "request_id"),
            Message = "钉钉群机器人消息发送成功。",
            Success = true
        };
    }

    private async Task<IntegrationSyncResult> SyncDepartmentsAsync(
        IntegrationTenantConfig config,
        string token,
        DingTalkSyncOptions options)
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
                Remark = "钉钉部门同步导入",
                Status = IntegrationBindingStatus.Pending
            });
        }

        return new IntegrationSyncResult
        {
            Message = $"钉钉部门同步完成：导入 {departments.Count} 个部门，待人工映射到内部组织。",
            ResultJson = JsonSerializer.Serialize(new
            {
                options.RootDepartmentId,
                options.RecursiveDepartment,
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
        DingTalkSyncOptions options)
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
                ? $"钉钉用户同步完成：导入 {users.Count} 个外部账号，未执行自动匹配。"
                : $"钉钉用户同步完成：导入 {users.Count} 个外部账号，自动绑定 {autoMatch.BoundCount} 个，冲突 {autoMatch.ConflictCount} 个，待处理 {autoMatch.PendingCount} 个。",
            ResultJson = JsonSerializer.Serialize(new
            {
                options.RootDepartmentId,
                options.RecursiveDepartment,
                options.AutoMatchUsers,
                DepartmentCount = departments.Count,
                UserCount = users.Count,
                AutoMatch = autoMatch
            }),
            SuccessCount = users.Count,
            TotalCount = users.Count
        };
    }

    private async Task<List<DingTalkDepartmentSyncItem>> FetchDepartmentsAsync(
        IntegrationTenantConfig config,
        string token,
        DingTalkSyncOptions options)
    {
        var departments = new Dictionary<string, DingTalkDepartmentSyncItem>(StringComparer.OrdinalIgnoreCase);
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var pending = new Queue<string>();
        pending.Enqueue(options.RootDepartmentId);

        while (pending.Count > 0)
        {
            var parentDepartmentId = pending.Dequeue();
            if (!visited.Add(parentDepartmentId))
            {
                continue;
            }

            var result = await SendDingTalkPostAsync(
                config,
                token,
                "/topapi/v2/department/listsub",
                JsonSerializer.Serialize(new
                {
                    dept_id = parentDepartmentId,
                    language = options.Language
                }, JsonOptions),
                "钉钉部门同步失败");

            foreach (var item in EnumerateArray(result))
            {
                var department = ParseDepartmentItem(item, parentDepartmentId);
                if (string.IsNullOrWhiteSpace(department.ExternalDepartmentId))
                {
                    continue;
                }

                departments[department.ExternalDepartmentId] = department;
                if (options.RecursiveDepartment)
                {
                    pending.Enqueue(department.ExternalDepartmentId);
                }
            }
        }

        return departments.Values
            .OrderBy(item => item.ParentExternalDepartmentId)
            .ThenBy(item => item.Name)
            .ToList();
    }

    private async Task<List<DingTalkUserSyncItem>> FetchUsersAsync(
        IntegrationTenantConfig config,
        string token,
        DingTalkSyncOptions options,
        IReadOnlyCollection<string> departmentIds)
    {
        var users = new Dictionary<string, DingTalkUserSyncItem>(StringComparer.OrdinalIgnoreCase);
        foreach (var departmentId in departmentIds)
        {
            var cursor = 0;
            var hasMore = false;
            do
            {
                var result = await SendDingTalkPostAsync(
                    config,
                    token,
                    "/topapi/v2/user/list",
                    JsonSerializer.Serialize(new
                    {
                        dept_id = departmentId,
                        cursor,
                        size = options.PageSize,
                        language = options.Language
                    }, JsonOptions),
                    "钉钉用户同步失败");

                if (result.TryGetProperty("list", out var list) && list.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in list.EnumerateArray())
                    {
                        var user = ParseUserItem(item, departmentId);
                        if (!string.IsNullOrWhiteSpace(user.ExternalUserId))
                        {
                            users[user.ExternalUserId] = user;
                        }
                    }
                }

                hasMore = ReadBool(result, "has_more");
                cursor = ReadInt(result, "next_cursor", cursor + options.PageSize);
            } while (hasMore);
        }

        return users.Values
            .OrderBy(item => item.DisplayName)
            .ThenBy(item => item.ExternalUserId)
            .ToList();
    }

    private async Task<JsonElement> SendDingTalkPostAsync(
        IntegrationTenantConfig config,
        string token,
        string path,
        string payload,
        string errorPrefix)
    {
        var url = $"{ResolveBaseUrl(config.ConfigJson)}{path}?access_token={Uri.EscapeDataString(token)}";
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
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
        EnsureDingTalkSuccess(document.RootElement, errorPrefix);
        if (document.RootElement.TryGetProperty("result", out var resultElement))
        {
            return resultElement.Clone();
        }

        return document.RootElement.Clone();
    }

    private async Task<string> GetAccessTokenAsync(IntegrationTenantConfig config, bool forceRefresh)
    {
        if (!forceRefresh &&
            !string.IsNullOrWhiteSpace(config.AccessTokenCipherText) &&
            config.TokenExpiresAt.HasValue &&
            config.TokenExpiresAt.Value > DateTime.UtcNow.AddMinutes(5))
        {
            return Decrypt(config.AccessTokenCipherText, "钉钉访问 Token 解密失败。");
        }

        var appKey = ReadRequiredConfig(config.ConfigJson, "appKey", "钉钉 App Key 未配置。");
        var appSecret = DecryptMainSecretValue(config.SecretCipherText, "appSecret", "钉钉 App Secret 未配置。");
        var url = $"{ResolveBaseUrl(config.ConfigJson)}/gettoken?appkey={Uri.EscapeDataString(appKey)}&appsecret={Uri.EscapeDataString(appSecret)}";
        using var response = await _httpClientFactory.CreateClient().GetAsync(url);
        var responseBody = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new UserFriendlyException($"钉钉 Token 获取失败：HTTP {(int)response.StatusCode}。");
        }

        using var document = JsonDocument.Parse(responseBody);
        EnsureDingTalkSuccess(document.RootElement, "钉钉 Token 获取失败");
        var token = ReadString(document.RootElement, "access_token");
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UserFriendlyException("钉钉 Token 响应缺少 access_token。");
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
            throw new UserFriendlyException("钉钉连接器定义已停用。");
        }

        if (config == null)
        {
            throw new UserFriendlyException("尚未保存钉钉租户级连接配置。");
        }

        if (!config.IsEnabled)
        {
            throw new UserFriendlyException("钉钉租户级连接配置已停用。");
        }

        if (!config.HasSecret())
        {
            throw new UserFriendlyException("钉钉 App Secret 未配置。");
        }
    }

    internal static string BuildWorkMessagePayload(
        string agentId,
        string messageType,
        string body,
        DingTalkWorkMessageTarget target)
    {
        var normalizedAgentId = Check.NotNullOrWhiteSpace(agentId, nameof(agentId), 64).Trim();
        var normalizedBody = Check.NotNullOrWhiteSpace(body, nameof(body), 64_000).Trim();
        var message = BuildDingTalkMessage(messageType, normalizedBody);
        return JsonSerializer.Serialize(new
        {
            agent_id = normalizedAgentId,
            userid_list = target.UserIdList,
            dept_id_list = target.DepartmentIdList,
            to_all_user = target.ToAllUser ? true : (bool?)null,
            msg = message
        }, JsonOptions);
    }

    internal static string BuildRobotMessagePayload(string messageType, string body)
    {
        return JsonSerializer.Serialize(BuildDingTalkMessage(messageType, Check.NotNullOrWhiteSpace(body, nameof(body), 64_000).Trim()), JsonOptions);
    }

    internal static DingTalkSyncOptions BuildSyncOptions(string configJson, string requestJson)
    {
        var rootDepartmentId = "1";
        var language = "zh_CN";
        var pageSize = 100;
        var recursiveDepartment = true;
        var autoMatchUsers = true;

        ApplyOptions(configJson);
        ApplyOptions(requestJson);

        return new DingTalkSyncOptions(
            string.IsNullOrWhiteSpace(rootDepartmentId) ? "1" : rootDepartmentId.Trim(),
            string.IsNullOrWhiteSpace(language) ? "zh_CN" : language.Trim(),
            Math.Clamp(pageSize, 1, 100),
            recursiveDepartment,
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
                rootDepartmentId = ReadOptionString(root, rootDepartmentId, "rootDepartmentId", "RootDepartmentId", "deptId", "DeptId");
                language = ReadOptionString(root, language, "language", "Language");
                pageSize = ReadOptionInt(root, pageSize, "pageSize", "PageSize");
                recursiveDepartment = ReadOptionBool(root, recursiveDepartment, "recursiveDepartment", "RecursiveDepartment");
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
                throw new UserFriendlyException("钉钉同步配置 JSON 格式不正确。");
            }
        }
    }

    internal static DingTalkDepartmentSyncItem ParseDepartmentItem(JsonElement item, string parentDepartmentId)
    {
        var externalDepartmentId = ReadString(item, "dept_id", "id");
        var name = ReadString(item, "name", "dept_name");
        return new DingTalkDepartmentSyncItem(
            externalDepartmentId,
            string.IsNullOrWhiteSpace(name) ? externalDepartmentId : name,
            ReadString(item, "parent_id", "parent_dept_id") is { Length: > 0 } parentId ? parentId : parentDepartmentId,
            item.GetRawText());
    }

    internal static DingTalkUserSyncItem ParseUserItem(JsonElement item, string fallbackDepartmentId)
    {
        var externalUserId = ReadString(item, "userid", "user_id", "id");
        var displayName = ReadString(item, "name", "nickname");
        var departmentId = ReadFirstStringFromArray(item, "dept_id_list", "department", "department_ids");
        return new DingTalkUserSyncItem(
            externalUserId,
            string.IsNullOrWhiteSpace(displayName) ? externalUserId : displayName,
            ReadString(item, "unionid", "union_id"),
            ReadString(item, "mobile"),
            ReadString(item, "email"),
            ReadString(item, "avatar", "avatar_url"),
            string.IsNullOrWhiteSpace(departmentId) ? fallbackDepartmentId : departmentId,
            item.GetRawText());
    }

    internal static string GenerateRobotSignature(string secret, long timestamp)
    {
        var stringToSign = $"{timestamp}\n{secret}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));
    }

    private static string AppendRobotSignature(string url, string secret)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var sign = Uri.EscapeDataString(GenerateRobotSignature(secret, timestamp));
        var separator = url.Contains('?') ? "&" : "?";
        return $"{url}{separator}timestamp={timestamp.ToString(CultureInfo.InvariantCulture)}&sign={sign}";
    }

    private static object BuildDingTalkMessage(string messageType, string body)
    {
        return messageType.Trim().ToLowerInvariant() switch
        {
            "markdown" or "md" => new
            {
                msgtype = "markdown",
                markdown = new
                {
                    title = "项目通知",
                    text = body
                }
            },
            _ => new
            {
                msgtype = "text",
                text = new
                {
                    content = body
                }
            }
        };
    }

    private static DingTalkWorkMessageTarget ResolveWorkMessageTarget(string targetType, string targetId)
    {
        var normalizedTargetId = Check.NotNullOrWhiteSpace(targetId, nameof(targetId), 256).Trim();
        return targetType.Trim().ToLowerInvariant() switch
        {
            "all" or "alluser" or "all_user" => new DingTalkWorkMessageTarget(null, null, true),
            "department" or "dept" or "deptid" or "dept_id" or "party" => new DingTalkWorkMessageTarget(null, normalizedTargetId, false),
            "user" or "userid" or "user_id" or "openid" or "open_id" => new DingTalkWorkMessageTarget(normalizedTargetId, null, false),
            _ => throw new UserFriendlyException("钉钉接收人类型不支持。")
        };
    }

    private static string ResolveRobotWebhookUrl(string configJson, string targetId)
    {
        var value = !string.IsNullOrWhiteSpace(targetId)
            ? targetId.Trim()
            : ReadOptionalConfig(configJson, "robotWebhook");
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new UserFriendlyException("钉钉群机器人 Webhook 或 access_token 未配置。");
        }

        if (value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            value.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return value;
        }

        return $"{ResolveBaseUrl(configJson)}/robot/send?access_token={Uri.EscapeDataString(value)}";
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

    private static void EnsureDingTalkSuccess(JsonElement root, string errorPrefix)
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

    private static IEnumerable<JsonElement> EnumerateArray(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Array)
        {
            yield break;
        }

        foreach (var item in element.EnumerateArray())
        {
            yield return item;
        }
    }

    private string Encrypt(string value)
    {
        return _encryptionService.Encrypt(value) ?? throw new InvalidOperationException("钉钉 Token 加密失败。");
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

    private string DecryptOptionalSecretValue(string value, string key)
    {
        var decrypted = _encryptionService.Decrypt(value);
        return TryReadSecretJson(decrypted ?? string.Empty, key, out var secret)
            ? secret
            : string.Empty;
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
            throw new UserFriendlyException("钉钉连接密钥 JSON 格式不正确。");
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

    private static bool ReadBool(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value))
        {
            return false;
        }

        return value.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String => bool.TryParse(value.GetString(), out var parsed) && parsed,
            _ => false
        };
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

    private static int ReadOptionInt(JsonElement element, int fallback, params string[] names)
    {
        foreach (var name in names)
        {
            if (!element.TryGetProperty(name, out var value))
            {
                continue;
            }

            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number))
            {
                return number;
            }

            if (value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out number))
            {
                return number;
            }
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

    internal sealed record DingTalkWorkMessageTarget(string? UserIdList, string? DepartmentIdList, bool ToAllUser);

    internal sealed record DingTalkSyncOptions(
        string RootDepartmentId,
        string Language,
        int PageSize,
        bool RecursiveDepartment,
        bool AutoMatchUsers);

    internal sealed record DingTalkDepartmentSyncItem(
        string ExternalDepartmentId,
        string Name,
        string ParentExternalDepartmentId,
        string RawJson);

    internal sealed record DingTalkUserSyncItem(
        string ExternalUserId,
        string DisplayName,
        string ExternalUnionId,
        string Mobile,
        string Email,
        string AvatarUrl,
        string DepartmentExternalId,
        string RawJson);
}
