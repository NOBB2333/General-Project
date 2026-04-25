namespace General.Admin.Project;

public class ProjectCycleItemDto
{
    public DateTime? EndDate { get; set; }

    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string OwnerName { get; set; } = string.Empty;

    public int Progress { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime? StartDate { get; set; }

    public string Summary { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;
}
