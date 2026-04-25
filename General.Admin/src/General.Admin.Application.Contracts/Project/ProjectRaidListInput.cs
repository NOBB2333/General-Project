namespace General.Admin.Project;

public class ProjectRaidListInput
{
    public string? Keyword { get; set; }

    public bool OnlyOpen { get; set; } = true;

    public string? Type { get; set; }
}
