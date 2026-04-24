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
        var rawConnectionString = configuration.GetConnectionString("Default");
        var provider = DatabaseProviderDetector.Detect(rawConnectionString);

        var builder = new DbContextOptionsBuilder<AdminDbContext>();

        if (provider == DatabaseProvider.PostgreSql)
        {
            builder.UseNpgsql(rawConnectionString);
        }
        else
        {
            builder.UseSqlite(SqliteConnectionStringHelper.Normalize(rawConnectionString));
        }

        return new AdminDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../General.Admin.DbMigrator/");

        var builder = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsoncAppSettingsDirectory(basePath); // 加载 appsettings/*.jsonc

        return builder.Build();
    }
}
