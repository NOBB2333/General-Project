using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using General.Admin.EntityFrameworkCore;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Encryption;
using Xunit;

namespace General.Admin.Integration;

[Collection(AdminTestConsts.CollectionDefinitionName)]
public class IntegrationSandboxConnectorAdapterTests : AdminEntityFrameworkCoreTestBase
{
    private readonly IRepository<IntegrationConnector, Guid> _connectorRepository;
    private readonly IRepository<IntegrationDepartmentBinding, Guid> _departmentBindingRepository;
    private readonly IntegrationDispatchService _dispatchService;
    private readonly IRepository<IntegrationExternalAccount, Guid> _externalAccountRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IRepository<IntegrationDeliveryRecord, Guid> _deliveryRecordRepository;
    private readonly IRepository<IntegrationTenantConfig, Guid> _configRepository;
    private readonly IntegrationConnectorService _connectorService;
    private readonly IntegrationSyncService _syncService;
    private readonly IntegrationWebhookService _webhookService;
    private readonly IStringEncryptionService _encryptionService;

    public IntegrationSandboxConnectorAdapterTests()
    {
        _connectorRepository = GetRequiredService<IRepository<IntegrationConnector, Guid>>();
        _departmentBindingRepository = GetRequiredService<IRepository<IntegrationDepartmentBinding, Guid>>();
        _dispatchService = GetRequiredService<IntegrationDispatchService>();
        _externalAccountRepository = GetRequiredService<IRepository<IntegrationExternalAccount, Guid>>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
        _deliveryRecordRepository = GetRequiredService<IRepository<IntegrationDeliveryRecord, Guid>>();
        _configRepository = GetRequiredService<IRepository<IntegrationTenantConfig, Guid>>();
        _connectorService = GetRequiredService<IntegrationConnectorService>();
        _syncService = GetRequiredService<IntegrationSyncService>();
        _webhookService = GetRequiredService<IntegrationWebhookService>();
        _encryptionService = GetRequiredService<IStringEncryptionService>();
    }

    [Fact]
    public async Task Sandbox_Should_Test_Connection_And_Send_Message()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var connector = await EnsureSandboxConnectorAsync();
            var testResult = await _connectorService.TestConnectionAsync(connector.Id);

            testResult.Success.ShouldBeTrue();
            testResult.Message.ShouldContain("集成沙箱连接测试成功");

            var sendResult = await _dispatchService.SendMessageAsync(new IntegrationMessageSendInput
            {
                Body = "沙箱消息",
                BusinessKey = "sandbox-message-001",
                BusinessModule = "Integration",
                ConnectorId = connector.Id,
                MessageType = "Markdown",
                SourceType = "SandboxAcceptance",
                TargetId = "sandbox-user-001",
                TargetType = "User"
            });

            sendResult.Success.ShouldBeTrue();
            sendResult.ExternalMessageId.ShouldStartWith("sandbox-");

