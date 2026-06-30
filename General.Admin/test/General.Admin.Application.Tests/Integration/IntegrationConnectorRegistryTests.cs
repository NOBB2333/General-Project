using System.Linq;
using System.Text.Json;
using Shouldly;
using Xunit;

namespace General.Admin.Integration;

public class IntegrationConnectorRegistryTests
{
    [Theory]
    [InlineData("dingtalk", "DingTalk", "钉钉", "appKey", "appSecret")]
    [InlineData("wecom", "WeCom", "企业微信", "corpId", "corpSecret")]
    public void GetBuiltinDescriptors_Should_Contain_Common_Enterprise_Connectors(
        string connectorCode,
        string providerType,
        string displayName,
        string requiredConfigKey,
        string secretConfigKey)
    {
        var descriptor = new IntegrationConnectorRegistry().Find(connectorCode);

        descriptor.ShouldNotBeNull();
        descriptor.ProviderType.ShouldBe(providerType);
        descriptor.DisplayName.ShouldBe(displayName);
        descriptor.Capabilities.ShouldContain("SendPersonalMessage");
        descriptor.Capabilities.ShouldContain("SendGroupMessage");
        descriptor.Capabilities.ShouldContain("SyncDepartment");
        descriptor.Capabilities.ShouldContain("SyncUser");
        descriptor.Capabilities.ShouldContain("WebhookEvent");

        using var schema = JsonDocument.Parse(descriptor.ConfigSchemaJson);
        var fields = schema.RootElement.GetProperty("fields").EnumerateArray().ToList();
        fields.ShouldContain(field =>
            field.GetProperty("Key").GetString() == requiredConfigKey &&
            field.GetProperty("Required").GetBoolean() &&
            !field.GetProperty("Secret").GetBoolean());
        fields.ShouldContain(field =>
            field.GetProperty("Key").GetString() == secretConfigKey &&
            field.GetProperty("Required").GetBoolean() &&
            field.GetProperty("Secret").GetBoolean());
    }

    [Fact]
    public void GetBuiltinDescriptors_Should_Contain_Sandbox_Connector_For_Local_Acceptance()
    {
        var descriptor = new IntegrationConnectorRegistry().Find("sandbox");

        descriptor.ShouldNotBeNull();
        descriptor.ProviderType.ShouldBe("Sandbox");
        descriptor.DisplayName.ShouldBe("集成沙箱");
        descriptor.Capabilities.ShouldContain("SendPersonalMessage");
        descriptor.Capabilities.ShouldContain("SendGroupMessage");
        descriptor.Capabilities.ShouldContain("SyncDepartment");
        descriptor.Capabilities.ShouldContain("SyncUser");
        descriptor.Capabilities.ShouldContain("SyncGroup");
        descriptor.Capabilities.ShouldContain("SyncMeeting");
        descriptor.Capabilities.ShouldContain("WebhookEvent");
        descriptor.Remark.ShouldContain("本地");

        using var schema = JsonDocument.Parse(descriptor.ConfigSchemaJson);
        var fields = schema.RootElement.GetProperty("fields").EnumerateArray().ToList();
        fields.ShouldContain(field => field.GetProperty("Key").GetString() == "acceptMessage");
        fields.ShouldContain(field => field.GetProperty("Key").GetString() == "forceFailure");
    }
}
