using Xunit;

namespace General.Admin.EntityFrameworkCore;

[CollectionDefinition(AdminTestConsts.CollectionDefinitionName)]
public class AdminEntityFrameworkCoreCollection : ICollectionFixture<AdminEntityFrameworkCoreFixture>
{

}
