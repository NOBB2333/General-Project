namespace General.Admin.PhaseOne;

public class PhaseOneProjectWorklogItemDto
{
    public double Hours { get; set; }

    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string TaskTitle { get; set; } = string.Empty;

    public Guid UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public DateTime WeekStartDate { get; set; }

    public DateTime WorkDate { get; set; }
}
