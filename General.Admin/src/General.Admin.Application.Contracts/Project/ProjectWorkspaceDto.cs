namespace General.Admin.Project;

public class ProjectWorkspaceDto
{
    public int BlockedTaskCount { get; set; }

    public int HighRiskCount { get; set; }

    public List<ProjectListItemDto> KeyProjects { get; set; } = [];

    public List<ProjectTaskItemDto> MyTasks { get; set; } = [];

    public int MyTodoCount { get; set; }

    public int OngoingProjectCount { get; set; }

    public int OverdueTaskCount { get; set; }

    public List<ProjectRaidItemDto> RaidAlerts { get; set; } = [];

    public List<ProjectMilestoneItemDto> WeeklyMilestones { get; set; } = [];
}
