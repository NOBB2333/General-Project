namespace General.Admin.Project;

public class ProjectTaskListInput
{
    public string? Keyword { get; set; }

    public bool OnlyMine { get; set; }

    public Guid? ProjectId { get; set; }

    public string? Status { get; set; }
}
