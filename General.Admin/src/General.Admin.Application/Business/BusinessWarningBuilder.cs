using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;
using ProjectEntity = General.Admin.Project.Project;

namespace General.Admin.Business;

public class BusinessWarningBuilder : ITransientDependency
{
    private readonly BusinessWarningOptions _options;

    public BusinessWarningBuilder()
        : this(Options.Create(new BusinessWarningOptions()))
    {
    }

    public BusinessWarningBuilder(IOptions<BusinessWarningOptions> options)
    {
        _options = options.Value;
    }

    public List<BusinessWarningItemDto> BuildWarningItems(
        List<BusinessProjectSummaryDto> summaries,
        BusinessContext context)
    {
        var warnings = new List<BusinessWarningItemDto>();

        foreach (var summary in summaries)
        {
            if (!context.ProjectById.TryGetValue(summary.Id, out var project))
            {
                continue;
            }

            if (summary.GrossProfitRate.HasValue && summary.GrossProfitRate.Value < _options.GrossProfitWarningRate)
            {
                warnings.Add(BuildWarningItem(
                    context,
                    project,
                    summary,
                    type: "利润风险",
                    level: summary.GrossProfitRate.Value < _options.GrossProfitCriticalRate ? ProjectPriorities.P0 : ProjectPriorities.P1,
                    actionTab: "profit",
                    description: $"当前毛利率 {summary.GrossProfitRate.Value:0.0}% ，建议优先复核分包成本和补充合同变化。"));
            }

            if (summary.BusinessCloseStatus.Contains("收尾", StringComparison.OrdinalIgnoreCase)
                || summary.BusinessCloseStatus.Contains("待回款", StringComparison.OrdinalIgnoreCase))
            {
                warnings.Add(BuildWarningItem(
                    context,
                    project,
                    summary,
                    type: "收尾风险",
                    level: summary.BusinessCloseStatus.Contains("待回款", StringComparison.OrdinalIgnoreCase) ? ProjectPriorities.P0 : ProjectPriorities.P1,
                    actionTab: "receivable",
                    description: "项目执行已接近结束，但经营收尾或尾款回收仍未闭环。"));
            }

            if ((summary.YearEndReceivableForecast ?? 0m) > (summary.ReceivedAmount ?? 0m))
            {
                warnings.Add(BuildWarningItem(
                    context,
                    project,
                    summary,
                    type: "回款风险",
                    level: summary.BusinessCloseStatus.Contains("待回款", StringComparison.OrdinalIgnoreCase) ? ProjectPriorities.P0 : ProjectPriorities.P1,
                    actionTab: "receivable",
                    description: $"预计回款 {FormatAmount(summary.YearEndReceivableForecast)}，当前已回款 {FormatAmount(summary.ReceivedAmount)}，请复核回款计划。"));
            }

            if (summary.SchedulePerformanceIndex.HasValue &&
                summary.SchedulePerformanceIndex.Value > 0m &&
                summary.SchedulePerformanceIndex.Value < _options.SchedulePerformanceWarningIndex)
            {
                warnings.Add(BuildWarningItem(
                    context,
                    project,
                    summary,
                    type: "进度绩效风险",
                    level: summary.SchedulePerformanceIndex.Value < _options.SchedulePerformanceCriticalIndex ? ProjectPriorities.P0 : ProjectPriorities.P1,
                    actionTab: "overview",
                    description: $"最新 SPI 为 {summary.SchedulePerformanceIndex.Value:0.00}，进度绩效低于基线计划，请复核任务完成和基线偏差。"));
            }

            if (summary.CostPerformanceIndex.HasValue &&
                summary.CostPerformanceIndex.Value > 0m &&
                summary.CostPerformanceIndex.Value < _options.CostPerformanceWarningIndex)
            {
                warnings.Add(BuildWarningItem(
                    context,
                    project,
                    summary,
                    type: "成本绩效风险",
                    level: summary.CostPerformanceIndex.Value < _options.CostPerformanceCriticalIndex ? ProjectPriorities.P0 : ProjectPriorities.P1,
                    actionTab: "profit",
                    description: $"最新 CPI 为 {summary.CostPerformanceIndex.Value:0.00}，成本绩效低于挣值口径，请复核实际成本和预算执行。"));
            }
        }

        return warnings
            .DistinctBy(item => new { item.ProjectId, item.Type })
            .OrderByDescending(item => ResolveWarningRank(item.Level))
            .ThenBy(item => item.ProjectCode)
            .ToList();
    }

    public List<BusinessFocusItemDto> BuildFocusItems(List<BusinessWarningItemDto> warnings)
    {
        return warnings
            .Take(4)
            .Select(item => new BusinessFocusItemDto
            {
                ActionTab = item.ActionTab,
                Description = item.Description,
                ProjectId = item.ProjectId,
                ProjectName = item.ProjectName,
                Type = item.Type
            })
            .ToList();
    }

    private static BusinessWarningItemDto BuildWarningItem(
        BusinessContext context,
        ProjectEntity project,
        BusinessProjectSummaryDto summary,
        string type,
        string level,
        string actionTab,
        string description)
    {
        return new BusinessWarningItemDto
        {
            ActionTab = actionTab,
            Description = description,
            Id = $"{project.Id:N}:{type}",
            LastUpdatedTime = context.LastUpdatedByProject.GetValueOrDefault(project.Id, DateTime.UtcNow).ToString("yyyy-MM-dd HH:mm"),
            Level = level,
            OwnerName = context.UserNameMap.GetValueOrDefault(project.ManagerUserId) ?? "-",
            OwnerUserId = project.ManagerUserId,
            ProjectCode = summary.ProjectCode,
            ProjectId = project.Id,
            ProjectName = project.Name,
            Status = summary.BusinessCloseStatus,
            Type = type
        };
    }

    private static int ResolveWarningRank(string level)
    {
        return level switch
        {
            ProjectPriorities.P0 => 4,
            ProjectPriorities.P1 => 3,
            ProjectPriorities.P2 => 2,
            ProjectPriorities.P3 => 1,
            _ => 0
        };
    }

    private static string FormatAmount(decimal? amount)
    {
        return amount.HasValue ? amount.Value.ToString("0.##") : "--";
    }
}
