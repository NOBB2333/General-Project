namespace General.Admin.PhaseOne;

public class PhaseOneAuditLogQueryInput
{
    public string? Keyword { get; set; }

    public int MaxResultCount { get; set; } = 200;
}
