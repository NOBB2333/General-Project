using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using General.Admin.EntityFrameworkCore;
using General.Admin.Platform;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Linq;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace General.Admin.Integration;

[Collection(AdminTestConsts.CollectionDefinitionName)]
public class IntegrationSyncSchedulingTests : AdminEntityFrameworkCoreTestBase
{
    private readonly IAsyncQueryableExecuter _asyncQueryableExecuter;
    private readonly IRepository<IntegrationConnector, Guid> _connectorRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IRepository<AppRequestAuditLog, Guid> _requestAuditLogRepository;
    private readonly IRepository<IntegrationSyncJob, Guid> _syncJobRepository;
    private readonly IRepository<IntegrationSyncRecord, Guid> _syncRecordRepository;

    public IntegrationSyncSchedulingTests()
    {
        _asyncQueryableExecuter = GetRequiredService<IAsyncQueryableExecuter>();
        _connectorRepository = GetRequiredService<IRepository<IntegrationConnector, Guid>>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
        _guidGenerator = GetRequiredService<IGuidGenerator>();
        _requestAuditLogRepository = GetRequiredService<IRepository<AppRequestAuditLog, Guid>>();
        _syncJobRepository = GetRequiredService<IRepository<IntegrationSyncJob, Guid>>();
        _syncRecordRepository = GetRequiredService<IRepository<IntegrationSyncRecord, Guid>>();
    }

    [Fact]
    public async Task RunDueJobsAsync_Should_Run_Only_Due_Jobs_And_Write_Sync_Record()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var connectorCode = $"sync-test-{Guid.NewGuid():N}"[..32];
            var connector = await CreateConnectorAsync(connectorCode);
            var dueJob = await CreateJobAsync(
                connector,
                $"department-{Guid.NewGuid():N}"[..32],
                "测试部门同步任务",
                IntegrationSyncTypes.Department,
                "0 0/1 * * * ?",
                DateTime.UtcNow.AddMinutes(-10));
            var futureJob = await CreateJobAsync(
                connector,
                $"user-{Guid.NewGuid():N}"[..32],
                "测试用户同步任务",
                IntegrationSyncTypes.User,
                "0 0 0 1 1 ? 2099",
                DateTime.UtcNow.AddMinutes(-10));
            var adapter = new TestSyncAdapter(connectorCode);
            var service = CreateSyncService(adapter);

            var result = await service.RunDueJobsAsync("PlatformScheduler");

            result.ShouldContain("到期 1 个");
            result.ShouldContain("成功 1 个");
            adapter.Inputs.Count.ShouldBe(1);
            adapter.Inputs[0].JobCode.ShouldBe(dueJob.JobCode);
            adapter.Inputs[0].SyncType.ShouldBe(IntegrationSyncTypes.Department);

            var records = await _syncRecordRepository.GetListAsync(x =>
                x.SyncJobId == dueJob.Id || x.SyncJobId == futureJob.Id);
            records.Count.ShouldBe(1);

            var record = records.Single();
            record.SyncJobId.ShouldBe(dueJob.Id);
            record.Status.ShouldBe(IntegrationSyncRecordStatus.Succeeded);
            record.TotalCount.ShouldBe(3);
            record.CreatedCount.ShouldBe(1);
            record.UpdatedCount.ShouldBe(2);
            record.Summary.ShouldBe("测试同步完成。");

            using var document = JsonDocument.Parse(record.RequestJson);
            document.RootElement.GetProperty("Trigger").GetString().ShouldBe("PlatformScheduler");
            var scheduledRequestJson = document.RootElement.GetProperty("RequestJson").GetString();
            scheduledRequestJson.ShouldNotBeNull();
            scheduledRequestJson!.ShouldContain("PlatformScheduler");

            var savedDueJob = await _syncJobRepository.GetAsync(dueJob.Id);
            savedDueJob.LastRunStatus.ShouldBe(IntegrationSyncRecordStatus.Succeeded);
            savedDueJob.LastRunSummary.ShouldBe("测试同步完成。");
            savedDueJob.LastRunTime.ShouldNotBeNull();

