using Volo.Abp.Modularity;

namespace General.Admin;

[DependsOn(
    typeof(AdminDomainModule),
    typeof(AdminTestBaseModule)
)]
public class AdminDomainTestModule : AbpModule
{

}
