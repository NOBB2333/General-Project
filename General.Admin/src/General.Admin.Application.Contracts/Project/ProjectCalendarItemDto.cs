namespace General.Admin.Project;

public class ProjectCalendarItemDto
{
    public string ColorToken { get; set; } = string.Empty;

    public DateTime? EndDate { get; set; }

    public Guid Id { get; set; }

    public string OwnerName { get; set; } = string.Empty;

    public DateTime? StartDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;
}
