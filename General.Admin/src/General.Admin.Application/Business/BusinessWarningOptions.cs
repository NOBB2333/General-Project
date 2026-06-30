namespace General.Admin.Business;

public class BusinessWarningOptions
{
    public decimal GrossProfitWarningRate { get; set; } = 25m;

    public decimal GrossProfitCriticalRate { get; set; } = 15m;

    public decimal SchedulePerformanceWarningIndex { get; set; } = 0.9m;

    public decimal SchedulePerformanceCriticalIndex { get; set; } = 0.75m;

    public decimal CostPerformanceWarningIndex { get; set; } = 0.9m;

    public decimal CostPerformanceCriticalIndex { get; set; } = 0.75m;
}
