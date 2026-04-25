using System.Threading;
using Volo.Abp.Domain.Repositories;

namespace General.Admin.Platform;

public class PlatformLogCleanupJobHandler : IPlatformScheduledJobHandler
{
    private readonly PlatformSchedulerExecutionLogWriter _executionLogWriter;
    private readonly IRepository<AppRequestAuditLog, Guid> _requestAuditLogRepository;

    public PlatformLogCleanupJobHandler(
        PlatformSchedulerExecutionLogWriter executionLogWriter,
        IRepository<AppRequestAuditLog, Guid> requestAuditLogRepository)
    {
        _executionLogWriter = executionLogWriter;
        _requestAuditLogRepository = requestAuditLogRepository;
    }

    public string JobKey => "log-cleanup";

    public async Task<string> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var allLogs = await _requestAuditLogRepository.GetListAsync(cancellationToken: cancellationToken);
        var expireBefore = DateTime.UtcNow.AddDays(-30);
        var expiredLogs = allLogs
            .Where(x => x.ExecutionTime < expireBefore)
            .ToList();
        var oldestLogTime = allLogs
            .OrderBy(x => x.ExecutionTime)
            .Select(x => (DateTime?)x.ExecutionTime)
            .FirstOrDefault();

        string result;
        if (expiredLogs.Count == 0)
        {
            result = oldestLogTime.HasValue
                ? $"演示清理完成：当前共有 {allLogs.Count} 条访问日志，最早一条为 {oldestLogTime.Value:G}，没有超过 30 天的数据需要清理。"
                : "演示清理完成：当前没有访问日志数据。";
        }
        else
        {
            await _requestAuditLogRepository.DeleteManyAsync(expiredLogs, autoSave: true, cancellationToken: cancellationToken);
            result = $"演示清理完成：已删除 {expiredLogs.Count} 条 30 天前的访问日志，剩余 {allLogs.Count - expiredLogs.Count} 条。";
        }

        await _executionLogWriter.WriteOperationAsync(JobKey, "清理老旧日志", result, cancellationToken: cancellationToken);
        return $"{result} 可在日志中心的操作日志查看执行记录。";
    }
}
