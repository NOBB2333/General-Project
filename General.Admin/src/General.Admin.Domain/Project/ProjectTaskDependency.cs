using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.Project;

/// <summary>
/// 任务依赖关系：定义前置任务与后续任务之间的依赖类型（FS/FF/SS/SF），用于 Sprint 和关键路径计算。
/// </summary>
public class ProjectTaskDependency : FullAuditedAggregateRoot<Guid>
{
    public string DependencyType { get; private set; }

    public Guid PredecessorTaskId { get; private set; }

    public Guid ProjectId { get; private set; }

    public Guid SuccessorTaskId { get; private set; }

    protected ProjectTaskDependency()
    {
        DependencyType = string.Empty;
    }

    public ProjectTaskDependency(
        Guid id,
        Guid projectId,
        Guid predecessorTaskId,
        Guid successorTaskId,
        string dependencyType) : base(id)
    {
        ProjectId = projectId;
        PredecessorTaskId = predecessorTaskId;
        SuccessorTaskId = successorTaskId;
        DependencyType = Check.NotNullOrWhiteSpace(dependencyType, nameof(dependencyType), 32);
    }
}
