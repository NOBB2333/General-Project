using Microsoft.EntityFrameworkCore;

namespace General.Admin.EntityFrameworkCore;

public static class AdminDbContextOptionsConfigurer
{
    public static string NormalizeConnectionString(DatabaseProvider provider, string? rawConnectionString)
    {
        return provider == DatabaseProvider.Sqlite
            ? SqliteConnectionStringHelper.Normalize(rawConnectionString)
            : rawConnectionString ?? string.Empty;
    }

    public static void Configure(
        DbContextOptionsBuilder dbContextOptionsBuilder,
        DatabaseProvider provider,
        string? rawConnectionString,
        string? migrationsAssembly = null)
    {
        var connectionString = NormalizeConnectionString(provider, rawConnectionString);

        switch (provider)
        {
            case DatabaseProvider.Sqlite:
                dbContextOptionsBuilder.UseSqlite(
                    connectionString,
                    options =>
                    {
                        if (!string.IsNullOrWhiteSpace(migrationsAssembly))
                        {
                            options.MigrationsAssembly(migrationsAssembly);
                        }
                    });
                dbContextOptionsBuilder.AddInterceptors(new SqlitePragmaConnectionInterceptor());
                break;

            case DatabaseProvider.PostgreSql:
                dbContextOptionsBuilder.UseNpgsql(
                    connectionString,
                    options =>
                    {
                        if (!string.IsNullOrWhiteSpace(migrationsAssembly))
                        {
                            options.MigrationsAssembly(migrationsAssembly);
                        }
                    });
                break;

            case DatabaseProvider.MySql:
                dbContextOptionsBuilder.UseMySQL(
                    connectionString,
                    options =>
                    {
                        if (!string.IsNullOrWhiteSpace(migrationsAssembly))
                        {
                            options.MigrationsAssembly(migrationsAssembly);
                        }
                    });
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(provider), provider, null);
        }
    }
}
