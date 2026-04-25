using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using Npgsql;
using General.Admin.Data;
using Volo.Abp.DependencyInjection;

namespace General.Admin.EntityFrameworkCore;

public class EntityFrameworkCoreAdminDbSchemaMigrator
    : IAdminDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreAdminDbSchemaMigrator(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        var dbContext = _serviceProvider.GetRequiredService<AdminDbContext>();
        var connectionString = dbContext.Database.GetConnectionString();
        var provider = DatabaseProviderDetector.DetectFromEfProviderName(dbContext.Database.ProviderName);
        var migrationsAssembly = typeof(AdminDbContext).Assembly.GetName().Name!;

        if (provider == DatabaseProvider.PostgreSql)
        {
            await EnsurePostgreSqlDatabaseExistsAsync(connectionString!);
        }
        else if (provider == DatabaseProvider.MySql)
        {
            await EnsureMySqlDatabaseExistsAsync(connectionString!);
        }

        switch (provider)
        {
            case DatabaseProvider.Sqlite:
                await dbContext.Database.MigrateAsync();
                break;

            case DatabaseProvider.PostgreSql:
                await using (var postgreSqlDbContext = CreateMigrationsDbContext<AdminPostgreSqlMigrationsDbContext>(
                                 DatabaseProvider.PostgreSql,
                                 connectionString!,
                                 migrationsAssembly))
                {
                    await postgreSqlDbContext.Database.MigrateAsync();
                }
                break;

            case DatabaseProvider.MySql:
                await using (var mySqlDbContext = CreateMigrationsDbContext<AdminMySqlMigrationsDbContext>(
                                 DatabaseProvider.MySql,
                                 connectionString!,
                                 migrationsAssembly))
                {
                    await mySqlDbContext.Database.MigrateAsync();
                }
                break;

            default:
                await dbContext.Database.MigrateAsync();
                break;
        }
    }

    private static TDbContext CreateMigrationsDbContext<TDbContext>(
        DatabaseProvider provider,
        string connectionString,
        string migrationsAssembly)
        where TDbContext : DbContext
    {
        var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();
        AdminDbContextOptionsConfigurer.Configure(optionsBuilder, provider, connectionString, migrationsAssembly);
        return (TDbContext)Activator.CreateInstance(typeof(TDbContext), optionsBuilder.Options)!;
    }

    private static async Task EnsurePostgreSqlDatabaseExistsAsync(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var dbName = builder.Database ?? "general_admin";

        builder.Database = "postgres";
        await using var conn = new NpgsqlConnection(builder.ConnectionString);
        await conn.OpenAsync();

        await using var checkCmd = conn.CreateCommand();
        checkCmd.CommandText = "SELECT 1 FROM pg_database WHERE datname = @name";
        checkCmd.Parameters.AddWithValue("@name", dbName);
        var exists = await checkCmd.ExecuteScalarAsync();

        if (exists == null)
        {
            var safeName = dbName.Replace("\"", "\"\"");
            await using var createCmd = conn.CreateCommand();
            createCmd.CommandText = $"CREATE DATABASE \"{safeName}\"";
            await createCmd.ExecuteNonQueryAsync();
        }
    }

    private static async Task EnsureMySqlDatabaseExistsAsync(string connectionString)
    {
        var builder = new MySqlConnectionStringBuilder(connectionString);
        var dbName = builder.Database;
        if (string.IsNullOrWhiteSpace(dbName))
        {
            throw new InvalidOperationException("MySQL connection string must contain a database name.");
        }

        builder.Database = string.Empty;
        await using var conn = new MySqlConnection(builder.ConnectionString);
        await conn.OpenAsync();

        var safeName = dbName.Replace("`", "``", StringComparison.Ordinal);
        await using var createCmd = conn.CreateCommand();
        createCmd.CommandText = $"CREATE DATABASE IF NOT EXISTS `{safeName}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;";
        await createCmd.ExecuteNonQueryAsync();
    }
}
