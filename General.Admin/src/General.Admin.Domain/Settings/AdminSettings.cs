namespace General.Admin.Settings;

public static class AdminSettings
{
    private const string Prefix = "Admin";
    public const string PlatformPrefix = Prefix + ".Platform";

    public const string SystemName = PlatformPrefix + ".SystemName";
    public const string LoginPageTitle = PlatformPrefix + ".LoginPageTitle";
    public const string DefaultPageSize = PlatformPrefix + ".DefaultPageSize";
    public const string SchedulerRecordKeepLastN = PlatformPrefix + ".SchedulerRecordKeepLastN";
    public const string AuditLogRetentionDays = PlatformPrefix + ".AuditLogRetentionDays";
    public const string ProjectEvmWorklogHourlyRate = PlatformPrefix + ".ProjectEvmWorklogHourlyRate";
    public const string AiAllowPublicProviderEndpoints = PlatformPrefix + ".AiAllowPublicProviderEndpoints";
    public const string IntegrationWebhookIpWhitelist = PlatformPrefix + ".IntegrationWebhookIpWhitelist";
    public const string IntegrationWebhookRequestLimitPerMinute = PlatformPrefix + ".IntegrationWebhookRequestLimitPerMinute";
    public const string IntegrationMessageSendLimitPerMinute = PlatformPrefix + ".IntegrationMessageSendLimitPerMinute";
}
