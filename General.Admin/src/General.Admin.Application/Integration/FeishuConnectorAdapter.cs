using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Security.Encryption;

namespace General.Admin.Integration;

[ExposeServices(
    typeof(IIntegrationConnectorAdapter),
    typeof(IIntegrationMessageSender),
    typeof(IIntegrationSyncAdapter))]
public class FeishuConnectorAdapter :
    IIntegrationConnectorAdapter,
    IIntegrationMessageSender,
    IIntegrationSyncAdapter,
    ITransientDependency
{
    private const string DefaultBaseUrl = "https://open.feishu.cn/open-apis";

    private readonly IRepository<IntegrationConnector, Guid> _connectorRepository;
    private readonly IRepository<IntegrationTenantConfig, Guid> _configRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IntegrationBindingService _bindingService;
    private readonly IntegrationSyncImportService _syncImportService;
    private readonly IStringEncryptionService _encryptionService;

    public FeishuConnectorAdapter(
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

    public string ConnectorCode => "feishu";

    public async Task<IntegrationConnectionTestResultDto> TestConnectionAsync(
        IntegrationConnector connector,
        IntegrationTenantConfig? tenantConfig)
    {
        try
        {
            EnsureBaseEnabled(connector, tenantConfig);
            await GetTenantAccessTokenAsync(tenantConfig!, forceRefresh: true);
            return new IntegrationConnectionTestResultDto
            {
                Success = true,
                Message = "飞书 tenant_access_token 获取成功。"
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
                Message = $"飞书连接测试失败：{ex.Message}"
            };
        }
    }

    public async Task<IntegrationMessageSendResult> SendMessageAsync(IntegrationMessageSendInput input)
    {
        var connector = await _connectorRepository.GetAsync(input.ConnectorId);
        if (!connector.ConnectorCode.Equals(ConnectorCode, StringComparison.OrdinalIgnoreCase))
        {
            throw new UserFriendlyException("当前飞书适配器不能处理其他连接器。");
        }

        var config = await _configRepository.FindAsync(x => x.ConnectorId == input.ConnectorId);
        EnsureBaseEnabled(connector, config);

        var token = await GetTenantAccessTokenAsync(config!, forceRefresh: false);
        var receiveIdType = ResolveReceiveIdType(input.TargetType);
        var (messageType, content) = BuildMessageContent(input.MessageType, input.Body);
        var url = $"{ResolveBaseUrl(config!.ConfigJson)}/im/v1/messages?receive_id_type={Uri.EscapeDataString(receiveIdType)}";
        var payload = JsonSerializer.Serialize(new
        {
            receive_id = Check.NotNullOrWhiteSpace(input.TargetId, nameof(input.TargetId), 256).Trim(),
            msg_type = messageType,
            content
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClientFactory.CreateClient().SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new UserFriendlyException($"飞书消息发送失败：HTTP {(int)response.StatusCode}。");
        }

        using var document = JsonDocument.Parse(responseBody);
        var code = document.RootElement.TryGetProperty("code", out var codeElement) ? codeElement.GetInt32() : -1;
        if (code != 0)
        {
            var message = document.RootElement.TryGetProperty("msg", out var messageElement)
                ? messageElement.GetString()
                : "未知错误";
            throw new UserFriendlyException($"飞书消息发送失败：{message}");
        }

        var externalMessageId = string.Empty;
        if (document.RootElement.TryGetProperty("data", out var dataElement) &&
            dataElement.TryGetProperty("message_id", out var messageIdElement))
        {
            externalMessageId = messageIdElement.GetString() ?? string.Empty;
        }

        return new IntegrationMessageSendResult
        {
            ExternalMessageId = externalMessageId,
            Message = "飞书消息发送成功。",
            Success = true
        };
    }

    public async Task<IntegrationSyncResult> ExecuteSyncAsync(IntegrationSyncExecuteInput input)
    {
        var connector = await _connectorRepository.GetAsync(input.ConnectorId);
        if (!connector.ConnectorCode.Equals(ConnectorCode, StringComparison.OrdinalIgnoreCase))
        {
            throw new UserFriendlyException("当前飞书适配器不能处理其他连接器。");
        }

        var config = await _configRepository.FindAsync(x => x.ConnectorId == input.ConnectorId);
        EnsureBaseEnabled(connector, config);

        var syncType = input.SyncType.Trim().ToLowerInvariant();
        if (syncType == "group")
        {
            return await _syncImportService.SyncGroupsAsync(connector, input.ConfigJson, input.RequestJson, "飞书");
        }

        if (syncType == "meeting")
        {
            return await _syncImportService.SyncMeetingsAsync(connector, input.ConfigJson, input.RequestJson, "飞书");
        }

        var token = await GetTenantAccessTokenAsync(config!, forceRefresh: false);
        var options = BuildSyncOptions(input.ConfigJson, input.RequestJson);
        return syncType switch
        {
            "department" => await SyncDepartmentsAsync(config!, token, options),
            "user" => await SyncUsersAsync(config!, token, options),
            _ => throw new UserFriendlyException("飞书同步暂只支持部门、用户、群和会议记录。")
        };
    }

    private async Task<IntegrationSyncResult> SyncDepartmentsAsync(
        IntegrationTenantConfig config,
        string token,
        FeishuSyncOptions options)
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
                Remark = "飞书部门同步导入",
                Status = IntegrationBindingStatus.Pending
            });
        }

        return new IntegrationSyncResult
        {
            Message = $"飞书部门同步完成：导入 {departments.Count} 个部门，待人工映射到内部组织。",
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
        FeishuSyncOptions options)
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
                ? $"飞书用户同步完成：导入 {users.Count} 个外部账号，未执行自动匹配。"
                : $"飞书用户同步完成：导入 {users.Count} 个外部账号，自动绑定 {autoMatch.BoundCount} 个，冲突 {autoMatch.ConflictCount} 个，待处理 {autoMatch.PendingCount} 个。",
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

    private async Task<List<FeishuDepartmentSyncItem>> FetchDepartmentsAsync(
        IntegrationTenantConfig config,
        string token,
        FeishuSyncOptions options)
    {
        var departments = new Dictionary<string, FeishuDepartmentSyncItem>(StringComparer.OrdinalIgnoreCase);
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

            var pageToken = string.Empty;
            var hasMore = false;
            do
            {
                var query = new Dictionary<string, string>
                {
                    ["department_id_type"] = options.DepartmentIdType,
                    ["fetch_child"] = "false",
                    ["page_size"] = options.PageSize.ToString()
                };
                if (!string.IsNullOrWhiteSpace(pageToken))
                {
                    query["page_token"] = pageToken;
                }

                var data = await SendFeishuGetAsync(
                    config,
                    token,
                    $"/contact/v3/departments/{Uri.EscapeDataString(parentDepartmentId)}/children",
                    query,
                    "飞书部门同步失败");
                foreach (var item in EnumerateItems(data))
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

                pageToken = ReadString(data, "page_token");
                hasMore = ReadBool(data, "has_more");
            } while (hasMore && !string.IsNullOrWhiteSpace(pageToken));
        }

        return departments.Values
            .OrderBy(item => item.ParentExternalDepartmentId)
            .ThenBy(item => item.Name)
            .ToList();
    }

    private async Task<List<FeishuUserSyncItem>> FetchUsersAsync(
        IntegrationTenantConfig config,
        string token,
        FeishuSyncOptions options,
        IReadOnlyCollection<string> departmentIds)
    {
        var users = new Dictionary<string, FeishuUserSyncItem>(StringComparer.OrdinalIgnoreCase);
        foreach (var departmentId in departmentIds)
        {
            var pageToken = string.Empty;
            var hasMore = false;
            do
            {
                var query = new Dictionary<string, string>
                {
                    ["department_id"] = departmentId,
                    ["department_id_type"] = options.DepartmentIdType,
                    ["page_size"] = options.PageSize.ToString(),
                    ["user_id_type"] = options.UserIdType
                };
                if (!string.IsNullOrWhiteSpace(pageToken))
                {
                    query["page_token"] = pageToken;
                }

                var data = await SendFeishuGetAsync(
                    config,
                    token,
                    "/contact/v3/users/find_by_department",
                    query,
                    "飞书用户同步失败");
                foreach (var item in EnumerateItems(data))
                {
                    var user = ParseUserItem(item, departmentId);
                    if (!string.IsNullOrWhiteSpace(user.ExternalUserId))
                    {
                        users[user.ExternalUserId] = user;
                    }
                }

                pageToken = ReadString(data, "page_token");
                hasMore = ReadBool(data, "has_more");
            } while (hasMore && !string.IsNullOrWhiteSpace(pageToken));
        }

        return users.Values
            .OrderBy(item => item.DisplayName)
            .ThenBy(item => item.ExternalUserId)
            .ToList();
    }

    private async Task<JsonElement> SendFeishuGetAsync(
        IntegrationTenantConfig config,
        string token,
        string path,
        IReadOnlyDictionary<string, string> query,
        string errorPrefix)
    {
        var queryString = string.Join("&", query
            .Where(item => !string.IsNullOrWhiteSpace(item.Value))
            .Select(item => $"{Uri.EscapeDataString(item.Key)}={Uri.EscapeDataString(item.Value)}"));
        var url = $"{ResolveBaseUrl(config.ConfigJson)}{path}";
        if (!string.IsNullOrWhiteSpace(queryString))
        {
            url = $"{url}?{queryString}";
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClientFactory.CreateClient().SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new UserFriendlyException($"{errorPrefix}：HTTP {(int)response.StatusCode}。");
        }

        using var document = JsonDocument.Parse(responseBody);
        var code = document.RootElement.TryGetProperty("code", out var codeElement) ? codeElement.GetInt32() : -1;
        if (code != 0)
        {
            var message = document.RootElement.TryGetProperty("msg", out var messageElement)
                ? messageElement.GetString()
                : "未知错误";
            throw new UserFriendlyException($"{errorPrefix}：{message}");
        }

        if (document.RootElement.TryGetProperty("data", out var dataElement))
        {
            return dataElement.Clone();
        }

        using var emptyDocument = JsonDocument.Parse("{}");
        return emptyDocument.RootElement.Clone();
    }

    private async Task<string> GetTenantAccessTokenAsync(IntegrationTenantConfig config, bool forceRefresh)
    {
        if (!forceRefresh &&
            !string.IsNullOrWhiteSpace(config.AccessTokenCipherText) &&
            config.TokenExpiresAt.HasValue &&
            config.TokenExpiresAt.Value > DateTime.UtcNow.AddMinutes(5))
        {
            return Decrypt(config.AccessTokenCipherText, "飞书访问 Token 解密失败。");
        }

        var appId = ReadRequiredConfig(config.ConfigJson, "appId", "飞书 App ID 未配置。");
        var appSecret = Decrypt(config.SecretCipherText, "飞书 App Secret 未配置。");
        var payload = JsonSerializer.Serialize(new
        {
            app_id = appId,
            app_secret = appSecret
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{ResolveBaseUrl(config.ConfigJson)}/auth/v3/tenant_access_token/internal")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
        using var response = await _httpClientFactory.CreateClient().SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new UserFriendlyException($"飞书 Token 获取失败：HTTP {(int)response.StatusCode}。");
        }

        using var document = JsonDocument.Parse(responseBody);
        var code = document.RootElement.TryGetProperty("code", out var codeElement) ? codeElement.GetInt32() : -1;
        if (code != 0)
        {
            var message = document.RootElement.TryGetProperty("msg", out var messageElement)
                ? messageElement.GetString()
                : "未知错误";
            throw new UserFriendlyException($"飞书 Token 获取失败：{message}");
        }

        var token = document.RootElement.GetProperty("tenant_access_token").GetString();
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UserFriendlyException("飞书 Token 响应缺少 tenant_access_token。");
        }

        var expireSeconds = document.RootElement.TryGetProperty("expire", out var expireElement)
            ? expireElement.GetInt32()
            : 7200;
        config.ResetAccessToken(Encrypt(token), DateTime.UtcNow.AddSeconds(Math.Max(60, expireSeconds)));
        await _configRepository.UpdateAsync(config, autoSave: true);
        return token;
    }

    private static void EnsureBaseEnabled(IntegrationConnector connector, IntegrationTenantConfig? config)
    {
        if (!connector.IsEnabled)
        {
            throw new UserFriendlyException("飞书连接器定义已停用。");
        }

        if (config == null)
        {
            throw new UserFriendlyException("尚未保存飞书租户级连接配置。");
        }

        if (!config.IsEnabled)
        {
            throw new UserFriendlyException("飞书租户级连接配置已停用。");
        }

        if (!config.HasSecret())
        {
            throw new UserFriendlyException("飞书 App Secret 未配置。");
        }
    }

    private static string ResolveBaseUrl(string configJson)
    {
        var baseUrl = ReadOptionalConfig(configJson, "baseUrl");
        return string.IsNullOrWhiteSpace(baseUrl)
            ? DefaultBaseUrl
            : baseUrl.Trim().TrimEnd('/');
    }

    private static string ResolveReceiveIdType(string targetType)
    {
        return targetType.Trim().ToLowerInvariant() switch
        {
            "chat" or "group" => "chat_id",
            "email" => "email",
            "openid" or "open_id" => "open_id",
            "unionid" or "union_id" => "union_id",
            "user" or "userid" or "user_id" => "user_id",
            _ => throw new UserFriendlyException("飞书接收人类型不支持。")
        };
    }

    internal static (string MessageType, string Content) BuildMessageContent(string messageType, string body)
    {
        var normalizedMessageType = Check.NotNullOrWhiteSpace(messageType, nameof(messageType), 64).Trim().ToLowerInvariant();
        var content = Check.NotNullOrWhiteSpace(body, nameof(body), 64_000).Trim();
        return normalizedMessageType switch
        {
            "card" or "interactivecard" or "interactive_card" or "interactive" => ("interactive", BuildInteractiveCardContent(content)),
            "markdown" or "post" => ("post", JsonSerializer.Serialize(new
            {
                post = new
                {
                    zh_cn = new
                    {
                        title = "项目通知",
                        content = new[]
                        {
                            new[]
                            {
                                new
                                {
                                    tag = "text",
                                    text = content
                                }
                            }
                        }
                    }
                }
            })),
            _ => ("text", JsonSerializer.Serialize(new { text = content }))
        };
    }

    internal static FeishuSyncOptions BuildSyncOptions(string configJson, string requestJson)
    {
        var rootDepartmentId = "0";
        var departmentIdType = "open_department_id";
        var userIdType = "user_id";
        var pageSize = 50;
        var recursiveDepartment = true;
        var autoMatchUsers = true;

        ApplyOptions(configJson);
        ApplyOptions(requestJson);

        return new FeishuSyncOptions(
            string.IsNullOrWhiteSpace(rootDepartmentId) ? "0" : rootDepartmentId.Trim(),
            string.IsNullOrWhiteSpace(departmentIdType) ? "open_department_id" : departmentIdType.Trim(),
            string.IsNullOrWhiteSpace(userIdType) ? "user_id" : userIdType.Trim(),
            Math.Clamp(pageSize, 1, 50),
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
                rootDepartmentId = ReadOptionString(root, rootDepartmentId, "rootDepartmentId", "RootDepartmentId");
                departmentIdType = ReadOptionString(root, departmentIdType, "departmentIdType", "DepartmentIdType");
                userIdType = ReadOptionString(root, userIdType, "userIdType", "UserIdType");
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
                throw new UserFriendlyException("飞书同步配置 JSON 格式不正确。");
            }
        }
    }

    internal static FeishuDepartmentSyncItem ParseDepartmentItem(JsonElement item, string parentDepartmentId)
    {
        var externalDepartmentId = ReadString(item, "open_department_id", "department_id", "id");
        var name = ReadString(item, "name", "department_name");
        return new FeishuDepartmentSyncItem(
            externalDepartmentId,
            string.IsNullOrWhiteSpace(name) ? externalDepartmentId : name,
            ReadString(item, "parent_department_id", "parent_open_department_id", "parent_id") is { Length: > 0 } parentId
                ? parentId
                : parentDepartmentId,
            item.GetRawText());
    }

    internal static FeishuUserSyncItem ParseUserItem(JsonElement item, string fallbackDepartmentId)
    {
        var externalUserId = ReadString(item, "user_id", "open_id", "union_id", "id");
        var displayName = ReadString(item, "name", "nickname", "en_name");
        var departmentId = ReadFirstStringFromArray(item, "department_ids", "department_id_list");
        return new FeishuUserSyncItem(
            externalUserId,
            string.IsNullOrWhiteSpace(displayName) ? externalUserId : displayName,
            ReadString(item, "union_id"),
            ReadString(item, "mobile", "mobile_visible"),
            ReadString(item, "email"),
            ReadAvatarUrl(item),
            string.IsNullOrWhiteSpace(departmentId) ? fallbackDepartmentId : departmentId,
            item.GetRawText());
    }

    private static string BuildInteractiveCardContent(string content)
    {
        try
        {
            using var document = JsonDocument.Parse(content);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new UserFriendlyException("飞书互动卡片内容必须是 JSON 对象。");
            }

            return document.RootElement.GetRawText();
        }
        catch (JsonException ex)
        {
            throw new UserFriendlyException($"飞书互动卡片内容不是合法 JSON：{ex.Message}");
        }
    }

    private static IEnumerable<JsonElement> EnumerateItems(JsonElement data)
    {
        if (!data.TryGetProperty("items", out var items) || items.ValueKind != JsonValueKind.Array)
        {
            yield break;
        }

        foreach (var item in items.EnumerateArray())
        {
            yield return item;
        }
    }

    private static string ReadAvatarUrl(JsonElement item)
    {
        if (item.TryGetProperty("avatar", out var avatar) && avatar.ValueKind == JsonValueKind.Object)
        {
            return ReadString(avatar, "avatar_240", "avatar_640", "avatar_origin", "avatar_url");
        }

        return ReadString(item, "avatar_url");
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
            else if (value.ValueKind == JsonValueKind.String)
            {
                return value.GetString()?.Trim() ?? string.Empty;
            }
        }

        return string.Empty;
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

    private string Encrypt(string value)
    {
        return _encryptionService.Encrypt(value) ?? throw new InvalidOperationException("飞书 Token 加密失败。");
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
        return document.RootElement.TryGetProperty(key, out var value)
            ? value.GetString() ?? string.Empty
            : string.Empty;
    }

    internal sealed record FeishuSyncOptions(
        string RootDepartmentId,
        string DepartmentIdType,
        string UserIdType,
        int PageSize,
        bool RecursiveDepartment,
        bool AutoMatchUsers);

    internal sealed record FeishuDepartmentSyncItem(
        string ExternalDepartmentId,
        string Name,
        string ParentExternalDepartmentId,
        string RawJson);

    internal sealed record FeishuUserSyncItem(
        string ExternalUserId,
        string DisplayName,
        string ExternalUnionId,
        string Mobile,
        string Email,
        string AvatarUrl,
        string DepartmentExternalId,
        string RawJson);
}
