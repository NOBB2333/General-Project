using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.Project;

/// <summary>
/// RAID 条目（风险/假设/问题/依赖）：含概率×影响自动评分、应对策略、触发条件、应急预案。
/// </summary>
public class ProjectRaidItem : FullAuditedAggregateRoot<Guid>
{
    public string Level { get; private set; }

    public Guid OwnerUserId { get; private set; }

    public int Impact { get; private set; }

    public DateTime? PlannedResolveDate { get; private set; }

    public int Probability { get; private set; }

    public Guid ProjectId { get; private set; }

    public string ContingencyPlan { get; private set; }

    public string ResponseStrategy { get; private set; }

    public int RiskScore { get; private set; }

    public string Status { get; private set; }

    public string Title { get; private set; }

    public string TriggerCondition { get; private set; }

    public string Type { get; private set; }

    protected ProjectRaidItem()
    {
        Level = string.Empty;
        ContingencyPlan = string.Empty;
        ResponseStrategy = string.Empty;
        Status = string.Empty;
        Title = string.Empty;
        TriggerCondition = string.Empty;
        Type = string.Empty;
    }

    public ProjectRaidItem(
        Guid id,
        Guid projectId,
        string type,
        string title,
        string level,
        Guid ownerUserId,
        string status,
        DateTime? plannedResolveDate = null,
        int probability = 0,
        int impact = 0,
        string? responseStrategy = null,
        string? triggerCondition = null,
        string? contingencyPlan = null) : base(id)
    {
        ProjectId = projectId;
        Type = Check.NotNullOrWhiteSpace(type, nameof(type), 32);
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), 256);
        Level = Check.NotNullOrWhiteSpace(level, nameof(level), 32);
        OwnerUserId = ownerUserId;
        Status = Check.NotNullOrWhiteSpace(status, nameof(status), 32);
        PlannedResolveDate = plannedResolveDate;
        Probability = ClampRiskValue(probability);
        Impact = ClampRiskValue(impact);
        RiskScore = Probability * Impact;
        ResponseStrategy = responseStrategy?.Trim() ?? string.Empty;
        TriggerCondition = triggerCondition?.Trim() ?? string.Empty;
        ContingencyPlan = contingencyPlan?.Trim() ?? string.Empty;
    }

    public void UpdateRiskAssessment(
        int probability,
        int impact,
        string? responseStrategy,
        string? triggerCondition,
        string? contingencyPlan)
    {
        Probability = ClampRiskValue(probability);
        Impact = ClampRiskValue(impact);
        RiskScore = Probability * Impact;
        ResponseStrategy = responseStrategy?.Trim() ?? string.Empty;
        TriggerCondition = triggerCondition?.Trim() ?? string.Empty;
        ContingencyPlan = contingencyPlan?.Trim() ?? string.Empty;
    }

    private static int ClampRiskValue(int value)
    {
        if (value <= 0)
        {
            return 0;
        }

        return Math.Min(value, 5);
    }
}