            var savedFutureJob = await _syncJobRepository.GetAsync(futureJob.Id);
            savedFutureJob.LastRunTime.ShouldBeNull();
            savedFutureJob.LastRunStatus.ShouldBeEmpty();
        });
    }

    [Fact]
    public async Task IntegrationSyncScheduledJobHandler_Should_Write_Platform_Operation_Log()
    {
        var connectorCode = $"sync-log-{Guid.NewGuid():N}"[..32];

        await WithUnitOfWorkAsync(async () =>
        {
            var connector = await CreateConnectorAsync(connectorCode);
            await CreateJobAsync(
                connector,
                $"log-{Guid.NewGuid():N}"[..32],
                "测试调度日志任务",
                IntegrationSyncTypes.User,
                "0 0/1 * * * ?",
                DateTime.UtcNow.AddMinutes(-10));

            var service = CreateSyncService(new TestSyncAdapter(connectorCode));
            var handler = new IntegrationSyncScheduledJobHandler(
                GetRequiredService<PlatformSchedulerExecutionLogWriter>(),
                service);

            var result = await handler.ExecuteAsync();

            result.ShouldContain("外部系统同步调度完成");
            result.ShouldContain("同步任务页面");
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var logs = await _requestAuditLogRepository.GetListAsync();
            var log = logs
                .Where(x => x.Url == "/api/app/platform/scheduler/integration-sync-dispatcher/run")
                .OrderByDescending(x => x.CreationTime)
                .FirstOrDefault();

            log.ShouldNotBeNull();
            log!.MenuTitle.ShouldBe("外部系统同步调度");
            log.Category.ShouldBe("operation");
            log.HttpMethod.ShouldBe("POST");
            log.HttpStatusCode.ShouldBe(200);
            log.UserName.ShouldBe("system");
            log.HasException.ShouldBeFalse();
            log.ActionSummary.ShouldNotBeNull();
            log.ActionSummary!.ShouldContain("外部系统同步调度完成");
        });
    }

    private IntegrationSyncService CreateSyncService(IIntegrationSyncAdapter adapter)
    {
        return new IntegrationSyncService(
            _asyncQueryableExecuter,
            _connectorRepository,
            _currentTenant,
            _guidGenerator,
            [adapter],
            _syncJobRepository,
            _syncRecordRepository);
    }

    private async Task<IntegrationConnector> CreateConnectorAsync(string connectorCode)
    {
        var connector = new IntegrationConnector(
            Guid.NewGuid(),
            connectorCode,
            "Test",
            "测试同步连接器",
            "{}",
            """["SyncDepartment","SyncUser"]""",
            _currentTenant.Id,
            isBuiltin: false,
            isEnabled: true,
            icon: "test",
            sort: 999,
            remark: "用于验证同步任务调度。");
        await _connectorRepository.InsertAsync(connector, autoSave: true);
        return connector;
    }

    private async Task<IntegrationSyncJob> CreateJobAsync(
        IntegrationConnector connector,
        string jobCode,
        string name,
        string syncType,
        string cronExpression,
        DateTime creationTime)
    {
        var job = new IntegrationSyncJob(
            Guid.NewGuid(),
            connector.Id,
            connector.ConnectorCode,
            connector.ProviderType,
            jobCode,
            name,
            syncType,
            IntegrationSyncDirections.Inbound,
            IntegrationSyncModes.Incremental,
            "{}",
            cronExpression,
            isEnabled: true,
            _currentTenant.Id,
            "同步调度回归测试任务");
        SetCreationTime(job, creationTime);

        await _syncJobRepository.InsertAsync(job, autoSave: true);
        SetCreationTime(job, creationTime);
        await _syncJobRepository.UpdateAsync(job, autoSave: true);
        return job;
    }

    private static void SetCreationTime(IntegrationSyncJob job, DateTime creationTime)
    {
        var property = typeof(IntegrationSyncJob).GetProperty(nameof(IntegrationSyncJob.CreationTime));
        var setter = property?.GetSetMethod(nonPublic: true)
            ?? throw new InvalidOperationException("无法设置同步任务创建时间。");
        setter.Invoke(job, BindingFlags.DoNotWrapExceptions, binder: null, parameters: [creationTime], culture: null);
    }

    private sealed class TestSyncAdapter(string connectorCode) : IIntegrationSyncAdapter
    {
        public string ConnectorCode { get; } = connectorCode;

        public List<IntegrationSyncExecuteInput> Inputs { get; } = [];

        public Task<IntegrationSyncResult> ExecuteSyncAsync(IntegrationSyncExecuteInput input)
        {
            Inputs.Add(input);
            return Task.FromResult(new IntegrationSyncResult
            {
                CreatedCount = 1,
                Message = "测试同步完成。",
                ResultJson = """{"source":"sync-scheduling-test"}""",
                SuccessCount = 3,
                TotalCount = 3,
                UpdatedCount = 2
            });
        }
    }
}
