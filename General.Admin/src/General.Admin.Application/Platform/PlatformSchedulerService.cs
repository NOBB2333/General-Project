using System.Collections.Concurrent;
using System.Threading;
using Quartz;
using Microsoft.Extensions.Logging;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;

namespace General.Admin.Platform;

public class PlatformSchedulerService : ITransientDependency
{
    private const int ClusterNodeRetentionDays = 7;
    private const int MaxClearRecordBatchSize = 500;
    private const int MaxDueTriggerBatchSize = 200;
    private const int MaxRecordResultCount = 200;
    private const string AutoTriggerMode = "auto";
    private const string DefaultTriggerKey = "default";
    private const string ManualTriggerMode = "manual";
    private static readonly string SchedulerInstanceId = $"{Environment.MachineName}:{Guid.NewGuid():N}";
    private static readonly DateTime SchedulerStartedAt = DateTime.Now;
    private static readonly TimeSpan ExecutionLockLease = TimeSpan.FromHours(6);
    private static readonly TimeSpan NodeOfflineThreshold = TimeSpan.FromSeconds(90);
    private static readonly ConcurrentDictionary<string, byte> RunningJobs = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<string, CancellationTokenSource> RunningJobCancellationSources =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly IAsyncQueryableExecuter _asyncQueryableExecuter;
    private readonly IEnumerable<IPlatformScheduledJobHandler> _jobHandlers;
    private readonly IRepository<PlatformScheduledJobClusterNode, Guid> _clusterNodeRepository;
    private readonly IRepository<PlatformScheduledJob, Guid> _jobRepository;
    private readonly IRepository<AppScheduledJobRecord, Guid> _recordRepository;
    private readonly IRepository<PlatformScheduledJobTrigger, Guid> _triggerRepository;
    private readonly ILogger<PlatformSchedulerService> _logger;

    public PlatformSchedulerService(
        IAsyncQueryableExecuter asyncQueryableExecuter,
        IEnumerable<IPlatformScheduledJobHandler> jobHandlers,
        IRepository<PlatformScheduledJobClusterNode, Guid> clusterNodeRepository,
        IRepository<PlatformScheduledJob, Guid> jobRepository,
        IRepository<AppScheduledJobRecord, Guid> recordRepository,
        IRepository<PlatformScheduledJobTrigger, Guid> triggerRepository,
        ILogger<PlatformSchedulerService> logger)
    {
        _asyncQueryableExecuter = asyncQueryableExecuter;
        _jobHandlers = jobHandlers;
        _clusterNodeRepository = clusterNodeRepository;
        _jobRepository = jobRepository;
        _recordRepository = recordRepository;
        _triggerRepository = triggerRepository;
        _logger = logger;
    }

    public async Task<List<PlatformScheduledJobDto>> GetListAsync()
    {
        var triggers = await _triggerRepository.GetListAsync();
        var triggerCounts = triggers
            .GroupBy(x => x.JobId)
            .ToDictionary(x => x.Key, x => x.Count());

        return (await _jobRepository.GetListAsync())
            .OrderBy(x => x.JobKey)
            .Select(x => MapJob(x, triggerCounts.GetValueOrDefault(x.Id)))
            .ToList();
    }

    public Task<List<PlatformScheduledJobHandlerDto>> GetHandlersAsync()
    {
        return Task.FromResult(_jobHandlers
            .Select(x => new PlatformScheduledJobHandlerDto { HandlerKey = x.JobKey })
            .OrderBy(x => x.HandlerKey)
            .ToList());
    }

    public async Task<List<PlatformScheduledJobClusterNodeDto>> GetClusterNodesAsync()
    {
        await UpsertCurrentClusterNodeAsync(CancellationToken.None);
        var now = DateTime.Now;
        return (await _clusterNodeRepository.GetListAsync())
            .OrderByDescending(x => x.LastHeartbeatTime)
            .Select(x => new PlatformScheduledJobClusterNodeDto
            {
                Description = x.Description,
                HostName = x.HostName,
                InstanceId = x.InstanceId,
                LastHeartbeatTime = x.LastHeartbeatTime,
                ProcessId = x.ProcessId,
                StartedAt = x.StartedAt,
                Status = now - x.LastHeartbeatTime > NodeOfflineThreshold
                    ? PlatformScheduledJobClusterNodeStatus.Offline
                    : x.Status
            })
            .ToList();
    }

