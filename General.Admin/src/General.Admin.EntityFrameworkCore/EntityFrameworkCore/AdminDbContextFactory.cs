using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace General.Admin.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class AdminDbContextFactory : IDesignTimeDbContextFactory<AdminDbContext>
{
    public AdminDbContext CreateDbContext(string[] args)
    {
        AdminEfCoreEntityExtensionMappings.Configure();

        var configuration = BuildConfiguration();
        var connectionString = SqliteConnectionStringHelper.Normalize(
            configuration.GetConnectionString("Default"));

        var builder = new DbContextOptionsBuilder<AdminDbContext>()
            .UseSqlite(connectionString);

        return new AdminDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../General.Admin.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}
