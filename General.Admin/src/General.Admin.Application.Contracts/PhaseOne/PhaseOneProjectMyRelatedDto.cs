namespace General.Admin.PhaseOne;

public class PhaseOneProjectMyRelatedDto
{
    public List<PhaseOneProjectIssueItemDto> MyIssues { get; set; } = [];

    public List<PhaseOneProjectOverviewItemDto> MyProjects { get; set; } = [];

    public List<PhaseOneProjectTaskItemDto> MyTasks { get; set; } = [];

    public List<PhaseOneProjectWorklogItemDto> RecentWorklogs { get; set; } = [];
}
