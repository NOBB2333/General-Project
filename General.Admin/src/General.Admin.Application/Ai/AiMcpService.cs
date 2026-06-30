using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using General.Admin.Permissions;
using Volo.Abp.Authorization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Ai;

public class AiMcpService : ITransientDependency
{
    private const string GatewayMcpToolPrefix = "mcp__";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly ICurrentTenant _currentTenant;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IPermissionChecker _permissionChecker;
    private readonly IRepository<AiMcpInvocation, Guid> _invocationRepository;
    private readonly IRepository<AiMcpServer, Guid> _serverRepository;
    private readonly IRepository<AiMcpTool, Guid> _toolRepository;
    private readonly AiUsageService _usageService;

    public AiMcpService(
        ICurrentTenant currentTenant,
        IGuidGenerator guidGenerator,
        IHttpClientFactory httpClientFactory,
        IPermissionChecker permissionChecker,
        IRepository<AiMcpInvocation, Guid> invocationRepository,
        IRepository<AiMcpServer, Guid> serverRepository,
        IRepository<AiMcpTool, Guid> toolRepository,
        AiUsageService usageService)
    {
        _currentTenant = currentTenant;
        _guidGenerator = guidGenerator;
        _httpClientFactory = httpClientFactory;
        _permissionChecker = permissionChecker;
        _invocationRepository = invocationRepository;
        _serverRepository = serverRepository;
        _toolRepository = toolRepository;
        _usageService = usageService;
    }

    public async Task<List<AiMcpServerDto>> GetServersAsync()
    {
        var tools = await _toolRepository.GetListAsync();
        return (await _serverRepository.GetListAsync())
            .OrderBy(x => x.Sort)
            .ThenBy(x => x.Name)
            .Select(item => MapServer(item, tools.Count(x => x.ServerId == item.Id)))
            .ToList();
    }

    public async Task<AiMcpServerDto> CreateServerAsync(AiMcpServerSaveInput input)
    {
        ValidateServerInput(input);
        var serverCode = Check.NotNullOrWhiteSpace(input.ServerCode, nameof(input.ServerCode), 64).Trim();
        if (await _serverRepository.AnyAsync(x => x.ServerCode == serverCode))
        {
            throw new UserFriendlyException($"MCP Server 编码已存在：{serverCode}");
        }

        var entity = new AiMcpServer(
            _guidGenerator.Create(),
            serverCode,
            input.Name,
            input.TransportType,
            _currentTenant.Id,
            input.Endpoint,
            input.Command,
            input.ArgumentsJson,
            input.HeadersJson,
            input.EnvironmentJson,
            input.ConfigJson,
            input.ProtocolVersion,
            input.PermissionCode,
            input.IsEnabled,
            input.TimeoutSeconds,
            input.Sort,
            input.Remark);
        await _serverRepository.InsertAsync(entity, autoSave: true);
        return MapServer(entity, 0);
    }

