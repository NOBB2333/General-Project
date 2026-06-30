using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using General.Admin.EntityFrameworkCore;
using General.Admin.Settings;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.SettingManagement;
using Volo.Abp.Settings;
using Xunit;

namespace General.Admin.Integration;

[Collection(AdminTestConsts.CollectionDefinitionName)]
public class IntegrationWebhookServiceTests : AdminEntityFrameworkCoreTestBase
{
    private readonly IRepository<IntegrationConnector, Guid> _connectorRepository;
    private readonly IRepository<IntegrationTenantConfig, Guid> _configRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly ISettingManager _settingManager;
    private readonly IRepository<IntegrationWebhookEvent, Guid> _webhookEventRepository;
    private readonly IntegrationWebhookService _service;

    public IntegrationWebhookServiceTests()
    {
        _connectorRepository = GetRequiredService<IRepository<IntegrationConnector, Guid>>();
        _configRepository = GetRequiredService<IRepository<IntegrationTenantConfig, Guid>>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
        _settingManager = GetRequiredService<ISettingManager>();
        _webhookEventRepository = GetRequiredService<IRepository<IntegrationWebhookEvent, Guid>>();
        _service = GetRequiredService<IntegrationWebhookService>();
    }

    [Fact]
    public async Task ReceiveAsync_Should_Keep_Original_Event_When_Dedupe_Key_Repeats()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var dedupeKey = $"dedupe-{Guid.NewGuid():N}";
            var first = await _service.ReceiveAsync(new IntegrationWebhookReceiveInput
            {
                ConnectorCode = "feishu",
                DedupeKey = dedupeKey,
                EventId = $"evt-{Guid.NewGuid():N}",
                EventType = "card.action",
                ProviderType = "Feishu",
                RawPayloadJson = """{"event":{"action":"open_project","project_id":"50000000-0000-0000-0000-000000000001"}}""",
                SignatureStatus = "Valid"
            });

            var second = await _service.ReceiveAsync(new IntegrationWebhookReceiveInput
            {
                ConnectorCode = "feishu",
                DedupeKey = dedupeKey,
                EventId = $"evt-{Guid.NewGuid():N}",
                EventType = "card.action",
                ProviderType = "Feishu",
                RawPayloadJson = """{"event":{"action":"duplicate_action","project_id":"duplicate"}}""",
                SignatureStatus = "Valid"
            });

            second.Id.ShouldBe(first.Id);
            second.Status.ShouldBe("Parsed");
            second.ActionCode.ShouldBe("open_project");
            second.BusinessKey.ShouldBe("50000000-0000-0000-0000-000000000001");

