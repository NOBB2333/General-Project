using System.Text.Json;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Encryption;

namespace General.Admin.Integration;

public class IntegrationConnectorService : ITransientDependency
{
    private readonly IRepository<IntegrationConnector, Guid> _connectorRepository;
    private readonly IRepository<IntegrationTenantConfig, Guid> _configRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IStringEncryptionService _encryptionService;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IntegrationConnectorRegistry _registry;
    private readonly IEnumerable<IIntegrationConnectorAdapter> _adapters;

    public IntegrationConnectorService(
        IRepository<IntegrationConnector, Guid> connectorRepository,
        IRepository<IntegrationTenantConfig, Guid> configRepository,
        ICurrentTenant currentTenant,
        IStringEncryptionService encryptionService,
        IGuidGenerator guidGenerator,
        IntegrationConnectorRegistry registry,
        IEnumerable<IIntegrationConnectorAdapter> adapters)
    {
        _connectorRepository = connectorRepository;
        _configRepository = configRepository;
        _currentTenant = currentTenant;
        _encryptionService = encryptionService;
        _guidGenerator = guidGenerator;
        _registry = registry;
        _adapters = adapters;
    }

    public async Task<List<IntegrationConnectorDto>> GetConnectorsAsync(IntegrationConnectorQueryInput input)
    {
        await EnsureBuiltinConnectorsAsync();
        var connectors = (await _connectorRepository.GetListAsync())
            .Where(x => input.IncludeDisabled || x.IsEnabled)
            .ToList();

        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            var keyword = input.Keyword.Trim();
            connectors = connectors
                .Where(x =>
                    x.DisplayName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    x.ConnectorCode.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    x.ProviderType.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var configs = (await _configRepository.GetListAsync())
            .GroupBy(x => x.ConnectorId)
            .ToDictionary(x => x.Key, x => x.OrderByDescending(item => item.CreationTime).First());

        return connectors
            .OrderBy(x => x.Sort)
            .ThenBy(x => x.DisplayName)
            .Select(x => MapConnector(x, configs.GetValueOrDefault(x.Id)))
            .ToList();
    }

    public async Task<IntegrationConnectorHealthSummaryDto> GetHealthAsync()
    {
        var connectors = await GetConnectorsAsync(new IntegrationConnectorQueryInput
        {
            IncludeDisabled = true
        });
        var items = connectors.Select(MapHealth).ToList();

        return new IntegrationConnectorHealthSummaryDto
        {
            DisabledCount = items.Count(x => x.HealthStatus == "Disabled"),
            HealthyCount = items.Count(x => x.HealthStatus == "Healthy"),
            Items = items,
            TotalCount = items.Count,
            UnconfiguredCount = items.Count(x => x.HealthStatus == "Unconfigured"),
            UnhealthyCount = items.Count(x => x.HealthStatus == "Unhealthy"),
            WarningCount = items.Count(x => x.HealthStatus == "Warning")
        };
    }

    public async Task<IntegrationConnectorDto> SaveTenantConfigAsync(Guid connectorId, IntegrationTenantConfigSaveInput input)
    {
        var connector = await _connectorRepository.GetAsync(connectorId);
        ValidateJson(input.ConfigJson);

        var config = await _configRepository.FindAsync(x => x.ConnectorId == connectorId);
        if (config == null)
        {
            config = new IntegrationTenantConfig(
                _guidGenerator.Create(),
                connectorId,
                _currentTenant.Id,
                input.ConfigJson,
                EncryptSecret(input.Secret),
                input.IsEnabled,
                input.CallbackPath,
                input.Remark);
            await _configRepository.InsertAsync(config, autoSave: true);
        }
        else
        {
            config.Update(input.ConfigJson, input.IsEnabled, input.CallbackPath, input.Remark);
            if (!string.IsNullOrWhiteSpace(input.Secret))
            {
                config.ResetSecret(EncryptSecret(input.Secret));
            }

            await _configRepository.UpdateAsync(config, autoSave: true);
        }

        return MapConnector(connector, config);
    }

    public async Task<IntegrationConnectorDto> ToggleTenantConfigAsync(Guid connectorId, bool isEnabled)
    {
        var connector = await _connectorRepository.GetAsync(connectorId);
        var config = await _configRepository.FindAsync(x => x.ConnectorId == connectorId);
        if (config == null)
        {
            config = new IntegrationTenantConfig(
                _guidGenerator.Create(),
                connectorId,
                _currentTenant.Id,
                "{}",
                string.Empty,
                isEnabled);
            await _configRepository.InsertAsync(config, autoSave: true);
        }
        else
        {
            config.Toggle(isEnabled);
            await _configRepository.UpdateAsync(config, autoSave: true);
        }

        return MapConnector(connector, config);
    }

    public async Task<IntegrationConnectorDto> RotateSecretAsync(Guid connectorId, IntegrationSecretRotateInput input)
    {
        var connector = await _connectorRepository.GetAsync(connectorId);
        var config = await _configRepository.FindAsync(x => x.ConnectorId == connectorId)
            ?? throw new UserFriendlyException("尚未保存租户级连接配置，不能执行密钥轮换。");

        if (string.IsNullOrWhiteSpace(input.Secret))
        {
            throw new UserFriendlyException("新密钥不能为空。");
        }

        config.ResetSecret(EncryptSecret(input.Secret));
        await _configRepository.UpdateAsync(config, autoSave: true);

        return MapConnector(connector, config);
    }

    public async Task<IntegrationConnectionTestResultDto> TestConnectionAsync(Guid connectorId)
    {
        var connector = await _connectorRepository.GetAsync(connectorId);
        var config = await _configRepository.FindAsync(x => x.ConnectorId == connectorId);
        var adapter = _adapters.FirstOrDefault(x =>
            x.ConnectorCode.Equals(connector.ConnectorCode, StringComparison.OrdinalIgnoreCase));
        var result = adapter == null
            ? BuildConnectionTestResult(connector, config)
            : await adapter.TestConnectionAsync(connector, config);
        if (config != null)
        {
            config.MarkTestResult(result.Success, result.Message);
            await _configRepository.UpdateAsync(config, autoSave: true);
        }

        return result;
    }

    private async Task EnsureBuiltinConnectorsAsync()
    {
        var existing = (await _connectorRepository.GetListAsync())
            .ToDictionary(x => x.ConnectorCode, StringComparer.OrdinalIgnoreCase);
        var toInsert = new List<IntegrationConnector>();
        var toUpdate = new List<IntegrationConnector>();

        foreach (var descriptor in _registry.GetBuiltinDescriptors())
        {
            var capabilitiesJson = SerializeCapabilities(descriptor.Capabilities);
            if (!existing.TryGetValue(descriptor.ConnectorCode, out var connector))
            {
                toInsert.Add(new IntegrationConnector(
                    _guidGenerator.Create(),
                    descriptor.ConnectorCode,
                    descriptor.ProviderType,
                    descriptor.DisplayName,
                    descriptor.ConfigSchemaJson,
                    capabilitiesJson,
                    _currentTenant.Id,
                    icon: descriptor.Icon,
                    sort: descriptor.Sort,
                    remark: descriptor.Remark));
                continue;
            }

            connector.UpdateDefinition(
                descriptor.ProviderType,
                descriptor.DisplayName,
                descriptor.ConfigSchemaJson,
                capabilitiesJson,
                connector.IsEnabled,
                descriptor.Icon,
                descriptor.Sort,
                descriptor.Remark);
            toUpdate.Add(connector);
        }

        if (toInsert.Count > 0)
        {
            await _connectorRepository.InsertManyAsync(toInsert, autoSave: true);
        }

        if (toUpdate.Count > 0)
        {
            await _connectorRepository.UpdateManyAsync(toUpdate, autoSave: true);
        }
    }

    private static IntegrationConnectionTestResultDto BuildConnectionTestResult(
        IntegrationConnector connector,
        IntegrationTenantConfig? config)
    {
        if (!connector.IsEnabled)
        {
            return new IntegrationConnectionTestResultDto
            {
                Success = false,
                Message = "连接器定义已停用，请先启用连接器。"
            };
        }

        if (config == null)
        {
            return new IntegrationConnectionTestResultDto
            {
                Success = false,
                Message = "尚未保存租户级连接配置。"
            };
        }

        if (!config.IsEnabled)
        {
            return new IntegrationConnectionTestResultDto
            {
                Success = false,
                Message = "租户级连接配置已停用。"
            };
        }

        if (!config.HasSecret())
        {
            return new IntegrationConnectionTestResultDto
            {
                Success = false,
                Message = "连接器密钥未配置。"
            };
        }

        return new IntegrationConnectionTestResultDto
        {
            Success = true,
            Message = "连接器基础配置完整。真实平台 Token 与消息接口将在对应连接器 MVP 中校验。"
        };
    }

    private string EncryptSecret(string? secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
        {
            return string.Empty;
        }

        return _encryptionService.Encrypt(secret.Trim()) ?? throw new InvalidOperationException("外部系统连接器密钥加密失败。");
    }

    private static void ValidateJson(string value)
    {
        try
        {
            using var _ = JsonDocument.Parse(string.IsNullOrWhiteSpace(value) ? "{}" : value);
        }
        catch (JsonException)
        {
            throw new UserFriendlyException("连接器配置 JSON 格式不正确。");
        }
    }

    private static IntegrationConnectorDto MapConnector(IntegrationConnector connector, IntegrationTenantConfig? config)
    {
        return new IntegrationConnectorDto
        {
            Capabilities = DeserializeCapabilities(connector.CapabilitiesJson),
            ConfigJson = config?.ConfigJson ?? "{}",
            ConfigSchemaJson = connector.ConfigSchemaJson,
            ConnectorCode = connector.ConnectorCode,
            DisplayName = connector.DisplayName,
            HasSecret = config?.HasSecret() ?? false,
            Icon = connector.Icon,
            Id = connector.Id,
            IsBuiltin = connector.IsBuiltin,
            IsConfigured = config != null,
            IsEnabled = connector.IsEnabled,
            IsTenantEnabled = config?.IsEnabled ?? false,
            LastTestMessage = config?.LastTestMessage ?? string.Empty,
            LastTestStatus = config?.LastTestStatus ?? "未测试",
            LastTestTime = config?.LastTestTime,
            ProviderType = connector.ProviderType,
            Remark = connector.Remark,
            Sort = connector.Sort,
            TenantConfigId = config?.Id
        };
    }

    private static IntegrationConnectorHealthDto MapHealth(IntegrationConnectorDto connector)
    {
        var (status, message) = ResolveHealth(connector);
        return new IntegrationConnectorHealthDto
        {
            ConnectorCode = connector.ConnectorCode,
            DisplayName = connector.DisplayName,
            HasSecret = connector.HasSecret,
            HealthMessage = message,
            HealthStatus = status,
            Id = connector.Id,
            IsConfigured = connector.IsConfigured,
            IsEnabled = connector.IsEnabled,
            IsTenantEnabled = connector.IsTenantEnabled,
            LastTestMessage = connector.LastTestMessage,
            LastTestStatus = connector.LastTestStatus,
            LastTestTime = connector.LastTestTime,
            ProviderType = connector.ProviderType
        };
    }

    private static (string Status, string Message) ResolveHealth(IntegrationConnectorDto connector)
    {
        if (!connector.IsEnabled)
        {
            return ("Disabled", "连接器定义已停用。");
        }

        if (!connector.IsConfigured)
        {
            return ("Unconfigured", "尚未保存租户级连接配置。");
        }

        if (!connector.IsTenantEnabled)
        {
            return ("Disabled", "租户级连接配置已停用。");
        }

        if (!connector.HasSecret)
        {
            return ("Unhealthy", "连接器密钥未配置。");
        }

        if (connector.LastTestStatus == "成功")
        {
            return ("Healthy", string.IsNullOrWhiteSpace(connector.LastTestMessage) ? "最近连接测试成功。" : connector.LastTestMessage);
        }

        if (connector.LastTestStatus == "失败")
        {
            return ("Unhealthy", string.IsNullOrWhiteSpace(connector.LastTestMessage) ? "最近连接测试失败。" : connector.LastTestMessage);
        }

        return ("Warning", string.IsNullOrWhiteSpace(connector.LastTestMessage)
            ? "连接器已配置但尚未完成连接测试。"
            : connector.LastTestMessage);
    }

    private static string SerializeCapabilities(IEnumerable<string> capabilities)
    {
        return JsonSerializer.Serialize(capabilities
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList());
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
