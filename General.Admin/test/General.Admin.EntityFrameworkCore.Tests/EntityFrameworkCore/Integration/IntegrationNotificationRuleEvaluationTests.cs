using System;
using System.Linq;
using System.Threading.Tasks;
using General.Admin.EntityFrameworkCore;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace General.Admin.Integration;

[Collection(AdminTestConsts.CollectionDefinitionName)]
public class IntegrationNotificationRuleEvaluationTests : AdminEntityFrameworkCoreTestBase
{
    private readonly IRepository<IntegrationConnector, Guid> _connectorRepository;
    private readonly IRepository<IntegrationDeliveryRecord, Guid> _deliveryRecordRepository;
    private readonly IRepository<IntegrationExternalAccount, Guid> _externalAccountRepository;
    private readonly IRepository<IntegrationUserBinding, Guid> _userBindingRepository;
    private readonly IntegrationNotificationService _service;

    public IntegrationNotificationRuleEvaluationTests()
    {
        _connectorRepository = GetRequiredService<IRepository<IntegrationConnector, Guid>>();
        _deliveryRecordRepository = GetRequiredService<IRepository<IntegrationDeliveryRecord, Guid>>();
        _externalAccountRepository = GetRequiredService<IRepository<IntegrationExternalAccount, Guid>>();
        _service = GetRequiredService<IntegrationNotificationService>();
        _userBindingRepository = GetRequiredService<IRepository<IntegrationUserBinding, Guid>>();
    }

    [Fact]
    public async Task EvaluateEventAsync_Should_Match_Condition_And_Render_Template()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var template = await CreateTemplateAsync("任务提醒 {{project.name}}", "任务 {{task.name}} 即将到期，SPI={{project.spi}}。");
            await CreateRuleAsync(
                template.Id,
                "project.task.due",
                conditionJson: """
                               {
                                 "equals": { "task.status": "Doing" },
                                 "min": { "project.spi": 0.8 }
                               }
                               """);

            var result = await _service.EvaluateEventAsync(new IntegrationNotificationEventInput
            {
                EventCode = "project.task.due",
                VariablesJson = """
                                {
                                  "project": { "name": "权限模型项目", "spi": 0.92 },
                                  "task": { "name": "接口联调", "status": "Doing" }
                                }
                                """
            });

            result.TotalRuleCount.ShouldBe(1);
            result.ReadyCount.ShouldBe(1);
            result.BlockedCount.ShouldBe(0);

