using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace General.Admin.EntityFrameworkCore;

public class AdminDbContextFactory : IDesignTimeDbContextFactory<AdminDbContext>
{
    public AdminDbContext CreateDbContext(string[] args)
    {
        var configuration = AdminDesignTimeDbContextFactoryHelper.BuildConfiguration();
        var provider = DatabaseProviderDetector.Detect(configuration);
        var optionsBuilder = new DbContextOptionsBuilder<AdminDbContext>();
        AdminDbContextOptionsConfigurer.Configure(optionsBuilder, provider, configuration.GetConnectionString("Default"));
        return new AdminDbContext(optionsBuilder.Options);
    }
}
