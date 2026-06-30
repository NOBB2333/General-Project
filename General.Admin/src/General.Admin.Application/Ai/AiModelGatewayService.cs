using System.Net.Http.Headers;
using System.Net.Http;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using General.Admin.Settings;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Security.Encryption;
using Volo.Abp.Settings;

namespace General.Admin.Ai;

public class AiModelGatewayService : ITransientDependency
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IStringEncryptionService _encryptionService;
    private readonly ISettingProvider _settingProvider;

    public AiModelGatewayService(
        IHttpClientFactory httpClientFactory,
        IStringEncryptionService encryptionService,
        ISettingProvider settingProvider)
    {
        _httpClientFactory = httpClientFactory;
        _encryptionService = encryptionService;
        _settingProvider = settingProvider;
    }

    public async Task<AiGatewayTestResult> TestProviderAsync(AiProvider provider, AiModel? model)
    {
        var testModel = model ?? CreateBuiltInTestModel(provider);
        if (testModel != null)
        {
            var result = await SendChatAsync(
                provider,
                testModel,
                [new AiGatewayMessage("user", "请只回复 pong，用于连通性测试。")],
                16);
            return result.Success
                ? AiGatewayTestResult.Ok($"供应商真实连通性测试通过，模型 `{testModel.DisplayName}` 已返回响应。")
                : AiGatewayTestResult.Fail(result.ErrorMessage);
        }

        var validation = ValidateProvider(provider);
        if (!validation.Success)
        {
            return validation;
        }

        var endpointPolicy = await ValidateEndpointPolicyAsync(provider);
        if (!endpointPolicy.Success)
        {
            return endpointPolicy;
        }

        try
        {
            var discovery = await DiscoverModelsAsync(provider);
            if (!discovery.Success)
            {
                return AiGatewayTestResult.Fail(discovery.Message);
            }

            var discoveredModel = discovery.Models.FirstOrDefault();
            if (discoveredModel == null)
            {
                return AiGatewayTestResult.Fail("供应商模型列表接口已返回，但没有可用于对话的模型。");
            }

            var result = await SendChatAsync(
                provider,
                CreateTransientModel(provider, discoveredModel),
                [new AiGatewayMessage("user", "请只回复 pong，用于连通性测试。")],
                16);
            return result.Success
                ? AiGatewayTestResult.Ok($"供应商真实连通性测试通过，模型 `{discoveredModel.DisplayName}` 已返回响应。")
                : AiGatewayTestResult.Fail(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            return AiGatewayTestResult.Fail($"供应商真实连通性测试失败：{ex.Message}");
        }
    }

    public async Task<AiProviderModelDiscoveryResultDto> DiscoverModelsAsync(AiProvider provider)
    {
        var validation = ValidateProvider(provider);
        if (!validation.Success)
        {
            return new AiProviderModelDiscoveryResultDto { Success = false, Message = validation.Message };
        }

        var endpointPolicy = await ValidateEndpointPolicyAsync(provider);
        if (!endpointPolicy.Success)
        {
            return new AiProviderModelDiscoveryResultDto { Success = false, Message = endpointPolicy.Message };
        }

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            using var request = CreateRequest(provider, HttpMethod.Get, BuildEndpoint(provider, "models"));
            using var response = await _httpClientFactory.CreateClient(nameof(AiModelGatewayService)).SendAsync(request, cts.Token);
            var content = await response.Content.ReadAsStringAsync(cts.Token);
            if (!response.IsSuccessStatusCode)
            {
                var builtInModels = ResolveBuiltInModels(provider);
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound && builtInModels.Count > 0)
                {
                    return new AiProviderModelDiscoveryResultDto
                    {
                        Models = builtInModels,
                        Success = true,
                        Message = "供应商未开放模型列表接口，已根据标准协议和已知 Endpoint 生成可用模型。"
                    };
                }

                return new AiProviderModelDiscoveryResultDto
                {
                    Success = false,
                    Message = ResolveErrorMessage(response.StatusCode, content)
                };
            }

            var models = ParseModelList(content);
            if (models.Count == 0)
            {
                var builtInModels = ResolveBuiltInModels(provider);
                if (builtInModels.Count > 0)
                {
                    models = builtInModels;
                }
            }

            return new AiProviderModelDiscoveryResultDto
            {
                Models = models,
                Success = true,
                Message = models.Count == 0 ? "模型列表接口已返回，但未解析到可用模型。" : $"已发现 {models.Count} 个模型。"
            };
        }
        catch (Exception ex)
        {
            return new AiProviderModelDiscoveryResultDto
            {
                Success = false,
                Message = $"获取模型列表失败：{ex.Message}"
            };
        }
    }

    public async Task<AiGatewayChatResult> SendChatAsync(
        AiProvider provider,
        AiModel model,
        IReadOnlyCollection<AiGatewayMessage> messages,
        int? maxOutputTokens = null,
        IReadOnlyCollection<AiGatewayToolDefinition>? tools = null)
    {
        var validation = ValidateProvider(provider);
        if (!validation.Success)
        {
            return AiGatewayChatResult.Fail(validation.Message);
        }

        var endpointPolicy = await ValidateEndpointPolicyAsync(provider);
        if (!endpointPolicy.Success)
        {
            return AiGatewayChatResult.Fail(endpointPolicy.Message);
        }

        if (!model.IsEnabled)
        {
            return AiGatewayChatResult.Fail("选择的模型已停用。");
        }

        var normalizedMessages = NormalizeMessages(messages);
        if (normalizedMessages.Count == 0)
        {
            return AiGatewayChatResult.Fail("会话消息不能为空。");
        }

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(90));
            var protocol = ResolveProtocol(provider);
            var payload = protocol == AiProviderProtocolNames.AnthropicMessages
                ? BuildAnthropicMessagesPayload(provider, model, normalizedMessages, maxOutputTokens, tools)
                : BuildOpenAIResponsesPayload(provider, model, normalizedMessages, maxOutputTokens, tools);
            using var request = CreateRequest(provider, HttpMethod.Post, BuildEndpoint(provider, protocol == AiProviderProtocolNames.AnthropicMessages ? "messages" : "responses"));
            request.Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");

            using var response = await _httpClientFactory.CreateClient(nameof(AiModelGatewayService)).SendAsync(request, cts.Token);
            var responseContent = await response.Content.ReadAsStringAsync(cts.Token);
            if (!response.IsSuccessStatusCode)
            {
                return AiGatewayChatResult.Fail(ResolveErrorMessage(response.StatusCode, responseContent), responseContent);
            }

            return protocol == AiProviderProtocolNames.AnthropicMessages
                ? ParseAnthropicMessage(responseContent)
                : ParseOpenAIResponse(responseContent);
        }
        catch (Exception ex)
        {
            return AiGatewayChatResult.Fail($"模型调用失败：{ex.Message}");
        }
    }

    public async IAsyncEnumerable<AiGatewayChatStreamChunk> StreamChatAsync(
        AiProvider provider,
        AiModel model,
        IReadOnlyCollection<AiGatewayMessage> messages,
        int? maxOutputTokens = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var validation = ValidateProvider(provider);
        if (!validation.Success)
        {
            yield return AiGatewayChatStreamChunk.Fail(validation.Message);
            yield break;
        }

        var endpointPolicy = await ValidateEndpointPolicyAsync(provider);
        if (!endpointPolicy.Success)
        {
            yield return AiGatewayChatStreamChunk.Fail(endpointPolicy.Message);
            yield break;
        }

        if (!model.IsEnabled)
        {
            yield return AiGatewayChatStreamChunk.Fail("选择的模型已停用。");
            yield break;
        }

        var normalizedMessages = NormalizeMessages(messages);
        if (normalizedMessages.Count == 0)
        {
            yield return AiGatewayChatStreamChunk.Fail("会话消息不能为空。");
            yield break;
        }

        var protocol = ResolveProtocol(provider);
        var payload = protocol == AiProviderProtocolNames.AnthropicMessages
            ? BuildAnthropicMessagesPayload(provider, model, normalizedMessages, maxOutputTokens, tools: null)
            : BuildOpenAIResponsesPayload(provider, model, normalizedMessages, maxOutputTokens, tools: null);
        payload["stream"] = true;

        var endpoint = protocol == AiProviderProtocolNames.AnthropicMessages ? "messages" : "responses";
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(120));
        using var request = CreateRequest(provider, HttpMethod.Post, BuildEndpoint(provider, endpoint));
        request.Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");

        using var response = await _httpClientFactory
            .CreateClient(nameof(AiModelGatewayService))
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cts.Token);
            yield return AiGatewayChatStreamChunk.Fail(ResolveErrorMessage(response.StatusCode, errorContent), errorContent);
            yield break;
        }

        var rawBuilder = new StringBuilder();
        await using var stream = await response.Content.ReadAsStreamAsync(cts.Token);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        while (!cts.Token.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cts.Token);
            if (line == null)
            {
                break;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (!line.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var data = line["data:".Length..].Trim();
            if (string.Equals(data, "[DONE]", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            rawBuilder.AppendLine(data);
            var chunk = protocol == AiProviderProtocolNames.AnthropicMessages
                ? ParseAnthropicMessagesStreamChunk(data)
                : ParseOpenAIResponsesStreamChunk(data);
            if (chunk != null)
            {
                yield return chunk;
                if (!chunk.Success)
                {
                    yield break;
                }
            }
        }

        yield return AiGatewayChatStreamChunk.Completed(rawBuilder.ToString());
    }

    private HttpRequestMessage CreateRequest(AiProvider provider, HttpMethod method, string endpoint)
    {
        var apiKey = DecryptApiKey(provider);
        var request = new HttpRequestMessage(method, endpoint);
        if (string.Equals(ResolveProtocol(provider), AiProviderProtocolNames.AnthropicMessages, StringComparison.OrdinalIgnoreCase))
        {
            request.Headers.TryAddWithoutValidation("x-api-key", apiKey);
            request.Headers.TryAddWithoutValidation("anthropic-version", ResolveAnthropicVersion(provider.ConfigJson));
        }
        else
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            if (!string.IsNullOrWhiteSpace(provider.OrganizationId))
            {
                request.Headers.TryAddWithoutValidation("OpenAI-Organization", provider.OrganizationId.Trim());
            }
        }

        return request;
    }

    private AiGatewayTestResult ValidateProvider(AiProvider provider)
    {
        if (!provider.IsEnabled)
        {
            return AiGatewayTestResult.Fail("供应商已停用，请先启用后再测试。");
        }

        if (string.IsNullOrWhiteSpace(provider.Endpoint))
        {
            return AiGatewayTestResult.Fail("供应商 Endpoint 不能为空。");
        }

        if (!Uri.TryCreate(provider.Endpoint, UriKind.Absolute, out var endpoint) ||
            (endpoint.Scheme != Uri.UriSchemeHttp && endpoint.Scheme != Uri.UriSchemeHttps))
        {
            return AiGatewayTestResult.Fail("供应商 Endpoint 必须是合法的 HTTP/HTTPS 地址。");
        }

        if (!provider.HasApiKey())
        {
            return AiGatewayTestResult.Fail("供应商密钥未配置。");
        }

        if (string.IsNullOrWhiteSpace(DecryptApiKey(provider)))
        {
            return AiGatewayTestResult.Fail("供应商密钥解密失败或为空，请重新配置密钥。");
        }

        if (!AiProviderProtocolNames.IsSupported(provider.ProviderType))
        {
            return AiGatewayTestResult.Fail("供应商协议仅支持 OpenAI Responses 或 Anthropic Messages。");
        }

        return AiGatewayTestResult.Ok("供应商配置有效。");
    }

    private async Task<AiGatewayTestResult> ValidateEndpointPolicyAsync(AiProvider provider)
    {
        if (await _settingProvider.GetAsync<bool>(AdminSettings.AiAllowPublicProviderEndpoints))
        {
            return AiGatewayTestResult.Ok("供应商 Endpoint 符合连接策略。");
        }

        if (!Uri.TryCreate(provider.Endpoint, UriKind.Absolute, out var endpoint))
        {
            return AiGatewayTestResult.Fail("供应商 Endpoint 必须是合法的 HTTP/HTTPS 地址。");
        }

        return IsPrivateEndpoint(endpoint)
            ? AiGatewayTestResult.Ok("供应商 Endpoint 符合私有化连接策略。")
            : AiGatewayTestResult.Fail("当前私有化部署策略不允许连接公网模型 Provider，请使用内网或本地模型网关。");
    }

    private static bool IsPrivateEndpoint(Uri endpoint)
    {
        var host = endpoint.Host;
        if (string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(host, "127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(host, "::1", StringComparison.OrdinalIgnoreCase) ||
            host.EndsWith(".local", StringComparison.OrdinalIgnoreCase) ||
            host.EndsWith(".internal", StringComparison.OrdinalIgnoreCase) ||
            host.EndsWith(".intranet", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!System.Net.IPAddress.TryParse(host, out var address))
        {
            return false;
        }

        var bytes = address.GetAddressBytes();
        return bytes.Length switch
        {
            4 => bytes[0] == 10 ||
                 bytes[0] == 127 ||
                 bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31 ||
                 bytes[0] == 192 && bytes[1] == 168,
            16 => address.IsIPv6LinkLocal || address.IsIPv6SiteLocal || System.Net.IPAddress.IsLoopback(address),
            _ => false
        };
    }

    private string DecryptApiKey(AiProvider provider)
    {
        try
        {
            return string.IsNullOrWhiteSpace(provider.ApiKeyCipherText)
                ? string.Empty
                : _encryptionService.Decrypt(provider.ApiKeyCipherText)?.Trim() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    internal static string BuildEndpoint(AiProvider provider, string relativePath)
    {
        var normalized = NormalizeBaseEndpoint(provider.Endpoint);
        var protocol = ResolveProtocol(provider);
        if (string.Equals(protocol, AiProviderProtocolNames.AnthropicMessages, StringComparison.OrdinalIgnoreCase) &&
            !normalized.EndsWith("/v1", StringComparison.OrdinalIgnoreCase))
        {
            normalized = $"{normalized}/v1";
        }

        return $"{normalized}/{relativePath.TrimStart('/')}";
    }

    private static string NormalizeBaseEndpoint(string endpoint)
    {
        var normalized = endpoint.Trim().TrimEnd('/');
        var knownSuffixes = new[]
        {
            "/chat/completions",
            "/responses",
            "/messages",
            "/models"
        };
        foreach (var suffix in knownSuffixes)
        {
            if (normalized.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                normalized = normalized[..^suffix.Length];
                break;
            }
        }

        return normalized.TrimEnd('/');
    }

    public IReadOnlyList<AiDiscoveredModelDto> GetBuiltInModels(AiProvider provider)
    {
        return ResolveBuiltInModels(provider);
    }

    internal static List<AiDiscoveredModelDto> ResolveBuiltInModels(AiProvider provider)
    {
        if (IsDeepSeekProvider(provider))
        {
            return
            [
                new AiDiscoveredModelDto
                {
                    DisplayName = "DeepSeek V4 Pro",
                    ModelCode = "deepseek-v4-pro",
                    ModelType = "standard"
                },
                new AiDiscoveredModelDto
                {
                    DisplayName = "DeepSeek V4 Flash",
                    ModelCode = "deepseek-v4-flash",
                    ModelType = "standard"
                }
            ];
        }

        return [];
    }

    private static AiModel? CreateBuiltInTestModel(AiProvider provider)
    {
        var model = ResolveBuiltInModels(provider).FirstOrDefault();
        return model == null ? null : CreateTransientModel(provider, model);
    }

    private static AiModel CreateTransientModel(AiProvider provider, AiDiscoveredModelDto model)
    {
        return new AiModel(
            Guid.Empty,
            provider.Id,
            provider.TenantId,
            model.ModelCode,
            string.IsNullOrWhiteSpace(model.DisplayName) ? model.ModelCode : model.DisplayName,
            string.IsNullOrWhiteSpace(model.ModelType) ? "standard" : model.ModelType,
            isEnabled: true,
            isDefault: true,
            contextWindow: IsDeepSeekProvider(provider) ? 1_000_000 : 256_000,
            maxOutputTokens: IsDeepSeekProvider(provider) ? 384_000 : 4096,
            capabilitiesJson: JsonSerializer.Serialize(new[] { "streaming", "textGeneration", "toolCalling" }));
    }

    private static bool IsDeepSeekProvider(AiProvider provider)
    {
        if (ContainsDeepSeek(provider.Name) || ContainsDeepSeek(provider.Endpoint))
        {
            return true;
        }

        return Uri.TryCreate(provider.Endpoint, UriKind.Absolute, out var uri) &&
               ContainsDeepSeek(uri.Host);
    }

    private static bool ContainsDeepSeek(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) &&
               value.Contains("deepseek", StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveErrorMessage(System.Net.HttpStatusCode statusCode, string responseContent)
    {
        var message = TryReadErrorMessage(responseContent);
        return string.IsNullOrWhiteSpace(message)
            ? $"模型服务返回 {(int)statusCode} {statusCode}。"
            : $"模型服务返回 {(int)statusCode} {statusCode}：{message}";
    }

    private static string TryReadErrorMessage(string responseContent)
    {
        if (string.IsNullOrWhiteSpace(responseContent))
        {
            return string.Empty;
        }

        try
        {
            using var document = JsonDocument.Parse(responseContent);
            if (document.RootElement.TryGetProperty("error", out var error))
            {
                if (error.ValueKind == JsonValueKind.String)
                {
                    return error.GetString() ?? string.Empty;
                }

                if (error.TryGetProperty("message", out var message))
                {
                    return message.GetString() ?? string.Empty;
                }
            }

            if (document.RootElement.TryGetProperty("message", out var rootMessage))
            {
                return rootMessage.GetString() ?? string.Empty;
            }
        }
        catch
        {
            return responseContent.Length <= 500 ? responseContent : responseContent[..500];
        }

        return string.Empty;
    }

    internal static AiGatewayChatResult ParseChatCompletion(string responseContent)
    {
        try
        {
            using var document = JsonDocument.Parse(responseContent);
            var root = document.RootElement;
            var model = root.TryGetProperty("model", out var modelElement) ? modelElement.GetString() ?? string.Empty : string.Empty;
            var promptTokens = 0;
            var completionTokens = 0;
            if (root.TryGetProperty("usage", out var usage))
            {
                promptTokens = usage.TryGetProperty("prompt_tokens", out var prompt) ? prompt.GetInt32() : 0;
                completionTokens = usage.TryGetProperty("completion_tokens", out var completion) ? completion.GetInt32() : 0;
            }

            if (!root.TryGetProperty("choices", out var choices) || choices.ValueKind != JsonValueKind.Array || choices.GetArrayLength() == 0)
            {
                return AiGatewayChatResult.Fail("模型服务未返回 choices。", responseContent);
            }

            var firstChoice = choices[0];
            var finishReason = firstChoice.TryGetProperty("finish_reason", out var finish)
                ? finish.GetString() ?? string.Empty
                : string.Empty;
            var content = string.Empty;
            var toolCalls = new List<AiGatewayToolCall>();
            if (firstChoice.TryGetProperty("message", out var message) &&
                message.TryGetProperty("content", out var contentElement))
            {
                content = contentElement.ValueKind switch
                {
                    JsonValueKind.String => contentElement.GetString() ?? string.Empty,
                    JsonValueKind.Null or JsonValueKind.Undefined => string.Empty,
                    _ => contentElement.GetRawText()
                };
            }
            else if (firstChoice.TryGetProperty("text", out var textElement))
            {
                content = textElement.GetString() ?? string.Empty;
            }

            if (firstChoice.TryGetProperty("message", out message) &&
                message.TryGetProperty("tool_calls", out var toolCallsElement) &&
                toolCallsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var toolCallElement in toolCallsElement.EnumerateArray())
                {
                    if (!toolCallElement.TryGetProperty("function", out var functionElement))
                    {
                        continue;
                    }

                    var toolName = functionElement.TryGetProperty("name", out var nameElement)
                        ? nameElement.GetString() ?? string.Empty
                        : string.Empty;
                    var argumentsJson = functionElement.TryGetProperty("arguments", out var argumentsElement)
                        ? argumentsElement.ValueKind == JsonValueKind.String
                            ? argumentsElement.GetString() ?? "{}"
                            : argumentsElement.GetRawText()
                        : "{}";
                    if (string.IsNullOrWhiteSpace(toolName))
                    {
                        continue;
                    }

                    toolCalls.Add(new AiGatewayToolCall(
                        toolCallElement.TryGetProperty("id", out var idElement) ? idElement.GetString() ?? string.Empty : string.Empty,
                        toolName,
                        NormalizeJsonObject(argumentsJson)));
                }
            }

            if (string.IsNullOrWhiteSpace(content) && toolCalls.Count == 0)
            {
                return AiGatewayChatResult.Fail("模型服务返回内容为空。", responseContent);
            }

            return AiGatewayChatResult.Ok(content.Trim(), promptTokens, completionTokens, model, finishReason, responseContent, toolCalls);
        }
        catch (Exception ex)
        {
            return AiGatewayChatResult.Fail($"模型响应解析失败：{ex.Message}", responseContent);
        }
    }

    internal static AiGatewayChatResult ParseOpenAIResponse(string responseContent)
    {
        try
        {
            using var document = JsonDocument.Parse(responseContent);
            var root = document.RootElement;
            var model = root.TryGetProperty("model", out var modelElement) ? modelElement.GetString() ?? string.Empty : string.Empty;
            var finishReason = root.TryGetProperty("status", out var statusElement) ? statusElement.GetString() ?? string.Empty : string.Empty;
            var promptTokens = 0;
            var completionTokens = 0;
            if (root.TryGetProperty("usage", out var usage))
            {
                promptTokens = usage.TryGetProperty("input_tokens", out var inputTokens) ? inputTokens.GetInt32() : 0;
                completionTokens = usage.TryGetProperty("output_tokens", out var outputTokens) ? outputTokens.GetInt32() : 0;
            }

            var content = root.TryGetProperty("output_text", out var outputText) ? outputText.GetString() ?? string.Empty : string.Empty;
            var toolCalls = new List<AiGatewayToolCall>();
            if (root.TryGetProperty("output", out var output) && output.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in output.EnumerateArray())
                {
                    var type = item.TryGetProperty("type", out var typeElement) ? typeElement.GetString() ?? string.Empty : string.Empty;
                    if (string.Equals(type, "message", StringComparison.OrdinalIgnoreCase) &&
                        item.TryGetProperty("content", out var contentItems) &&
                        contentItems.ValueKind == JsonValueKind.Array)
                    {
                        content = string.Concat(contentItems
                            .EnumerateArray()
                            .Select(ReadOpenAIResponseContentText)
                            .Where(x => !string.IsNullOrWhiteSpace(x)));
                    }
                    else if (string.Equals(type, "function_call", StringComparison.OrdinalIgnoreCase))
                    {
                        var name = item.TryGetProperty("name", out var nameElement) ? nameElement.GetString() ?? string.Empty : string.Empty;
                        if (string.IsNullOrWhiteSpace(name))
                        {
                            continue;
                        }

                        var callId = item.TryGetProperty("call_id", out var callIdElement) ? callIdElement.GetString() ?? string.Empty : string.Empty;
                        var arguments = item.TryGetProperty("arguments", out var argumentsElement)
                            ? argumentsElement.ValueKind == JsonValueKind.String ? argumentsElement.GetString() ?? "{}" : argumentsElement.GetRawText()
                            : "{}";
                        toolCalls.Add(new AiGatewayToolCall(callId, name, NormalizeJsonObject(arguments)));
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(content) && toolCalls.Count == 0)
            {
                return AiGatewayChatResult.Fail("模型服务返回内容为空。", responseContent);
            }

            return AiGatewayChatResult.Ok(content.Trim(), promptTokens, completionTokens, model, finishReason, responseContent, toolCalls);
        }
        catch (Exception ex)
        {
            return AiGatewayChatResult.Fail($"模型响应解析失败：{ex.Message}", responseContent);
        }
    }

    internal static AiGatewayChatResult ParseAnthropicMessage(string responseContent)
    {
        try
        {
            using var document = JsonDocument.Parse(responseContent);
            var root = document.RootElement;
            var model = root.TryGetProperty("model", out var modelElement) ? modelElement.GetString() ?? string.Empty : string.Empty;
            var finishReason = root.TryGetProperty("stop_reason", out var stopReason) ? stopReason.GetString() ?? string.Empty : string.Empty;
            var promptTokens = 0;
            var completionTokens = 0;
            if (root.TryGetProperty("usage", out var usage))
            {
                promptTokens = usage.TryGetProperty("input_tokens", out var inputTokens) ? inputTokens.GetInt32() : 0;
                completionTokens = usage.TryGetProperty("output_tokens", out var outputTokens) ? outputTokens.GetInt32() : 0;
            }

            if (!root.TryGetProperty("content", out var contentItems) ||
                contentItems.ValueKind != JsonValueKind.Array)
            {
                return AiGatewayChatResult.Fail("模型服务未返回 content。", responseContent);
            }

            var contentBuilder = new StringBuilder();
            var toolCalls = new List<AiGatewayToolCall>();
            foreach (var item in contentItems.EnumerateArray())
            {
                var type = item.TryGetProperty("type", out var typeElement) ? typeElement.GetString() ?? string.Empty : string.Empty;
                if (string.Equals(type, "text", StringComparison.OrdinalIgnoreCase))
                {
                    contentBuilder.Append(item.TryGetProperty("text", out var text) ? text.GetString() ?? string.Empty : string.Empty);
                    continue;
                }

                if (!string.Equals(type, "tool_use", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var name = item.TryGetProperty("name", out var nameElement) ? nameElement.GetString() ?? string.Empty : string.Empty;
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                var id = item.TryGetProperty("id", out var idElement) ? idElement.GetString() ?? string.Empty : string.Empty;
                var argumentsJson = item.TryGetProperty("input", out var inputElement)
                    ? inputElement.ValueKind == JsonValueKind.String ? inputElement.GetString() ?? "{}" : inputElement.GetRawText()
                    : "{}";
                toolCalls.Add(new AiGatewayToolCall(id, name, NormalizeJsonObject(argumentsJson)));
            }

            var content = contentBuilder.ToString();
            if (string.IsNullOrWhiteSpace(content) && toolCalls.Count == 0)
            {
                return AiGatewayChatResult.Fail("模型服务返回内容为空。", responseContent);
            }

            return AiGatewayChatResult.Ok(content.Trim(), promptTokens, completionTokens, model, finishReason, responseContent, toolCalls);
        }
        catch (Exception ex)
        {
            return AiGatewayChatResult.Fail($"模型响应解析失败：{ex.Message}", responseContent);
        }
    }

    private static AiGatewayChatStreamChunk? ParseChatCompletionStreamChunk(string responseContent)
    {
        try
        {
            using var document = JsonDocument.Parse(responseContent);
            var root = document.RootElement;
            var model = root.TryGetProperty("model", out var modelElement) ? modelElement.GetString() ?? string.Empty : string.Empty;
            var promptTokens = 0;
            var completionTokens = 0;
            if (root.TryGetProperty("usage", out var usage) && usage.ValueKind == JsonValueKind.Object)
            {
                promptTokens = usage.TryGetProperty("prompt_tokens", out var prompt) ? prompt.GetInt32() : 0;
                completionTokens = usage.TryGetProperty("completion_tokens", out var completion) ? completion.GetInt32() : 0;
            }

            var delta = string.Empty;
            var finishReason = string.Empty;
            if (root.TryGetProperty("choices", out var choices) &&
                choices.ValueKind == JsonValueKind.Array &&
                choices.GetArrayLength() > 0)
            {
                var firstChoice = choices[0];
                finishReason = firstChoice.TryGetProperty("finish_reason", out var finish)
                    ? finish.GetString() ?? string.Empty
                    : string.Empty;
                if (firstChoice.TryGetProperty("delta", out var deltaElement) &&
                    deltaElement.TryGetProperty("content", out var contentElement))
                {
                    delta = contentElement.ValueKind == JsonValueKind.String
                        ? contentElement.GetString() ?? string.Empty
                        : contentElement.GetRawText();
                }
                else if (firstChoice.TryGetProperty("text", out var textElement))
                {
                    delta = textElement.GetString() ?? string.Empty;
                }
            }

            if (string.IsNullOrEmpty(delta) &&
                string.IsNullOrEmpty(finishReason) &&
                promptTokens <= 0 &&
                completionTokens <= 0 &&
                string.IsNullOrEmpty(model))
            {
                return null;
            }

            return AiGatewayChatStreamChunk.Delta(delta, promptTokens, completionTokens, model, finishReason);
        }
        catch
        {
            return null;
        }
    }

    private static AiGatewayChatStreamChunk? ParseOpenAIResponsesStreamChunk(string responseContent)
    {
        try
        {
            using var document = JsonDocument.Parse(responseContent);
            var root = document.RootElement;
            var type = root.TryGetProperty("type", out var typeElement) ? typeElement.GetString() ?? string.Empty : string.Empty;
            if (type.Contains("failed", StringComparison.OrdinalIgnoreCase) ||
                type.Contains("error", StringComparison.OrdinalIgnoreCase))
            {
                return AiGatewayChatStreamChunk.Fail(TryReadStreamError(root), responseContent);
            }

            if (string.Equals(type, "response.output_text.delta", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(type, "response.refusal.delta", StringComparison.OrdinalIgnoreCase))
            {
                var delta = root.TryGetProperty("delta", out var deltaElement) ? deltaElement.GetString() ?? string.Empty : string.Empty;
                return string.IsNullOrEmpty(delta) ? null : AiGatewayChatStreamChunk.Delta(delta, 0, 0, string.Empty, string.Empty);
            }

            if (string.Equals(type, "response.completed", StringComparison.OrdinalIgnoreCase) &&
                root.TryGetProperty("response", out var response))
            {
                var model = response.TryGetProperty("model", out var modelElement) ? modelElement.GetString() ?? string.Empty : string.Empty;
                var status = response.TryGetProperty("status", out var statusElement) ? statusElement.GetString() ?? string.Empty : "completed";
                var promptTokens = 0;
                var completionTokens = 0;
                if (response.TryGetProperty("usage", out var usage))
                {
                    promptTokens = usage.TryGetProperty("input_tokens", out var inputTokens) ? inputTokens.GetInt32() : 0;
                    completionTokens = usage.TryGetProperty("output_tokens", out var outputTokens) ? outputTokens.GetInt32() : 0;
                }

                return AiGatewayChatStreamChunk.Delta(string.Empty, promptTokens, completionTokens, model, status);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private static AiGatewayChatStreamChunk? ParseAnthropicMessagesStreamChunk(string responseContent)
    {
        try
        {
            using var document = JsonDocument.Parse(responseContent);
            var root = document.RootElement;
            var type = root.TryGetProperty("type", out var typeElement) ? typeElement.GetString() ?? string.Empty : string.Empty;
            if (string.Equals(type, "error", StringComparison.OrdinalIgnoreCase))
            {
                return AiGatewayChatStreamChunk.Fail(TryReadStreamError(root), responseContent);
            }

            if (string.Equals(type, "message_start", StringComparison.OrdinalIgnoreCase) &&
                root.TryGetProperty("message", out var message))
            {
                var model = message.TryGetProperty("model", out var modelElement) ? modelElement.GetString() ?? string.Empty : string.Empty;
                var promptTokens = 0;
                if (message.TryGetProperty("usage", out var usage))
                {
                    promptTokens = usage.TryGetProperty("input_tokens", out var inputTokens) ? inputTokens.GetInt32() : 0;
                }

                return AiGatewayChatStreamChunk.Delta(string.Empty, promptTokens, 0, model, string.Empty);
            }

            if (string.Equals(type, "content_block_delta", StringComparison.OrdinalIgnoreCase) &&
                root.TryGetProperty("delta", out var deltaElement))
            {
                var text = deltaElement.TryGetProperty("text", out var textElement) ? textElement.GetString() ?? string.Empty : string.Empty;
                return string.IsNullOrEmpty(text) ? null : AiGatewayChatStreamChunk.Delta(text, 0, 0, string.Empty, string.Empty);
            }

            if (string.Equals(type, "message_delta", StringComparison.OrdinalIgnoreCase))
            {
                var finishReason = string.Empty;
                var completionTokens = 0;
                if (root.TryGetProperty("delta", out var delta))
                {
                    finishReason = delta.TryGetProperty("stop_reason", out var stopReason) ? stopReason.GetString() ?? string.Empty : string.Empty;
                }

                if (root.TryGetProperty("usage", out var usage))
                {
                    completionTokens = usage.TryGetProperty("output_tokens", out var outputTokens) ? outputTokens.GetInt32() : 0;
                }

                return AiGatewayChatStreamChunk.Delta(string.Empty, 0, completionTokens, string.Empty, finishReason);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private static string TryReadStreamError(JsonElement root)
    {
        if (root.TryGetProperty("error", out var error))
        {
            if (error.ValueKind == JsonValueKind.String)
            {
                return error.GetString() ?? "模型流式响应返回错误。";
            }

            if (error.TryGetProperty("message", out var message))
            {
                return message.GetString() ?? "模型流式响应返回错误。";
            }
        }

        if (root.TryGetProperty("message", out var rootMessage))
        {
            return rootMessage.GetString() ?? "模型流式响应返回错误。";
        }

        return "模型流式响应返回错误。";
    }

    private static Dictionary<string, object?> BuildOpenAIResponsesPayload(
        AiProvider provider,
        AiModel model,
        IReadOnlyCollection<AiGatewayNormalizedMessage> messages,
        int? maxOutputTokens,
        IReadOnlyCollection<AiGatewayToolDefinition>? tools)
    {
        var payload = new Dictionary<string, object?>
        {
            ["input"] = messages.Select(x => new
            {
                role = x.Role,
                content = x.Content
            }).ToList(),
            ["max_output_tokens"] = ResolveMaxOutputTokens(model, maxOutputTokens),
            ["model"] = model.ModelCode
        };
        var normalizedTools = BuildOpenAIResponseToolPayloads(tools);
        if (normalizedTools.Count > 0)
        {
            payload["tools"] = normalizedTools;
            payload["tool_choice"] = "auto";
        }

        var temperature = ResolveTemperature(provider.ConfigJson);
        if (temperature.HasValue)
        {
            payload["temperature"] = temperature.Value;
        }

        return payload;
    }

    private static Dictionary<string, object?> BuildAnthropicMessagesPayload(
        AiProvider provider,
        AiModel model,
        IReadOnlyCollection<AiGatewayNormalizedMessage> messages,
        int? maxOutputTokens,
        IReadOnlyCollection<AiGatewayToolDefinition>? tools)
    {
        var systemPrompt = string.Join(
            Environment.NewLine,
            messages.Where(x => x.Role == "system").Select(x => x.Content));
        var conversationMessages = messages
            .Where(x => x.Role != "system")
            .Select(x => new
            {
                role = x.Role == "assistant" ? "assistant" : "user",
                content = x.Content
            })
            .ToList();
        var payload = new Dictionary<string, object?>
        {
            ["max_tokens"] = ResolveMaxOutputTokens(model, maxOutputTokens),
            ["messages"] = conversationMessages,
            ["model"] = model.ModelCode
        };
        if (!string.IsNullOrWhiteSpace(systemPrompt))
        {
            payload["system"] = systemPrompt;
        }

        var normalizedTools = BuildAnthropicToolPayloads(tools);
        if (normalizedTools.Count > 0)
        {
            payload["tools"] = normalizedTools;
            payload["tool_choice"] = new { type = "auto" };
        }

        var temperature = ResolveTemperature(provider.ConfigJson);
        if (temperature.HasValue)
        {
            payload["temperature"] = temperature.Value;
        }

        return payload;
    }

    private static List<AiGatewayNormalizedMessage> NormalizeMessages(IReadOnlyCollection<AiGatewayMessage> messages)
    {
        return messages
            .Where(x => !string.IsNullOrWhiteSpace(x.Content))
            .Select(x => new AiGatewayNormalizedMessage(NormalizeRole(x.Role), x.Content.Trim()))
            .ToList();
    }

    private static int ResolveMaxOutputTokens(AiModel model, int? maxOutputTokens)
    {
        var configured = maxOutputTokens ?? model.MaxOutputTokens;
        if (configured <= 0)
        {
            return 1024;
        }

        return Math.Clamp(configured, 1, 384_000);
    }

    internal static List<object> BuildToolPayloads(IReadOnlyCollection<AiGatewayToolDefinition>? tools)
    {
        if (tools == null || tools.Count == 0)
        {
            return [];
        }

        return tools
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .Take(20)
            .Select(tool => new
            {
                type = "function",
                function = new
                {
                    name = tool.Name,
                    description = tool.Description,
                    parameters = NormalizeJsonSchema(tool.ParametersJson)
                }
            })
            .Cast<object>()
            .ToList();
    }

    internal static List<object> BuildOpenAIResponseToolPayloads(IReadOnlyCollection<AiGatewayToolDefinition>? tools)
    {
        if (tools == null || tools.Count == 0)
        {
            return [];
        }

        return tools
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .Take(20)
            .Select(tool => new
            {
                description = tool.Description,
                name = tool.Name,
                parameters = NormalizeJsonSchema(tool.ParametersJson),
                type = "function"
            })
            .Cast<object>()
            .ToList();
    }

    internal static List<object> BuildAnthropicToolPayloads(IReadOnlyCollection<AiGatewayToolDefinition>? tools)
    {
        if (tools == null || tools.Count == 0)
        {
            return [];
        }

        return tools
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .Take(20)
            .Select(tool => new
            {
                name = tool.Name,
                description = tool.Description,
                input_schema = NormalizeJsonSchema(tool.ParametersJson)
            })
            .Cast<object>()
            .ToList();
    }

    private static object NormalizeJsonSchema(string json)
    {
        try
        {
            var normalizedJson = NormalizeJsonObject(json);
            var node = JsonNode.Parse(normalizedJson) as JsonObject;
            if (node == null)
            {
                return new { type = "object" };
            }

            if (!node.ContainsKey("type"))
            {
                node["type"] = "object";
            }

            return JsonSerializer.Deserialize<object>(node.ToJsonString(JsonOptions), JsonOptions) ?? new { type = "object" };
        }
        catch
        {
            return new { type = "object" };
        }
    }

    private static string NormalizeJsonObject(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return "{}";
        }

        try
        {
            var node = JsonNode.Parse(json);
            return node is JsonObject ? node.ToJsonString(JsonOptions) : "{}";
        }
        catch
        {
            return "{}";
        }
    }

    private static decimal? ResolveTemperature(string configJson)
    {
        if (string.IsNullOrWhiteSpace(configJson))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(configJson);
            if (document.RootElement.TryGetProperty("temperature", out var temperature) &&
                temperature.TryGetDecimal(out var value))
            {
                return Math.Clamp(value, 0, 2);
            }
        }
        catch
        {
            return null;
        }

        return null;
    }

    private static string ResolveAnthropicVersion(string configJson)
    {
        if (string.IsNullOrWhiteSpace(configJson))
        {
            return "2023-06-01";
        }

        try
        {
            using var document = JsonDocument.Parse(configJson);
            if (document.RootElement.TryGetProperty("anthropicVersion", out var version))
            {
                var value = version.GetString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }
        }
        catch
        {
            return "2023-06-01";
        }

        return "2023-06-01";
    }

    private static string ResolveProtocol(AiProvider provider)
    {
        return string.Equals(provider.ProviderType, AiProviderProtocolNames.AnthropicMessages, StringComparison.OrdinalIgnoreCase)
            ? AiProviderProtocolNames.AnthropicMessages
            : AiProviderProtocolNames.OpenAIResponses;
    }

    private static string NormalizeRole(string role)
    {
        return role switch
        {
            "assistant" => "assistant",
            "system" => "system",
            _ => "user"
        };
    }

    private static string ReadOpenAIResponseContentText(JsonElement element)
    {
        if (element.TryGetProperty("text", out var text))
        {
            return text.GetString() ?? string.Empty;
        }

        if (element.TryGetProperty("type", out var type) &&
            string.Equals(type.GetString(), "output_text", StringComparison.OrdinalIgnoreCase) &&
            element.TryGetProperty("content", out var content))
        {
            return content.GetString() ?? string.Empty;
        }

        return string.Empty;
    }

    private static List<AiDiscoveredModelDto> ParseModelList(string responseContent)
    {
        try
        {
            using var document = JsonDocument.Parse(responseContent);
            if (!document.RootElement.TryGetProperty("data", out var data) ||
                data.ValueKind != JsonValueKind.Array)
            {
                return [];
            }

            return data
                .EnumerateArray()
                .Select(ReadDiscoveredModel)
                .Where(x => !string.IsNullOrWhiteSpace(x.ModelCode))
                .GroupBy(x => x.ModelCode, StringComparer.OrdinalIgnoreCase)
                .Select(x => x.First())
                .OrderBy(x => x.ModelCode, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch
        {
            return [];
        }
    }

    private static AiDiscoveredModelDto ReadDiscoveredModel(JsonElement element)
    {
        var id = element.TryGetProperty("id", out var idElement) ? idElement.GetString() ?? string.Empty : string.Empty;
        var displayName = element.TryGetProperty("display_name", out var displayNameElement)
            ? displayNameElement.GetString() ?? string.Empty
            : id;
        return new AiDiscoveredModelDto
        {
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? id : displayName,
            ModelCode = id,
            ModelType = "standard"
        };
    }

    private static List<string> DeserializeCapabilities(string capabilitiesJson)
    {
        if (string.IsNullOrWhiteSpace(capabilitiesJson))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(capabilitiesJson) ?? [];
        }
        catch
        {
            return [];
        }
    }
}

public sealed record AiGatewayMessage(string Role, string Content);

public sealed record AiGatewayNormalizedMessage(string Role, string Content);

public sealed record AiGatewayToolDefinition(string Name, string Description, string ParametersJson);

public sealed record AiGatewayToolCall(string Id, string Name, string ArgumentsJson);

public sealed class AiGatewayTestResult
{
    public string Message { get; private init; } = string.Empty;

    public bool Success { get; private init; }

    public static AiGatewayTestResult Fail(string message)
    {
        return new AiGatewayTestResult { Success = false, Message = message };
    }

    public static AiGatewayTestResult Ok(string message)
    {
        return new AiGatewayTestResult { Success = true, Message = message };
    }
}

public sealed class AiGatewayChatResult
{
    public int CompletionTokens { get; private init; }

    public string Content { get; private init; } = string.Empty;

    public string ErrorMessage { get; private init; } = string.Empty;

    public string FinishReason { get; private init; } = string.Empty;

    public string Model { get; private init; } = string.Empty;

    public int PromptTokens { get; private init; }

    public string RawResponseJson { get; private init; } = string.Empty;

    public bool Success { get; private init; }

    public List<AiGatewayToolCall> ToolCalls { get; private init; } = [];

    public static AiGatewayChatResult Fail(string message, string rawResponseJson = "")
    {
        return new AiGatewayChatResult
        {
            ErrorMessage = message,
            RawResponseJson = rawResponseJson,
            Success = false
        };
    }

    public static AiGatewayChatResult Ok(
        string content,
        int promptTokens,
        int completionTokens,
        string model,
        string finishReason,
        string rawResponseJson,
        IReadOnlyCollection<AiGatewayToolCall>? toolCalls = null)
    {
        return new AiGatewayChatResult
        {
            CompletionTokens = completionTokens,
            Content = content,
            FinishReason = finishReason,
            Model = model,
            PromptTokens = promptTokens,
            RawResponseJson = rawResponseJson,
            Success = true,
            ToolCalls = toolCalls?.ToList() ?? []
        };
    }
}

public sealed class AiGatewayChatStreamChunk
{
    public int CompletionTokens { get; private init; }

    public string ContentDelta { get; private init; } = string.Empty;

    public string ErrorMessage { get; private init; } = string.Empty;

    public string FinishReason { get; private init; } = string.Empty;

    public string Model { get; private init; } = string.Empty;

    public int PromptTokens { get; private init; }

    public string RawResponseJson { get; private init; } = string.Empty;

    public bool IsCompleted { get; private init; }

    public bool Success { get; private init; } = true;

    public static AiGatewayChatStreamChunk Completed(string rawResponseJson)
    {
        return new AiGatewayChatStreamChunk
        {
            IsCompleted = true,
            RawResponseJson = rawResponseJson,
            Success = true
        };
    }

    public static AiGatewayChatStreamChunk Delta(
        string contentDelta,
        int promptTokens,
        int completionTokens,
        string model,
        string finishReason)
    {
        return new AiGatewayChatStreamChunk
        {
            CompletionTokens = Math.Max(0, completionTokens),
            ContentDelta = contentDelta,
            FinishReason = finishReason,
            Model = model,
            PromptTokens = Math.Max(0, promptTokens),
            Success = true
        };
    }

    public static AiGatewayChatStreamChunk Fail(string message, string rawResponseJson = "")
    {
        return new AiGatewayChatStreamChunk
        {
            ErrorMessage = message,
            RawResponseJson = rawResponseJson,
            Success = false
        };
    }
}
