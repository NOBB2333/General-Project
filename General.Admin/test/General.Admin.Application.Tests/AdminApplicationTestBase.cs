using Volo.Abp.Modularity;

namespace General.Admin;

public abstract class AdminApplicationTestBase<TStartupModule> : AdminTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
