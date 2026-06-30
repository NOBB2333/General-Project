using System.Text.Json;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace General.Admin.Integration;

public class WeComConnectorAdapterTests
{
    [Fact]
    public void BuildAppMessagePayload_Should_Build_Markdown_App_Message()
    {
        var payload = WeComConnectorAdapter.BuildAppMessagePayload(
            "10001",
            "Markdown",
            "项目 **风险** 提醒",
            new WeComConnectorAdapter.WeComAppMessageTarget("user-1|user-2", null, null));

        using var document = JsonDocument.Parse(payload);
        document.RootElement.GetProperty("agentid").GetString().ShouldBe("10001");
        document.RootElement.GetProperty("touser").GetString().ShouldBe("user-1|user-2");
        document.RootElement.TryGetProperty("toparty", out _).ShouldBeFalse();
        document.RootElement.GetProperty("msgtype").GetString().ShouldBe("markdown");
        document.RootElement.GetProperty("markdown").GetProperty("content").GetString()
            .ShouldBe("项目 **风险** 提醒");
    }

    [Fact]
    public void BuildRobotMessagePayload_Should_Build_Template_Card_Message()
    {
        var payload = WeComConnectorAdapter.BuildRobotMessagePayload(
            "Card",
            """
            {
              "card_type": "text_notice",
              "source": {
                "icon_url": "https://example.com/icon.png",
                "desc": "项目管理"
              },
              "main_title": {
                "title": "项目风险提醒"
              }
            }
            """);

        using var document = JsonDocument.Parse(payload);
        document.RootElement.GetProperty("msgtype").GetString().ShouldBe("template_card");
        document.RootElement.GetProperty("template_card").GetProperty("card_type").GetString().ShouldBe("text_notice");
        document.RootElement.GetProperty("template_card").GetProperty("main_title").GetProperty("title").GetString()
            .ShouldBe("项目风险提醒");
    }

    [Fact]
    public void BuildRobotMessagePayload_Should_Reject_Invalid_Template_Card()
    {
        var exception = Should.Throw<UserFriendlyException>(() =>
            WeComConnectorAdapter.BuildRobotMessagePayload("Card", "不是 JSON"));

        exception.Message.ShouldContain("企业微信卡片消息内容不是合法 JSON");
    }

    [Fact]
    public void BuildSyncOptions_Should_Merge_Config_And_Run_Request()
    {
        var options = WeComConnectorAdapter.BuildSyncOptions(
            """
            {
              "rootDepartmentId": "1",
              "fetchChildUsers": false,
              "autoMatchUsers": true
            }
            """,
            """
            {
              "RequestJson": "{\"rootDepartmentId\":\"18\",\"fetchChildUsers\":true,\"autoMatchUsers\":false}"
            }
            """);

        options.RootDepartmentId.ShouldBe("18");
        options.FetchChildUsers.ShouldBeTrue();
        options.AutoMatchUsers.ShouldBeFalse();
    }

    [Fact]
    public void ParseDepartmentItem_Should_Read_WeCom_Department_Fields()
    {
        using var document = JsonDocument.Parse("""
                                                {
                                                  "id": 18,
                                                  "parentid": 1,
                                                  "name": "交付中心"
                                                }
                                                """);

        var item = WeComConnectorAdapter.ParseDepartmentItem(document.RootElement, "1");

        item.ExternalDepartmentId.ShouldBe("18");
        item.ParentExternalDepartmentId.ShouldBe("1");
        item.Name.ShouldBe("交付中心");
        item.RawJson.ShouldContain("交付中心");
    }

    [Fact]
    public void ParseUserItem_Should_Read_WeCom_User_Fields()
    {
        using var document = JsonDocument.Parse("""
                                                {
                                                  "userid": "wc-user",
                                                  "unionid": "union-user",
                                                  "name": "王项目",
                                                  "mobile": "13800138000",
                                                  "email": "pm@general.local",
                                                  "department": [18],
                                                  "avatar": "https://avatar.example.com/pm.png"
                                                }
                                                """);

        var item = WeComConnectorAdapter.ParseUserItem(document.RootElement, "1");

        item.ExternalUserId.ShouldBe("wc-user");
        item.ExternalUnionId.ShouldBe("union-user");
        item.DisplayName.ShouldBe("王项目");
        item.Mobile.ShouldBe("13800138000");
        item.Email.ShouldBe("pm@general.local");
        item.DepartmentExternalId.ShouldBe("18");
        item.AvatarUrl.ShouldBe("https://avatar.example.com/pm.png");
    }

    [Fact]
    public void BuildSyncOptions_Should_Reject_Invalid_Json()
    {
        var exception = Should.Throw<UserFriendlyException>(() =>
            WeComConnectorAdapter.BuildSyncOptions("{", "{}"));

        exception.Message.ShouldBe("企业微信同步配置 JSON 格式不正确。");
    }
}
