using Volo.Abp.Settings;

namespace General.Admin.Settings;

public class AdminSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(
            new SettingDefinition(AdminSettings.SystemName, "General Admin"),
            new SettingDefinition(AdminSettings.LoginPageTitle, "General Admin"),
            new SettingDefinition(AdminSettings.DefaultPageSize, "20"),
            new SettingDefinition(AdminSettings.SchedulerRecordKeepLastN, "100"),
            new SettingDefinition(AdminSettings.AuditLogRetentionDays, "30"),
            new SettingDefinition(AdminSettings.ProjectEvmWorklogHourlyRate, "100"),
            new SettingDefinition(AdminSettings.AiAllowPublicProviderEndpoints, "true"),
            new SettingDefinition(AdminSettings.IntegrationWebhookIpWhitelist, string.Empty),
            new SettingDefinition(AdminSettings.IntegrationWebhookRequestLimitPerMinute, "120"),
            new SettingDefinition(AdminSettings.IntegrationMessageSendLimitPerMinute, "60"));
    }
}
