using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using General.Admin.EntityFrameworkCore;
using General.Admin.Logging;
using Serilog;

namespace General.Admin;

public class Program
{
    public async static Task<int> Main(string[] args)
    {
        // Npgsql 6+ 要求 DateTime 为 UTC；ABP 内部使用 Local，启用兼容模式
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.AddAppSettingsSecretsJson()
                .AddJsoncAppSettingsDirectory()
                .ConfigureAppConfiguration((_, configurationBuilder) =>
                {
                    var configuration = configurationBuilder.Build();
                    var rawConnectionString = configuration.GetConnectionString("Default");
                    var provider = DatabaseProviderDetector.Detect(configuration);

                    var resolvedConnectionString = AdminDbContextOptionsConfigurer.NormalizeConnectionString(provider, rawConnectionString);

                    configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:Default"] = resolvedConnectionString
                    });
                })
                .UseAutofac()
                .UseSerilog((context, _, loggerConfiguration) =>
                {
                    LoggingChannelLoggerConfigurator.Configure(loggerConfiguration, context.Configuration);
                });
            Log.Information("Starting General.Admin.HttpApi.Host.");
            await builder.AddApplicationAsync<AdminHttpApiHostModule>();
            var app = builder.Build();
            await app.InitializeApplicationAsync();
            await app.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            if (ex is HostAbortedException)
            {
                throw;
            }

            Log.Fatal(ex, "Host terminated unexpectedly!");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
