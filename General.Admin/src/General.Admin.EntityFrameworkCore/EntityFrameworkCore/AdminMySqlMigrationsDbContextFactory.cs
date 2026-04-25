using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace General.Admin.EntityFrameworkCore;

public class AdminMySqlMigrationsDbContextFactory : IDesignTimeDbContextFactory<AdminMySqlMigrationsDbContext>
{
    public AdminMySqlMigrationsDbContext CreateDbContext(string[] args)
    {
        var configuration = AdminDesignTimeDbContextFactoryHelper.BuildConfiguration();
        var optionsBuilder = new DbContextOptionsBuilder<AdminMySqlMigrationsDbContext>();
        AdminDbContextOptionsConfigurer.Configure(optionsBuilder, DatabaseProvider.MySql, configuration.GetConnectionString("Default"));
        return new AdminMySqlMigrationsDbContext(optionsBuilder.Options);
    }
}
