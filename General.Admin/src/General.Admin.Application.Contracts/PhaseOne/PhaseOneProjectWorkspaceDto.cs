namespace General.Admin.PhaseOne;

public class PhaseOneProjectWorkspaceDto
{
    public int BlockedTaskCount { get; set; }

    public int HighRiskCount { get; set; }

    public List<PhaseOneProjectListItemDto> KeyProjects { get; set; } = [];

    public List<PhaseOneProjectTaskItemDto> MyTasks { get; set; } = [];

    public int MyTodoCount { get; set; }

    public int OngoingProjectCount { get; set; }

    public int OverdueTaskCount { get; set; }

    public List<PhaseOneProjectRaidItemDto> RaidAlerts { get; set; } = [];

    public List<PhaseOneProjectMilestoneItemDto> WeeklyMilestones { get; set; } = [];
}
