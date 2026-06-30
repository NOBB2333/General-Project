using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using Quartz;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Linq;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Integration;

public class IntegrationSyncService : ITransientDependency
{
    private readonly IAsyncQueryableExecuter _asyncQueryableExecuter;
    private readonly IRepository<IntegrationConnector, Guid> _connectorRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IEnumerable<IIntegrationSyncAdapter> _syncAdapters;
    private readonly IRepository<IntegrationSyncJob, Guid> _syncJobRepository;
    private readonly IRepository<IntegrationSyncRecord, Guid> _syncRecordRepository;

    public IntegrationSyncService(
        IAsyncQueryableExecuter asyncQueryableExecuter,
        IRepository<IntegrationConnector, Guid> connectorRepository,
        ICurrentTenant currentTenant,
        IGuidGenerator guidGenerator,
        IEnumerable<IIntegrationSyncAdapter> syncAdapters,
        IRepository<IntegrationSyncJob, Guid> syncJobRepository,
        IRepository<IntegrationSyncRecord, Guid> syncRecordRepository)
    {
        _asyncQueryableExecuter = asyncQueryableExecuter;
        _connectorRepository = connectorRepository;
        _currentTenant = currentTenant;
        _guidGenerator = guidGenerator;
        _syncAdapters = syncAdapters;
        _syncJobRepository = syncJobRepository;
        _syncRecordRepository = syncRecordRepository;
    }

    public async Task<PlatformPagedResultDto<IntegrationSyncJobDto>> GetJobsAsync(IntegrationSyncJobQueryInput input)
    {
        var queryable = await BuildJobQueryAsync(input);
        var totalCount = await _asyncQueryableExecuter.CountAsync(queryable);
        var jobs = await _asyncQueryableExecuter.ToListAsync(queryable
            .OrderByDescending(x => x.LastRunTime ?? x.CreationTime)
            .ThenBy(x => x.Name)
            .Skip(NormalizeSkip(input.SkipCount))
            .Take(NormalizeTake(input.MaxResultCount)));

        return new PlatformPagedResultDto<IntegrationSyncJobDto>
        {
            Items = jobs.Select(MapJob).ToList(),
            TotalCount = totalCount
        };
    }

    public async Task<IntegrationSyncJobDto> SaveJobAsync(IntegrationSyncJobSaveInput input)
    {
        ValidateJson(input.ConfigJson, "同步任务配置 JSON 格式不正确。");
        var connector = await _connectorRepository.GetAsync(input.ConnectorId);
        var jobCode = Check.NotNullOrWhiteSpace(input.JobCode, nameof(input.JobCode), 64).Trim();
        var existing = input.Id.HasValue
            ? await _syncJobRepository.GetAsync(input.Id.Value)
            : await _syncJobRepository.FindAsync(x => x.JobCode == jobCode);

        if (existing == null)
        {
            var job = new IntegrationSyncJob(
                _guidGenerator.Create(),
                connector.Id,
                connector.ConnectorCode,
                connector.ProviderType,
                jobCode,
                input.Name,
                input.SyncType,
                input.Direction,
                input.Mode,
                input.ConfigJson,
                input.CronExpression,
                input.IsEnabled,
                _currentTenant.Id,
                input.Remark);
            await _syncJobRepository.InsertAsync(job, autoSave: true);
            return MapJob(job);
        }

        existing.Update(
            connector.Id,
            connector.ConnectorCode,
            connector.ProviderType,
            input.Name,
            input.SyncType,
            input.Direction,
            input.Mode,
            input.ConfigJson,
            input.CronExpression,
            input.IsEnabled,
            input.Remark);
        await _syncJobRepository.UpdateAsync(existing, autoSave: true);
        return MapJob(existing);
    }

    public async Task<IntegrationSyncJobDto> ToggleJobAsync(Guid id, IntegrationSyncJobToggleInput input)
    {
        var job = await _syncJobRepository.GetAsync(id);
        job.Toggle(input.IsEnabled);
        await _syncJobRepository.UpdateAsync(job, autoSave: true);
        return MapJob(job);
    }

    public async Task<IntegrationSyncRecordDto> RunJobAsync(Guid id, IntegrationSyncRunInput input)
    {
        var trigger = input.Force ? "ManualForce" : "Manual";
        return await RunJobAsync(id, input, trigger);
    }

