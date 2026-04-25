using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace General.Admin.EntityFrameworkCore;

public class AdminSqliteMigrationsDbContextFactory : IDesignTimeDbContextFactory<AdminSqliteMigrationsDbContext>
{
    public AdminSqliteMigrationsDbContext CreateDbContext(string[] args)
    {
        var configuration = AdminDesignTimeDbContextFactoryHelper.BuildConfiguration();
        var optionsBuilder = new DbContextOptionsBuilder<AdminSqliteMigrationsDbContext>();
        AdminDbContextOptionsConfigurer.Configure(optionsBuilder, DatabaseProvider.Sqlite, configuration.GetConnectionString("Default"));
        return new AdminSqliteMigrationsDbContext(optionsBuilder.Options);
    }
}
