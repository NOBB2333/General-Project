using System;
using System.Linq;
using System.Threading.Tasks;
using General.Admin.EntityFrameworkCore;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Xunit;

namespace General.Admin.Integration;

[Collection(AdminTestConsts.CollectionDefinitionName)]
public class IntegrationBindingServiceTests : AdminEntityFrameworkCoreTestBase
{
    private readonly IRepository<IntegrationConnector, Guid> _connectorRepository;
    private readonly IRepository<IntegrationExternalAccount, Guid> _externalAccountRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;
    private readonly IntegrationBindingService _service;

    public IntegrationBindingServiceTests()
    {
        _connectorRepository = GetRequiredService<IRepository<IntegrationConnector, Guid>>();
        _externalAccountRepository = GetRequiredService<IRepository<IntegrationExternalAccount, Guid>>();
        _userRepository = GetRequiredService<IRepository<IdentityUser, Guid>>();
        _service = GetRequiredService<IntegrationBindingService>();
    }

    [Fact]
    public async Task BindUserAsync_Should_Bind_Feishu_Account_And_Create_Id_Mapping()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var connector = await GetOrCreateFeishuConnectorAsync();
            var userId = await CreateUserAsync();
            var externalUserId = $"ou_{Guid.NewGuid():N}";
            var account = await _service.SaveExternalAccountAsync(new IntegrationExternalAccountSaveInput
            {
                ConnectorId = connector.Id,
                DisplayName = "飞书测试用户",
                Email = "feishu.binding@general.local",
                ExternalUnionId = $"on_{Guid.NewGuid():N}",
                ExternalUserId = externalUserId,
                Mobile = "13800138000",
                RawJson = """{"source":"feishu-test"}""",
                Status = IntegrationBindingStatus.Pending
            });

            var binding = await _service.BindUserAsync(new IntegrationUserBindingSaveInput
            {
                ExternalAccountId = account.Id,
                InternalUserId = userId,
                MatchType = IntegrationBindingMatchTypes.Manual,
                Remark = "飞书用户绑定回归测试"
            });

            binding.ConnectorCode.ShouldBe("feishu");
            binding.ExternalUserId.ShouldBe(externalUserId);
            binding.InternalUserId.ShouldBe(userId);
            binding.MatchType.ShouldBe(IntegrationBindingMatchTypes.Manual);
            binding.Status.ShouldBe(IntegrationBindingStatus.Bound);
            binding.BoundTime.ShouldNotBeNull();

            var savedAccount = await _externalAccountRepository.GetAsync(account.Id);
            savedAccount.Status.ShouldBe(IntegrationBindingStatus.Bound);

            var mappings = await _service.GetIdMappingsAsync(new IntegrationIdMappingQueryInput
            {
                ConnectorId = connector.Id,
                Keyword = externalUserId,
                MaxResultCount = 20
            });
            mappings.TotalCount.ShouldBe(1);
            var mapping = mappings.Items.Single();
            mapping.ConnectorCode.ShouldBe("feishu");
            mapping.InternalObjectType.ShouldBe("User");
            mapping.InternalObjectId.ShouldBe(userId.ToString());
            mapping.ExternalObjectType.ShouldBe("ExternalUser");
            mapping.ExternalObjectId.ShouldBe(externalUserId);
            mapping.Source.ShouldBe(IntegrationBindingMatchTypes.Manual);
            mapping.Status.ShouldBe(IntegrationBindingStatus.Bound);
        });
    }

    private async Task<IntegrationConnector> GetOrCreateFeishuConnectorAsync()
    {
        var connector = await _connectorRepository.FindAsync(item => item.ConnectorCode == "feishu");
        if (connector != null)
        {
            return connector;
        }

        connector = new IntegrationConnector(
            Guid.NewGuid(),
            "feishu",
            "Feishu",
            "飞书",
            "{}",
            """["SendPersonalMessage","SendGroupMessage","MarkdownMessage","InteractiveCard","SyncDepartment","SyncUser","WebhookEvent"]""",
            tenantId: null,
            isBuiltin: true,
            isEnabled: true,
            icon: "icon-feishu",
            sort: 10,
            remark: "用于验证飞书用户绑定。");
        await _connectorRepository.InsertAsync(connector, autoSave: true);
        return connector;
    }

    private async Task<Guid> CreateUserAsync()
    {
        var userId = Guid.NewGuid();
        var userName = $"feishu.binding.{userId:N}"[..32];
        var user = new IdentityUser(userId, userName, "feishu.binding@general.local")
        {
            Name = "飞书",
            Surname = "测试用户"
        };
        await _userRepository.InsertAsync(user, autoSave: true);
        return userId;
    }
}