    private async Task<IntegrationSyncRecordDto> RunJobAsync(Guid id, IntegrationSyncRunInput input, string trigger)
    {
        ValidateJson(input.RequestJson, "同步请求 JSON 格式不正确。");
        var job = await _syncJobRepository.GetAsync(id);
        if (!job.IsEnabled && !input.Force)
        {
            throw new UserFriendlyException("同步任务已停用。如需人工执行，请使用强制执行。");
        }

        var mode = ResolveRunMode(input.Mode, job.Mode);
        var requestJson = BuildRequestJson(job, input, mode, trigger);
        var record = new IntegrationSyncRecord(
            _guidGenerator.Create(),
            job.Id,
            job.ConnectorId,
            job.ConnectorCode,
            job.ProviderType,
            job.SyncType,
            mode,
            requestJson,
            _currentTenant.Id);
        await _syncRecordRepository.InsertAsync(record, autoSave: true);

        return await ExecuteRecordAsync(job, record, requestJson, mode);
    }

    public async Task<PlatformPagedResultDto<IntegrationSyncRecordDto>> GetRecordsAsync(IntegrationSyncRecordQueryInput input)
    {
        var queryable = await BuildRecordQueryAsync(input);
        var totalCount = await _asyncQueryableExecuter.CountAsync(queryable);
        var records = await _asyncQueryableExecuter.ToListAsync(queryable
            .OrderByDescending(x => x.CreationTime)
            .Skip(NormalizeSkip(input.SkipCount))
            .Take(NormalizeTake(input.MaxResultCount)));

        return new PlatformPagedResultDto<IntegrationSyncRecordDto>
        {
            Items = records.Select(MapRecord).ToList(),
            TotalCount = totalCount
        };
    }

    public async Task<IntegrationSyncRecordDto> RetryRecordAsync(Guid id)
    {
        var record = await _syncRecordRepository.GetAsync(id);
        if (!record.CanRetry())
        {
            throw new UserFriendlyException("当前同步记录状态不允许重试。");
        }

        var job = await _syncJobRepository.GetAsync(record.SyncJobId);
        record.ResetForRetry();
        await _syncRecordRepository.UpdateAsync(record, autoSave: true);
        return await ExecuteRecordAsync(job, record, record.RequestJson, record.Mode);
    }

    public async Task<IntegrationSyncRecordDto> SkipRecordAsync(Guid id, IntegrationSyncRecordSkipInput input)
    {
        var record = await _syncRecordRepository.GetAsync(id);
        record.MarkSkipped(input.Reason);
        await _syncRecordRepository.UpdateAsync(record, autoSave: true);
        return MapRecord(record);
    }

    public async Task<string> RunDueJobsAsync(string triggerSource, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var queryable = await _syncJobRepository.GetQueryableAsync();
        var jobs = await _asyncQueryableExecuter.ToListAsync(queryable
            .Where(x => x.IsEnabled && x.CronExpression != string.Empty)
            .OrderBy(x => x.LastRunTime ?? x.CreationTime)
            .Take(100));
        var dueJobs = jobs
            .Where(x => IsDue(x, now))
            .Take(20)
            .ToList();

        var succeededCount = 0;
        var failedCount = 0;
        foreach (var job in dueJobs)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var record = await RunJobAsync(job.Id, new IntegrationSyncRunInput
            {
                Force = false,
                Mode = job.Mode,
                RequestJson = JsonSerializer.Serialize(new
                {
                    triggerSource,
                    scheduledAt = now,
                    job.JobCode
                })
            }, triggerSource);
            if (record.Status == IntegrationSyncRecordStatus.Succeeded)
            {
                succeededCount++;
            }
            else
            {
                failedCount++;
            }
        }

