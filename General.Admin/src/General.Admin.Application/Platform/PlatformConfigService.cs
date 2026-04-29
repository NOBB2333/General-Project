using General.Admin.Settings;
using Volo.Abp.DependencyInjection;
using Volo.Abp.SettingManagement;
using Volo.Abp.Settings;

namespace General.Admin.Platform;

public class PlatformConfigService : ITransientDependency
{
    private static readonly IReadOnlyList<PlatformConfigDescriptor> ManagedConfigs =
    [
        new(AdminSettings.SystemName, "系统名称", "general", "General Admin", "string", "用于页面和浏览器标题展示。"),
        new(AdminSettings.LoginPageTitle, "登录页标题", "general", "General Admin", "string", "用于登录页展示。"),
        new(AdminSettings.DefaultPageSize, "默认分页数", "ui", "20", "number", "平台列表默认分页数量。"),
        new(AdminSettings.SchedulerRecordKeepLastN, "任务记录保留数", "scheduler", "100", "number", "执行记录清理时默认保留的最近记录数。"),
        new(AdminSettings.AuditLogRetentionDays, "日志保留天数", "audit", "30", "number", "日志清理任务默认保留天数。")
    ];

    private readonly ISettingManager _settingManager;
    private readonly ISettingProvider _settingProvider;

    public PlatformConfigService(ISettingManager settingManager, ISettingProvider settingProvider)
    {
        _settingManager = settingManager;
        _settingProvider = settingProvider;
    }

    public async Task<List<PlatformConfigDto>> GetListAsync()
    {
        var result = new List<PlatformConfigDto>();
        foreach (var item in ManagedConfigs)
        {
            result.Add(new PlatformConfigDto
            {
                Code = item.Code,
                Description = item.Description,
                GroupCode = item.GroupCode,
                IsReadonly = false,
                Name = item.Name,
                Value = await _settingProvider.GetOrNullAsync(item.Code) ?? item.DefaultValue,
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
        await _settingManager.SetGlobalAsync(descriptor.Code, value);
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
