using System.Threading;
using General.Admin.Project;

namespace General.Admin.Platform;

public class ProjectPerformanceSnapshotJobHandler : IPlatformScheduledJobHandler
{
    private readonly PlatformSchedulerExecutionLogWriter _executionLogWriter;
    private readonly ProjectPerformanceService _projectPerformanceService;

    public ProjectPerformanceSnapshotJobHandler(
        PlatformSchedulerExecutionLogWriter executionLogWriter,
        ProjectPerformanceService projectPerformanceService)
    {
        _executionLogWriter = executionLogWriter;
        _projectPerformanceService = projectPerformanceService;
    }

    public string JobKey => "project-performance-snapshot";

    public async Task<string> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var snapshotDate = GetWeekStart(DateTime.Today);
        var createdCount = await _projectPerformanceService.CreateMissingWeeklySnapshotsAsync(snapshotDate);
        var result = $"项目绩效快照完成：统计日期 {snapshotDate:yyyy-MM-dd}，新增 {createdCount} 条 Weekly EVM 快照。";
        await _executionLogWriter.WriteOperationAsync(JobKey, "项目绩效快照", result, cancellationToken: cancellationToken);
        return $"{result} 可在日志中心的操作日志查看执行记录。";
    }

    private static DateTime GetWeekStart(DateTime value)
    {
        var diff = ((int)value.DayOfWeek + 6) % 7;
        return value.Date.AddDays(-diff);
    }
}
