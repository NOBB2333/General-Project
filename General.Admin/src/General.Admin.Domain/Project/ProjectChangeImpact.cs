using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.Project;

/// <summary>
/// 项目基线变更影响分析：覆盖范围、进度、成本影响，可记录关联经营编号但不在项目域审批经营实体。
/// </summary>
public class ProjectChangeImpact : FullAuditedAggregateRoot<Guid>
{
    public string Analysis { get; private set; }

    public Guid ProjectChangeRequestId { get; private set; }

    public Guid ProjectId { get; private set; }

    public string RelatedBusinessCode { get; private set; }

    public string RelatedBudgetCode { get; private set; }

    public string RelatedContractCode { get; private set; }

    public Guid? RelatedMilestoneId { get; private set; }

    public Guid? RelatedTaskId { get; private set; }

    public decimal? EstimatedCostDelta { get; private set; }

    public int? EstimatedScheduleDeltaDays { get; private set; }

    public string ImpactType { get; private set; }

    public string Severity { get; private set; }

    protected ProjectChangeImpact()
    {
        Analysis = string.Empty;
        ImpactType = string.Empty;
        RelatedBusinessCode = string.Empty;
        RelatedBudgetCode = string.Empty;
        RelatedContractCode = string.Empty;
        Severity = string.Empty;
    }

    public ProjectChangeImpact(
        Guid id,
        Guid projectId,
        Guid projectChangeRequestId,
        string impactType,
        string severity,
        string analysis,
        Guid? relatedTaskId = null,
        Guid? relatedMilestoneId = null,
        string? relatedBusinessCode = null,
        string? relatedBudgetCode = null,
        string? relatedContractCode = null,
        decimal? estimatedCostDelta = null,
        int? estimatedScheduleDeltaDays = null) : base(id)
    {
        ProjectId = projectId;
        ProjectChangeRequestId = projectChangeRequestId;
        ImpactType = Check.NotNullOrWhiteSpace(impactType, nameof(impactType), 32);
        Severity = Check.NotNullOrWhiteSpace(severity, nameof(severity), 32);
        Analysis = Check.NotNullOrWhiteSpace(analysis, nameof(analysis), 2048);
        RelatedTaskId = relatedTaskId;
        RelatedMilestoneId = relatedMilestoneId;
        RelatedBusinessCode = relatedBusinessCode?.Trim() ?? string.Empty;
        RelatedBudgetCode = relatedBudgetCode?.Trim() ?? string.Empty;
        RelatedContractCode = relatedContractCode?.Trim() ?? string.Empty;
        EstimatedCostDelta = estimatedCostDelta;
        EstimatedScheduleDeltaDays = estimatedScheduleDeltaDays;
    }
}
