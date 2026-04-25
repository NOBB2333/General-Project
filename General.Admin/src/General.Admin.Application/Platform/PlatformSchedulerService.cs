using System.Collections.Concurrent;
using System.Threading;
using Quartz;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace General.Admin.Platform;

public class PlatformSchedulerService : ITransientDependency
{
    private static readonly ConcurrentDictionary<string, byte> RunningJobs = new(StringComparer.OrdinalIgnoreCase);

    private readonly IEnumerable<IPlatformScheduledJobHandler> _jobHandlers;
    private readonly IRepository<PlatformScheduledJob, Guid> _jobRepository;
    private readonly ILogger<PlatformSchedulerService> _logger;

    public PlatformSchedulerService(
        IEnumerable<IPlatformScheduledJobHandler> jobHandlers,
        IRepository<PlatformScheduledJob, Guid> jobRepository,
        ILogger<PlatformSchedulerService> logger)
    {
        _jobHandlers = jobHandlers;
        _jobRepository = jobRepository;
        _logger = logger;
    }

    public async Task<List<PlatformScheduledJobDto>> GetListAsync()
    {
        return (await _jobRepository.GetListAsync())
            .OrderBy(x => x.JobKey)
            .Select(MapJob)
            .ToList();
    }

    public async Task<string> RunAsync(string jobKey)
    {
        var normalizedKey = jobKey.Trim();
        var job = (await _jobRepository.GetListAsync())
            .FirstOrDefault(x => x.JobKey.Equals(normalizedKey, StringComparison.OrdinalIgnoreCase));

        if (job == null)
        {
            throw new InvalidOperationException("定时任务不存在。");
        }

        return await ExecuteJobAsync(job, manualTrigger: true, CancellationToken.None);
    }

    public async Task ToggleAsync(string jobKey, bool isEnabled)
    {
        var normalizedKey = jobKey.Trim();
        var job = (await _jobRepository.GetListAsync())
            .FirstOrDefault(x => x.JobKey.Equals(normalizedKey, StringComparison.OrdinalIgnoreCase));

        if (job == null)
        {
            throw new InvalidOperationException("定时任务不存在。");
        }

        job.UpdateSchedule(
            isEnabled,
            isEnabled ? CalculateNextRunTime(job.CronExpression, DateTime.Now) : null);
        await _jobRepository.UpdateAsync(job, autoSave: true);
    }

    public async Task ProcessDueJobsAsync(CancellationToken cancellationToken)
    {
        var dueJobs = (await _jobRepository.GetListAsync(cancellationToken: cancellationToken))
            .Where(x => x.IsEnabled && x.NextRunTime.HasValue && x.NextRunTime.Value <= DateTime.Now)
            .OrderBy(x => x.NextRunTime)
            .ToList();

        foreach (var job in dueJobs)
        {
            await ExecuteJobAsync(job, manualTrigger: false, cancellationToken);
        }
    }

    private static PlatformScheduledJobDto MapJob(PlatformScheduledJob item)
    {
        return new PlatformScheduledJobDto
        {
            CronExpression = item.CronExpression,
            Description = item.Description,
            IsEnabled = item.IsEnabled,
            IsRunning = RunningJobs.ContainsKey(item.JobKey),
            JobKey = item.JobKey,
            LastRunResult = item.LastRunResult,
            LastRunTime = item.LastRunTime,
            NextRunTime = item.NextRunTime,
            Title = item.Title
        };
    }

    private async Task<string> ExecuteJobAsync(PlatformScheduledJob job, bool manualTrigger, CancellationToken cancellationToken)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["LogChannel"] = "scheduler",
            ["LogChannelSource"] = "job",
            ["JobKey"] = job.JobKey,
            ["JobTitle"] = job.Title,
            ["TriggerMode"] = manualTrigger ? "manual" : "auto"
        });

        if (!RunningJobs.TryAdd(job.JobKey, 0))
        {
            _logger.LogWarning("Skipped scheduled job execution because the job is already running.");
            if (manualTrigger)
            {
                throw new InvalidOperationException("定时任务正在执行中，请稍后重试。");
            }

            return "定时任务正在执行中，已跳过本次触发。";
        }

        var executedAt = DateTime.Now;
        var startedAt = DateTimeOffset.Now;

        try
        {
            _logger.LogInformation("Starting scheduled job execution.");
            var handler = _jobHandlers.FirstOrDefault(x =>
                x.JobKey.Equals(job.JobKey, StringComparison.OrdinalIgnoreCase));
            if (handler == null)
            {
                throw new InvalidOperationException("未找到对应的定时任务执行器。");
            }

            var result = await handler.ExecuteAsync(cancellationToken);
            var message = $"{(manualTrigger ? "手动" : "自动")}执行成功：{result}";
            job.MarkRun(
                executedAt,
                message,
                job.IsEnabled ? CalculateNextRunTime(job.CronExpression, executedAt) : null);
            await _jobRepository.UpdateAsync(job, autoSave: true, cancellationToken: cancellationToken);
            _logger.LogInformation(
                "Scheduled job execution completed successfully in {ElapsedMilliseconds}ms.",
                (DateTimeOffset.Now - startedAt).TotalMilliseconds);
            return message;
        }
        catch (Exception ex)
        {
            var message = $"{(manualTrigger ? "手动" : "自动")}执行失败：{ex.GetBaseException().Message}";
            job.MarkRun(
                executedAt,
                message,
                job.IsEnabled ? CalculateNextRunTime(job.CronExpression, executedAt) : null);
            await _jobRepository.UpdateAsync(job, autoSave: true, cancellationToken: cancellationToken);
            _logger.LogError(
                ex,
                "Scheduled job execution failed after {ElapsedMilliseconds}ms.",
                (DateTimeOffset.Now - startedAt).TotalMilliseconds);
            return message;
        }
        finally
        {
            RunningJobs.TryRemove(job.JobKey, out _);
        }
    }

    private static DateTime? CalculateNextRunTime(string cronExpression, DateTime baseTime)
    {
        if (!CronExpression.IsValidExpression(cronExpression))
        {
            return null;
        }

        var expression = new CronExpression(cronExpression)
        {
            TimeZone = TimeZoneInfo.Local
        };

        return expression.GetNextValidTimeAfter(new DateTimeOffset(baseTime))?.LocalDateTime;
    }
}
