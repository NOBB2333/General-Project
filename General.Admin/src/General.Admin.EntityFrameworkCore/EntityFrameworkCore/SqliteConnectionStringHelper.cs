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
            var solutionRoot = FindSolutionRoot(Directory.GetCurrentDirectory());
            var absolutePath = Path.GetFullPath(Path.Combine(solutionRoot, rawPath));
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
}
