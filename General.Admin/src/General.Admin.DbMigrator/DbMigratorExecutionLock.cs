using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using General.Admin.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Npgsql;

namespace General.Admin.DbMigrator;

internal sealed class DbMigratorExecutionLock : IDisposable
{
    private readonly FileStream _stream;
    private bool _disposed;

    private DbMigratorExecutionLock(FileStream stream, string filePath)
    {
        _stream = stream;
        FilePath = filePath;
    }

    public string FilePath { get; }

    public static DbMigratorExecutionLock Acquire(IConfiguration configuration)
    {
        var provider = DatabaseProviderDetector.Detect(configuration);
        var connectionString = configuration.GetConnectionString("Default") ?? string.Empty;
        var targetDescription = DescribeTarget(provider, connectionString);
        var filePath = BuildLockFilePath(provider, connectionString);
        var currentOwner = new LockOwnerInfo(
            Environment.ProcessId,
            Process.GetCurrentProcess().StartTime.ToUniversalTime(),
            targetDescription);

        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                var stream = new FileStream(filePath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read);
                WriteOwnerInfo(stream, currentOwner);
                return new DbMigratorExecutionLock(stream, filePath);
            }
            catch (IOException ex) when (File.Exists(filePath))
            {
                var existingOwner = TryReadOwnerInfo(filePath);
                if (ShouldRecoverStaleLock(filePath, existingOwner) && TryDeleteLockFile(filePath))
                {
                    continue;
                }

                throw new InvalidOperationException(
                    $"Another DbMigrator instance is already running for {targetDescription}.{FormatOwnerInfo(existingOwner)}",
                    ex);
            }
        }

        throw new InvalidOperationException($"Could not acquire DbMigrator lock for {targetDescription}.");
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _stream.Dispose();
        TryDeleteLockFile(FilePath);
        _disposed = true;
    }

    private static string BuildLockFilePath(DatabaseProvider provider, string connectionString)
    {
        if (provider == DatabaseProvider.Sqlite)
        {
            var sqliteBuilder = new SqliteConnectionStringBuilder(connectionString);
            var dataSource = sqliteBuilder.DataSource;
            if (!string.IsNullOrWhiteSpace(dataSource))
            {
                var fullPath = Path.GetFullPath(dataSource);
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
                return $"{fullPath}.dbmigrator.lock";
            }
        }

        var directoryPath = Path.Combine(AppContext.BaseDirectory, "Locks");
        Directory.CreateDirectory(directoryPath);

        var lockKey = $"{provider}:{connectionString}";
        var lockHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(lockKey))).ToLowerInvariant();
        return Path.Combine(directoryPath, $"{lockHash}.lock");
    }

    private static void WriteOwnerInfo(FileStream stream, LockOwnerInfo ownerInfo)
    {
        stream.Position = 0;
        stream.SetLength(0);

        using var writer = new StreamWriter(stream, new UTF8Encoding(false), 1024, leaveOpen: true);
        writer.WriteLine($"pid={ownerInfo.ProcessId}");
        writer.WriteLine($"startedAtUtc={ownerInfo.StartedAtUtc:O}");
        writer.WriteLine($"target={ownerInfo.Target}");
        writer.Flush();

        stream.Flush(true);
        stream.Position = 0;
    }

    private static LockOwnerInfo? TryReadOwnerInfo(string filePath)
    {
        try
        {
            var lines = File.ReadAllLines(filePath);
            if (lines.Length < 3)
            {
                return null;
            }

            int? processId = null;
            DateTime? startedAtUtc = null;
            string? target = null;

            foreach (var line in lines)
            {
                if (line.StartsWith("pid=", StringComparison.Ordinal) &&
                    int.TryParse(line["pid=".Length..], out var parsedProcessId))
                {
                    processId = parsedProcessId;
                    continue;
                }

                if (line.StartsWith("startedAtUtc=", StringComparison.Ordinal) &&
                    DateTime.TryParse(line["startedAtUtc=".Length..], out var parsedStartedAtUtc))
                {
                    startedAtUtc = parsedStartedAtUtc;
                    continue;
                }

                if (line.StartsWith("target=", StringComparison.Ordinal))
                {
                    target = line["target=".Length..];
                }
            }

            if (processId == null || startedAtUtc == null || string.IsNullOrWhiteSpace(target))
            {
                return null;
            }

            return new LockOwnerInfo(processId.Value, startedAtUtc.Value, target);
        }
        catch
        {
            return null;
        }
    }

    private static string FormatOwnerInfo(LockOwnerInfo? ownerInfo)
    {
        if (ownerInfo == null)
        {
            return string.Empty;
        }

        return $" Lock holder info: pid={ownerInfo.ProcessId}; startedAtUtc={ownerInfo.StartedAtUtc:O}; target={ownerInfo.Target}";
    }

    private static bool ShouldRecoverStaleLock(string filePath, LockOwnerInfo? ownerInfo)
    {
        TimeSpan fileAge;
        try
        {
            fileAge = DateTime.UtcNow - File.GetLastWriteTimeUtc(filePath);
        }
        catch
        {
            return false;
        }

        if (fileAge <= TimeSpan.FromSeconds(30))
        {
            return false;
        }

        if (ownerInfo != null)
        {
            return !IsProcessAlive(ownerInfo);
        }

        return true;
    }

    private static bool IsProcessAlive(LockOwnerInfo ownerInfo)
    {
        try
        {
            var process = Process.GetProcessById(ownerInfo.ProcessId);
            if (process.HasExited)
            {
                return false;
            }

            try
            {
                var processStartTimeUtc = process.StartTime.ToUniversalTime();
                return Math.Abs((processStartTimeUtc - ownerInfo.StartedAtUtc).TotalSeconds) < 5;
            }
            catch
            {
                return true;
            }
        }
        catch
        {
            return false;
        }
    }

    private static bool TryDeleteLockFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string DescribeTarget(DatabaseProvider provider, string connectionString)
    {
        try
        {
            return provider switch
            {
                DatabaseProvider.Sqlite => DescribeSqliteTarget(connectionString),
                DatabaseProvider.PostgreSql => DescribePostgreSqlTarget(connectionString),
                DatabaseProvider.MySql => DescribeMySqlTarget(connectionString),
                _ => provider.ToString()
            };
        }
        catch
        {
            return provider.ToString();
        }
    }

    private static string DescribeSqliteTarget(string connectionString)
    {
        var builder = new SqliteConnectionStringBuilder(connectionString);
        var dataSource = string.IsNullOrWhiteSpace(builder.DataSource) ? "(default sqlite db)" : builder.DataSource;
        return $"SQLite database '{dataSource}'";
    }

    private static string DescribePostgreSqlTarget(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        return $"PostgreSQL database '{builder.Database}' on '{builder.Host}'";
    }

    private static string DescribeMySqlTarget(string connectionString)
    {
        var builder = new MySqlConnectionStringBuilder(connectionString);
        return $"MySQL database '{builder.Database}' on '{builder.Server}'";
    }

    private sealed record LockOwnerInfo(int ProcessId, DateTime StartedAtUtc, string Target);
}
