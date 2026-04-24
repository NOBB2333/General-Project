using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using General.Admin.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace General.Admin.DbMigrator;

class Program
{
    static async Task Main(string[] args)
    {
        // Npgsql 6+ 要求 DateTime 为 UTC；ABP 内部使用 Local，启用兼容模式
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Volo.Abp", LogEventLevel.Warning)
#if DEBUG
                .MinimumLevel.Override("General.Admin", LogEventLevel.Debug)
#else
                .MinimumLevel.Override("General.Admin", LogEventLevel.Information)
#endif
                .Enrich.FromLogContext()
            .WriteTo.Async(c => c.File("Logs/logs.txt"))
            .WriteTo.Async(c => c.Console())
            .CreateLogger();

        await CreateHostBuilder(args).RunConsoleAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .AddAppSettingsSecretsJson()
            .AddJsoncAppSettingsDirectory()
            .ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                var configuration = configurationBuilder.Build();
                var rawConnectionString = configuration.GetConnectionString("Default");
                var provider = DatabaseProviderDetector.Detect(rawConnectionString);

                // 仅 SQLite 模式需要规范化路径；PostgreSQL 连接串直接透传
                var resolvedConnectionString = provider == DatabaseProvider.PostgreSql
                    ? rawConnectionString
                    : SqliteConnectionStringHelper.Normalize(rawConnectionString);

                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Default"] = resolvedConnectionString
                });
            })
            .ConfigureLogging((context, logging) => logging.ClearProviders())
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<DbMigratorHostedService>();
            });
}
