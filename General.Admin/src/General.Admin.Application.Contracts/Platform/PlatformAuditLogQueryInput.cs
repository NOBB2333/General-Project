namespace General.Admin.Platform;

public class PlatformAuditLogQueryInput
{
    public string? Category { get; set; }

    public DateTime? EndTime { get; set; }

    public string? Keyword { get; set; }

    public int MaxResultCount { get; set; } = 200;

    public DateTime? StartTime { get; set; }
}
