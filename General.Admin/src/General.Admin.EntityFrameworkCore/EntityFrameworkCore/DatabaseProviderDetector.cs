using Microsoft.Extensions.Configuration;

namespace General.Admin.EntityFrameworkCore;

public enum DatabaseProvider
{
    Sqlite,
    PostgreSql,
    MySql,
    Other
}

public static class DatabaseProviderDetector
{
    public static DatabaseProvider Detect(IConfiguration configuration)
    {
        return Detect(configuration["Database:Provider"], configuration.GetConnectionString("Default"));
    }

    public static DatabaseProvider Detect(string? configuredProvider, string? connectionString)
    {
        if (TryParse(configuredProvider, out var provider))
        {
            return provider;
        }

        return Detect(connectionString);
    }

    public static DatabaseProvider Detect(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return DatabaseProvider.Sqlite;
        }

        var lower = connectionString.ToLowerInvariant();
        if (lower.Contains("data source="))
        {
            return DatabaseProvider.Sqlite;
        }

        if (lower.Contains("port=3306") || (lower.Contains("server=") && lower.Contains("uid=")))
        {
            return DatabaseProvider.MySql;
        }

        if (lower.Contains("host=") || lower.Contains("username=") || lower.Contains("user id="))
        {
            return DatabaseProvider.PostgreSql;
        }

        return DatabaseProvider.Other;
    }

    public static DatabaseProvider DetectFromEfProviderName(string? providerName)
    {
        if (string.IsNullOrWhiteSpace(providerName))
        {
            return DatabaseProvider.Other;
        }

        if (providerName.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            return DatabaseProvider.Sqlite;
        }

        if (providerName.Contains("Npgsql", StringComparison.OrdinalIgnoreCase))
        {
            return DatabaseProvider.PostgreSql;
        }

        if (providerName.Contains("MySQL", StringComparison.OrdinalIgnoreCase) || providerName.Contains("MySql", StringComparison.OrdinalIgnoreCase))
        {
            return DatabaseProvider.MySql;
        }

        return DatabaseProvider.Other;
    }

    private static bool TryParse(string? configuredProvider, out DatabaseProvider provider)
    {
        provider = default;
        if (string.IsNullOrWhiteSpace(configuredProvider))
        {
            return false;
        }

        var normalized = configuredProvider.Trim().Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase);
        provider = normalized.ToLowerInvariant() switch
        {
            "sqlite" => DatabaseProvider.Sqlite,
            "postgresql" => DatabaseProvider.PostgreSql,
            "postgres" => DatabaseProvider.PostgreSql,
            "pgsql" => DatabaseProvider.PostgreSql,
            "mysql" => DatabaseProvider.MySql,
            _ => DatabaseProvider.Other
        };

        return true;
    }
}
