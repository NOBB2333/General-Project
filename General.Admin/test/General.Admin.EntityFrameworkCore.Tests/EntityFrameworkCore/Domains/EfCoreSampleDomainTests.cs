using General.Admin.Samples;
using Xunit;

namespace General.Admin.EntityFrameworkCore.Domains;

[Collection(AdminTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<AdminEntityFrameworkCoreTestModule>
{

}
