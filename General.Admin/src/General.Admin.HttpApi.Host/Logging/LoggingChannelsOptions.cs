using System.Collections.Generic;

namespace General.Admin.Logging;

public sealed class LoggingChannelsOptions
{
    public const string SectionName = "LoggingChannels";

    public bool Enabled { get; set; } = true;

    public string BasePath { get; set; } = "Logs/channels";

    public string FileNameTemplate { get; set; } = "{channel}/{channel}-.log";

    public string FallbackChannel { get; set; } = LoggingChannelConstants.Log;

    public LoggingChannelSinkOptions Default { get; set; } = new();

    public Dictionary<string, LoggingChannelSinkOptions> Overrides { get; set; } = new();

    public Dictionary<string, string> Override { get; set; } = new();
}

public sealed class LoggingChannelSinkOptions
{
    public bool Enabled { get; set; } = true;

    public string MinimumLevel { get; set; } = "Information";

    public string RollingInterval { get; set; } = "Day";

    public int? RetainedFileCountLimit { get; set; } = 30;

    public long? FileSizeLimitBytes { get; set; } = 10485760;

    public string? OutputTemplate { get; set; }
}
