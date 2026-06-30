using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using General.Admin.EntityFrameworkCore;
using General.Admin.Settings;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.SettingManagement;
using Volo.Abp.Settings;
using Xunit;

namespace General.Admin.Integration;

[Collection(AdminTestConsts.CollectionDefinitionName)]
public class IntegrationDeliveryRetryTests : AdminEntityFrameworkCoreTestBase
{
    private readonly IRepository<IntegrationConnector, Guid> _connectorRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IRepository<IntegrationDeliveryRecord, Guid> _deliveryRecordRepository;
    private readonly IntegrationDeliveryService _deliveryService;
    private readonly IntegrationDispatchService _dispatchService;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IntegrationRateLimitService _rateLimitService;
    private readonly ISettingManager _settingManager;

    public IntegrationDeliveryRetryTests()
    {
        _connectorRepository = GetRequiredService<IRepository<IntegrationConnector, Guid>>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
        _deliveryRecordRepository = GetRequiredService<IRepository<IntegrationDeliveryRecord, Guid>>();
        _deliveryService = GetRequiredService<IntegrationDeliveryService>();
        _dispatchService = GetRequiredService<IntegrationDispatchService>();
        _guidGenerator = GetRequiredService<IGuidGenerator>();
        _rateLimitService = GetRequiredService<IntegrationRateLimitService>();
        _settingManager = GetRequiredService<ISettingManager>();
    }

    [Fact]
    public async Task RetryAsync_Should_Resend_Failed_Record_And_Mark_Succeeded()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var connector = await CreateConnectorAsync();
            var requestJson = JsonSerializer.Serialize(new IntegrationMessageSendInput
            {
                Body = "重试测试消息",
                BusinessKey = "retry-case-001",
                BusinessModule = "Integration",
                ConnectorId = connector.Id,
                MaxRetryCount = 3,
                MessageType = "Text",
                SourceType = "RetryTest",
                TargetId = "user-001",
                TargetType = "User"
            });
            var record = new IntegrationDeliveryRecord(
                Guid.NewGuid(),
                connector.Id,
                connector.ConnectorCode,
                connector.ProviderType,
                "Text",
                "User",
                "user-001",
                "重试测试消息",
                requestJson,
                _currentTenant.Id,
                "RetryTest",
                "Integration",
                "retry-case-001",
                3);
            record.MarkSending();
            record.MarkFailed("首次发送失败", """{"error":"timeout"}""", 10);
            await _deliveryRecordRepository.InsertAsync(record, autoSave: true);

            var dispatchService = new IntegrationDispatchService(
                _connectorRepository,
                _deliveryRecordRepository,
                _currentTenant,
                _guidGenerator,
                _rateLimitService,
                new List<IIntegrationMessageSender> { new SuccessMessageSender(connector.ConnectorCode) });

            var result = await dispatchService.RetryAsync(record.Id);

            result.Success.ShouldBeTrue();
            result.ExternalMessageId.ShouldBe("retry-message-id");
            result.Status.ShouldBe("Succeeded");

