namespace General.Admin.Project;

public class ProjectMyRelatedDto
{
    public List<ProjectIssueItemDto> MyIssues { get; set; } = [];

    public List<ProjectOverviewItemDto> MyProjects { get; set; } = [];

    public List<ProjectTaskItemDto> MyTasks { get; set; } = [];

    public List<ProjectWorklogItemDto> RecentWorklogs { get; set; } = [];
}
