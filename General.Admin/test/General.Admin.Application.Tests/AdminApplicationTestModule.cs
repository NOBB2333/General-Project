using Volo.Abp.Modularity;

namespace General.Admin;

[DependsOn(
    typeof(AdminApplicationModule),
    typeof(AdminDomainTestModule)
)]
public class AdminApplicationTestModule : AbpModule
{

}
