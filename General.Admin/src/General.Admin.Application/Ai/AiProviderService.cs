using System.Text.Json;
using General.Admin.Settings;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Encryption;
using Volo.Abp.Settings;

namespace General.Admin.Ai;

public class AiProviderService : ITransientDependency
{
    private readonly IStringEncryptionService _encryptionService;
    private readonly ICurrentTenant _currentTenant;
    private readonly AiModelGatewayService _gatewayService;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IRepository<AiModel, Guid> _modelRepository;
    private readonly IRepository<AiProvider, Guid> _providerRepository;
    private readonly ISettingProvider _settingProvider;

    public AiProviderService(
        IStringEncryptionService encryptionService,
        ICurrentTenant currentTenant,
        AiModelGatewayService gatewayService,
        IGuidGenerator guidGenerator,
        IRepository<AiModel, Guid> modelRepository,
        IRepository<AiProvider, Guid> providerRepository,
        ISettingProvider settingProvider)
    {
        _encryptionService = encryptionService;
        _currentTenant = currentTenant;
        _gatewayService = gatewayService;
        _guidGenerator = guidGenerator;
        _modelRepository = modelRepository;
        _providerRepository = providerRepository;
        _settingProvider = settingProvider;
    }

    public async Task<List<AiProviderDto>> GetProvidersAsync()
    {
        await EnsureBuiltInModelsForEnabledProvidersAsync();
        var providers = (await _providerRepository.GetListAsync())
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.Name)
            .ToList();
        var models = await _modelRepository.GetListAsync();
        var modelCounts = models
            .GroupBy(x => x.ProviderId)
            .ToDictionary(x => x.Key, x => x.Count());

