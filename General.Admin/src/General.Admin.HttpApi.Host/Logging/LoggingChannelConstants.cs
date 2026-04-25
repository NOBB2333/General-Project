namespace General.Admin.Logging;

public static class LoggingChannelConstants
{
    public const string PropertyName = "LogChannel";
    public const string SourcePropertyName = "LogChannelSource";
    public const string ApiGroupPropertyName = "ApiGroup";
    public const string RoutePatternPropertyName = "RoutePattern";
    public const string TraceIdPropertyName = "TraceId";
    public const string JobKeyPropertyName = "JobKey";
    public const string JobTitlePropertyName = "JobTitle";
    public const string TriggerModePropertyName = "TriggerMode";

    public const string Platform = "platform";
    public const string Project = "project";
    public const string Business = "business";
    public const string Scheduler = "scheduler";
    public const string Log = "log";

    public static readonly string[] GovernedChannels =
    [
        Platform,
        Project,
        Business,
        Scheduler,
        Log
    ];
}