            var savedEvents = await _webhookEventRepository.GetListAsync(item => item.DedupeKey == dedupeKey);
            savedEvents.Count.ShouldBe(1);
            savedEvents[0].Status.ShouldBe("Parsed");
            savedEvents[0].ActionCode.ShouldBe("open_project");
        });
    }

    [Fact]
    public async Task ReceiveAsync_Should_Parse_Feishu_Webhook_With_Verification_Token()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var eventId = $"evt-feishu-{Guid.NewGuid():N}";
            await EnsureConnectorConfigAsync(
                "feishu",
                "Feishu",
                """{"verificationToken":"verify-token"}""");

            var result = await _service.ReceiveAsync(new IntegrationWebhookReceiveInput
            {
                ConnectorCode = "feishu",
                ProviderType = "Feishu",
                RawBody = $$"""
                          {
                            "schema": "2.0",
                            "header": {
                              "event_id": "{{eventId}}",
                              "event_type": "card.action",
                              "token": "verify-token"
                            },
                            "event": {
                              "action": {
                                "value": {
                                  "action": "open_project",
                                  "project_id": "50000000-0000-0000-0000-000000000001"
                                }
                              }
                            }
                          }
                          """
            });

            result.Status.ShouldBe("Parsed");
            result.SignatureStatus.ShouldBe("Valid");
            result.EventType.ShouldBe("card.action");
            result.DedupeKey.ShouldBe($"feishu:{eventId}");
            result.ActionCode.ShouldBe("open_project");
            result.BusinessKey.ShouldBe("50000000-0000-0000-0000-000000000001");
            result.NormalizedPayloadJson.ShouldContain("\"connector_code\":\"feishu\"");
        });
    }

    [Fact]
    public async Task ReceiveAsync_Should_Keep_Event_When_Feishu_Verification_Token_Is_Invalid()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var eventId = $"evt-feishu-invalid-{Guid.NewGuid():N}";
            await EnsureConnectorConfigAsync(
                "feishu",
                "Feishu",
                """{"verificationToken":"verify-token"}""");

            var result = await _service.ReceiveAsync(new IntegrationWebhookReceiveInput
            {
                ConnectorCode = "feishu",
                ProviderType = "Feishu",
                RawBody = $$"""
                          {
                            "header": {
                              "event_id": "{{eventId}}",
                              "event_type": "card.action",
                              "token": "wrong-token"
                            },
                            "event": {
                              "action": {
                                "value": {
                                  "action": "open_project",
                                  "project_id": "project-1"
                                }
                              }
                            }
                          }
                          """
            });

            result.Status.ShouldBe("Failed");
            result.SignatureStatus.ShouldBe("Invalid");
            result.ErrorMessage.ShouldBe("Webhook 签名校验失败。");

            var saved = await _webhookEventRepository.GetAsync(result.Id);
            saved.RawBody.ShouldContain("wrong-token");
        });
    }

    [Fact]
    public async Task ReceiveAsync_Should_Parse_DingTalk_Webhook_With_Sha1_Signature()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var eventId = $"evt-dingtalk-{Guid.NewGuid():N}";
            await EnsureConnectorConfigAsync(
                "dingtalk",
                "DingTalk",
                """{"callbackToken":"dt-token"}""");
            var rawBody = $$"""
                            {
                              "eventType": "card.action",
                              "eventId": "{{eventId}}",
                              "action": "open_project",
                              "businessKey": "project-1"
                            }
                            """;
            const string timestamp = "1782438101";
            const string nonce = "nonce-1";
            var signature = ComputeSortedSha1("dt-token", timestamp, nonce, rawBody);

            var result = await _service.ReceiveAsync(new IntegrationWebhookReceiveInput
            {
                ConnectorCode = "dingtalk",
                HeadersJson = $$"""{"timestamp":"{{timestamp}}","nonce":"{{nonce}}","signature":"{{signature}}"}""",
                ProviderType = "DingTalk",
                RawBody = rawBody
            });

            result.Status.ShouldBe("Parsed");
            result.SignatureStatus.ShouldBe("Valid");
            result.EventType.ShouldBe("card.action");
            result.DedupeKey.ShouldBe($"dingtalk:{eventId}");
            result.ActionCode.ShouldBe("open_project");
            result.BusinessKey.ShouldBe("project-1");
        });
    }

    [Fact]
    public async Task ReceiveAsync_Should_Parse_WeCom_Webhook_With_Sha1_Signature()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var eventId = $"evt-wecom-{Guid.NewGuid():N}";
            await EnsureConnectorConfigAsync(
                "wecom",
                "WeCom",
                """{"token":"wc-token"}""");
            var rawBody = $$"""
                            {
                              "Event": "card_action",
                              "MsgId": "{{eventId}}",
                              "EventKey": "open_project",
                              "businessKey": "project-1"
                            }
                            """;
            const string timestamp = "1782438101";
            const string nonce = "nonce-1";
            var signature = ComputeSortedSha1("wc-token", timestamp, nonce, rawBody);

            var result = await _service.ReceiveAsync(new IntegrationWebhookReceiveInput
            {
                ConnectorCode = "wecom",
                HeadersJson = $$"""{"timestamp":"{{timestamp}}","nonce":"{{nonce}}","msg_signature":"{{signature}}"}""",
                ProviderType = "WeCom",
                RawBody = rawBody
            });

            result.Status.ShouldBe("Parsed");
            result.SignatureStatus.ShouldBe("Valid");
            result.EventType.ShouldBe("card_action");
            result.DedupeKey.ShouldBe($"wecom:{eventId}");
            result.ActionCode.ShouldBe("open_project");
            result.BusinessKey.ShouldBe("project-1");
        });
    }

    [Fact]
    public async Task ReceiveAsync_Should_Parse_Group_Command_Text()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectKey = $"PRJ-{Guid.NewGuid():N}"[..12];
            var result = await _service.ReceiveAsync(new IntegrationWebhookReceiveInput
            {
                ConnectorCode = "feishu",
                DedupeKey = $"command-{Guid.NewGuid():N}",
                EventType = "im.message.receive_v1",
                ProviderType = "Feishu",
                RawPayloadJson = $$"""
                                  {
                                    "event": {
                                      "message": {
                                        "text": "/project {{projectKey}}"
                                      }
                                    }
                                  }
                                  """,
                SignatureStatus = "Valid"
            });

            result.Status.ShouldBe("Parsed");
            result.ActionCode.ShouldBe("command.project");
            result.BusinessKey.ShouldBe(projectKey);
        });
    }

    [Fact]
    public async Task ReceiveAsync_Should_Block_When_Webhook_Rate_Limit_Is_Exceeded()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            await _settingManager.SetAsync(
                AdminSettings.IntegrationWebhookRequestLimitPerMinute,
                "1",
                GlobalSettingValueProvider.ProviderName,
                null);

            var sourceIp = $"10.0.0.{Random.Shared.Next(1, 200)}";
            try
            {
                var input = new IntegrationWebhookReceiveInput
                {
                    ConnectorCode = "feishu",
                    EventId = $"evt-{Guid.NewGuid():N}",
                    EventType = "card.action",
                    ProviderType = "Feishu",
                    RawPayloadJson = """{"event":{"action":"open_project","project_id":"50000000-0000-0000-0000-000000000001"}}""",
                    SignatureStatus = "Valid",
                    SourceIp = sourceIp
                };

                await _service.ReceiveAsync(input);

                var exception = await Should.ThrowAsync<UserFriendlyException>(() => _service.ReceiveAsync(new IntegrationWebhookReceiveInput
                {
                    ConnectorCode = input.ConnectorCode,
                    EventId = $"evt-{Guid.NewGuid():N}",
                    EventType = input.EventType,
                    ProviderType = input.ProviderType,
                    RawPayloadJson = input.RawPayloadJson,
                    SignatureStatus = input.SignatureStatus,
                    SourceIp = sourceIp
                }));
                exception.Message.ShouldContain("外部系统回调请求超过频率限制");

                var savedEvents = await _webhookEventRepository.GetListAsync(item => item.SourceIp == sourceIp);
                savedEvents.Count.ShouldBe(1);
            }
            finally
            {
                await _settingManager.SetAsync(
                    AdminSettings.IntegrationWebhookRequestLimitPerMinute,
                    "120",
                    GlobalSettingValueProvider.ProviderName,
                    null);
            }
        });
    }

    [Fact]
    public async Task ReceiveAsync_Should_Block_When_Source_Ip_Is_Not_In_Whitelist()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            await _settingManager.SetAsync(
                AdminSettings.IntegrationWebhookIpWhitelist,
                "203.0.113.10,2001:db8::/32",
                GlobalSettingValueProvider.ProviderName,
                null);

            try
            {
                var exception = await Should.ThrowAsync<UserFriendlyException>(() => _service.ReceiveAsync(new IntegrationWebhookReceiveInput
                {
                    ConnectorCode = "feishu",
                    EventId = $"evt-{Guid.NewGuid():N}",
                    EventType = "card.action",
                    ProviderType = "Feishu",
                    RawPayloadJson = """{"event":{"action":"open_project","project_id":"50000000-0000-0000-0000-000000000001"}}""",
                    SignatureStatus = "Valid",
                    SourceIp = "198.51.100.20"
                }));
                exception.Message.ShouldContain("Webhook 来源 IP 不在白名单内");

                var savedEvents = await _webhookEventRepository.GetListAsync(item => item.SourceIp == "198.51.100.20");
                savedEvents.Count.ShouldBe(0);
            }
            finally
            {
                await _settingManager.SetAsync(
                    AdminSettings.IntegrationWebhookIpWhitelist,
                    string.Empty,
                    GlobalSettingValueProvider.ProviderName,
                    null);
            }
        });
    }

    private async Task<IntegrationConnector> EnsureConnectorConfigAsync(
        string connectorCode,
        string providerType,
        string configJson)
    {
        var connectors = (await _connectorRepository.GetListAsync())
            .Where(x => x.ConnectorCode == connectorCode)
            .ToList();
        if (connectors.Count == 0)
        {
            var connector = new IntegrationConnector(
                Guid.NewGuid(),
                connectorCode,
                providerType,
                connectorCode,
                "{}",
                "[]",
                _currentTenant.Id);
            await _connectorRepository.InsertAsync(connector, autoSave: true);
            connectors.Add(connector);
        }

        foreach (var connector in connectors)
        {
            var config = await _configRepository.FindAsync(x => x.ConnectorId == connector.Id);
            if (config == null)
            {
                config = new IntegrationTenantConfig(
                    Guid.NewGuid(),
                    connector.Id,
                    _currentTenant.Id,
                    configJson,
                    "secret",
                    isEnabled: true);
                await _configRepository.InsertAsync(config, autoSave: true);
            }
            else
            {
                config.Update(configJson, true, config.CallbackPath, config.Remark);
                await _configRepository.UpdateAsync(config, autoSave: true);
            }
        }

        return connectors[0];
    }

    private static string ComputeSortedSha1(params string[] values)
    {
        var sorted = values
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToArray();
        return Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(string.Concat(sorted)))).ToLowerInvariant();
    }
}
