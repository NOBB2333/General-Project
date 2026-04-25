using System.IO;
using Microsoft.Data.Sqlite;

namespace General.Admin.EntityFrameworkCore;

public static class SqliteConnectionStringHelper
{
    public static string Normalize(string? connectionString)
    {
        var builder = string.IsNullOrWhiteSpace(connectionString)
            ? new SqliteConnectionStringBuilder()
            : new SqliteConnectionStringBuilder(connectionString);

        var rawPath = string.IsNullOrWhiteSpace(builder.DataSource)
            ? "build/data/general-admin.db"
            : builder.DataSource.Trim();

        if (Path.IsPathRooted(rawPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(rawPath)!);
            builder.DataSource = rawPath;
        }
        else
        {
            var baseDirectory = ResolveRelativeBaseDirectory(Directory.GetCurrentDirectory());
            var absolutePath = Path.GetFullPath(Path.Combine(baseDirectory, rawPath));
            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);
            builder.DataSource = absolutePath;
        }

        builder.Mode = SqliteOpenMode.ReadWriteCreate;
        builder.Cache = SqliteCacheMode.Shared;
        builder.DefaultTimeout = 30;
        builder.Pooling = true;

        return builder.ToString();
    }

    private static string FindSolutionRoot(string startDirectory)
    {
        var current = new DirectoryInfo(startDirectory);
        var nestedSolutionDirectory = new DirectoryInfo(Path.Combine(startDirectory, "General.Admin"));
        if (nestedSolutionDirectory.Exists &&
            (nestedSolutionDirectory.GetFiles("*.sln").Any() || nestedSolutionDirectory.GetFiles("*.slnx").Any()))
        {
            return nestedSolutionDirectory.FullName;
        }

        while (current != null)
        {
            if (current.GetFiles("*.sln").Any() || current.GetFiles("*.slnx").Any())
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return startDirectory;
    }

    /// <summary>
    /// 解析 SQLite 相对路径的基准目录。
    /// </summary>
    private static string ResolveRelativeBaseDirectory(string startDirectory)
    {
        var solutionRoot = FindSolutionRoot(startDirectory);
        if (!string.Equals(solutionRoot, startDirectory, StringComparison.Ordinal))
        {
            return solutionRoot;
        }

        // 发布后 Host 在 backend/app 启动，Migrator 在 backend/dbmigrator 启动。
        // 如果仍按当前目录解析 build/data/general-admin.db，会生成两个 SQLite 文件。
        // 这里统一按父目录 backend 解析，让 JSON/JSONC 继续优先，同时两边共用同一个库。
        var currentDirectory = new DirectoryInfo(startDirectory);
        var parentDirectory = currentDirectory.Parent;
        if (parentDirectory != null &&
            IsPublishedAppDirectory(currentDirectory) &&
            Directory.Exists(Path.Combine(parentDirectory.FullName, "app")) &&
            Directory.Exists(Path.Combine(parentDirectory.FullName, "dbmigrator")))
        {
            return parentDirectory.FullName;
        }

        return startDirectory;
    }

    private static bool IsPublishedAppDirectory(DirectoryInfo directory)
    {
        return string.Equals(directory.Name, "app", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(directory.Name, "dbmigrator", StringComparison.OrdinalIgnoreCase);
    }
}
