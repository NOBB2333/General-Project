using General.Admin.Settings;
using Volo.Abp.DependencyInjection;
using Volo.Abp.SettingManagement;
using Volo.Abp.Settings;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Platform;

public class PlatformConfigService : ITransientDependency
{
    private static readonly IReadOnlyList<PlatformConfigDescriptor> ManagedConfigs =
    [
        new(AdminSettings.SystemName, "系统名称", "general", "General Admin", "string", "用于页面和浏览器标题展示。"),
        new(AdminSettings.LoginPageTitle, "登录页标题", "general", "General Admin", "string", "用于登录页展示。"),
        new(AdminSettings.DefaultPageSize, "默认分页数", "ui", "20", "number", "平台列表默认分页数量。"),
        new(AdminSettings.SchedulerRecordKeepLastN, "任务记录保留数", "scheduler", "100", "number", "执行记录清理时默认保留的最近记录数。"),
        new(AdminSettings.AuditLogRetentionDays, "日志保留天数", "audit", "30", "number", "日志清理任务默认保留天数。"),
        new(AdminSettings.ProjectEvmWorklogHourlyRate, "EVM 工时单价", "project", "100", "number", "项目绩效快照中用工时折算 AC 实际成本的默认单价。"),
        new(AdminSettings.IntegrationWebhookIpWhitelist, "外部回调 IP 白名单", "integration", string.Empty, "string", "Webhook 接收入口来源 IP 白名单，支持逗号、分号或换行分隔的 IP / CIDR；空值表示不限制。"),
        new(AdminSettings.IntegrationWebhookRequestLimitPerMinute, "外部回调每分钟请求上限", "integration", "120", "number", "Webhook 接收入口按来源和事件维度执行的每分钟请求频控；0 表示关闭。"),
        new(AdminSettings.IntegrationMessageSendLimitPerMinute, "外部消息每分钟发送上限", "integration", "60", "number", "外部系统消息投递按租户、连接器和接收目标执行的每分钟发送频控；0 表示关闭。")
    ];

    private readonly ISettingManager _settingManager;
    private readonly ISettingProvider _settingProvider;
    private readonly ICurrentTenant _currentTenant;

    public PlatformConfigService(
        ISettingManager settingManager,
        ISettingProvider settingProvider,
        ICurrentTenant currentTenant)
    {
        _settingManager = settingManager;
        _settingProvider = settingProvider;
        _currentTenant = currentTenant;
    }

    public async Task<List<PlatformConfigDto>> GetListAsync()
    {
        var result = new List<PlatformConfigDto>();
        foreach (var item in ManagedConfigs)
        {
            var tenantValue = _currentTenant.Id.HasValue
                ? await _settingManager.GetOrNullAsync(
                    item.Code,
                    TenantSettingValueProvider.ProviderName,
                    _currentTenant.Id.Value.ToString())
                : null;
            result.Add(new PlatformConfigDto
            {
                Code = item.Code,
                DefaultValue = item.DefaultValue,
                Description = item.Description,
                GroupCode = item.GroupCode,
                GroupName = ResolveGroupName(item.GroupCode),
                HasTenantValue = tenantValue != null,
                IsReadonly = false,
                Name = item.Name,
                ProviderName = tenantValue != null
                    ? TenantSettingValueProvider.ProviderName
                    : GlobalSettingValueProvider.ProviderName,
                ProviderKey = _currentTenant.Id?.ToString(),
                Value = tenantValue ?? await _settingProvider.GetOrNullAsync(item.Code) ?? item.DefaultValue,
                ValueType = item.ValueType
            });
        }

        return result
            .OrderBy(x => x.GroupCode)
            .ThenBy(x => x.Code)
            .ToList();
    }

    public async Task UpdateAsync(string code, PlatformConfigSaveInput input)
    {
        var descriptor = ManagedConfigs.FirstOrDefault(x => x.Code.Equals(code, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("配置项不存在或不允许在线修改。");
        var value = NormalizeValue(descriptor, input.Value);
        var providerName = NormalizeProviderName(input.ProviderName);
        var providerKey = NormalizeProviderKey(providerName, input.ProviderKey);
        await _settingManager.SetAsync(descriptor.Code, value, providerName, providerKey);
    }

    private static string NormalizeProviderName(string? providerName)
    {
        if (string.IsNullOrWhiteSpace(providerName))
        {
            return GlobalSettingValueProvider.ProviderName;
        }

        var normalized = providerName.Trim();
        if (normalized != GlobalSettingValueProvider.ProviderName &&
            normalized != TenantSettingValueProvider.ProviderName)
        {
            throw new InvalidOperationException("只允许修改全局配置或租户配置。");
        }

        return normalized;
    }

    private string? NormalizeProviderKey(string providerName, string? providerKey)
    {
        if (providerName == GlobalSettingValueProvider.ProviderName)
        {
            return null;
        }

        if (Guid.TryParse(providerKey, out var tenantId))
        {
            return tenantId.ToString();
        }

        if (_currentTenant.Id.HasValue)
        {
            return _currentTenant.Id.Value.ToString();
        }

        throw new InvalidOperationException("保存租户配置时必须指定租户。");
    }

    private static string ResolveGroupName(string groupCode)
    {
        return groupCode switch
        {
            "audit" => "审计日志",
            "general" => "通用",
            "integration" => "外部系统集成",
            "scheduler" => "定时任务",
            "ui" => "界面",
            _ => groupCode
        };
    }

    private static string NormalizeValue(PlatformConfigDescriptor descriptor, string? value)
    {
        var normalized = value?.Trim() ?? string.Empty;
        if (descriptor.ValueType == "number")
        {
            if (!int.TryParse(normalized, out var number) || number < 0)
            {
                throw new InvalidOperationException("配置值必须为非负整数。");
            }

            return number.ToString();
        }

        return string.IsNullOrWhiteSpace(normalized) ? descriptor.DefaultValue : normalized;
    }

    private sealed record PlatformConfigDescriptor(
        string Code,
        string Name,
        string GroupCode,
        string DefaultValue,
        string ValueType,
        string Description);
}
