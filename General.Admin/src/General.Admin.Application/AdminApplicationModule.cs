using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Account;
using Volo.Abp.Mapperly;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;

namespace General.Admin;



/// <summary>
/// 三个定时任务
/// </summary>
[DependsOn(
    typeof(AdminDomainModule),
    typeof(AbpAccountApplicationModule),
    typeof(AdminApplicationContractsModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpSettingManagementApplicationModule)
    )]
public class AdminApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<AdminApplicationModule>();
        context.Services.AddTransient<IPlatformScheduledJobHandler, PlatformLogCleanupJobHandler>();
        context.Services.AddTransient<IPlatformScheduledJobHandler, ProjectWeeklyReportReminderJobHandler>();
        context.Services.AddTransient<IPlatformScheduledJobHandler, PlatformTenantHealthCheckJobHandler>();
    }
}
