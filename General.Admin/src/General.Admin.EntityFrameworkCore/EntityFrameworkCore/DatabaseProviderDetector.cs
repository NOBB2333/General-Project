namespace General.Admin.EntityFrameworkCore;

public enum DatabaseProvider { Sqlite, PostgreSql }

/// <summary>
/// 根据连接串内容自动判断数据库类型。
/// PostgreSQL 连接串包含 host= / HOST= / server=（Npgsql 两种格式均支持）。
/// SQLite 连接串包含 data source= 或为空。
/// </summary>
public static class DatabaseProviderDetector
{
    public static DatabaseProvider Detect(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return DatabaseProvider.Sqlite;

        var lower = connectionString.ToLowerInvariant();
        if (lower.Contains("host=") || lower.Contains("user id=") && !lower.Contains("data source="))
            return DatabaseProvider.PostgreSql;

        return DatabaseProvider.Sqlite;
    }
}