    public async Task<AiMcpServerDto> UpdateServerAsync(Guid id, AiMcpServerSaveInput input)
    {
        ValidateServerInput(input);
        var entity = await _serverRepository.GetAsync(id);
        if (!entity.ServerCode.Equals(input.ServerCode.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new UserFriendlyException("MCP Server 编码创建后不能修改。");
        }

        entity.Update(
            input.Name,
            input.TransportType,
            input.Endpoint,
            input.Command,
            input.ArgumentsJson,
            input.HeadersJson,
            input.EnvironmentJson,
            input.ConfigJson,
            input.ProtocolVersion,
            input.PermissionCode,
            input.IsEnabled,
            input.TimeoutSeconds,
            input.Sort,
            input.Remark);
        await _serverRepository.UpdateAsync(entity, autoSave: true);
        var toolCount = await _toolRepository.CountAsync(x => x.ServerId == id);
        return MapServer(entity, (int)toolCount);
    }

    public async Task DeleteServerAsync(Guid id)
    {
        var tools = await _toolRepository.GetListAsync(x => x.ServerId == id);
        if (tools.Count > 0)
        {
            await _toolRepository.DeleteManyAsync(tools, autoSave: false);
        }

        await _serverRepository.DeleteAsync(id, autoSave: true);
    }

    public async Task<AiMcpConnectionTestResultDto> TestConnectionAsync(Guid id)
    {
        var server = await _serverRepository.GetAsync(id);
        try
        {
            var responseJson = await SendMcpRequestSequenceAsync(
                server,
                [McpRequestFrame.Request(BuildInitializeRequest(server))],
                CancellationToken.None);
            server.MarkError(null);
            await _serverRepository.UpdateAsync(server, autoSave: true);
            return new AiMcpConnectionTestResultDto
            {
                RawResponseJson = responseJson,
                Success = true
            };
        }
        catch (Exception ex)
        {
            server.MarkError(ex.Message);
            await _serverRepository.UpdateAsync(server, autoSave: true);
            return new AiMcpConnectionTestResultDto
            {
                ErrorMessage = ex.Message,
                Success = false
            };
        }
    }

    public async Task<AiMcpDiscoveryResultDto> DiscoverToolsAsync(Guid id)
    {
        var server = await _serverRepository.GetAsync(id);
        if (!server.IsEnabled)
        {
            throw new UserFriendlyException("MCP Server 已停用，不能发现工具。");
        }

        try
        {
            var responseJson = await SendMcpRequestSequenceAsync(
                server,
                [
                    McpRequestFrame.Request(BuildInitializeRequest(server)),
                    McpRequestFrame.Notification(BuildInitializedNotification()),
                    McpRequestFrame.Request(BuildJsonRpcRequest("tools/list", null))
                ],
                CancellationToken.None);
            var discoveredTools = ParseToolsList(responseJson);
            var existingTools = await _toolRepository.GetListAsync(x => x.ServerId == id);
            foreach (var discovered in discoveredTools)
            {
                var matched = existingTools.FirstOrDefault(x => x.ToolName.Equals(discovered.ToolName, StringComparison.OrdinalIgnoreCase));
                if (matched == null)
                {
                    await _toolRepository.InsertAsync(new AiMcpTool(
                        _guidGenerator.Create(),
                        server.Id,
                        discovered.ToolName,
                        discovered.Description,
                        discovered.InputSchemaJson,
                        _currentTenant.Id,
                        server.PermissionCode,
                        isEnabled: true), autoSave: false);
                    continue;
                }

                matched.Refresh(discovered.Description, discovered.InputSchemaJson);
                await _toolRepository.UpdateAsync(matched, autoSave: false);
            }

            server.MarkDiscovery(DateTime.UtcNow);
            await _serverRepository.UpdateAsync(server, autoSave: true);
            var tools = await GetToolsAsync(server.Id);
            return new AiMcpDiscoveryResultDto
            {
                ToolCount = tools.Count,
                Tools = tools
            };
        }
        catch (Exception ex)
        {
            server.MarkError(ex.Message);
            await _serverRepository.UpdateAsync(server, autoSave: true);
            return new AiMcpDiscoveryResultDto
            {
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<List<AiMcpToolDto>> GetToolsAsync(Guid? serverId = null)
    {
        var servers = (await _serverRepository.GetListAsync()).ToDictionary(x => x.Id, x => x.Name);
        var tools = await _toolRepository.GetListAsync();
        if (serverId.HasValue)
        {
            tools = tools.Where(x => x.ServerId == serverId.Value).ToList();
        }

        return tools
            .OrderBy(x => servers.GetValueOrDefault(x.ServerId, string.Empty))
            .ThenBy(x => x.ToolName)
            .Select(item => MapTool(item, servers))
            .ToList();
    }

    public async Task<List<AiGatewayToolDefinition>> GetGatewayToolDefinitionsAsync()
    {
        var servers = (await _serverRepository.GetListAsync(x => x.IsEnabled))
            .ToDictionary(x => x.Id);
        var tools = await _toolRepository.GetListAsync(x => x.IsEnabled);
        var result = new List<AiGatewayToolDefinition>();
        foreach (var tool in tools.OrderBy(x => x.ToolName))
        {
            if (!servers.TryGetValue(tool.ServerId, out var server))
            {
                continue;
            }

            var permissionCode = ResolveMcpPermissionCode(server, tool);
            if (!await _permissionChecker.IsGrantedAsync(permissionCode))
            {
                continue;
            }

            result.Add(new AiGatewayToolDefinition(
                ToGatewayToolName(server, tool),
                $"MCP 工具：{server.Name}/{tool.ToolName}。{tool.Description}".Trim('。'),
                string.IsNullOrWhiteSpace(tool.InputSchemaJson) ? "{}" : tool.InputSchemaJson));
        }

        return result;
    }

    public async Task<AiMcpToolDto> UpdateToolPolicyAsync(Guid id, AiMcpToolPolicyInput input)
    {
        var tool = await _toolRepository.GetAsync(id);
        tool.UpdatePolicy(input.IsEnabled, input.PermissionCode);
        await _toolRepository.UpdateAsync(tool, autoSave: true);
        var servers = (await _serverRepository.GetListAsync()).ToDictionary(x => x.Id, x => x.Name);
        return MapTool(tool, servers);
    }

    public async Task<AiMcpInvokeResultDto> InvokeToolAsync(Guid toolId, AiMcpInvokeInput input)
    {
        var tool = await _toolRepository.GetAsync(toolId);
        if (!tool.IsEnabled)
        {
            throw new UserFriendlyException("MCP 工具已停用，不能调用。");
        }

        var server = await _serverRepository.GetAsync(tool.ServerId);
        if (!server.IsEnabled)
        {
            throw new UserFriendlyException("MCP Server 已停用，不能调用工具。");
        }

        await EnsureMcpPermissionAsync(server, tool);
        ValidateJsonObject(input.ArgumentsJson, "工具参数");

        var invocation = new AiMcpInvocation(
            _guidGenerator.Create(),
            server.Id,
            tool.Id,
            tool.ToolName,
            input.ArgumentsJson,
            server.TimeoutSeconds,
            _currentTenant.Id);
        await _invocationRepository.InsertAsync(invocation, autoSave: true);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(server.TimeoutSeconds));
            var responseJson = await SendMcpRequestSequenceAsync(
                server,
                [
                    McpRequestFrame.Request(BuildInitializeRequest(server)),
                    McpRequestFrame.Notification(BuildInitializedNotification()),
                    McpRequestFrame.Request(BuildJsonRpcRequest("tools/call", new
                    {
                        name = tool.ToolName,
                        arguments = JsonSerializer.Deserialize<object>(NormalizeJson(input.ArgumentsJson, "{}"), JsonOptions)
                    }))
                ],
                cancellation.Token);
            var outputJson = ExtractResultJson(responseJson);
            stopwatch.Stop();
            invocation.Complete(outputJson, stopwatch.ElapsedMilliseconds);
            await _invocationRepository.UpdateAsync(invocation, autoSave: true);
            await RecordMcpUsageAsync(server, tool, invocation);
            return MapInvocationResult(invocation);
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            invocation.Fail("Timeout", $"MCP 工具调用超过 {server.TimeoutSeconds} 秒未返回。", stopwatch.ElapsedMilliseconds);
            await _invocationRepository.UpdateAsync(invocation, autoSave: true);
            await RecordMcpUsageAsync(server, tool, invocation);
            return MapInvocationResult(invocation);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            invocation.Fail("Failed", ex.Message, stopwatch.ElapsedMilliseconds);
            await _invocationRepository.UpdateAsync(invocation, autoSave: true);
            await RecordMcpUsageAsync(server, tool, invocation);
            return MapInvocationResult(invocation);
        }
    }

    public async Task<AiToolInvokeResultDto> InvokeGatewayToolAsync(string gatewayToolName, string argumentsJson)
    {
        var toolId = ParseGatewayToolId(gatewayToolName);
        var tool = await _toolRepository.GetAsync(toolId);
        var server = await _serverRepository.GetAsync(tool.ServerId);
        var result = await InvokeToolAsync(toolId, new AiMcpInvokeInput
        {
            ArgumentsJson = argumentsJson
        });
        return new AiToolInvokeResultDto
        {
            ElapsedMilliseconds = result.ElapsedMilliseconds,
            ErrorMessage = result.ErrorMessage,
            InvocationId = result.InvocationId,
            OutputJson = result.OutputJson,
            SourceReferences =
            [
                new AiSourceReferenceDto
                {
                    SourceId = tool.Id.ToString(),
                    SourceType = "MCP",
                    Title = $"{server.Name}/{tool.ToolName}"
                }
            ],
            Status = result.Status,
            ToolCode = $"mcp.{server.ServerCode}.{tool.ToolName}"
        };
    }

    public async Task<List<AiMcpInvocationDto>> GetInvocationsAsync(AiMcpInvocationQueryInput input)
    {
        var servers = (await _serverRepository.GetListAsync()).ToDictionary(x => x.Id, x => x.Name);
        var invocations = await _invocationRepository.GetListAsync();
        if (input.ServerId.HasValue)
        {
            invocations = invocations.Where(x => x.ServerId == input.ServerId.Value).ToList();
        }

        if (input.ToolId.HasValue)
        {
            invocations = invocations.Where(x => x.ToolId == input.ToolId.Value).ToList();
        }

        if (!string.IsNullOrWhiteSpace(input.ToolName))
        {
            var toolName = input.ToolName.Trim();
            invocations = invocations.Where(x => x.ToolName.Contains(toolName, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(input.Status))
        {
            var status = input.Status.Trim();
            invocations = invocations.Where(x => x.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return invocations
            .OrderByDescending(x => x.CreationTime)
            .Take(Math.Clamp(input.MaxResultCount, 1, 200))
            .Select(item => MapInvocation(item, servers))
            .ToList();
    }

    private async Task EnsureMcpPermissionAsync(AiMcpServer server, AiMcpTool tool)
    {
        var permissionCode = ResolveMcpPermissionCode(server, tool);
        if (!await _permissionChecker.IsGrantedAsync(permissionCode))
        {
            throw new AbpAuthorizationException($"当前用户没有调用 MCP 工具的权限：{permissionCode}");
        }
    }

    private static string ResolveMcpPermissionCode(AiMcpServer server, AiMcpTool tool)
    {
        return string.IsNullOrWhiteSpace(tool.PermissionCode)
            ? string.IsNullOrWhiteSpace(server.PermissionCode) ? AdminPermissions.Ai.McpManage : server.PermissionCode
            : tool.PermissionCode;
    }

    public static bool IsGatewayToolName(string toolName)
    {
        return toolName.StartsWith(GatewayMcpToolPrefix, StringComparison.OrdinalIgnoreCase);
    }

    private static string ToGatewayToolName(AiMcpServer server, AiMcpTool tool)
    {
        return $"{GatewayMcpToolPrefix}{NormalizeGatewaySegment(server.ServerCode)}__{tool.Id:N}";
    }

    private static Guid ParseGatewayToolId(string gatewayToolName)
    {
        if (!IsGatewayToolName(gatewayToolName))
        {
            throw new UserFriendlyException($"不是 MCP 网关工具名称：{gatewayToolName}");
        }

        var idText = gatewayToolName.Split("__", StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
        return Guid.TryParse(idText, out var toolId)
            ? toolId
            : throw new UserFriendlyException($"MCP 网关工具名称不合法：{gatewayToolName}");
    }

    private static string NormalizeGatewaySegment(string value)
    {
        var chars = value
            .Select(ch => char.IsLetterOrDigit(ch) ? ch : '_')
            .ToArray();
        var normalized = new string(chars).Trim('_');
        return string.IsNullOrWhiteSpace(normalized) ? "server" : normalized;
    }

    private async Task RecordMcpUsageAsync(AiMcpServer server, AiMcpTool tool, AiMcpInvocation invocation)
    {
        await _usageService.RecordUsageAsync(new AiUsageRecordInput
        {
            AuditSummary = $"MCP 工具调用：{server.Name}/{tool.ToolName}",
            ElapsedMilliseconds = invocation.ElapsedMilliseconds,
            ErrorMessage = invocation.ErrorMessage,
            RequestPreview = invocation.InputJson,
            ResponsePreview = invocation.OutputJson,
            Scenario = server.ServerCode,
            Source = "McpTool",
            Status = invocation.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase) ? "Success" : invocation.Status,
            ToolCode = tool.ToolName
        });
    }

    private async Task<string> SendMcpRequestSequenceAsync(AiMcpServer server, IReadOnlyList<McpRequestFrame> requests, CancellationToken cancellationToken)
    {
        return server.TransportType switch
        {
            "Stdio" => await SendStdioSequenceAsync(server, requests, cancellationToken),
            "Sse" => await SendHttpSequenceAsync(server, requests, cancellationToken),
            "StreamableHttp" => await SendHttpSequenceAsync(server, requests, cancellationToken),
            _ => throw new UserFriendlyException($"不支持的 MCP 传输类型：{server.TransportType}")
        };
    }

    private async Task<string> SendHttpSequenceAsync(AiMcpServer server, IReadOnlyList<McpRequestFrame> requests, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(server.Endpoint))
        {
            throw new UserFriendlyException("HTTP/SSE MCP Server 必须配置 Endpoint。");
        }

        var responseJson = "{}";
        foreach (var request in requests)
        {
            var currentResponse = await SendHttpRequestAsync(server, request.Json, request.ExpectsResponse, cancellationToken);
            if (!request.ExpectsResponse)
            {
                continue;
            }

            responseJson = currentResponse;
            ThrowIfJsonRpcError(responseJson);
        }

        return responseJson;
    }

    private async Task<string> SendHttpRequestAsync(AiMcpServer server, string requestJson, bool expectsResponse, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(server.TimeoutSeconds);
        using var request = new HttpRequestMessage(HttpMethod.Post, server.Endpoint)
        {
            Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (server.TransportType == "Sse")
        {
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
        }

        foreach (var header in DeserializeDictionary(server.HeadersJson))
        {
            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new UserFriendlyException($"MCP HTTP 调用失败：{(int)response.StatusCode} {content}");
        }

        return expectsResponse ? ExtractSseJson(content) : "{}";
    }

    private async Task<string> SendStdioSequenceAsync(AiMcpServer server, IReadOnlyList<McpRequestFrame> requests, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(server.Command))
        {
            throw new UserFriendlyException("stdio MCP Server 必须配置启动命令。");
        }

        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = server.Command,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };
        foreach (var argument in DeserializeList(server.ArgumentsJson))
        {
            process.StartInfo.ArgumentList.Add(argument);
        }

        foreach (var environment in DeserializeDictionary(server.EnvironmentJson))
        {
            process.StartInfo.Environment[environment.Key] = environment.Value;
        }

        if (!process.Start())
        {
            throw new UserFriendlyException("stdio MCP Server 启动失败。");
        }

        try
        {
            var responseJson = "{}";
            foreach (var request in requests)
            {
                await process.StandardInput.WriteLineAsync(request.Json.AsMemory(), cancellationToken);
                await process.StandardInput.FlushAsync(cancellationToken);
                if (!request.ExpectsResponse)
                {
                    continue;
                }

                responseJson = await ReadNextJsonLineAsync(process, cancellationToken);
                ThrowIfJsonRpcError(responseJson);
            }

            return responseJson;
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
    }

    private static async Task<string> ReadNextJsonLineAsync(Process process, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && !process.HasExited)
        {
            var line = await process.StandardOutput.ReadLineAsync(cancellationToken);
            if (line == null)
            {
                break;
            }

            var trimmed = line.Trim();
            if (!trimmed.StartsWith('{'))
            {
                continue;
            }

            using var _ = JsonDocument.Parse(trimmed);
            return trimmed;
        }

        var error = await process.StandardError.ReadToEndAsync(cancellationToken);
        throw new UserFriendlyException(string.IsNullOrWhiteSpace(error) ? "stdio MCP Server 未返回 JSON-RPC 响应。" : error);
    }

    private static string BuildInitializeRequest(AiMcpServer server)
    {
        return BuildJsonRpcRequest("initialize", new
        {
            protocolVersion = server.ProtocolVersion,
            capabilities = new { },
            clientInfo = new
            {
                name = "General Admin",
                version = "1.0"
            }
        });
    }

    private static string BuildInitializedNotification()
    {
        return BuildJsonRpcNotification("notifications/initialized", new { });
    }

    private static string BuildJsonRpcRequest(string method, object? parameters)
    {
        var payload = new Dictionary<string, object?>
        {
            ["jsonrpc"] = "2.0",
            ["id"] = Guid.NewGuid().ToString("N"),
            ["method"] = method
        };
        if (parameters != null)
        {
            payload["params"] = parameters;
        }

        return JsonSerializer.Serialize(payload, JsonOptions);
    }

    private static string BuildJsonRpcNotification(string method, object? parameters)
    {
        var payload = new Dictionary<string, object?>
        {
            ["jsonrpc"] = "2.0",
            ["method"] = method
        };
        if (parameters != null)
        {
            payload["params"] = parameters;
        }

        return JsonSerializer.Serialize(payload, JsonOptions);
    }

    private static IReadOnlyList<DiscoveredMcpTool> ParseToolsList(string responseJson)
    {
        using var document = JsonDocument.Parse(responseJson);
        ThrowIfJsonRpcError(document.RootElement);
        if (!document.RootElement.TryGetProperty("result", out var result) ||
            !result.TryGetProperty("tools", out var tools) ||
            tools.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var list = new List<DiscoveredMcpTool>();
        foreach (var tool in tools.EnumerateArray())
        {
            var name = ReadString(tool, "name");
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            list.Add(new DiscoveredMcpTool(
                name,
                ReadString(tool, "description"),
                tool.TryGetProperty("inputSchema", out var inputSchema) ? inputSchema.GetRawText() : "{}"));
        }

        return list;
    }

    private static string ExtractResultJson(string responseJson)
    {
        using var document = JsonDocument.Parse(responseJson);
        ThrowIfJsonRpcError(document.RootElement);
        return document.RootElement.TryGetProperty("result", out var result) ? result.GetRawText() : responseJson;
    }

    private static void ThrowIfJsonRpcError(string responseJson)
    {
        using var document = JsonDocument.Parse(responseJson);
        ThrowIfJsonRpcError(document.RootElement);
    }

    private static void ThrowIfJsonRpcError(JsonElement root)
    {
        if (!root.TryGetProperty("error", out var error) || error.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return;
        }

        var message = ReadString(error, "message");
        throw new UserFriendlyException(string.IsNullOrWhiteSpace(message) ? error.GetRawText() : message);
    }

    private static string ExtractSseJson(string content)
    {
        var trimmed = content.Trim();
        if (trimmed.StartsWith('{'))
        {
            return trimmed;
        }

        foreach (var line in content.Split('\n'))
        {
            var text = line.Trim();
            if (!text.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var data = text[5..].Trim();
            if (data.StartsWith('{'))
            {
                return data;
            }
        }

        throw new UserFriendlyException("MCP SSE 响应中没有找到 JSON-RPC data。");
    }

    private static void ValidateServerInput(AiMcpServerSaveInput input)
    {
        ValidateJsonArray(input.ArgumentsJson, "启动参数");
        ValidateJsonObject(input.HeadersJson, "请求头");
        ValidateJsonObject(input.EnvironmentJson, "环境变量");
        ValidateJsonObject(input.ConfigJson, "扩展配置");
        if (input.TransportType.Equals("Stdio", StringComparison.OrdinalIgnoreCase))
        {
            Check.NotNullOrWhiteSpace(input.Command, nameof(input.Command), 512);
        }
        else
        {
            Check.NotNullOrWhiteSpace(input.Endpoint, nameof(input.Endpoint), 512);
        }
    }

    private static void ValidateJsonObject(string? json, string label)
    {
        using var document = JsonDocument.Parse(NormalizeJson(json, "{}"));
        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            throw new UserFriendlyException($"{label} 必须是 JSON 对象。");
        }
    }

    private static void ValidateJsonArray(string? json, string label)
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "[]" : json.Trim());
        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            throw new UserFriendlyException($"{label} 必须是 JSON 数组。");
        }
    }

    private static Dictionary<string, string> DeserializeDictionary(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(NormalizeJson(json, "{}"), JsonOptions) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static List<string> DeserializeList(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<string>>(string.IsNullOrWhiteSpace(json) ? "[]" : json.Trim(), JsonOptions) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static string NormalizeJson(string? value, string defaultValue)
    {
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
    }

    private static string ReadString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;
    }

    private static AiMcpServerDto MapServer(AiMcpServer item, int toolCount)
    {
        return new AiMcpServerDto
        {
            ArgumentsJson = item.ArgumentsJson,
            Command = item.Command,
            ConfigJson = item.ConfigJson,
            CreationTime = item.CreationTime,
            Endpoint = item.Endpoint,
            EnvironmentJson = item.EnvironmentJson,
            HeadersJson = item.HeadersJson,
            Id = item.Id,
            IsEnabled = item.IsEnabled,
            LastDiscoveryTime = item.LastDiscoveryTime,
            LastErrorMessage = item.LastErrorMessage,
            Name = item.Name,
            PermissionCode = item.PermissionCode,
            ProtocolVersion = item.ProtocolVersion,
            Remark = item.Remark,
            ServerCode = item.ServerCode,
            Sort = item.Sort,
            TimeoutSeconds = item.TimeoutSeconds,
            ToolCount = toolCount,
            TransportType = item.TransportType
        };
    }

    private static AiMcpToolDto MapTool(AiMcpTool item, IReadOnlyDictionary<Guid, string> serverNames)
    {
        return new AiMcpToolDto
        {
            CreationTime = item.CreationTime,
            Description = item.Description,
            Id = item.Id,
            InputSchemaJson = item.InputSchemaJson,
            IsEnabled = item.IsEnabled,
            LastDiscoveredTime = item.LastDiscoveredTime,
            PermissionCode = item.PermissionCode,
            ServerId = item.ServerId,
            ServerName = serverNames.GetValueOrDefault(item.ServerId, string.Empty),
            ToolName = item.ToolName
        };
    }

    private static AiMcpInvokeResultDto MapInvocationResult(AiMcpInvocation item)
    {
        return new AiMcpInvokeResultDto
        {
            ElapsedMilliseconds = item.ElapsedMilliseconds,
            ErrorMessage = item.ErrorMessage,
            InvocationId = item.Id,
            OutputJson = item.OutputJson,
            Status = item.Status,
            ToolName = item.ToolName
        };
    }

    private static AiMcpInvocationDto MapInvocation(AiMcpInvocation item, IReadOnlyDictionary<Guid, string> serverNames)
    {
        return new AiMcpInvocationDto
        {
            CreationTime = item.CreationTime,
            ElapsedMilliseconds = item.ElapsedMilliseconds,
            ErrorMessage = item.ErrorMessage,
            InputJson = item.InputJson,
            InvocationId = item.Id,
            OutputJson = item.OutputJson,
            ServerId = item.ServerId,
            ServerName = serverNames.GetValueOrDefault(item.ServerId, string.Empty),
            Status = item.Status,
            TimeoutSeconds = item.TimeoutSeconds,
            ToolId = item.ToolId,
            ToolName = item.ToolName
        };
    }

    private sealed record DiscoveredMcpTool(string ToolName, string Description, string InputSchemaJson);

    private sealed record McpRequestFrame(string Json, bool ExpectsResponse)
    {
        public static McpRequestFrame Request(string json) => new(json, true);

        public static McpRequestFrame Notification(string json) => new(json, false);
    }
}
