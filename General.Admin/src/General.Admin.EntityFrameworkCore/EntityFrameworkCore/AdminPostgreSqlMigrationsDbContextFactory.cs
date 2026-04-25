using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace General.Admin.EntityFrameworkCore;

public class AdminPostgreSqlMigrationsDbContextFactory : IDesignTimeDbContextFactory<AdminPostgreSqlMigrationsDbContext>
{
    public AdminPostgreSqlMigrationsDbContext CreateDbContext(string[] args)
    {
        var configuration = AdminDesignTimeDbContextFactoryHelper.BuildConfiguration();
        var optionsBuilder = new DbContextOptionsBuilder<AdminPostgreSqlMigrationsDbContext>();
        AdminDbContextOptionsConfigurer.Configure(optionsBuilder, DatabaseProvider.PostgreSql, configuration.GetConnectionString("Default"));
        return new AdminPostgreSqlMigrationsDbContext(optionsBuilder.Options);
    }
}
