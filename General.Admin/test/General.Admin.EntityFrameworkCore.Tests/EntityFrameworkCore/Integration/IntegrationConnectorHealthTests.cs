using System;
using System.Linq;
using System.Threading.Tasks;
using General.Admin.EntityFrameworkCore;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace General.Admin.Integration;

[Collection(AdminTestConsts.CollectionDefinitionName)]
public class IntegrationConnectorHealthTests : AdminEntityFrameworkCoreTestBase
{
    private readonly IRepository<IntegrationConnector, Guid> _connectorRepository;
    private readonly IRepository<IntegrationTenantConfig, Guid> _configRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IntegrationConnectorService _service;

    public IntegrationConnectorHealthTests()
    {
        _connectorRepository = GetRequiredService<IRepository<IntegrationConnector, Guid>>();
        _configRepository = GetRequiredService<IRepository<IntegrationTenantConfig, Guid>>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
        _service = GetRequiredService<IntegrationConnectorService>();
    }

    [Fact]
    public async Task GetHealthAsync_Should_Report_Connector_Configuration_And_Last_Test_Status()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var connector = await CreateConnectorAsync();

            var unconfigured = await _service.GetHealthAsync();
            var unconfiguredItem = unconfigured.Items.Single(x => x.Id == connector.Id);
            unconfiguredItem.HealthStatus.ShouldBe("Unconfigured");
            unconfiguredItem.HealthMessage.ShouldBe("尚未保存租户级连接配置。");

            await _service.SaveTenantConfigAsync(connector.Id, new IntegrationTenantConfigSaveInput
            {
                ConfigJson = "{}",
                IsEnabled = true,
                Secret = "health-test-secret"
            });

            var warning = await _service.GetHealthAsync();
            var warningItem = warning.Items.Single(x => x.Id == connector.Id);
            warningItem.HealthStatus.ShouldBe("Warning");
            warningItem.HealthMessage.ShouldBe("连接器已配置但尚未完成连接测试。");
            warningItem.HasSecret.ShouldBeTrue();

            var testResult = await _service.TestConnectionAsync(connector.Id);
            testResult.Success.ShouldBeTrue();

            var healthy = await _service.GetHealthAsync();
            var healthyItem = healthy.Items.Single(x => x.Id == connector.Id);
            healthyItem.HealthStatus.ShouldBe("Healthy");
            healthyItem.LastTestStatus.ShouldBe("成功");
            healthyItem.HealthMessage.ShouldBe("连接器基础配置完整。真实平台 Token 与消息接口将在对应连接器 MVP 中校验。");
        });
    }

    [Fact]
    public async Task RotateSecretAsync_Should_Reset_Token_And_Require_Retest()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var connector = await CreateConnectorAsync();
            await _service.SaveTenantConfigAsync(connector.Id, new IntegrationTenantConfigSaveInput
            {
                ConfigJson = "{}",
                IsEnabled = true,
                Secret = "old-secret"
            });

            var config = await _configRepository.FindAsync(x => x.ConnectorId == connector.Id);
            config.ShouldNotBeNull();
            config.ResetAccessToken("encrypted-token", DateTime.UtcNow.AddHours(1));
            config.MarkTestResult(true, "旧密钥连接成功。");
            await _configRepository.UpdateAsync(config, autoSave: true);
            var oldSecretCipherText = config.SecretCipherText;

            var result = await _service.RotateSecretAsync(connector.Id, new IntegrationSecretRotateInput
            {
                Secret = "new-secret"
            });

            result.HasSecret.ShouldBeTrue();
            result.LastTestStatus.ShouldBe("待重测");
            result.LastTestMessage.ShouldBe("连接器密钥已轮换，请重新测试连接。");

            var updated = await _configRepository.GetAsync(config.Id);
            updated.SecretCipherText.ShouldNotBe(oldSecretCipherText);
            updated.AccessTokenCipherText.ShouldBeEmpty();
            updated.TokenExpiresAt.ShouldBeNull();

            var health = await _service.GetHealthAsync();
            var healthItem = health.Items.Single(x => x.Id == connector.Id);
            healthItem.HealthStatus.ShouldBe("Warning");
            healthItem.HealthMessage.ShouldBe("连接器密钥已轮换，请重新测试连接。");
        });
    }

    private async Task<IntegrationConnector> CreateConnectorAsync()
    {
        var connector = new IntegrationConnector(
            Guid.NewGuid(),
            $"health-{Guid.NewGuid():N}"[..32],
            "Test",
            "健康检查测试连接器",
            "{}",
            """["SendPersonalMessage"]""",
            _currentTenant.Id,
            isBuiltin: false,
            isEnabled: true,
            icon: "test",
            sort: 999,
            remark: "用于验证连接器健康检查。");
        await _connectorRepository.InsertAsync(connector, autoSave: true);
        return connector;
    }
}
