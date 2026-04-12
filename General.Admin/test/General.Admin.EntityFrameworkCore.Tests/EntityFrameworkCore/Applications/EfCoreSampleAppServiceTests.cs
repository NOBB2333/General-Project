using General.Admin.Samples;
using Xunit;

namespace General.Admin.EntityFrameworkCore.Applications;

[Collection(AdminTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<AdminEntityFrameworkCoreTestModule>
{

}
