using System.Text.Json;
using Volo.Abp.DependencyInjection;

namespace General.Admin.Integration;

public class IntegrationConnectorRegistry : ISingletonDependency
{
    private static readonly IntegrationConnectorDescriptor[] BuiltinDescriptors =
    [
        new(
            "feishu",
            "Feishu",
            "飞书 / Lark",
            "lucide:message-square",
            BuildSchema(
                ("appId", "App ID", true, false),
                ("baseUrl", "API Base URL", false, false),
                ("verificationToken", "Verification Token", false, true),
                ("encryptKey", "Encrypt Key", false, true)),
            ["SendPersonalMessage", "SendGroupMessage", "MarkdownMessage", "InteractiveCard", "SyncDepartment", "SyncUser", "WebhookEvent"],
            10,
            "用于飞书组织同步、个人通知、项目群消息和互动卡片。"),
        new(
            "dingtalk",
            "DingTalk",
            "钉钉",
            "lucide:message-circle",
            BuildSchema(
                ("appKey", "App Key", true, false),
                ("appSecret", "App Secret", true, true),
                ("agentId", "Agent ID", false, false),
                ("baseUrl", "API Base URL", false, false),
                ("robotWebhook", "群机器人 Webhook 或 access_token", false, false),
                ("robotSecret", "群机器人签名密钥", false, true)),
            ["SendPersonalMessage", "SendGroupMessage", "MarkdownMessage", "SyncDepartment", "SyncUser", "WebhookEvent"],
            20,
            "用于钉钉工作通知、群机器人消息、组织和用户同步。"),
        new(
            "wecom",
            "WeCom",
            "企业微信",
            "lucide:messages-square",
            BuildSchema(
                ("corpId", "Corp ID", true, false),
                ("corpSecret", "Corp Secret", true, true),
                ("agentId", "Agent ID", true, false),
                ("baseUrl", "API Base URL", false, false),
                ("robotWebhook", "群机器人 Webhook 或 Key", false, false),
                ("token", "回调 Token", false, true),
                ("encodingAesKey", "Encoding AES Key", false, true)),
            ["SendPersonalMessage", "SendGroupMessage", "MarkdownMessage", "SyncDepartment", "SyncUser", "WebhookEvent"],
            30,
            "用于企业微信应用消息、群机器人通知、组织和用户同步。"),
        new(
            "sandbox",
            "Sandbox",
            "集成沙箱",
            "lucide:flask-conical",
            BuildSchema(
                ("acceptMessage", "允许消息投递", false, false),
                ("forceFailure", "强制失败", false, false),
                ("echoPrefix", "回显前缀", false, false)),
            ["SendPersonalMessage", "SendGroupMessage", "MarkdownMessage", "InteractiveCard", "SyncDepartment", "SyncUser", "SyncGroup", "SyncMeeting", "WebhookEvent"],
            90,
            "用于本地、测试和验收环境验证 Integration 投递、同步、Webhook、幂等和重试链路，不代表真实外部平台。")
    ];

    public IReadOnlyList<IntegrationConnectorDescriptor> GetBuiltinDescriptors()
    {
        return BuiltinDescriptors;
    }

    public IntegrationConnectorDescriptor? Find(string connectorCode)
    {
        return BuiltinDescriptors.FirstOrDefault(x =>
            x.ConnectorCode.Equals(connectorCode, StringComparison.OrdinalIgnoreCase));
    }

    private static string BuildSchema(params (string Key, string Label, bool Required, bool Secret)[] fields)
    {
        return JsonSerializer.Serialize(new
        {
            fields = fields.Select(field => new
            {
                field.Key,
                field.Label,
                field.Required,
                field.Secret,
                Type = field.Secret ? "password" : "text"
            })
        });
    }
}