            var record = (await _deliveryRecordRepository.GetListAsync(item =>
                item.BusinessKey == "sandbox-message-001")).Single();
            record.Status.ShouldBe("Succeeded");
            record.ProviderType.ShouldBe("Sandbox");
            record.ExternalMessageId.ShouldBe(sendResult.ExternalMessageId);
        });
    }

    [Fact]
    public async Task Sandbox_Should_Run_User_And_Department_Sync()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var connector = await EnsureSandboxConnectorAsync();
            var departmentJob = await _syncService.SaveJobAsync(new IntegrationSyncJobSaveInput
            {
                ConnectorId = connector.Id,
                ConfigJson = """{"departments":[{"id":"dept-001","name":"沙箱部门"}]}""",
                Direction = IntegrationSyncDirections.Inbound,
                IsEnabled = true,
                JobCode = $"sandbox-dept-{Guid.NewGuid():N}"[..32],
                Mode = IntegrationSyncModes.Full,
                Name = "沙箱部门同步",
                SyncType = IntegrationSyncTypes.Department
            });
            var userJob = await _syncService.SaveJobAsync(new IntegrationSyncJobSaveInput
            {
                ConnectorId = connector.Id,
                ConfigJson = """{"users":[{"id":"user-001","name":"沙箱用户","email":"sandbox@example.com","deptId":"dept-001"}]}""",
                Direction = IntegrationSyncDirections.Inbound,
                IsEnabled = true,
                JobCode = $"sandbox-user-{Guid.NewGuid():N}"[..32],
                Mode = IntegrationSyncModes.Full,
                Name = "沙箱用户同步",
                SyncType = IntegrationSyncTypes.User
            });

            var departmentRecord = await _syncService.RunJobAsync(departmentJob.Id, new IntegrationSyncRunInput { Force = true });
            var userRecord = await _syncService.RunJobAsync(userJob.Id, new IntegrationSyncRunInput { Force = true });

            departmentRecord.Status.ShouldBe(IntegrationSyncRecordStatus.Succeeded);
            userRecord.Status.ShouldBe(IntegrationSyncRecordStatus.Succeeded);

            var department = (await _departmentBindingRepository.GetListAsync(item =>
                item.ConnectorId == connector.Id &&
                item.ExternalDepartmentId == "dept-001")).Single();
            department.ExternalDepartmentName.ShouldBe("沙箱部门");

            var account = (await _externalAccountRepository.GetListAsync(item =>
                item.ConnectorId == connector.Id &&
                item.ExternalUserId == "user-001")).Single();
            account.DisplayName.ShouldBe("沙箱用户");
            account.Email.ShouldBe("sandbox@example.com");
        });
    }

    [Fact]
    public async Task Sandbox_Should_Parse_Webhook_And_Dedupe()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var connector = await EnsureSandboxConnectorAsync();
            var rawBody = """
                          {
                            "eventId": "sandbox-event-001",
                            "eventType": "card.action",
                            "actionCode": "open_project",
                            "businessKey": "project-001"
                          }
                          """;

            var first = await _webhookService.ReceiveAsync(new IntegrationWebhookReceiveInput
            {
                ConnectorCode = connector.ConnectorCode,
                ProviderType = connector.ProviderType,
                RawBody = rawBody
            });
            var second = await _webhookService.ReceiveAsync(new IntegrationWebhookReceiveInput
            {
                ConnectorCode = connector.ConnectorCode,
                ProviderType = connector.ProviderType,
                RawBody = rawBody
            });

            first.Status.ShouldBe("Parsed");
            first.SignatureStatus.ShouldBe("Valid");
            first.DedupeKey.ShouldBe("sandbox:sandbox-event-001");
            first.ActionCode.ShouldBe("open_project");
            first.BusinessKey.ShouldBe("project-001");
            second.Id.ShouldBe(first.Id);
        });
    }

    [Fact]
    public async Task Sandbox_Should_Support_Forced_Failure_For_Retry_Acceptance()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var connector = await EnsureSandboxConnectorAsync("""{"acceptMessage":true,"forceFailure":true}""");

            var result = await _dispatchService.SendMessageAsync(new IntegrationMessageSendInput
            {
                Body = "沙箱失败消息",
                BusinessKey = "sandbox-failure-001",
                BusinessModule = "Integration",
                ConnectorId = connector.Id,
                MessageType = "Text",
                SourceType = "SandboxAcceptance",
                TargetId = "sandbox-user-002",
                TargetType = "User"
            });

            result.Success.ShouldBeFalse();
            result.Status.ShouldBe("Failed");

            var record = (await _deliveryRecordRepository.GetListAsync(item =>
                item.BusinessKey == "sandbox-failure-001")).Single();
            record.Status.ShouldBe("Failed");
            record.ErrorMessage.ShouldContain("集成沙箱按配置拒绝消息投递");
        });
    }

    private async Task<IntegrationConnector> EnsureSandboxConnectorAsync(string configJson = """{"acceptMessage":true,"echoPrefix":"[验收] "}""")
    {
        await _connectorService.GetConnectorsAsync(new IntegrationConnectorQueryInput { IncludeDisabled = true });
        var connector = (await _connectorRepository.GetListAsync(item => item.ConnectorCode == "sandbox")).Single();
        var config = await _configRepository.FindAsync(item => item.ConnectorId == connector.Id);
        var secret = _encryptionService.Encrypt("sandbox-secret") ?? "sandbox-secret";
        if (config == null)
        {
            config = new IntegrationTenantConfig(
                Guid.NewGuid(),
                connector.Id,
                _currentTenant.Id,
                configJson,
                secret,
                isEnabled: true);
            await _configRepository.InsertAsync(config, autoSave: true);
        }
        else
        {
            config.Update(configJson, true, config.CallbackPath, config.Remark);
            config.ResetSecret(secret);
            await _configRepository.UpdateAsync(config, autoSave: true);
        }

        return connector;
    }
}
