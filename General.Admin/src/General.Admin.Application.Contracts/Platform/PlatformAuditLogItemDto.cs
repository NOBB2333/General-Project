namespace General.Admin.Platform;

public class PlatformAuditLogItemDto
{
    public Guid Id { get; set; }

    public string? ActionSummary { get; set; }

    public string? BrowserInfo { get; set; }

    public string? Category { get; set; }

    public string? ClientIpAddress { get; set; }

    public int ExecutionDuration { get; set; }

    public DateTime ExecutionTime { get; set; }

    public string? ExceptionMessage { get; set; }

    public bool HasException { get; set; }

    public string? HttpMethod { get; set; }

    public int? HttpStatusCode { get; set; }

    public string? MenuTitle { get; set; }

    public string? TenantName { get; set; }

    public string? Url { get; set; }

    public string? UserName { get; set; }
}
