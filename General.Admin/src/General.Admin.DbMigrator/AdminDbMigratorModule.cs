using General.Admin.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace General.Admin.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AdminEntityFrameworkCoreModule),
    typeof(AdminApplicationContractsModule)
    )]
public class AdminDbMigratorModule : AbpModule
{
}
