using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace General.Admin.Infrastructure;

public sealed class PlatformSchedulerBackgroundService : BackgroundService
{
    private readonly ILogger<PlatformSchedulerBackgroundService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public PlatformSchedulerBackgroundService(
        ILogger<PlatformSchedulerBackgroundService> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));

        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessJobsAsync(stoppingToken);

            if (!await timer.WaitForNextTickAsync(stoppingToken))
            {
                break;
            }
        }
    }

    private async Task ProcessJobsAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var schedulerService = scope.ServiceProvider.GetRequiredService<PlatformSchedulerService>();
            await schedulerService.ProcessDueJobsAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Ignore cancellation during shutdown.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process scheduled platform jobs.");
        }
    }
}
