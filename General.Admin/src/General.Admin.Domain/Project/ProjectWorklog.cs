using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.Project;

public class ProjectWorklog : FullAuditedAggregateRoot<Guid>
{
    public double Hours { get; private set; }

    public Guid ProjectId { get; private set; }

    public string Summary { get; private set; }

    public Guid? TaskId { get; private set; }

    public Guid UserId { get; private set; }

    public DateTime WeekStartDate { get; private set; }

    public DateTime WorkDate { get; private set; }

    protected ProjectWorklog()
    {
        Summary = string.Empty;
    }

    public ProjectWorklog(
        Guid id,
        Guid projectId,
        Guid userId,
        DateTime weekStartDate,
        DateTime workDate,
        double hours,
        string summary,
        Guid? taskId = null) : base(id)
    {
        ProjectId = projectId;
        UserId = userId;
        WeekStartDate = weekStartDate.Date;
        WorkDate = workDate.Date;
        Hours = hours;
        Summary = Check.NotNullOrWhiteSpace(summary, nameof(summary), 256);
        TaskId = taskId;
    }
}
