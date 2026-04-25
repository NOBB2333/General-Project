using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace General.Admin.Logging;

public static class LoggingChannelLoggerConfigurator
{
    private const string DefaultOutputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";
    private const string SchedulerOutputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] [{JobKey}] {Message:lj}{NewLine}{Exception}";

    public static void Configure(LoggerConfiguration loggerConfiguration, IConfiguration configuration)
    {
        try
        {
            var options = configuration.GetSection(LoggingChannelsOptions.SectionName).Get<LoggingChannelsOptions>()
                          ?? new LoggingChannelsOptions();

            if (!options.Enabled)
            {
                ConfigureFallback(loggerConfiguration);
                return;
            }

            var fallbackChannel = string.IsNullOrWhiteSpace(options.FallbackChannel)
                ? LoggingChannelConstants.Log
                : options.FallbackChannel.Trim();
            var managedChannels = LoggingChannelConstants.GovernedChannels
                .Concat([fallbackChannel])
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var defaultSettings = Normalize(options.Default, channelName: "default");
            var effectiveSettings = managedChannels.ToDictionary(
                channel => channel,
                channel => ResolveChannelSettings(channel, defaultSettings, options.Overrides),
                StringComparer.OrdinalIgnoreCase);

            loggerConfiguration
                .MinimumLevel.Is(effectiveSettings.Values.Min(x => x.MinimumLevel))
                .Enrich.FromLogContext()
                .WriteTo.Async(c => c.Console(outputTemplate: defaultSettings.OutputTemplate));

            foreach (var levelOverride in options.Override)
            {
                loggerConfiguration.MinimumLevel.Override(
                    levelOverride.Key,
                    ParseLogEventLevel(levelOverride.Value, $"Override:{levelOverride.Key}"));
            }

            foreach (var channel in managedChannels)
            {
                var settings = effectiveSettings[channel];
                if (!settings.Enabled)
                {
                    continue;
                }

                var path = ResolveLogPath(options.BasePath, options.FileNameTemplate, channel);
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                loggerConfiguration.WriteTo.Logger(lc =>
                {
                    lc.MinimumLevel.Is(settings.MinimumLevel);
                    lc.Filter.ByIncludingOnly(e => ShouldWriteToChannel(e, channel, fallbackChannel, managedChannels));
                    lc.WriteTo.Async(c => c.File(
                        path,
                        rollingInterval: settings.RollingInterval,
                        retainedFileCountLimit: settings.RetainedFileCountLimit,
                        fileSizeLimitBytes: settings.FileSizeLimitBytes,
                        rollOnFileSizeLimit: true,
                        shared: true,
                        outputTemplate: settings.OutputTemplate));
                });
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to configure logging channels, falling back to Logs/logs.txt. {ex}");
            ConfigureFallback(loggerConfiguration);
        }
    }

    private static EffectiveLoggingChannelSinkOptions ResolveChannelSettings(
        string channel,
        EffectiveLoggingChannelSinkOptions defaults,
        IReadOnlyDictionary<string, LoggingChannelSinkOptions>? overrides)
    {
        if (overrides == null || !overrides.TryGetValue(channel, out var overrideSettings))
        {
            return defaults;
        }

        return Normalize(overrideSettings, channel, defaults);
    }

    private static EffectiveLoggingChannelSinkOptions Normalize(
        LoggingChannelSinkOptions? source,
        string channelName,
        EffectiveLoggingChannelSinkOptions? defaults = null)
    {
        var minimumLevel = source?.MinimumLevel ?? defaults?.MinimumLevel.ToString() ?? "Information";
        var rollingInterval = source?.RollingInterval ?? defaults?.RollingInterval.ToString() ?? "Day";

        return new EffectiveLoggingChannelSinkOptions
        {
            Enabled = source?.Enabled ?? defaults?.Enabled ?? true,
            MinimumLevel = ParseLogEventLevel(minimumLevel, $"{channelName}:MinimumLevel"),
            RollingInterval = ParseRollingInterval(rollingInterval, $"{channelName}:RollingInterval"),
            RetainedFileCountLimit = source?.RetainedFileCountLimit ?? defaults?.RetainedFileCountLimit,
            FileSizeLimitBytes = source?.FileSizeLimitBytes ?? defaults?.FileSizeLimitBytes,
            OutputTemplate = string.IsNullOrWhiteSpace(source?.OutputTemplate)
                ? ResolveDefaultOutputTemplate(channelName, defaults)
                : source!.OutputTemplate!
        };
    }

    private static string ResolveDefaultOutputTemplate(
        string channelName,
        EffectiveLoggingChannelSinkOptions? defaults)
    {
        if (string.Equals(channelName, LoggingChannelConstants.Scheduler, StringComparison.OrdinalIgnoreCase))
        {
            return SchedulerOutputTemplate;
        }

        return defaults?.OutputTemplate ?? DefaultOutputTemplate;
    }

    private static LogEventLevel ParseLogEventLevel(string rawValue, string settingName)
    {
        if (Enum.TryParse<LogEventLevel>(rawValue, ignoreCase: true, out var result))
        {
            return result;
        }

        throw new InvalidOperationException($"Invalid log level '{rawValue}' for setting '{settingName}'.");
    }

    private static RollingInterval ParseRollingInterval(string rawValue, string settingName)
    {
        if (Enum.TryParse<RollingInterval>(rawValue, ignoreCase: true, out var result))
        {
            return result;
        }

        throw new InvalidOperationException($"Invalid rolling interval '{rawValue}' for setting '{settingName}'.");
    }

    private static string ResolveLogPath(string basePath, string fileNameTemplate, string channel)
    {
        var root = string.IsNullOrWhiteSpace(basePath) ? "Logs/channels" : basePath.Trim();
        var template = string.IsNullOrWhiteSpace(fileNameTemplate) ? "{channel}/{channel}-.log" : fileNameTemplate.Trim();
        var relativePath = template.Replace("{channel}", channel, StringComparison.OrdinalIgnoreCase);

        return Path.Combine(root, relativePath);
    }

    private static bool ShouldWriteToChannel(
        LogEvent logEvent,
        string channel,
        string fallbackChannel,
        IReadOnlyCollection<string> managedChannels)
    {
        var eventChannel = ExtractChannel(logEvent);

        if (string.Equals(channel, fallbackChannel, StringComparison.OrdinalIgnoreCase))
        {
            return string.IsNullOrWhiteSpace(eventChannel)
                   || !managedChannels.Contains(eventChannel, StringComparer.OrdinalIgnoreCase)
                   || string.Equals(eventChannel, fallbackChannel, StringComparison.OrdinalIgnoreCase);
        }

        return string.Equals(eventChannel, channel, StringComparison.OrdinalIgnoreCase);
    }

    private static string? ExtractChannel(LogEvent logEvent)
    {
        if (!logEvent.Properties.TryGetValue(LoggingChannelConstants.PropertyName, out var propertyValue))
        {
            return null;
        }

        return propertyValue is ScalarValue { Value: string channelValue }
            ? channelValue
            : null;
    }

    private static void ConfigureFallback(LoggerConfiguration loggerConfiguration)
    {
        loggerConfiguration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Async(c => c.File("Logs/logs.txt", shared: true, outputTemplate: DefaultOutputTemplate))
            .WriteTo.Async(c => c.Console(outputTemplate: DefaultOutputTemplate));
    }

    private sealed class EffectiveLoggingChannelSinkOptions
    {
        public bool Enabled { get; init; }

        public LogEventLevel MinimumLevel { get; init; }

        public RollingInterval RollingInterval { get; init; }

        public int? RetainedFileCountLimit { get; init; }

        public long? FileSizeLimitBytes { get; init; }

        public string OutputTemplate { get; init; } = DefaultOutputTemplate;
    }
}
