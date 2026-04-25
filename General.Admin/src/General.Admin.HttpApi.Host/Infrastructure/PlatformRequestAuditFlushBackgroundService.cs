using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;
using Microsoft.Extensions.DependencyInjection;

namespace General.Admin.Infrastructure;

public class PlatformRequestAuditFlushBackgroundService : BackgroundService
{
    private const int BatchSize = 100;
    private static readonly TimeSpan BatchWindow = TimeSpan.FromMilliseconds(250);
    private bool _auditTableMissingLogged;

    private readonly ILogger<PlatformRequestAuditFlushBackgroundService> _logger;
    private readonly PlatformRequestAuditQueue _queue;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public PlatformRequestAuditFlushBackgroundService(
        ILogger<PlatformRequestAuditFlushBackgroundService> logger,
        PlatformRequestAuditQueue queue,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _queue = queue;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var batch = new List<PlatformRequestAuditEntry>(BatchSize);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var entry = await _queue.ReadAsync(stoppingToken);
                batch.Add(entry);

                await Task.Delay(BatchWindow, stoppingToken);

                while (batch.Count < BatchSize && _queue.TryRead(out var queuedEntry) && queuedEntry != null)
                {
                    batch.Add(queuedEntry);
                }

                await FlushAsync(batch, stoppingToken);
                batch.Clear();
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                if (IsMissingAuditTableException(ex))
                {
                    if (!_auditTableMissingLogged)
                    {
                        _auditTableMissingLogged = true;
                        _logger.LogWarning(
                            "Request audit log table is missing. Run the latest database migration to enable request audit persistence.");
                    }

                    batch.Clear();
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                    continue;
                }

                _logger.LogError(ex, "Failed to flush request audit logs.");
                batch.Clear();
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }
    }

    private async Task FlushAsync(List<PlatformRequestAuditEntry> batch, CancellationToken cancellationToken)
    {
        if (batch.Count == 0)
        {
            return;
        }

        using var scope = _serviceScopeFactory.CreateScope();
        var unitOfWorkManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<AppRequestAuditLog, Guid>>();

        using var unitOfWork = unitOfWorkManager.Begin(requiresNew: true, isTransactional: false);

        var entities = batch.Select(entry => new AppRequestAuditLog(
                entry.Id == Guid.Empty ? Guid.NewGuid() : entry.Id,
                entry.ExecutionTime,
                entry.ExecutionDuration,
                entry.HasException,
                entry.HttpMethod,
                entry.HttpStatusCode,
                entry.Url,
                entry.UserName,
                entry.TenantName,
                entry.ClientIpAddress,
                entry.BrowserInfo,
                entry.ActionSummary,
                entry.MenuTitle,
                entry.Category,
                entry.ExceptionMessage))
            .ToList();

        await repository.InsertManyAsync(entities, autoSave: true, cancellationToken: cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
    }

    private static bool IsMissingAuditTableException(Exception exception)
    {
        return exception.ToString().Contains("no such table: AppRequestAuditLogs", StringComparison.OrdinalIgnoreCase);
    }
}
