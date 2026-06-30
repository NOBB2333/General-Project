using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.Project;

/// <summary>
/// 项目基线版本：保存范围、进度、成本基线，是 EVM 计算和受控变更的依据。
/// </summary>
public class ProjectBaseline : FullAuditedAggregateRoot<Guid>
{
    public decimal BudgetAtCompletion { get; private set; }

    public string CostBaselineSummary { get; private set; }

    public Guid? CreatedFromChangeRequestId { get; private set; }

    public DateTime EffectiveDate { get; private set; }

    public bool IsActive { get; private set; }

    public Guid ProjectId { get; private set; }

    public string ScheduleBaselineSummary { get; private set; }

    public string ScopeBaselineSummary { get; private set; }

    public int VersionNo { get; private set; }

    protected ProjectBaseline()
    {
        CostBaselineSummary = string.Empty;
        ScheduleBaselineSummary = string.Empty;
        ScopeBaselineSummary = string.Empty;
    }

    public ProjectBaseline(
        Guid id,
        Guid projectId,
        int versionNo,
        DateTime effectiveDate,
        decimal budgetAtCompletion,
        string scopeBaselineSummary,
        string scheduleBaselineSummary,
        string costBaselineSummary,
        bool isActive,
        Guid? createdFromChangeRequestId = null) : base(id)
    {
        ProjectId = projectId;
        VersionNo = Math.Max(1, versionNo);
        EffectiveDate = effectiveDate.Date;
        BudgetAtCompletion = budgetAtCompletion < 0 ? 0 : budgetAtCompletion;
        ScopeBaselineSummary = Check.NotNullOrWhiteSpace(scopeBaselineSummary, nameof(scopeBaselineSummary), 2048);
        ScheduleBaselineSummary = Check.NotNullOrWhiteSpace(scheduleBaselineSummary, nameof(scheduleBaselineSummary), 2048);
        CostBaselineSummary = Check.NotNullOrWhiteSpace(costBaselineSummary, nameof(costBaselineSummary), 2048);
        IsActive = isActive;
        CreatedFromChangeRequestId = createdFromChangeRequestId;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