        return providers
            .Select(item => MapProvider(item, modelCounts.GetValueOrDefault(item.Id)))
            .ToList();
    }

    public async Task<AiProviderDto> CreateProviderAsync(AiProviderSaveInput input)
    {
        EnsureProviderProtocol(input.ProviderType);
        await EnsureEndpointAllowedAsync(input.Endpoint);
        if (input.IsDefault)
        {
            await ClearDefaultProvidersAsync();
        }

        var entity = new AiProvider(
            _guidGenerator.Create(),
            input.Name,
            input.ProviderType,
            input.Endpoint,
            EncryptApiKey(input.ApiKey),
            _currentTenant.Id,
            input.IsEnabled,
            input.IsDefault,
            input.OrganizationId,
            input.ConfigJson,
            input.Remark);

        await _providerRepository.InsertAsync(entity, autoSave: true);
        await EnsureBuiltInModelsAsync(entity);
        var modelCount = await _modelRepository.CountAsync(x => x.ProviderId == entity.Id);
        return MapProvider(entity, (int)modelCount);
    }

    public async Task<AiProviderDto> UpdateProviderAsync(Guid id, AiProviderSaveInput input)
    {
        var entity = await _providerRepository.GetAsync(id);
        EnsureProviderProtocol(input.ProviderType);
        await EnsureEndpointAllowedAsync(input.Endpoint);
        if (input.IsDefault)
        {
            await ClearDefaultProvidersAsync(id);
        }

        entity.Update(
            input.Name,
            input.ProviderType,
            input.Endpoint,
            input.IsEnabled,
            input.IsDefault,
            input.OrganizationId,
            input.ConfigJson,
            input.Remark);

        if (!string.IsNullOrWhiteSpace(input.ApiKey))
        {
            entity.ResetApiKey(EncryptApiKey(input.ApiKey));
        }

        await _providerRepository.UpdateAsync(entity, autoSave: true);
        await EnsureBuiltInModelsAsync(entity);
        var modelCount = await _modelRepository.CountAsync(x => x.ProviderId == id);
        return MapProvider(entity, (int)modelCount);
    }

    public async Task DeleteProviderAsync(Guid id)
    {
        var models = await _modelRepository.GetListAsync(x => x.ProviderId == id);
        if (models.Count > 0)
        {
            await _modelRepository.DeleteManyAsync(models, autoSave: true);
        }

        await _providerRepository.DeleteAsync(id, autoSave: true);
    }

    public async Task<AiProviderTestResultDto> TestProviderAsync(Guid id)
    {
        var provider = await _providerRepository.GetAsync(id);
        if (!provider.IsEnabled)
        {
            return new AiProviderTestResultDto { Success = false, Message = "供应商已停用，请先启用后再测试。" };
        }

        if (string.IsNullOrWhiteSpace(provider.Endpoint))
        {
            return new AiProviderTestResultDto { Success = false, Message = "供应商 Endpoint 不能为空。" };
        }

        if (!Uri.TryCreate(provider.Endpoint, UriKind.Absolute, out var endpoint) ||
            (endpoint.Scheme != Uri.UriSchemeHttp && endpoint.Scheme != Uri.UriSchemeHttps))
        {
            return new AiProviderTestResultDto { Success = false, Message = "供应商 Endpoint 必须是合法的 HTTP/HTTPS 地址。" };
        }

        if (!await IsEndpointAllowedAsync(endpoint))
        {
            return new AiProviderTestResultDto { Success = false, Message = "当前私有化部署策略不允许连接公网模型 Provider，请使用内网或本地模型网关。" };
        }

        if (!provider.HasApiKey())
        {
            return new AiProviderTestResultDto { Success = false, Message = "供应商密钥未配置。" };
        }

        var model = (await _modelRepository.GetListAsync(x => x.ProviderId == id && x.IsEnabled))
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.Sort)
            .ThenBy(x => x.DisplayName)
            .FirstOrDefault();
        if (model == null)
        {
            await EnsureBuiltInModelsAsync(provider);
            model = (await _modelRepository.GetListAsync(x => x.ProviderId == id && x.IsEnabled))
                .OrderByDescending(x => x.IsDefault)
                .ThenBy(x => x.Sort)
                .ThenBy(x => x.DisplayName)
                .FirstOrDefault();
        }

        var result = await _gatewayService.TestProviderAsync(provider, model);
        return new AiProviderTestResultDto
        {
            Success = result.Success,
            Message = result.Message
        };
    }

    public async Task<AiProviderModelDiscoveryResultDto> DiscoverModelsAsync(Guid id)
    {
        var provider = await _providerRepository.GetAsync(id);
        if (!provider.IsEnabled)
        {
            return new AiProviderModelDiscoveryResultDto { Success = false, Message = "供应商已停用，请先启用后再获取模型。" };
        }

        if (!AiProviderProtocolNames.IsSupported(provider.ProviderType))
        {
            return new AiProviderModelDiscoveryResultDto { Success = false, Message = "供应商协议仅支持 OpenAI Responses 或 Anthropic Messages。" };
        }

        if (!provider.HasApiKey())
        {
            return new AiProviderModelDiscoveryResultDto { Success = false, Message = "供应商密钥未配置。" };
        }

        return await _gatewayService.DiscoverModelsAsync(provider);
    }

    public async Task<AiProviderModelDiscoveryResultDto> SyncModelsAsync(Guid id)
    {
        var provider = await _providerRepository.GetAsync(id);
        if (!provider.IsEnabled)
        {
            return new AiProviderModelDiscoveryResultDto { Success = false, Message = "供应商已停用，请先启用后再同步模型。" };
        }

        var discovery = await DiscoverModelsAsync(id);
        if (!discovery.Success)
        {
            return discovery;
        }

        var syncResult = await UpsertProviderModelsAsync(provider, discovery.Models);
        discovery.CreatedCount = syncResult.CreatedCount;
        discovery.UpdatedCount = syncResult.UpdatedCount;
        discovery.Message = $"{discovery.Message} 已同步 {syncResult.CreatedCount + syncResult.UpdatedCount} 个模型。";
        return discovery;
    }

    public async Task<List<AiModelDto>> GetModelsAsync(Guid? providerId = null)
    {
        await EnsureBuiltInModelsForEnabledProvidersAsync(providerId);
        var providers = (await _providerRepository.GetListAsync())
            .ToDictionary(x => x.Id, x => x.Name);
        var allModels = await _modelRepository.GetListAsync();
        var fallbackNames = allModels.ToDictionary(x => x.Id, x => x.DisplayName);
        var models = providerId.HasValue
            ? allModels.Where(x => x.ProviderId == providerId.Value).ToList()
            : allModels;

        return models
            .OrderBy(x => providers.GetValueOrDefault(x.ProviderId, string.Empty))
            .ThenBy(x => x.Sort)
            .ThenBy(x => x.DisplayName)
            .Select(item => MapModel(
                item,
                providers.GetValueOrDefault(item.ProviderId, string.Empty),
                item.FallbackModelId.HasValue ? fallbackNames.GetValueOrDefault(item.FallbackModelId.Value, string.Empty) : string.Empty))
            .ToList();
    }

    public async Task<AiModelDto> CreateModelAsync(AiModelSaveInput input)
    {
        var provider = await _providerRepository.GetAsync(input.ProviderId);
        var modelId = _guidGenerator.Create();
        var fallbackModelId = await ResolveFallbackModelIdAsync(input.FallbackModelId, modelId);
        var capabilities = input.Capabilities.ToList();
        EnsureModelCapabilities(capabilities);
        if (input.IsDefault)
        {
            await ClearDefaultModelsAsync(input.ProviderId);
        }

        var entity = new AiModel(
            modelId,
            input.ProviderId,
            provider.TenantId,
            input.ModelCode,
            input.DisplayName,
            input.ModelType,
            input.IsEnabled,
            input.IsDefault,
            input.ContextWindow,
            input.MaxOutputTokens,
            fallbackModelId,
            SerializeCapabilities(capabilities),
            input.InputTokenPrice,
            input.OutputTokenPrice,
            input.Currency,
            input.Sort,
            input.Remark);

        await _modelRepository.InsertAsync(entity, autoSave: true);
        var fallbackModelName = fallbackModelId.HasValue
            ? (await _modelRepository.GetAsync(fallbackModelId.Value)).DisplayName
            : string.Empty;
        return MapModel(entity, provider.Name, fallbackModelName);
    }

    public async Task<AiModelDto> UpdateModelAsync(Guid id, AiModelSaveInput input)
    {
        var entity = await _modelRepository.GetAsync(id);
        if (entity.ProviderId != input.ProviderId)
        {
            throw new UserFriendlyException("模型暂不支持切换供应商，请删除后重新创建。");
        }

        var provider = await _providerRepository.GetAsync(input.ProviderId);
        var fallbackModelId = await ResolveFallbackModelIdAsync(input.FallbackModelId, id);
        var capabilities = input.Capabilities.ToList();
        EnsureModelCapabilities(capabilities);
        if (input.IsDefault)
        {
            await ClearDefaultModelsAsync(input.ProviderId, id);
        }

        entity.Update(
            input.ModelCode,
            input.DisplayName,
            input.ModelType,
            input.IsEnabled,
            input.IsDefault,
            input.ContextWindow,
            input.MaxOutputTokens,
            fallbackModelId,
            SerializeCapabilities(capabilities),
            input.InputTokenPrice,
            input.OutputTokenPrice,
            input.Currency,
            input.Sort,
            input.Remark);

        await _modelRepository.UpdateAsync(entity, autoSave: true);
        var fallbackModelName = fallbackModelId.HasValue
            ? (await _modelRepository.GetAsync(fallbackModelId.Value)).DisplayName
            : string.Empty;
        return MapModel(entity, provider.Name, fallbackModelName);
    }

    public async Task DeleteModelAsync(Guid id)
    {
        await _modelRepository.DeleteAsync(id, autoSave: true);
    }

    private async Task EnsureBuiltInModelsAsync(AiProvider provider)
    {
        var builtInModels = _gatewayService.GetBuiltInModels(provider);
        if (builtInModels.Count == 0)
        {
            return;
        }

        await UpsertProviderModelsAsync(provider, builtInModels);
    }

    private async Task EnsureBuiltInModelsForEnabledProvidersAsync(Guid? providerId = null)
    {
        var providers = providerId.HasValue
            ? await _providerRepository.GetListAsync(x => x.Id == providerId.Value && x.IsEnabled)
            : await _providerRepository.GetListAsync(x => x.IsEnabled);
        foreach (var provider in providers)
        {
            await EnsureBuiltInModelsAsync(provider);
        }
    }

    private async Task<(int CreatedCount, int UpdatedCount)> UpsertProviderModelsAsync(
        AiProvider provider,
        IReadOnlyCollection<AiDiscoveredModelDto> discoveredModels)
    {
        var normalizedModels = discoveredModels
            .Where(x => !string.IsNullOrWhiteSpace(x.ModelCode))
            .GroupBy(x => x.ModelCode.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(x => x.First())
            .OrderBy(x => x.ModelCode, StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (normalizedModels.Count == 0)
        {
            return (0, 0);
        }

        var existingModels = await _modelRepository.GetListAsync(x => x.ProviderId == provider.Id);
        var existingByCode = existingModels
            .GroupBy(x => x.ModelCode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);
        var hasDefault = existingModels.Any(x => x.IsDefault);
        var nextSort = existingModels.Count == 0 ? 0 : existingModels.Max(x => x.Sort) + 10;
        var createdModels = new List<AiModel>();
        var updatedModels = new List<AiModel>();

        for (var index = 0; index < normalizedModels.Count; index++)
        {
            var item = normalizedModels[index];
            var modelCode = item.ModelCode.Trim();
            var displayName = string.IsNullOrWhiteSpace(item.DisplayName) ? modelCode : item.DisplayName.Trim();
            var modelType = string.IsNullOrWhiteSpace(item.ModelType) ? "standard" : item.ModelType.Trim();
            var isDefault = !hasDefault && index == 0;
            if (existingByCode.TryGetValue(modelCode, out var existing))
            {
                var capabilities = DeserializeCapabilities(existing.CapabilitiesJson);
                EnsureModelCapabilities(capabilities);
                var nextCapabilitiesJson = SerializeCapabilities(capabilities);
                var nextIsDefault = existing.IsDefault || isDefault;
                var nextContextWindow = Math.Max(existing.ContextWindow, ResolveDefaultContextWindow(provider));
                var nextMaxOutputTokens = Math.Max(existing.MaxOutputTokens, ResolveDefaultMaxOutputTokens(provider));
                var shouldUpdate =
                    !existing.IsEnabled ||
                    existing.IsDefault != nextIsDefault ||
                    existing.ContextWindow != nextContextWindow ||
                    existing.MaxOutputTokens != nextMaxOutputTokens ||
                    !existing.DisplayName.Equals(displayName, StringComparison.Ordinal) ||
                    !existing.ModelType.Equals(modelType, StringComparison.Ordinal) ||
                    !existing.CapabilitiesJson.Equals(nextCapabilitiesJson, StringComparison.Ordinal);

                if (shouldUpdate)
                {
                    existing.Update(
                        modelCode,
                        displayName,
                        modelType,
                        isEnabled: true,
                        isDefault: nextIsDefault,
                        contextWindow: nextContextWindow,
                        maxOutputTokens: nextMaxOutputTokens,
                        fallbackModelId: existing.FallbackModelId,
                        capabilitiesJson: nextCapabilitiesJson,
                        inputTokenPrice: existing.InputTokenPrice,
                        outputTokenPrice: existing.OutputTokenPrice,
                        currency: existing.Currency,
                        sort: existing.Sort,
                        remark: existing.Remark);
                    updatedModels.Add(existing);
                }

                hasDefault = hasDefault || nextIsDefault;
                continue;
            }

            var model = new AiModel(
                _guidGenerator.Create(),
                provider.Id,
                provider.TenantId,
                modelCode,
                displayName,
                modelType,
                isEnabled: true,
                isDefault: isDefault,
                contextWindow: ResolveDefaultContextWindow(provider),
                maxOutputTokens: ResolveDefaultMaxOutputTokens(provider),
                capabilitiesJson: SerializeCapabilities(["streaming", "textGeneration", "toolCalling"]),
                currency: "USD",
                sort: nextSort);
            nextSort += 10;
            createdModels.Add(model);
            hasDefault = hasDefault || isDefault;
        }

        if (createdModels.Count > 0)
        {
            await _modelRepository.InsertManyAsync(createdModels, autoSave: true);
        }

        if (updatedModels.Count > 0)
        {
            await _modelRepository.UpdateManyAsync(updatedModels, autoSave: true);
        }

        return (createdModels.Count, updatedModels.Count);
    }

    private static void EnsureModelCapabilities(List<string> capabilities)
    {
        capabilities.RemoveAll(x => x.Equals("chat", StringComparison.OrdinalIgnoreCase));

        if (!capabilities.Contains("textGeneration", StringComparer.OrdinalIgnoreCase))
        {
            capabilities.Add("textGeneration");
        }

        if (!capabilities.Contains("streaming", StringComparer.OrdinalIgnoreCase))
        {
            capabilities.Add("streaming");
        }

        if (!capabilities.Contains("toolCalling", StringComparer.OrdinalIgnoreCase))
        {
            capabilities.Add("toolCalling");
        }
    }

    private static int ResolveDefaultContextWindow(AiProvider provider)
    {
        return IsDeepSeekProvider(provider) ? 1_000_000 : 256_000;
    }

    private static int ResolveDefaultMaxOutputTokens(AiProvider provider)
    {
        return IsDeepSeekProvider(provider) ? 384_000 : 4096;
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

    private async Task ClearDefaultProvidersAsync(Guid? exceptId = null)
    {
        var defaults = await _providerRepository.GetListAsync(x => x.IsDefault && (!exceptId.HasValue || x.Id != exceptId.Value));
        foreach (var item in defaults)
        {
            item.ClearDefault();
        }

        if (defaults.Count > 0)
        {
            await _providerRepository.UpdateManyAsync(defaults, autoSave: true);
        }
    }

    private static void EnsureProviderProtocol(string providerType)
    {
        if (!AiProviderProtocolNames.IsSupported(providerType))
        {
            throw new UserFriendlyException("供应商协议仅支持 OpenAI Responses 或 Anthropic Messages。");
        }
    }

    private async Task EnsureEndpointAllowedAsync(string endpoint)
    {
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return;
        }

        if (!await IsEndpointAllowedAsync(uri))
        {
            throw new UserFriendlyException("当前私有化部署策略不允许配置公网模型 Provider，请使用内网或本地模型网关。");
        }
    }

    private async Task<bool> IsEndpointAllowedAsync(Uri endpoint)
    {
        if (await _settingProvider.GetAsync<bool>(AdminSettings.AiAllowPublicProviderEndpoints))
        {
            return true;
        }

        return IsPrivateEndpoint(endpoint);
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

    private async Task ClearDefaultModelsAsync(Guid providerId, Guid? exceptId = null)
    {
        var defaults = await _modelRepository.GetListAsync(x =>
            x.ProviderId == providerId && x.IsDefault && (!exceptId.HasValue || x.Id != exceptId.Value));
        foreach (var item in defaults)
        {
            item.ClearDefault();
        }

        if (defaults.Count > 0)
        {
            await _modelRepository.UpdateManyAsync(defaults, autoSave: true);
        }
    }

    private async Task<Guid?> ResolveFallbackModelIdAsync(Guid? fallbackModelId, Guid modelId)
    {
        if (!fallbackModelId.HasValue)
        {
            return null;
        }

        if (fallbackModelId.Value == modelId)
        {
            throw new UserFriendlyException("降级模型不能选择自身。");
        }

        var fallbackModel = await _modelRepository.GetAsync(fallbackModelId.Value);
        if (!fallbackModel.IsEnabled)
        {
            throw new UserFriendlyException("降级模型必须处于启用状态。");
        }

        return fallbackModel.Id;
    }

    private string EncryptApiKey(string? apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return string.Empty;
        }

        return _encryptionService.Encrypt(apiKey.Trim()) ?? throw new InvalidOperationException("AI 供应商密钥加密失败。");
    }

    private static AiProviderDto MapProvider(AiProvider item, int modelCount)
    {
        return new AiProviderDto
        {
            CreationTime = item.CreationTime,
            Endpoint = item.Endpoint,
            HasApiKey = item.HasApiKey(),
            Id = item.Id,
            IsDefault = item.IsDefault,
            IsEnabled = item.IsEnabled,
            LastModificationTime = item.LastModificationTime,
            ModelCount = modelCount,
            Name = item.Name,
            OrganizationId = item.OrganizationId,
            ProviderType = item.ProviderType,
            Remark = item.Remark
        };
    }

    private static AiModelDto MapModel(AiModel item, string providerName, string fallbackModelName)
    {
        return new AiModelDto
        {
            Capabilities = DeserializeCapabilities(item.CapabilitiesJson),
            ContextWindow = item.ContextWindow,
            Currency = item.Currency,
            DisplayName = item.DisplayName,
            FallbackModelId = item.FallbackModelId,
            FallbackModelName = fallbackModelName,
            Id = item.Id,
            InputTokenPrice = item.InputTokenPrice,
            IsDefault = item.IsDefault,
            IsEnabled = item.IsEnabled,
            MaxOutputTokens = item.MaxOutputTokens,
            ModelCode = item.ModelCode,
            ModelType = item.ModelType,
            OutputTokenPrice = item.OutputTokenPrice,
            ProviderId = item.ProviderId,
            ProviderName = providerName,
            Remark = item.Remark,
            Sort = item.Sort
        };
    }

    private static string SerializeCapabilities(IEnumerable<string> capabilities)
    {
        var normalized = capabilities
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return JsonSerializer.Serialize(normalized);
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