    public async Task<List<PlatformScheduledJobTriggerDto>> GetTriggersAsync(string jobKey)
    {
        var job = await FindJobByKeyAsync(jobKey);
        var triggers = (await _triggerRepository.GetListAsync())
            .Where(x => x.JobId == job.Id)
            .OrderBy(x => x.TriggerKey)
            .Select(MapTrigger)
            .ToList();

        if (triggers.Count == 0)
        {
            await EnsureDefaultTriggerAsync(job, CancellationToken.None);
            triggers = (await _triggerRepository.GetListAsync())
                .Where(x => x.JobId == job.Id)
                .OrderBy(x => x.TriggerKey)
                .Select(MapTrigger)
                .ToList();
        }

        return triggers;
    }

    public async Task<List<PlatformScheduledJobRecordDto>> GetRecordsAsync(
        string jobKey,
        PlatformScheduledJobRecordQueryInput input)
    {
        var normalizedKey = jobKey.Trim();
        var queryable = await _recordRepository.GetQueryableAsync();
        queryable = queryable.Where(x => x.JobKey == normalizedKey);

        if (!string.IsNullOrWhiteSpace(input.Status))
        {
            var status = input.Status.Trim();
            queryable = queryable.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(input.TriggerMode))
        {
            var triggerMode = input.TriggerMode.Trim();
            queryable = queryable.Where(x => x.TriggerMode == triggerMode);
        }

        if (input.StartedFrom.HasValue)
        {
            queryable = queryable.Where(x => x.StartTime >= input.StartedFrom.Value);
        }

        if (input.StartedTo.HasValue)
        {
            var startedTo = input.StartedTo.Value.Date.AddDays(1);
            queryable = queryable.Where(x => x.StartTime < startedTo);
        }

        var maxResultCount = Math.Clamp(input.MaxResultCount, 1, MaxRecordResultCount);
        var records = await _asyncQueryableExecuter.ToListAsync(
            queryable
                .OrderByDescending(x => x.StartTime)
                .Take(maxResultCount));

        return records.Select(MapRecord).ToList();
    }