            var item = result.Items.Single();
            item.CanSend.ShouldBeTrue();
            item.BlockReason.ShouldBeEmpty();
            item.RenderedTitle.ShouldBe("任务提醒 权限模型项目");
            item.RenderedBody.ShouldBe("任务 接口联调 即将到期，SPI=0.92。");
        });
    }

    [Fact]
    public async Task EvaluateEventAsync_Should_Block_Rule_In_Cross_Day_Quiet_Period()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var template = await CreateTemplateAsync("风险提醒", "风险 {{risk.name}} 需要处理。");
            await CreateRuleAsync(
                template.Id,
                "project.risk.due",
                quietStartTime: "22:00",
                quietEndTime: "08:00");

            var result = await _service.EvaluateEventAsync(new IntegrationNotificationEventInput
            {
                EventCode = "project.risk.due",
                OccurredAt = new DateTime(2026, 6, 26, 23, 30, 0, DateTimeKind.Utc),
                VariablesJson = """{"risk":{"name":"需求延期"}}"""
            });

            result.TotalRuleCount.ShouldBe(1);
            result.ReadyCount.ShouldBe(0);
            result.BlockedCount.ShouldBe(1);
            result.Items.Single().BlockReason.ShouldBe("当前处于规则静默期。");
        });
    }

    [Fact]
    public async Task EvaluateEventAsync_Should_Block_When_Rate_Limit_Window_Is_Hit()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var template = await CreateTemplateAsync("周报提醒", "请填写 {{report.name}}。");
            await CreateRuleAsync(
                template.Id,
                "project.weekly-report.due",
                rateLimitWindowMinutes: 60,
                rateLimitMaxCount: 1);

            var first = await _service.EvaluateEventAsync(new IntegrationNotificationEventInput
            {
                CommitTriggered = true,
                EventCode = "project.weekly-report.due",
                OccurredAt = new DateTime(2026, 6, 26, 10, 0, 0, DateTimeKind.Utc),
                VariablesJson = """{"report":{"name":"本周项目周报"}}"""
            });

            first.ReadyCount.ShouldBe(1);
            first.Items.Single().CanSend.ShouldBeTrue();

            var second = await _service.EvaluateEventAsync(new IntegrationNotificationEventInput
            {
                EventCode = "project.weekly-report.due",
                OccurredAt = new DateTime(2026, 6, 26, 10, 30, 0, DateTimeKind.Utc),
                VariablesJson = """{"report":{"name":"本周项目周报"}}"""
            });

            second.ReadyCount.ShouldBe(0);
            second.BlockedCount.ShouldBe(1);
            second.Items.Single().BlockReason.ShouldBe("规则在 60 分钟频控窗口内已触发。");
        });
    }

    [Fact]
    public async Task DispatchEventAsync_Should_Create_Delivery_Record_For_Direct_Target()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var connector = await CreateConnectorAsync();
            var template = await CreateTemplateAsync("任务提醒 {{project.name}}", "任务 {{task.name}} 即将到期。");
            await CreateRuleAsync(
                template.Id,
                "project.task.due",
                connector.Id,
                recipientRuleJson: """{"mode":"Direct","targetIds":["ou_test"],"targetType":"OpenId"}""");

            var result = await _service.DispatchEventAsync(new IntegrationNotificationEventInput
            {
                EventCode = "project.task.due",
                VariablesJson = """
                                {
                                  "project": { "id": "project-1", "name": "权限模型项目" },
                                  "task": { "id": "task-1", "name": "接口联调" }
                                }
                                """
            });

            result.FailedCount.ShouldBe(1);
            result.Items.Count.ShouldBe(1);
            var item = result.Items.Single();
            item.TargetId.ShouldBe("ou_test");
            item.TargetType.ShouldBe("OpenId");
            item.DeliveryRecordId.ShouldNotBeNull();

            var record = await _deliveryRecordRepository.GetAsync(item.DeliveryRecordId!.Value);
            record.SourceType.ShouldBe("NotificationRule");
            record.BusinessKey.ShouldBe("project-1");
            record.TargetId.ShouldBe("ou_test");
            record.PayloadPreview.ShouldContain("任务提醒 权限模型项目");
            record.PayloadPreview.ShouldContain("任务 接口联调 即将到期。");
        });
    }

    [Fact]
    public async Task DispatchEventAsync_Should_Resolve_Internal_User_Bindings()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var connector = await CreateConnectorAsync();
            var internalUserId = Guid.NewGuid();
            var externalAccount = new IntegrationExternalAccount(
                Guid.NewGuid(),
                connector.Id,
                connector.ConnectorCode,
                connector.ProviderType,
                "ou_bound",
                null,
                "张三",
                status: IntegrationBindingStatus.Bound);
            await _externalAccountRepository.InsertAsync(externalAccount, autoSave: true);
            await _userBindingRepository.InsertAsync(new IntegrationUserBinding(
                Guid.NewGuid(),
                connector.Id,
                connector.ConnectorCode,
                connector.ProviderType,
                externalAccount.Id,
                "ou_bound",
                "张三",
                null,
                internalUserId,
                "张三",
                IntegrationBindingMatchTypes.Manual,
                IntegrationBindingStatus.Bound), autoSave: true);
            var template = await CreateTemplateAsync("工时提醒", "请补充 {{report.weekStartDate}} 起始周的工时。");
            await CreateRuleAsync(
                template.Id,
                "project.weekly-report.due",
                connector.Id,
                recipientRuleJson: """{"mode":"InternalUsers","userIdsPath":"recipient.userIds","targetType":"OpenId"}""");

            var result = await _service.DispatchEventAsync(new IntegrationNotificationEventInput
            {
                EventCode = "project.weekly-report.due",
                VariablesJson = $$"""
                                {
                                  "recipient": { "userIds": ["{{internalUserId}}"] },
                                  "report": { "weekStartDate": "2026-06-22" }
                                }
                                """
            });

            result.FailedCount.ShouldBe(1);
            var item = result.Items.Single();
            item.TargetId.ShouldBe("ou_bound");
            item.TargetType.ShouldBe("OpenId");
            item.DeliveryRecordId.ShouldNotBeNull();
        });
    }

    private async Task<IntegrationMessageTemplateDto> CreateTemplateAsync(string titleTemplate, string bodyTemplate)
    {
        return await _service.SaveTemplateAsync(new IntegrationMessageTemplateSaveInput
        {
            BodyTemplate = bodyTemplate,
            CardJson = "{}",
            MessageType = IntegrationMessageTemplateTypes.Markdown,
            Name = $"测试模板-{Guid.NewGuid():N}",
            PreviewVariablesJson = "{}",
            Status = IntegrationMessageTemplateStatus.Active,
            TemplateCode = $"tpl-{Guid.NewGuid():N}"[..32],
            TitleTemplate = titleTemplate,
            VariableSchemaJson = "{}"
        });
    }

    private async Task<IntegrationNotificationRuleDto> CreateRuleAsync(
        Guid templateId,
        string eventCode,
        Guid? connectorId = null,
        string conditionJson = "{}",
        string quietStartTime = "",
        string quietEndTime = "",
        int rateLimitWindowMinutes = 0,
        int rateLimitMaxCount = 0,
        string recipientRuleJson = """{"mode":"Direct","targetIds":["ou_test"]}""")
    {
        return await _service.SaveRuleAsync(new IntegrationNotificationRuleSaveInput
        {
            ChannelType = IntegrationNotificationChannelTypes.Personal,
            ConditionJson = conditionJson,
            ConnectorId = connectorId,
            EventCode = eventCode,
            EventName = eventCode,
            Name = $"测试规则-{Guid.NewGuid():N}",
            QuietEndTime = quietEndTime,
            QuietStartTime = quietStartTime,
            RateLimitMaxCount = rateLimitMaxCount,
            RateLimitWindowMinutes = rateLimitWindowMinutes,
            RecipientRuleJson = recipientRuleJson,
            RuleCode = $"rule-{Guid.NewGuid():N}"[..32],
            Status = IntegrationNotificationRuleStatus.Active,
            TargetType = "OpenId",
            TemplateId = templateId
        });
    }

    private async Task<IntegrationConnector> CreateConnectorAsync()
    {
        var connector = new IntegrationConnector(
            Guid.NewGuid(),
            $"noop-{Guid.NewGuid():N}"[..32],
            "Noop",
            "测试连接器",
            "{}",
            """["message"]""",
            null);
        await _connectorRepository.InsertAsync(connector, autoSave: true);
        return connector;
    }
}