        return $"外部系统同步调度完成：扫描 {jobs.Count} 个启用任务，到期 {dueJobs.Count} 个，成功 {succeededCount} 个，失败 {failedCount} 个。";
    }

    private async Task<IntegrationSyncRecordDto> ExecuteRecordAsync(
        IntegrationSyncJob job,
        IntegrationSyncRecord record,
        string requestJson,
        string mode)
    {
        var stopwatch = Stopwatch.StartNew();
        record.MarkRunning();
        await _syncRecordRepository.UpdateAsync(record, autoSave: true);

        try
        {
            var adapter = _syncAdapters.FirstOrDefault(x =>
                x.ConnectorCode.Equals(job.ConnectorCode, StringComparison.OrdinalIgnoreCase));
            if (adapter == null)
            {
                throw new UserFriendlyException($"连接器 {job.ConnectorCode} 暂未实现同步适配器。");
            }

            var result = await adapter.ExecuteSyncAsync(new IntegrationSyncExecuteInput
            {
                ConfigJson = job.ConfigJson,
                ConnectorId = job.ConnectorId,
                JobCode = job.JobCode,
                Mode = mode,
                RequestJson = requestJson,
                SyncType = job.SyncType
            });
            stopwatch.Stop();

            var totalCount = result.TotalCount > 0
                ? result.TotalCount
                : result.SuccessCount + result.FailedCount + result.SkippedCount + result.ConflictCount;
            record.MarkSucceeded(
                totalCount,
                result.CreatedCount,
                result.UpdatedCount,
                result.SkippedCount,
                result.ConflictCount,
                result.FailedCount,
                result.ResultJson,
                result.ConflictJson,
                result.Message,
                (int)stopwatch.ElapsedMilliseconds);
            job.MarkRunResult(record.Status, record.Summary, record.ErrorMessage);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            stopwatch.Stop();
            var message = ex is UserFriendlyException ? ex.Message : $"同步执行失败：{ex.Message}";
            record.MarkFailed(
                message,
                JsonSerializer.Serialize(new { error = message, type = ex.GetType().Name }),
                (int)stopwatch.ElapsedMilliseconds);
            job.MarkRunResult(record.Status, record.Summary, record.ErrorMessage);
        }

        await _syncRecordRepository.UpdateAsync(record, autoSave: true);
        await _syncJobRepository.UpdateAsync(job, autoSave: true);
        return MapRecord(record);
    }

    private async Task<IQueryable<IntegrationSyncJob>> BuildJobQueryAsync(IntegrationSyncJobQueryInput input)
    {
        var queryable = await _syncJobRepository.GetQueryableAsync();
        if (!string.IsNullOrWhiteSpace(input.Status) && !input.Status.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            var isEnabled = input.Status.Equals(IntegrationSyncJobStatus.Active, StringComparison.OrdinalIgnoreCase);
            queryable = queryable.Where(x => x.IsEnabled == isEnabled);
        }

        if (!string.IsNullOrWhiteSpace(input.ConnectorCode))
        {
            var connectorCode = input.ConnectorCode.Trim();
            queryable = queryable.Where(x => x.ConnectorCode == connectorCode);
        }

        if (!string.IsNullOrWhiteSpace(input.ProviderType))
        {
            var providerType = input.ProviderType.Trim();
            queryable = queryable.Where(x => x.ProviderType == providerType);
        }

        if (!string.IsNullOrWhiteSpace(input.SyncType))
        {
            var syncType = input.SyncType.Trim();
            queryable = queryable.Where(x => x.SyncType == syncType);
        }

        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            var keyword = input.Keyword.Trim();
            queryable = queryable.Where(x =>
                x.JobCode.Contains(keyword) ||
                x.Name.Contains(keyword) ||
                x.Remark.Contains(keyword) ||
                x.LastRunSummary.Contains(keyword) ||
                x.LastRunError.Contains(keyword));
        }

        return queryable;
    }

    private async Task<IQueryable<IntegrationSyncRecord>> BuildRecordQueryAsync(IntegrationSyncRecordQueryInput input)
    {
        var queryable = await _syncRecordRepository.GetQueryableAsync();
        if (input.SyncJobId.HasValue)
        {
            queryable = queryable.Where(x => x.SyncJobId == input.SyncJobId.Value);
        }

        if (!string.IsNullOrWhiteSpace(input.Status) && !input.Status.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            var status = input.Status.Trim();
            queryable = queryable.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(input.ConnectorCode))
        {
            var connectorCode = input.ConnectorCode.Trim();
            queryable = queryable.Where(x => x.ConnectorCode == connectorCode);
        }

        if (!string.IsNullOrWhiteSpace(input.ProviderType))
        {
            var providerType = input.ProviderType.Trim();
            queryable = queryable.Where(x => x.ProviderType == providerType);
        }

        if (!string.IsNullOrWhiteSpace(input.SyncType))
        {
            var syncType = input.SyncType.Trim();
            queryable = queryable.Where(x => x.SyncType == syncType);
        }

        if (!string.IsNullOrWhiteSpace(input.Mode))
        {
            var mode = input.Mode.Trim();
            queryable = queryable.Where(x => x.Mode == mode);
        }

        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            var keyword = input.Keyword.Trim();
            queryable = queryable.Where(x =>
                x.Summary.Contains(keyword) ||
                x.ErrorMessage.Contains(keyword) ||
                x.RequestJson.Contains(keyword));
        }

        return queryable;
    }

    private static string BuildRequestJson(IntegrationSyncJob job, IntegrationSyncRunInput input, string mode, string trigger)
    {
        return JsonSerializer.Serialize(new
        {
            job.ConfigJson,
            job.ConnectorCode,
            job.Direction,
            job.JobCode,
            Mode = mode,
            RequestJson = NormalizeJson(input.RequestJson),
            job.SyncType,
            Trigger = string.IsNullOrWhiteSpace(trigger) ? "Manual" : trigger.Trim()
        });
    }

    private static IntegrationSyncJobDto MapJob(IntegrationSyncJob job)
    {
        return new IntegrationSyncJobDto
        {
            ConfigJson = job.ConfigJson,
            ConnectorCode = job.ConnectorCode,
            ConnectorId = job.ConnectorId,
            CronExpression = job.CronExpression,
            Direction = job.Direction,
            Id = job.Id,
            IsEnabled = job.IsEnabled,
            JobCode = job.JobCode,
            LastRunError = job.LastRunError,
            LastRunStatus = job.LastRunStatus,
            LastRunSummary = job.LastRunSummary,
            LastRunTime = job.LastRunTime,
            Mode = job.Mode,
            Name = job.Name,
            ProviderType = job.ProviderType,
            Remark = job.Remark,
            SyncType = job.SyncType
        };
    }

    private static IntegrationSyncRecordDto MapRecord(IntegrationSyncRecord record)
    {
        return new IntegrationSyncRecordDto
        {
            ConflictCount = record.ConflictCount,
            ConflictJson = record.ConflictJson,
            ConnectorCode = record.ConnectorCode,
            ConnectorId = record.ConnectorId,
            CreatedCount = record.CreatedCount,
            CreationTime = record.CreationTime,
            DurationMilliseconds = record.DurationMilliseconds,
            ErrorMessage = record.ErrorMessage,
            FailedCount = record.FailedCount,
            FinishedTime = record.FinishedTime,
            Id = record.Id,
            Mode = record.Mode,
            ProviderType = record.ProviderType,
            RequestJson = record.RequestJson,
            ResultJson = record.ResultJson,
            RetryCount = record.RetryCount,
            SkippedCount = record.SkippedCount,
            StartedTime = record.StartedTime,
            Status = record.Status,
            Summary = record.Summary,
            SyncJobId = record.SyncJobId,
            SyncType = record.SyncType,
            TotalCount = record.TotalCount,
            UpdatedCount = record.UpdatedCount
        };
    }

    private static int NormalizeSkip(int skipCount)
    {
        return Math.Max(0, skipCount);
    }

    private static int NormalizeTake(int maxResultCount)
    {
        return Math.Clamp(maxResultCount, 1, 200);
    }

    private static string ResolveRunMode(string inputMode, string jobMode)
    {
        if (IntegrationSyncModes.All.Contains(inputMode))
        {
            return inputMode.Trim();
        }

        return IntegrationSyncModes.All.Contains(jobMode) ? jobMode : IntegrationSyncModes.Incremental;
    }

    private static bool IsDue(IntegrationSyncJob job, DateTime now)
    {
        if (!CronExpression.IsValidExpression(job.CronExpression))
        {
            return false;
        }

        var expression = new CronExpression(job.CronExpression)
        {
            TimeZone = TimeZoneInfo.Utc
        };
        var fromTime = new DateTimeOffset(job.LastRunTime ?? job.CreationTime, TimeSpan.Zero);
        var nextRunTime = expression.GetNextValidTimeAfter(fromTime);
        return nextRunTime.HasValue && nextRunTime.Value.UtcDateTime <= now;
    }

    private static string NormalizeJson(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "{}" : value.Trim();
    }

    private static void ValidateJson(string value, string errorMessage)
    {
        try
        {
            using var _ = JsonDocument.Parse(NormalizeJson(value));
        }
        catch (JsonException)
        {
            throw new UserFriendlyException(errorMessage);
        }
    }
}