    public async Task<PlatformScheduledJobDto> CreateAsync(PlatformScheduledJobSaveInput input)
    {
        ValidateCronExpression(input.CronExpression);
        EnsureHandlerExists(input.HandlerKey);
        var normalizedJobKey = NormalizeRequired(input.JobKey, 64, "任务键不能为空。");
        if ((await _jobRepository.GetListAsync()).Any(x => x.JobKey.Equals(normalizedJobKey, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("任务键已存在。");
        }

        var nextRunTime = input.IsEnabled ? CalculateNextRunTime(input.CronExpression, DateTime.Now) : null;
        var job = new PlatformScheduledJob(
            Guid.NewGuid(),
            normalizedJobKey,
            input.Title,
            input.CronExpression,
            input.Description,
            input.IsEnabled,
            nextRunTime,
            handlerKey: input.HandlerKey);
        await _jobRepository.InsertAsync(job, autoSave: true);
        await _triggerRepository.InsertAsync(
            new PlatformScheduledJobTrigger(
                Guid.NewGuid(),
                job.Id,
                DefaultTriggerKey,
                "默认触发器",
                input.CronExpression,
                input.Description,
                input.IsEnabled,
                nextRunTime),
            autoSave: true);

        return MapJob(job, 1);
    }

    public async Task<PlatformScheduledJobDto> UpdateAsync(string jobKey, PlatformScheduledJobSaveInput input)
    {
        ValidateCronExpression(input.CronExpression);
        EnsureHandlerExists(input.HandlerKey);
        var job = await FindJobByKeyAsync(jobKey);
        if (RunningJobs.ContainsKey(job.JobKey))
        {
            throw new InvalidOperationException("定时任务正在执行中，不能编辑。");
        }

        var nextRunTime = input.IsEnabled ? CalculateNextRunTime(input.CronExpression, DateTime.Now) : null;
        job.UpdateDefinition(
            input.Title,
            input.HandlerKey,
            input.CronExpression,
            input.Description,
            input.IsEnabled,
            nextRunTime);
        await _jobRepository.UpdateAsync(job, autoSave: true);

        var defaultTrigger = (await _triggerRepository.GetListAsync())
            .FirstOrDefault(x => x.JobId == job.Id && x.TriggerKey.Equals(DefaultTriggerKey, StringComparison.OrdinalIgnoreCase));
        if (defaultTrigger == null)
        {
            await EnsureDefaultTriggerAsync(job, CancellationToken.None);
        }
        else
        {
            defaultTrigger.Update(
                defaultTrigger.Title,
                input.CronExpression,
                defaultTrigger.Description,
                input.IsEnabled,
                nextRunTime);
            await _triggerRepository.UpdateAsync(defaultTrigger, autoSave: true);
        }

        var triggerCount = (await _triggerRepository.GetListAsync()).Count(x => x.JobId == job.Id);
        return MapJob(job, triggerCount);
    }

    public async Task<PlatformScheduledJobTriggerDto> CreateTriggerAsync(string jobKey, PlatformScheduledJobTriggerSaveInput input)
    {
        ValidateCronExpression(input.CronExpression);
        var job = await FindJobByKeyAsync(jobKey);
        var normalizedTriggerKey = NormalizeRequired(input.TriggerKey, 64, "触发器键不能为空。");
        if ((await _triggerRepository.GetListAsync()).Any(x =>
                x.JobId == job.Id && x.TriggerKey.Equals(normalizedTriggerKey, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("触发器键已存在。");
        }

        var trigger = new PlatformScheduledJobTrigger(
            Guid.NewGuid(),
            job.Id,
            normalizedTriggerKey,
            input.Title,
            input.CronExpression,
            input.Description,
            input.IsEnabled,
            input.IsEnabled ? CalculateNextRunTime(input.CronExpression, DateTime.Now) : null);
        await _triggerRepository.InsertAsync(trigger, autoSave: true);
        return MapTrigger(trigger);
    }

    public async Task<PlatformScheduledJobTriggerDto> UpdateTriggerAsync(
        string jobKey,
        string triggerKey,
        PlatformScheduledJobTriggerSaveInput input)
    {
        ValidateCronExpression(input.CronExpression);
        var job = await FindJobByKeyAsync(jobKey);
        var trigger = await FindTriggerAsync(job.Id, triggerKey);
        trigger.Update(
            input.Title,
            input.CronExpression,
            input.Description,
            input.IsEnabled,
            input.IsEnabled ? CalculateNextRunTime(input.CronExpression, DateTime.Now) : null);
        await _triggerRepository.UpdateAsync(trigger, autoSave: true);
        return MapTrigger(trigger);
    }

    public async Task ToggleTriggerAsync(string jobKey, string triggerKey, bool isEnabled)
    {
        var job = await FindJobByKeyAsync(jobKey);
        var trigger = await FindTriggerAsync(job.Id, triggerKey);
        trigger.Toggle(
            isEnabled,
            isEnabled ? CalculateNextRunTime(trigger.CronExpression, DateTime.Now) : null);
        await _triggerRepository.UpdateAsync(trigger, autoSave: true);
    }

    public async Task DeleteTriggerAsync(string jobKey, string triggerKey)
    {
        var job = await FindJobByKeyAsync(jobKey);
        var trigger = await FindTriggerAsync(job.Id, triggerKey);
        await _triggerRepository.DeleteAsync(trigger, autoSave: true);
    }

    public async Task<string> RunAsync(string jobKey)
    {
        var job = await FindJobByKeyAsync(jobKey);
        return await ExecuteJobAsync(job, null, manualTrigger: true, CancellationToken.None);
    }

    public async Task ToggleAsync(string jobKey, bool isEnabled)
    {
        var job = await FindJobByKeyAsync(jobKey);

        job.UpdateSchedule(
            isEnabled,
            isEnabled ? CalculateNextRunTime(job.CronExpression, DateTime.Now) : null);
        await _jobRepository.UpdateAsync(job, autoSave: true);
    }

    public async Task<string> CancelAsync(string jobKey)
    {
        var job = await FindJobByKeyAsync(jobKey);

        if (!RunningJobCancellationSources.TryGetValue(job.JobKey, out var cancellationTokenSource))
        {
            return "定时任务当前未在执行。";
        }

        await cancellationTokenSource.CancelAsync();
        return "已发送取消执行信号，任务将在响应取消后结束。";
    }

    public async Task DeleteAsync(string jobKey)
    {
        var normalizedKey = jobKey.Trim();
        if (RunningJobs.ContainsKey(normalizedKey))
        {
            throw new InvalidOperationException("定时任务正在执行中，请先取消或等待执行结束。");
        }

        var job = await FindJobByKeyOrNullAsync(normalizedKey);

        if (job == null)
        {
            return;
        }

        var triggers = (await _triggerRepository.GetListAsync())
            .Where(x => x.JobId == job.Id)
            .ToList();
        if (triggers.Count > 0)
        {
            await _triggerRepository.DeleteManyAsync(triggers, autoSave: true);
        }

        var recordQueryable = await _recordRepository.GetQueryableAsync();
        var records = await _asyncQueryableExecuter.ToListAsync(
            recordQueryable.Where(x => x.JobKey == job.JobKey));
        if (records.Count > 0)
        {
            await _recordRepository.DeleteManyAsync(records, autoSave: true);
        }

        await _jobRepository.DeleteAsync(job, autoSave: true);
    }

    public async Task ClearRecordsAsync(string jobKey, int keepLastN = 0)
    {
        var normalizedKey = jobKey.Trim();
        keepLastN = Math.Clamp(keepLastN, 0, 10_000);

        while (true)
        {
            var recordQueryable = await _recordRepository.GetQueryableAsync();
            var records = await _asyncQueryableExecuter.ToListAsync(
                recordQueryable
                    .Where(x => x.JobKey == normalizedKey)
                    .OrderByDescending(x => x.StartTime)
                    .Skip(keepLastN)
                    .Take(MaxClearRecordBatchSize));

            if (records.Count == 0)
            {
                return;
            }

            await _recordRepository.DeleteManyAsync(records, autoSave: true);
            if (records.Count < MaxClearRecordBatchSize)
            {
                return;
            }
        }
    }

    public async Task ProcessDueJobsAsync(CancellationToken cancellationToken)
    {
        await UpsertCurrentClusterNodeAsync(cancellationToken);
        var now = DateTime.Now;
        var triggerQueryable = await _triggerRepository.GetQueryableAsync();
        var dueTriggers = await _asyncQueryableExecuter.ToListAsync(
            triggerQueryable
                .Where(x => x.IsEnabled && x.NextRunTime.HasValue && x.NextRunTime.Value <= now)
                .OrderBy(x => x.NextRunTime)
                .Take(MaxDueTriggerBatchSize));

        var dueJobIds = dueTriggers
            .Select(x => x.JobId)
            .Distinct()
            .ToList();
        var jobs = dueJobIds.Count == 0
            ? new Dictionary<Guid, PlatformScheduledJob>()
            : (await _asyncQueryableExecuter.ToListAsync(
                (await _jobRepository.GetQueryableAsync())
                    .Where(x => x.IsEnabled && dueJobIds.Contains(x.Id))))
                .ToDictionary(x => x.Id);

        foreach (var trigger in dueTriggers)
        {
            if (!jobs.TryGetValue(trigger.JobId, out var job))
            {
                trigger.Toggle(false, null);
                await _triggerRepository.UpdateAsync(trigger, autoSave: true, cancellationToken: cancellationToken);
                continue;
            }

            await ExecuteJobAsync(job, trigger, manualTrigger: false, cancellationToken);
        }
    }

    private static PlatformScheduledJobDto MapJob(PlatformScheduledJob item, int triggerCount)
    {
        return new PlatformScheduledJobDto
        {
            CronExpression = item.CronExpression,
            Description = item.Description,
            HandlerKey = item.HandlerKey,
            IsEnabled = item.IsEnabled,
            IsRunning = RunningJobs.ContainsKey(item.JobKey) ||
                        (item.LockExpirationTime.HasValue && item.LockExpirationTime.Value > DateTime.Now),
            JobKey = item.JobKey,
            LastRunResult = item.LastRunResult,
            LastRunTime = item.LastRunTime,
            LockExpirationTime = item.LockExpirationTime,
            NextRunTime = item.NextRunTime,
            RunningInstanceId = item.RunningInstanceId,
            Title = item.Title,
            TriggerCount = triggerCount
        };
    }

    private static PlatformScheduledJobRecordDto MapRecord(AppScheduledJobRecord item)
    {
        return new PlatformScheduledJobRecordDto
        {
            DurationMilliseconds = item.DurationMilliseconds,
            EndTime = item.EndTime,
            ErrorMessage = item.ErrorMessage,
            InstanceId = item.InstanceId,
            JobKey = item.JobKey,
            JobTitle = item.JobTitle,
            Result = item.Result,
            StartTime = item.StartTime,
            Status = item.Status,
            TriggerKey = item.TriggerKey,
            TriggerMode = item.TriggerMode
        };
    }

    private static PlatformScheduledJobTriggerDto MapTrigger(PlatformScheduledJobTrigger item)
    {
        return new PlatformScheduledJobTriggerDto
        {
            CronExpression = item.CronExpression,
            Description = item.Description,
            IsEnabled = item.IsEnabled,
            LastRunResult = item.LastRunResult,
            LastRunTime = item.LastRunTime,
            NextRunTime = item.NextRunTime,
            Title = item.Title,
            TriggerKey = item.TriggerKey
        };
    }

    private async Task<string> ExecuteJobAsync(
        PlatformScheduledJob job,
        PlatformScheduledJobTrigger? trigger,
        bool manualTrigger,
        CancellationToken cancellationToken)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["LogChannel"] = "scheduler",
            ["LogChannelSource"] = "job",
            ["JobKey"] = job.JobKey,
            ["JobTitle"] = job.Title,
            ["TriggerKey"] = trigger?.TriggerKey ?? string.Empty,
            ["TriggerMode"] = manualTrigger ? ManualTriggerMode : AutoTriggerMode,
            ["SchedulerInstanceId"] = SchedulerInstanceId
        });

        if (!RunningJobs.TryAdd(job.JobKey, 0))
        {
            _logger.LogWarning("Skipped scheduled job execution because the job is already running.");
            await InsertSkippedRecordAsync(
                job,
                trigger,
                manualTrigger,
                "定时任务正在执行中，已跳过本次触发。",
                cancellationToken);
            if (manualTrigger)
            {
                throw new InvalidOperationException("定时任务正在执行中，请稍后重试。");
            }

            return "定时任务正在执行中，已跳过本次触发。";
        }

        var executedAt = DateTime.Now;
        var startedAt = DateTimeOffset.Now;
        AppScheduledJobRecord? record = null;
        var lockAcquired = false;

        try
        {
            lockAcquired = await TryAcquireExecutionLockAsync(job, cancellationToken);
            if (!lockAcquired)
            {
                var skippedMessage = "定时任务已被其他实例锁定，已跳过本次触发。";
                await InsertSkippedRecordAsync(job, trigger, manualTrigger, skippedMessage, cancellationToken);
                return skippedMessage;
            }

            record = await InsertRunningRecordAsync(job, trigger, manualTrigger, executedAt, cancellationToken);
            _logger.LogInformation("Starting scheduled job execution.");
            var handler = _jobHandlers.FirstOrDefault(x =>
                x.JobKey.Equals(job.HandlerKey, StringComparison.OrdinalIgnoreCase));
            if (handler == null)
            {
                throw new InvalidOperationException("未找到对应的定时任务执行器。");
            }

            using var executionCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            RunningJobCancellationSources[job.JobKey] = executionCancellationSource;
            var result = await handler.ExecuteAsync(executionCancellationSource.Token);
            var message = $"{(manualTrigger ? "手动" : "自动")}执行成功：{result}";
            job.MarkRun(
                executedAt,
                message,
                job.IsEnabled ? CalculateNextRunTime(job.CronExpression, executedAt) : null);
            trigger?.MarkRun(
                executedAt,
                message,
                trigger.IsEnabled ? CalculateNextRunTime(trigger.CronExpression, executedAt) : null);
            job.ReleaseExecutionLock(SchedulerInstanceId);
            record.MarkSuccess(DateTime.Now, message);
            await _jobRepository.UpdateAsync(job, autoSave: true, cancellationToken: cancellationToken);
            if (trigger != null)
            {
                await _triggerRepository.UpdateAsync(trigger, autoSave: true, cancellationToken: cancellationToken);
            }
            await _recordRepository.UpdateAsync(record, autoSave: true, cancellationToken: cancellationToken);
            _logger.LogInformation(
                "Scheduled job execution completed successfully in {ElapsedMilliseconds}ms.",
                (DateTimeOffset.Now - startedAt).TotalMilliseconds);
            return message;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            var message = $"{(manualTrigger ? "手动" : "自动")}执行已取消。";
            job.MarkRun(
                executedAt,
                message,
                job.IsEnabled ? CalculateNextRunTime(job.CronExpression, executedAt) : null);
            trigger?.MarkRun(
                executedAt,
                message,
                trigger.IsEnabled ? CalculateNextRunTime(trigger.CronExpression, executedAt) : null);
            job.ReleaseExecutionLock(SchedulerInstanceId);
            if (record != null)
            {
                record.MarkCancelled(DateTime.Now, message);
                await _recordRepository.UpdateAsync(record, autoSave: true, cancellationToken: CancellationToken.None);
            }

            await _jobRepository.UpdateAsync(job, autoSave: true, cancellationToken: CancellationToken.None);
            if (trigger != null)
            {
                await _triggerRepository.UpdateAsync(trigger, autoSave: true, cancellationToken: CancellationToken.None);
            }
            _logger.LogWarning(
                "Scheduled job execution cancelled after {ElapsedMilliseconds}ms.",
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
            trigger?.MarkRun(
                executedAt,
                message,
                trigger.IsEnabled ? CalculateNextRunTime(trigger.CronExpression, executedAt) : null);
            job.ReleaseExecutionLock(SchedulerInstanceId);
            if (record != null)
            {
                record.MarkFailed(DateTime.Now, message, ex.ToString());
                await _recordRepository.UpdateAsync(record, autoSave: true, cancellationToken: cancellationToken);
            }

            await _jobRepository.UpdateAsync(job, autoSave: true, cancellationToken: cancellationToken);
            if (trigger != null)
            {
                await _triggerRepository.UpdateAsync(trigger, autoSave: true, cancellationToken: cancellationToken);
            }
            _logger.LogError(
                ex,
                "Scheduled job execution failed after {ElapsedMilliseconds}ms.",
                (DateTimeOffset.Now - startedAt).TotalMilliseconds);
            return message;
        }
        finally
        {
            if (lockAcquired)
            {
                job.ReleaseExecutionLock(SchedulerInstanceId);
            }

            RunningJobs.TryRemove(job.JobKey, out _);
            if (RunningJobCancellationSources.TryRemove(job.JobKey, out var cancellationTokenSource))
            {
                cancellationTokenSource.Dispose();
            }
        }
    }

    private async Task<bool> TryAcquireExecutionLockAsync(PlatformScheduledJob job, CancellationToken cancellationToken)
    {
        if (!job.TryAcquireExecutionLock(SchedulerInstanceId, DateTime.Now, ExecutionLockLease))
        {
            return false;
        }

        try
        {
            await _jobRepository.UpdateAsync(job, autoSave: true, cancellationToken: cancellationToken);
            return true;
        }
        catch (AbpDbConcurrencyException)
        {
            return false;
        }
    }

    private async Task<AppScheduledJobRecord> InsertRunningRecordAsync(
        PlatformScheduledJob job,
        PlatformScheduledJobTrigger? trigger,
        bool manualTrigger,
        DateTime startTime,
        CancellationToken cancellationToken)
    {
        var record = new AppScheduledJobRecord(
            Guid.NewGuid(),
            job.JobKey,
            job.Title,
            trigger?.TriggerKey ?? string.Empty,
            manualTrigger ? ManualTriggerMode : AutoTriggerMode,
            startTime,
            SchedulerInstanceId);
        return await _recordRepository.InsertAsync(record, autoSave: true, cancellationToken: cancellationToken);
    }

    private async Task InsertSkippedRecordAsync(
        PlatformScheduledJob job,
        PlatformScheduledJobTrigger? trigger,
        bool manualTrigger,
        string message,
        CancellationToken cancellationToken)
    {
        var now = DateTime.Now;
        var record = new AppScheduledJobRecord(
            Guid.NewGuid(),
            job.JobKey,
            job.Title,
            trigger?.TriggerKey ?? string.Empty,
            manualTrigger ? ManualTriggerMode : AutoTriggerMode,
            now,
            SchedulerInstanceId);
        record.MarkSkipped(now, message);
        await _recordRepository.InsertAsync(record, autoSave: true, cancellationToken: cancellationToken);
    }

    private void EnsureHandlerExists(string handlerKey)
    {
        var normalizedHandlerKey = NormalizeRequired(handlerKey, 64, "执行器不能为空。");
        if (!_jobHandlers.Any(x => x.JobKey.Equals(normalizedHandlerKey, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("执行器不存在。");
        }
    }

    private async Task EnsureDefaultTriggerAsync(PlatformScheduledJob job, CancellationToken cancellationToken)
    {
        var exists = (await _triggerRepository.GetListAsync(cancellationToken: cancellationToken))
            .Any(x => x.JobId == job.Id && x.TriggerKey.Equals(DefaultTriggerKey, StringComparison.OrdinalIgnoreCase));
        if (exists)
        {
            return;
        }

        await _triggerRepository.InsertAsync(
            new PlatformScheduledJobTrigger(
                Guid.NewGuid(),
                job.Id,
                DefaultTriggerKey,
                "默认触发器",
                job.CronExpression,
                job.Description,
                job.IsEnabled,
                job.NextRunTime,
                job.LastRunTime,
                job.LastRunResult),
            autoSave: true,
            cancellationToken: cancellationToken);
    }

    private async Task<PlatformScheduledJob> FindJobByKeyAsync(string jobKey)
    {
        return await FindJobByKeyOrNullAsync(jobKey) ?? throw new InvalidOperationException("定时任务不存在。");
    }

    private async Task<PlatformScheduledJob?> FindJobByKeyOrNullAsync(string jobKey)
    {
        var normalizedKey = NormalizeRequired(jobKey, 64, "任务键不能为空。");
        return (await _jobRepository.GetListAsync())
            .FirstOrDefault(x => x.JobKey.Equals(normalizedKey, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<PlatformScheduledJobTrigger> FindTriggerAsync(Guid jobId, string triggerKey)
    {
        var normalizedTriggerKey = NormalizeRequired(triggerKey, 64, "触发器键不能为空。");
        var trigger = (await _triggerRepository.GetListAsync())
            .FirstOrDefault(x => x.JobId == jobId && x.TriggerKey.Equals(normalizedTriggerKey, StringComparison.OrdinalIgnoreCase));
        return trigger ?? throw new InvalidOperationException("触发器不存在。");
    }

    private static string NormalizeRequired(string value, int maxLength, string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(errorMessage);
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new InvalidOperationException(errorMessage);
        }

        return normalized;
    }

    private async Task UpsertCurrentClusterNodeAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.Now;
        await CleanupExpiredClusterNodesAsync(now, cancellationToken);
        var nodeQueryable = await _clusterNodeRepository.GetQueryableAsync();
        var node = await _asyncQueryableExecuter.FirstOrDefaultAsync(
            nodeQueryable.Where(x => x.InstanceId == SchedulerInstanceId),
            cancellationToken);
        var description = $"{Environment.OSVersion.Platform} {Environment.OSVersion.VersionString}";
        if (node == null)
        {
            await _clusterNodeRepository.InsertAsync(
                new PlatformScheduledJobClusterNode(
                    Guid.NewGuid(),
                    SchedulerInstanceId,
                    Environment.MachineName,
                    Environment.ProcessId.ToString(),
                    SchedulerStartedAt,
                    now,
                    PlatformScheduledJobClusterNodeStatus.Online,
                    description),
                autoSave: true,
                cancellationToken: cancellationToken);
            return;
        }

        node.Heartbeat(now, PlatformScheduledJobClusterNodeStatus.Online, description);
        await _clusterNodeRepository.UpdateAsync(node, autoSave: true, cancellationToken: cancellationToken);
    }

    private async Task CleanupExpiredClusterNodesAsync(DateTime now, CancellationToken cancellationToken)
    {
        var expiredBefore = now.AddDays(-ClusterNodeRetentionDays);
        var nodeQueryable = await _clusterNodeRepository.GetQueryableAsync();
        var expiredNodes = await _asyncQueryableExecuter.ToListAsync(
            nodeQueryable.Where(x => x.InstanceId != SchedulerInstanceId && x.LastHeartbeatTime < expiredBefore),
            cancellationToken);
        if (expiredNodes.Count > 0)
        {
            await _clusterNodeRepository.DeleteManyAsync(
                expiredNodes,
                autoSave: true,
                cancellationToken: cancellationToken);
        }
    }

    private static void ValidateCronExpression(string cronExpression)
    {
        if (!CronExpression.IsValidExpression(cronExpression))
        {
            throw new InvalidOperationException("Cron 表达式不合法。");
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
