using System.IO;
using Microsoft.Extensions.Configuration;

namespace General.Admin.EntityFrameworkCore;

public static class AdminDesignTimeDbContextFactoryHelper
{
    public static IConfigurationRoot BuildConfiguration()
    {
        var solutionRoot = SolutionPathHelper.FindSolutionRoot(Directory.GetCurrentDirectory());
        var basePath = Path.Combine(solutionRoot, "General.Admin", "src", "General.Admin.DbMigrator");

        if (!Directory.Exists(basePath))
        {
            basePath = Path.Combine(solutionRoot, "src", "General.Admin.DbMigrator");
        }

        var builder = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsoncAppSettingsDirectory(basePath);

        return builder.Build();
    }
}
