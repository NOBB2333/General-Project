using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
        /* We intentionally resolve the AdminDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        var dbContext = _serviceProvider.GetRequiredService<AdminDbContext>();

        var connectionString = dbContext.Database.GetConnectionString();
        if (DatabaseProviderDetector.Detect(connectionString) == DatabaseProvider.PostgreSql)
        {
            await EnsurePostgreSqlDatabaseExistsAsync(connectionString!);
        }

        await dbContext.Database.MigrateAsync();
    }

    /// <summary>
    /// 连接 postgres 维护库，若目标库不存在则自动创建，无需手动 CREATE DATABASE。
    /// </summary>
    private static async Task EnsurePostgreSqlDatabaseExistsAsync(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var dbName = builder.Database ?? "general_admin";

        // 连接到 postgres 维护库进行检查/创建
        builder.Database = "postgres";
        await using var conn = new NpgsqlConnection(builder.ConnectionString);
        await conn.OpenAsync();

        await using var checkCmd = conn.CreateCommand();
        checkCmd.CommandText = "SELECT 1 FROM pg_database WHERE datname = @name";
        checkCmd.Parameters.AddWithValue("@name", dbName);
        var exists = await checkCmd.ExecuteScalarAsync();

        if (exists == null)
        {
            // 数据库名做基本转义防注入
            var safeName = dbName.Replace("\"", "\"\"");
            await using var createCmd = conn.CreateCommand();
            createCmd.CommandText = $"CREATE DATABASE \"{safeName}\"";
            await createCmd.ExecuteNonQueryAsync();
            Console.WriteLine($"[DbMigrator] PostgreSQL 数据库已自动创建: {dbName}");
        }
    }
}
