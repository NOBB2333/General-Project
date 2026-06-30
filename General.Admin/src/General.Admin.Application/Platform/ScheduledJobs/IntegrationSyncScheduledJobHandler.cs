using System.Threading;
using General.Admin.Integration;

namespace General.Admin.Platform;

public class IntegrationSyncScheduledJobHandler : IPlatformScheduledJobHandler
{
    private readonly PlatformSchedulerExecutionLogWriter _executionLogWriter;
    private readonly IntegrationSyncService _syncService;

    public IntegrationSyncScheduledJobHandler(
        PlatformSchedulerExecutionLogWriter executionLogWriter,
        IntegrationSyncService syncService)
    {
        _executionLogWriter = executionLogWriter;
        _syncService = syncService;
    }

    public string JobKey => IntegrationSyncJobStatus.SchedulerJobKey;

    public async Task<string> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var result = await _syncService.RunDueJobsAsync("PlatformScheduler", cancellationToken);
        await _executionLogWriter.WriteOperationAsync(JobKey, "外部系统同步调度", result, cancellationToken: cancellationToken);
        return $"{result} 可在外部系统集成的同步任务页面查看执行记录。";
    }
}