            var savedRecord = await _deliveryRecordRepository.GetAsync(record.Id);
            savedRecord.Status.ShouldBe("Succeeded");
            savedRecord.AttemptCount.ShouldBe(2);
            savedRecord.ExternalMessageId.ShouldBe("retry-message-id");
            savedRecord.BusinessKey.ShouldBe("retry-case-001");
            savedRecord.ErrorMessage.ShouldBeEmpty();
            savedRecord.NextRetryTime.ShouldBeNull();
        });
    }

    [Fact]
    public async Task SendMessageAsync_Should_Block_When_Message_Rate_Limit_Is_Exceeded()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            await _settingManager.SetAsync(
                AdminSettings.IntegrationMessageSendLimitPerMinute,
                "1",
                GlobalSettingValueProvider.ProviderName,
                null);

            try
            {
                var connector = await CreateConnectorAsync();
                var input = new IntegrationMessageSendInput
                {
                    Body = "消息频控测试",
                    BusinessKey = $"rate-limit-{Guid.NewGuid():N}",
                    BusinessModule = "Integration",
                    ConnectorId = connector.Id,
                    MaxRetryCount = 3,
                    MessageType = "Text",
                    SourceType = "RateLimitTest",
                    TargetId = "rate-limit-user",
                    TargetType = "User"
                };

                var first = await _dispatchService.SendMessageAsync(input);
                first.Success.ShouldBeFalse();

                var exception = await Should.ThrowAsync<UserFriendlyException>(() => _dispatchService.SendMessageAsync(input));
                exception.Message.ShouldContain("外部系统消息发送超过频率限制");

                var records = await _deliveryRecordRepository.GetListAsync(x => x.SourceType == "RateLimitTest");
                records.Count.ShouldBe(1);
            }
            finally
            {
                await _settingManager.SetAsync(
                    AdminSettings.IntegrationMessageSendLimitPerMinute,
                    "60",
                    GlobalSettingValueProvider.ProviderName,
                    null);
            }
        });
    }

    [Fact]
    public async Task ExportRecordsAsync_Should_Export_Filtered_Audit_Csv()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var connector = await CreateConnectorAsync();
            var succeededRecord = new IntegrationDeliveryRecord(
                Guid.NewGuid(),
                connector.Id,
                connector.ConnectorCode,
                connector.ProviderType,
                "Markdown",
                "User",
                "ou-success",
                "投递审计导出测试",
                """{"body":"投递审计导出测试"}""",
                _currentTenant.Id,
                "AuditExportTest",
                "Integration",
                "export-success",
                3);
            succeededRecord.MarkSending();
            succeededRecord.MarkSucceeded("msg-success", """{"ok":true}""", 23);
            await _deliveryRecordRepository.InsertAsync(succeededRecord, autoSave: true);

            var failedRecord = new IntegrationDeliveryRecord(
                Guid.NewGuid(),
                connector.Id,
                connector.ConnectorCode,
                connector.ProviderType,
                "Text",
                "User",
                "ou-failed",
                "不应出现在成功导出中",
                """{"body":"不应出现在成功导出中"}""",
                _currentTenant.Id,
                "AuditExportTest",
                "Integration",
                "export-failed",
                3);
            failedRecord.MarkSending();
            failedRecord.MarkFailed("测试失败", """{"error":"failed"}""", 8);
            await _deliveryRecordRepository.InsertAsync(failedRecord, autoSave: true);

            var result = await _deliveryService.ExportRecordsAsync(new IntegrationDeliveryRecordQueryInput
            {
                ConnectorCode = connector.ConnectorCode,
                Status = "Succeeded"
            });

            result.ContentType.ShouldBe("text/csv;charset=utf-8");
            result.FileName.ShouldStartWith("外部系统投递审计_");

            var csv = Encoding.UTF8.GetString(result.Content);
            csv.ShouldStartWith("\uFEFF");
            csv.ShouldContain("投递创建时间,最近尝试时间,连接器,平台,消息类型");
            csv.ShouldContain("export-success");
            csv.ShouldContain("msg-success");
            csv.ShouldContain("ou-success");
            csv.ShouldNotContain("export-failed");
            csv.ShouldNotContain("不应出现在成功导出中");
        });
    }

    private async Task<IntegrationConnector> CreateConnectorAsync()
    {
        var connectorCode = $"retry-{Guid.NewGuid():N}"[..32];
        var connector = new IntegrationConnector(
            Guid.NewGuid(),
            connectorCode,
            "Test",
            "重试测试连接器",
            "{}",
            """["SendPersonalMessage"]""",
            _currentTenant.Id,
            isBuiltin: false,
            isEnabled: true,
            icon: "test",
            sort: 999,
            remark: "用于验证失败投递重试。");
        await _connectorRepository.InsertAsync(connector, autoSave: true);
        return connector;
    }

    private sealed class SuccessMessageSender(string connectorCode) : IIntegrationMessageSender
    {
        public string ConnectorCode { get; } = connectorCode;

        public Task<IntegrationMessageSendResult> SendMessageAsync(IntegrationMessageSendInput input)
        {
            return Task.FromResult(new IntegrationMessageSendResult
            {
                ExternalMessageId = "retry-message-id",
                Message = "重试发送成功。",
                Success = true
            });
        }
    }
}
