using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using General.Admin.EntityFrameworkCore;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace General.Admin.Integration;

[Collection(AdminTestConsts.CollectionDefinitionName)]
public class IntegrationTenantIsolationTests : AdminEntityFrameworkCoreTestBase
{
    private readonly IRepository<IntegrationConnector, Guid> _connectorRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IRepository<IntegrationDeliveryRecord, Guid> _deliveryRecordRepository;
    private readonly IntegrationConnectorService _connectorService;
    private readonly IntegrationDeliveryService _deliveryService;
    private readonly IntegrationWebhookService _webhookService;

    public IntegrationTenantIsolationTests()
    {
        _connectorRepository = GetRequiredService<IRepository<IntegrationConnector, Guid>>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
        _deliveryRecordRepository = GetRequiredService<IRepository<IntegrationDeliveryRecord, Guid>>();
        _connectorService = GetRequiredService<IntegrationConnectorService>();
        _deliveryService = GetRequiredService<IntegrationDeliveryService>();
        _webhookService = GetRequiredService<IntegrationWebhookService>();
    }

    [Fact]
    public async Task Integration_Query_Services_Should_Not_Leak_Data_Between_Tenants()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var tenantA = Guid.NewGuid();
            var tenantB = Guid.NewGuid();
            await SeedTenantDataAsync(tenantA, "tenant-a", "10.20.0.1");
            await SeedTenantDataAsync(tenantB, "tenant-b", "10.20.0.2");

            using (_currentTenant.Change(tenantA))
            {
                var connectors = await _connectorService.GetConnectorsAsync(new IntegrationConnectorQueryInput
                {
                    IncludeDisabled = true,
                    Keyword = "TenantIsolation"
                });
                connectors.Count.ShouldBe(1);
                connectors.Single().ConnectorCode.ShouldStartWith("tenant-a");

                var deliveries = await _deliveryService.GetRecordsAsync(new IntegrationDeliveryRecordQueryInput
                {
                    MaxResultCount = 20
                });
                deliveries.TotalCount.ShouldBe(1);
                deliveries.Items.Single().BusinessKey.ShouldBe("tenant-a-delivery");

                var webhooks = await _webhookService.GetEventsAsync(new IntegrationWebhookEventQueryInput
                {
                    MaxResultCount = 20
                });
                webhooks.TotalCount.ShouldBe(1);
                webhooks.Items.Single().BusinessKey.ShouldBe("tenant-a-project");
            }

            using (_currentTenant.Change(tenantB))
            {
                var connectors = await _connectorService.GetConnectorsAsync(new IntegrationConnectorQueryInput
                {
                    IncludeDisabled = true,
                    Keyword = "TenantIsolation"
                });
                connectors.Count.ShouldBe(1);
                connectors.Single().ConnectorCode.ShouldStartWith("tenant-b");

                var deliveries = await _deliveryService.GetRecordsAsync(new IntegrationDeliveryRecordQueryInput
                {
                    MaxResultCount = 20
                });
                deliveries.TotalCount.ShouldBe(1);
                deliveries.Items.Single().BusinessKey.ShouldBe("tenant-b-delivery");

                var webhooks = await _webhookService.GetEventsAsync(new IntegrationWebhookEventQueryInput
                {
                    MaxResultCount = 20
                });
                webhooks.TotalCount.ShouldBe(1);
                webhooks.Items.Single().BusinessKey.ShouldBe("tenant-b-project");
            }
        });
    }

    private async Task SeedTenantDataAsync(Guid tenantId, string prefix, string sourceIp)
    {
        using (_currentTenant.Change(tenantId))
        {
            var connector = await CreateConnectorAsync(prefix, tenantId);
            await CreateDeliveryRecordAsync(connector, prefix, tenantId);
            await _webhookService.ReceiveAsync(new IntegrationWebhookReceiveInput
            {
                ConnectorCode = connector.ConnectorCode,
                EventId = $"evt-{prefix}-{Guid.NewGuid():N}",
                EventType = "card.action",
                ProviderType = "TenantIsolation",
                RawPayloadJson = JsonSerializer.Serialize(new
                {
                    @event = new
                    {
                        action = "open_project",
                        project_id = $"{prefix}-project"
                    }
                }),
                SignatureStatus = "Valid",
                SourceIp = sourceIp
            });
        }
    }

    private async Task<IntegrationConnector> CreateConnectorAsync(string prefix, Guid tenantId)
    {
        var connector = new IntegrationConnector(
            Guid.NewGuid(),
            $"{prefix}-{Guid.NewGuid():N}"[..32],
            "TenantIsolation",
            $"TenantIsolation {prefix}",
            "{}",
            """["SendPersonalMessage","WebhookEvent"]""",
            tenantId,
            isBuiltin: false,
            isEnabled: true,
            icon: "test",
            sort: 999,
            remark: "用于验证外部系统集成租户隔离。");
        await _connectorRepository.InsertAsync(connector, autoSave: true);
        return connector;
    }

    private async Task CreateDeliveryRecordAsync(IntegrationConnector connector, string prefix, Guid tenantId)
    {
        var record = new IntegrationDeliveryRecord(
            Guid.NewGuid(),
            connector.Id,
            connector.ConnectorCode,
            connector.ProviderType,
            "Text",
            "User",
            $"{prefix}-user",
            $"租户隔离测试 {prefix}",
            JsonSerializer.Serialize(new { body = $"租户隔离测试 {prefix}" }),
            tenantId,
            "TenantIsolationTest",
            "Integration",
            $"{prefix}-delivery",
            3);
        record.MarkSending();
        record.MarkSucceeded($"{prefix}-message", """{"ok":true}""", 10);
        await _deliveryRecordRepository.InsertAsync(record, autoSave: true);
    }
}
