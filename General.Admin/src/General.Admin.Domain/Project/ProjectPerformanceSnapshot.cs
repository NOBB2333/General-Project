using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.Project;

/// <summary>
/// 项目绩效快照：按周/月固化 PV、EV、AC、SPI、CPI，支撑 EVM 趋势监控。
/// </summary>
public class ProjectPerformanceSnapshot : FullAuditedAggregateRoot<Guid>
{
    public decimal ActualCost { get; private set; }

    public decimal CostPerformanceIndex { get; private set; }

    public decimal EarnedValue { get; private set; }

    public decimal PlannedValue { get; private set; }

    public Guid? ProjectBaselineId { get; private set; }

    public Guid ProjectId { get; private set; }

    public decimal SchedulePerformanceIndex { get; private set; }

    public DateTime SnapshotDate { get; private set; }

    public string SnapshotPeriod { get; private set; }

    protected ProjectPerformanceSnapshot()
    {
        SnapshotPeriod = string.Empty;
    }

    public ProjectPerformanceSnapshot(
        Guid id,
        Guid projectId,
        DateTime snapshotDate,
        string snapshotPeriod,
        decimal plannedValue,
        decimal earnedValue,
        decimal actualCost,
        Guid? projectBaselineId = null) : base(id)
    {
        ProjectId = projectId;
        SnapshotDate = snapshotDate.Date;
        SnapshotPeriod = Check.NotNullOrWhiteSpace(snapshotPeriod, nameof(snapshotPeriod), 16);
        PlannedValue = NormalizeAmount(plannedValue);
        EarnedValue = NormalizeAmount(earnedValue);
        ActualCost = NormalizeAmount(actualCost);
        SchedulePerformanceIndex = PlannedValue <= 0 ? 0 : Math.Round(EarnedValue / PlannedValue, 2);
        CostPerformanceIndex = ActualCost <= 0 ? 0 : Math.Round(EarnedValue / ActualCost, 2);
        ProjectBaselineId = projectBaselineId;
    }

    private static decimal NormalizeAmount(decimal amount)
    {
        return amount < 0 ? 0 : amount;
    }
}
