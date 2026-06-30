using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Encryption;

namespace General.Admin.Ai;

public class AiWebSearchService : ITransientDependency
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly ICurrentTenant _currentTenant;
    private readonly IStringEncryptionService _encryptionService;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IRepository<AiWebSearchConfig, Guid> _searchConfigRepository;
    private readonly IRepository<AiWebSearchRecord, Guid> _searchRecordRepository;

    public AiWebSearchService(
        ICurrentTenant currentTenant,
        IStringEncryptionService encryptionService,
        IGuidGenerator guidGenerator,
        IHttpClientFactory httpClientFactory,
        IRepository<AiWebSearchConfig, Guid> searchConfigRepository,
        IRepository<AiWebSearchRecord, Guid> searchRecordRepository)
    {
        _currentTenant = currentTenant;
        _encryptionService = encryptionService;
        _guidGenerator = guidGenerator;
        _httpClientFactory = httpClientFactory;
        _searchConfigRepository = searchConfigRepository;
        _searchRecordRepository = searchRecordRepository;
    }

    public async Task<List<AiWebSearchConfigDto>> GetConfigsAsync(bool includeDisabled = true)
    {
        return (await _searchConfigRepository.GetListAsync())
            .Where(x => includeDisabled || x.IsEnabled)
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.Name)
            .Select(MapConfig)
            .ToList();
    }

    public async Task<AiWebSearchConfigDto> CreateConfigAsync(AiWebSearchConfigSaveInput input)
    {
        if (input.IsDefault)
        {
            await ClearDefaultConfigsAsync();
        }

        var entity = new AiWebSearchConfig(
            _guidGenerator.Create(),
            input.Name,
            input.ProviderType,
            input.Endpoint,
            EncryptApiKey(input.ApiKey),
            _currentTenant.Id,
            Serialize(input.AllowDomains),
            Serialize(input.DenyDomains),
            input.MaxResults,
            input.TimeoutSeconds,
            input.ConfigJson,
            input.IsEnabled,
            input.IsDefault,
            input.Remark);
        await _searchConfigRepository.InsertAsync(entity, autoSave: true);
        return MapConfig(entity);
    }

    public async Task<AiWebSearchConfigDto> UpdateConfigAsync(Guid id, AiWebSearchConfigSaveInput input)
    {
        var entity = await _searchConfigRepository.GetAsync(id);
        if (input.IsDefault)
        {
            await ClearDefaultConfigsAsync(id);
        }

        entity.Update(
            input.Name,
            input.ProviderType,
            input.Endpoint,
            Serialize(input.AllowDomains),
            Serialize(input.DenyDomains),
            input.MaxResults,
            input.TimeoutSeconds,
            input.ConfigJson,
            input.IsEnabled,
            input.IsDefault,
            input.Remark);

        if (!string.IsNullOrWhiteSpace(input.ApiKey))
        {
            entity.ResetApiKey(EncryptApiKey(input.ApiKey));
        }

        await _searchConfigRepository.UpdateAsync(entity, autoSave: true);
        return MapConfig(entity);
    }

    public async Task DeleteConfigAsync(Guid id)
    {
        await _searchConfigRepository.DeleteAsync(id, autoSave: true);
    }

    public async Task<List<AiWebSearchRecordDto>> GetRecordsAsync(AiWebSearchRecordQueryInput input)
    {
        var records = await _searchRecordRepository.GetListAsync();
        if (input.ConfigId.HasValue)
        {
            records = records.Where(x => x.ConfigId == input.ConfigId.Value).ToList();
        }

        if (input.ConversationId.HasValue)
        {
            records = records.Where(x => x.ConversationId == input.ConversationId.Value).ToList();
        }

        if (!string.IsNullOrWhiteSpace(input.Status))
        {
            var status = input.Status.Trim();
            records = records.Where(x => x.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            var keyword = input.Keyword.Trim();
            records = records.Where(x =>
                x.Query.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                x.ProviderName.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return records
            .OrderByDescending(x => x.CreationTime)
            .Take(Math.Clamp(input.MaxResultCount, 1, 200))
            .Select(MapRecord)
            .ToList();
    }

    public async Task<AiWebSearchResultDto> SearchAsync(AiWebSearchInput input)
    {
        var config = await ResolveConfigAsync(input.ConfigId);
        if (config is null)
        {
            throw new UserFriendlyException("未配置可用的联网搜索 Provider。");
        }

        if (!config.IsEnabled)
        {
            throw new UserFriendlyException("联网搜索 Provider 已停用。");
        }

        var requestJson = JsonSerializer.Serialize(new
        {
            input.Query,
            MaxResults = Math.Clamp(input.MaxResults <= 0 ? config.MaxResults : input.MaxResults, 1, config.MaxResults),
            config.ProviderType
        }, JsonOptions);

        var record = new AiWebSearchRecord(
            _guidGenerator.Create(),
            input.Query,
            config.Name,
            requestJson,
            _currentTenant.Id,
            config.Id,
            input.ConversationId,
            input.MessageId);
        await _searchRecordRepository.InsertAsync(record, autoSave: true);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var responseSummary = await ExecuteSearchAsync(config, input, stopwatch);
            stopwatch.Stop();
            record.Complete(responseSummary.Json, responseSummary.ResultCount, stopwatch.ElapsedMilliseconds);
            await _searchRecordRepository.UpdateAsync(record, autoSave: true);
            return new AiWebSearchResultDto
            {
                RecordId = record.Id,
                ResponseSummaryJson = record.ResponseSummaryJson,
                ResultCount = record.ResultCount,
                Status = record.Status
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            record.Fail(ex.Message, stopwatch.ElapsedMilliseconds);
            await _searchRecordRepository.UpdateAsync(record, autoSave: true);
            return new AiWebSearchResultDto
            {
                ErrorMessage = record.ErrorMessage,
                RecordId = record.Id,
                ResponseSummaryJson = record.ResponseSummaryJson,
                ResultCount = record.ResultCount,
                Status = record.Status
            };
        }
    }

    private async Task<AiWebSearchConfig?> ResolveConfigAsync(Guid? configId)
    {
        if (configId.HasValue)
        {
            return await _searchConfigRepository.GetAsync(configId.Value);
        }

        return (await _searchConfigRepository.GetListAsync(x => x.IsEnabled))
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.Name)
            .FirstOrDefault();
    }

    private async Task<SearchResponseSummary> ExecuteSearchAsync(AiWebSearchConfig config, AiWebSearchInput input, Stopwatch stopwatch)
    {
        var endpoint = ResolveEndpoint(config);
        EnsureEndpointAllowedByPolicy(endpoint, config);

        using var request = BuildRequest(config, endpoint, input);
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(config.TimeoutSeconds));
        var client = _httpClientFactory.CreateClient();
        using var response = await client.SendAsync(request, timeoutCts.Token);
        var content = await response.Content.ReadAsStringAsync(timeoutCts.Token);
        if (!response.IsSuccessStatusCode)
        {
            throw new UserFriendlyException($"联网搜索调用失败：{(int)response.StatusCode} {response.ReasonPhrase} {Trim(content, 512)}");
        }

        var trimmed = Trim(content, 64_000);
        return new SearchResponseSummary(
            JsonSerializer.Serialize(new
            {
                provider = config.ProviderType,
                elapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                raw = trimmed
            }, JsonOptions),
            ResolveResultCount(trimmed));
    }

    private HttpRequestMessage BuildRequest(AiWebSearchConfig config, Uri endpoint, AiWebSearchInput input)
    {
        var apiKey = DecryptApiKey(config.ApiKeyCipherText);
        var maxResults = Math.Clamp(input.MaxResults <= 0 ? config.MaxResults : input.MaxResults, 1, config.MaxResults);
        return config.ProviderType.Trim().ToLowerInvariant() switch
        {
            "bingwebsearch" => BuildGetRequest(endpoint, new Dictionary<string, string>
            {
                ["q"] = input.Query,
                ["count"] = maxResults.ToString()
            }, headers =>
            {
                if (!string.IsNullOrWhiteSpace(apiKey))
                {
                    headers.Add("Ocp-Apim-Subscription-Key", apiKey);
                }
            }),
            "serpapi" => BuildGetRequest(endpoint, new Dictionary<string, string>
            {
                ["api_key"] = apiKey,
                ["q"] = input.Query,
                ["num"] = maxResults.ToString()
            }),
            "tavily" => BuildJsonPostRequest(endpoint, new
            {
                api_key = apiKey,
                query = input.Query,
                max_results = maxResults,
                search_depth = "basic"
            }),
            _ => BuildJsonPostRequest(endpoint, new
            {
                query = input.Query,
                maxResults,
                config = SafeDeserialize(config.ConfigJson)
            }, apiKey)
        };
    }

    private static HttpRequestMessage BuildGetRequest(Uri endpoint, IReadOnlyDictionary<string, string> query, Action<HttpRequestHeaders>? configureHeaders = null)
    {
        var builder = new UriBuilder(endpoint);
        var prefix = string.IsNullOrWhiteSpace(builder.Query) ? string.Empty : builder.Query.TrimStart('?') + "&";
        builder.Query = prefix + string.Join("&", query
            .Where(x => !string.IsNullOrWhiteSpace(x.Value))
            .Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value)}"));
        var request = new HttpRequestMessage(HttpMethod.Get, builder.Uri);
        configureHeaders?.Invoke(request.Headers);
        return request;
    }

    private static HttpRequestMessage BuildJsonPostRequest(Uri endpoint, object payload, string? bearerToken = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json")
        };
        if (!string.IsNullOrWhiteSpace(bearerToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }

        return request;
    }

    private static Uri ResolveEndpoint(AiWebSearchConfig config)
    {
        var endpoint = config.Endpoint;
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            endpoint = config.ProviderType.Trim().ToLowerInvariant() switch
            {
                "bingwebsearch" => "https://api.bing.microsoft.com/v7.0/search",
                "serpapi" => "https://serpapi.com/search.json",
                "tavily" => "https://api.tavily.com/search",
                _ => string.Empty
            };
        }

        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new UserFriendlyException("联网搜索 Endpoint 必须是合法的 HTTP/HTTPS 地址。");
        }

        return uri;
    }

    private static void EnsureEndpointAllowedByPolicy(Uri endpoint, AiWebSearchConfig config)
    {
        var host = endpoint.Host;
        var allowDomains = DeserializeList(config.AllowDomainsJson);
        var denyDomains = DeserializeList(config.DenyDomainsJson);
        if (denyDomains.Any(domain => IsDomainMatch(host, domain)))
        {
            throw new UserFriendlyException($"联网搜索 Endpoint 命中拒绝域名策略：{host}");
        }

        if (allowDomains.Count > 0 && allowDomains.All(domain => !IsDomainMatch(host, domain)))
        {
            throw new UserFriendlyException($"联网搜索 Endpoint 不在允许域名策略内：{host}");
        }
    }

    private static bool IsDomainMatch(string host, string domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
        {
            return false;
        }

        var normalized = domain.Trim().TrimStart('.');
        return host.Equals(normalized, StringComparison.OrdinalIgnoreCase) ||
               host.EndsWith($".{normalized}", StringComparison.OrdinalIgnoreCase);
    }

    private async Task ClearDefaultConfigsAsync(Guid? exceptId = null)
    {
        var defaults = await _searchConfigRepository.GetListAsync(x => x.IsDefault && (!exceptId.HasValue || x.Id != exceptId.Value));
        foreach (var item in defaults)
        {
            item.ClearDefault();
        }

        if (defaults.Count > 0)
        {
            await _searchConfigRepository.UpdateManyAsync(defaults, autoSave: true);
        }
    }

    private string EncryptApiKey(string? apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return string.Empty;
        }

        return _encryptionService.Encrypt(apiKey.Trim()) ?? throw new InvalidOperationException("联网搜索密钥加密失败。");
    }

    private string DecryptApiKey(string? cipherText)
    {
        if (string.IsNullOrWhiteSpace(cipherText))
        {
            return string.Empty;
        }

        return _encryptionService.Decrypt(cipherText) ?? string.Empty;
    }

    private static AiWebSearchConfigDto MapConfig(AiWebSearchConfig item)
    {
        return new AiWebSearchConfigDto
        {
            AllowDomains = DeserializeList(item.AllowDomainsJson),
            ConfigJson = item.ConfigJson,
            CreationTime = item.CreationTime,
            DenyDomains = DeserializeList(item.DenyDomainsJson),
            Endpoint = item.Endpoint,
            HasApiKey = item.HasApiKey(),
            Id = item.Id,
            IsDefault = item.IsDefault,
            IsEnabled = item.IsEnabled,
            MaxResults = item.MaxResults,
            Name = item.Name,
            ProviderType = item.ProviderType,
            Remark = item.Remark,
            TimeoutSeconds = item.TimeoutSeconds
        };
    }

    private static AiWebSearchRecordDto MapRecord(AiWebSearchRecord item)
    {
        return new AiWebSearchRecordDto
        {
            ConfigId = item.ConfigId,
            ConversationId = item.ConversationId,
            CreationTime = item.CreationTime,
            ElapsedMilliseconds = item.ElapsedMilliseconds,
            ErrorMessage = item.ErrorMessage,
            Id = item.Id,
            MessageId = item.MessageId,
            ProviderName = item.ProviderName,
            Query = item.Query,
            RequestJson = item.RequestJson,
            ResponseSummaryJson = item.ResponseSummaryJson,
            ResultCount = item.ResultCount,
            Status = item.Status
        };
    }

    private static int ResolveResultCount(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            if (TryGetArrayCount(root, "organic_results", out var count) ||
                TryGetArrayCount(root, "results", out count))
            {
                return count;
            }

            if (root.TryGetProperty("webPages", out var webPages) && TryGetArrayCount(webPages, "value", out count))
            {
                return count;
            }
        }
        catch
        {
            return 0;
        }

        return 0;
    }

    private static bool TryGetArrayCount(JsonElement element, string propertyName, out int count)
    {
        count = 0;
        if (!element.TryGetProperty(propertyName, out var array) || array.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        count = array.GetArrayLength();
        return true;
    }

    private static object SafeDeserialize(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<JsonElement>(json, JsonOptions);
        }
        catch
        {
            return new { };
        }
    }

    private static string Serialize(IReadOnlyCollection<string> values)
    {
        return JsonSerializer.Serialize(values
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList(), JsonOptions);
    }

    private static List<string> DeserializeList(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json, JsonOptions) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static string Trim(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
        {
            return value;
        }

        return value[..maxLength];
    }

    private sealed record SearchResponseSummary(string Json, int ResultCount);
}
