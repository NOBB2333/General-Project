using System;
using System.Data;
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
                if (await SqliteTableExistsAsync(dbContext, "AppMenus"))
                {
                    await EnsureLegacySqliteSchemaAsync(dbContext);
                }
                else
                {
                    await using var sqliteDbContext = CreateMigrationsDbContext<AdminSqliteMigrationsDbContext>(
                        DatabaseProvider.Sqlite,
                        connectionString!,
                        migrationsAssembly);
                    await sqliteDbContext.Database.MigrateAsync();
                }
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

    private static async Task EnsureLegacySqliteSchemaAsync(DbContext dbContext)
    {
        if (!await SqliteColumnExistsAsync(dbContext, "AppProjectTasks", "Description"))
        {
            await dbContext.Database.ExecuteSqlRawAsync(
                "ALTER TABLE \"AppProjectTasks\" ADD COLUMN \"Description\" TEXT NOT NULL DEFAULT ''");
        }

        if (!await SqliteColumnExistsAsync(dbContext, "AppProjectTasks", "IsOutOfContract"))
        {
            await dbContext.Database.ExecuteSqlRawAsync(
                "ALTER TABLE \"AppProjectTasks\" ADD COLUMN \"IsOutOfContract\" INTEGER NOT NULL DEFAULT 0");
        }

        if (!await SqliteColumnExistsAsync(dbContext, "AppProjectTasks", "RequirementId"))
        {
            await dbContext.Database.ExecuteSqlRawAsync(
                "ALTER TABLE \"AppProjectTasks\" ADD COLUMN \"RequirementId\" TEXT NULL");
        }

        if (!await SqliteColumnExistsAsync(dbContext, "AppProjectTasks", "SprintId"))
        {
            await dbContext.Database.ExecuteSqlRawAsync(
                "ALTER TABLE \"AppProjectTasks\" ADD COLUMN \"SprintId\" TEXT NULL");
        }

        if (!await SqliteColumnExistsAsync(dbContext, "AppProjectTasks", "WbsNodeId"))
        {
            await dbContext.Database.ExecuteSqlRawAsync(
                "ALTER TABLE \"AppProjectTasks\" ADD COLUMN \"WbsNodeId\" TEXT NULL");
        }

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "AppProjectTaskParticipants" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_AppProjectTaskParticipants" PRIMARY KEY,
                "ProjectId" TEXT NOT NULL,
                "TaskId" TEXT NOT NULL,
                "UserId" TEXT NOT NULL,
                "RoleName" TEXT NOT NULL,
                "ExtraProperties" TEXT NOT NULL,
                "ConcurrencyStamp" TEXT NOT NULL,
                "CreationTime" TEXT NOT NULL,
                "CreatorId" TEXT NULL,
                "LastModificationTime" TEXT NULL,
                "LastModifierId" TEXT NULL,
                "IsDeleted" INTEGER NOT NULL DEFAULT 0,
                "DeleterId" TEXT NULL,
                "DeletionTime" TEXT NULL,
                CONSTRAINT "FK_AppProjectTaskParticipants_AppProjectTasks_TaskId"
                    FOREIGN KEY ("TaskId") REFERENCES "AppProjectTasks" ("Id") ON DELETE RESTRICT,
                CONSTRAINT "FK_AppProjectTaskParticipants_AppProjects_ProjectId"
                    FOREIGN KEY ("ProjectId") REFERENCES "AppProjects" ("Id") ON DELETE RESTRICT
            );
            """);

        await dbContext.Database.ExecuteSqlRawAsync(
            "CREATE INDEX IF NOT EXISTS \"IX_AppProjectTaskParticipants_ProjectId_RoleName\" ON \"AppProjectTaskParticipants\" (\"ProjectId\", \"RoleName\")");
        await dbContext.Database.ExecuteSqlRawAsync(
            "CREATE INDEX IF NOT EXISTS \"IX_AppProjectTaskParticipants_ProjectId_UserId\" ON \"AppProjectTaskParticipants\" (\"ProjectId\", \"UserId\")");
        await dbContext.Database.ExecuteSqlRawAsync(
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_AppProjectTaskParticipants_TaskId_RoleName_UserId\" ON \"AppProjectTaskParticipants\" (\"TaskId\", \"RoleName\", \"UserId\")");

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "AppProjectSprints" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_AppProjectSprints" PRIMARY KEY,
                "EndDate" TEXT NULL,
                "Name" TEXT NOT NULL,
                "OwnerUserId" TEXT NOT NULL,
                "Progress" INTEGER NOT NULL,
                "ProjectId" TEXT NOT NULL,
                "SprintCode" TEXT NOT NULL,
                "StartDate" TEXT NULL,
                "Status" TEXT NOT NULL,
                "Summary" TEXT NOT NULL,
                "ExtraProperties" TEXT NOT NULL,
                "ConcurrencyStamp" TEXT NOT NULL,
                "CreationTime" TEXT NOT NULL,
                "CreatorId" TEXT NULL,
                "LastModificationTime" TEXT NULL,
                "LastModifierId" TEXT NULL,
                "IsDeleted" INTEGER NOT NULL DEFAULT 0,
                "DeleterId" TEXT NULL,
                "DeletionTime" TEXT NULL
            );
            """);

        await dbContext.Database.ExecuteSqlRawAsync(
            "CREATE INDEX IF NOT EXISTS \"IX_AppProjectSprints_OwnerUserId_Status\" ON \"AppProjectSprints\" (\"OwnerUserId\", \"Status\")");
        await dbContext.Database.ExecuteSqlRawAsync(
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_AppProjectSprints_ProjectId_SprintCode\" ON \"AppProjectSprints\" (\"ProjectId\", \"SprintCode\")");
        await dbContext.Database.ExecuteSqlRawAsync(
            "CREATE INDEX IF NOT EXISTS \"IX_AppProjectSprints_ProjectId_Status\" ON \"AppProjectSprints\" (\"ProjectId\", \"Status\")");
        await dbContext.Database.ExecuteSqlRawAsync(
            "CREATE INDEX IF NOT EXISTS \"IX_AppProjectTasks_RequirementId\" ON \"AppProjectTasks\" (\"RequirementId\")");
        await dbContext.Database.ExecuteSqlRawAsync(
            "CREATE INDEX IF NOT EXISTS \"IX_AppProjectTasks_SprintId\" ON \"AppProjectTasks\" (\"SprintId\")");
        await dbContext.Database.ExecuteSqlRawAsync(
            "CREATE INDEX IF NOT EXISTS \"IX_AppProjectTasks_WbsNodeId\" ON \"AppProjectTasks\" (\"WbsNodeId\")");
        await dbContext.Database.ExecuteSqlRawAsync(
            "CREATE INDEX IF NOT EXISTS \"IX_AppRequestAuditLogs_Category_HttpMethod_ExecutionTime\" ON \"AppRequestAuditLogs\" (\"Category\", \"HttpMethod\", \"ExecutionTime\")");
        await dbContext.Database.ExecuteSqlRawAsync(
            "CREATE INDEX IF NOT EXISTS \"IX_AppRequestAuditLogs_HasException_ExecutionTime\" ON \"AppRequestAuditLogs\" (\"HasException\", \"ExecutionTime\")");
        await dbContext.Database.ExecuteSqlRawAsync(
            "CREATE INDEX IF NOT EXISTS \"IX_AppRequestAuditLogs_HttpMethod_ExecutionTime\" ON \"AppRequestAuditLogs\" (\"HttpMethod\", \"ExecutionTime\")");
    }

    private static async Task<bool> SqliteTableExistsAsync(DbContext dbContext, string tableName)
    {
        return await SqliteScalarExistsAsync(
            dbContext,
            "SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = $name LIMIT 1",
            ("$name", tableName));
    }

    private static async Task<bool> SqliteColumnExistsAsync(DbContext dbContext, string tableName, string columnName)
    {
        return await SqliteScalarExistsAsync(
            dbContext,
            "SELECT 1 FROM pragma_table_info($table) WHERE name = $column LIMIT 1",
            ("$table", tableName),
            ("$column", columnName));
    }

    private static async Task<bool> SqliteScalarExistsAsync(
        DbContext dbContext,
        string commandText,
        params (string Name, object Value)[] parameters)
    {
        var connection = dbContext.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;
        if (shouldClose)
        {
            await connection.OpenAsync();
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = commandText;
            foreach (var (name, value) in parameters)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = name;
                parameter.Value = value;
                command.Parameters.Add(parameter);
            }

            var result = await command.ExecuteScalarAsync();
            return result is not null && result != DBNull.Value;
        }
        finally
        {
            if (shouldClose)
            {
                await connection.CloseAsync();
            }
        }
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
