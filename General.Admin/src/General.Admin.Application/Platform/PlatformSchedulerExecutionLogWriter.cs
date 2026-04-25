using System.Threading;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace General.Admin.Platform;

public class PlatformSchedulerExecutionLogWriter : ITransientDependency
{
    private readonly IRepository<AppRequestAuditLog, Guid> _requestAuditLogRepository;

    public PlatformSchedulerExecutionLogWriter(IRepository<AppRequestAuditLog, Guid> requestAuditLogRepository)
    {
        _requestAuditLogRepository = requestAuditLogRepository;
    }

    public async Task WriteOperationAsync(
        string jobKey,
        string title,
        string result,
        bool hasException = false,
        CancellationToken cancellationToken = default)
    {
        await _requestAuditLogRepository.InsertAsync(
            new AppRequestAuditLog(
                Guid.NewGuid(),
                DateTime.Now,
                0,
                hasException,
                "POST",
                hasException ? 500 : 200,
                $"/api/app/platform/scheduler/{jobKey}/run",
                "system",
                null,
                "127.0.0.1",
                "scheduler-demo",
                result,
                title,
                "operation",
                hasException ? result : null),
            autoSave: true,
            cancellationToken: cancellationToken);
    }
}
