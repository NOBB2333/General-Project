using System.Text.Json;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace General.Admin.Integration;

public class FeishuConnectorAdapterTests
{
    [Fact]
    public void BuildMessageContent_Should_Build_Interactive_Card_Content()
    {
        const string body = """
                            {
                              "config": {
                                "wide_screen_mode": true
                              },
                              "header": {
                                "title": {
                                  "tag": "plain_text",
                                  "content": "项目风险提醒"
                                }
                              },
                              "elements": []
                            }
                            """;

        var result = FeishuConnectorAdapter.BuildMessageContent("Card", body);

        result.MessageType.ShouldBe("interactive");
        using var document = JsonDocument.Parse(result.Content);
        document.RootElement.GetProperty("config").GetProperty("wide_screen_mode").GetBoolean().ShouldBeTrue();
        document.RootElement.GetProperty("header").GetProperty("title").GetProperty("content").GetString()
            .ShouldBe("项目风险提醒");
    }

    [Fact]
    public void BuildMessageContent_Should_Reject_Invalid_Card_Json()
    {
        var exception = Should.Throw<UserFriendlyException>(() =>
            FeishuConnectorAdapter.BuildMessageContent("Card", "不是 JSON"));

        exception.Message.ShouldContain("飞书互动卡片内容不是合法 JSON");
    }

    [Fact]
    public void BuildMessageContent_Should_Reject_Non_Object_Card_Json()
    {
        var exception = Should.Throw<UserFriendlyException>(() =>
            FeishuConnectorAdapter.BuildMessageContent("InteractiveCard", "[]"));

        exception.Message.ShouldBe("飞书互动卡片内容必须是 JSON 对象。");
    }

    [Fact]
    public void BuildMessageContent_Should_Keep_Text_And_Markdown_Message_Payloads()
    {
        var text = FeishuConnectorAdapter.BuildMessageContent("Text", "普通通知");
        text.MessageType.ShouldBe("text");
        using var textDocument = JsonDocument.Parse(text.Content);
        textDocument.RootElement.GetProperty("text").GetString().ShouldBe("普通通知");

        var markdown = FeishuConnectorAdapter.BuildMessageContent("Markdown", "项目 **风险** 提醒");
        markdown.MessageType.ShouldBe("post");
        using var markdownDocument = JsonDocument.Parse(markdown.Content);
        var zhCn = markdownDocument.RootElement.GetProperty("post").GetProperty("zh_cn");
        zhCn.GetProperty("title").GetString().ShouldBe("项目通知");
        zhCn.GetProperty("content")[0][0].GetProperty("text").GetString().ShouldBe("项目 **风险** 提醒");
    }

    [Fact]
    public void BuildSyncOptions_Should_Merge_Config_And_Run_Request()
    {
        var options = FeishuConnectorAdapter.BuildSyncOptions(
            """
            {
              "rootDepartmentId": "0",
              "pageSize": 100,
              "autoMatchUsers": true
            }
            """,
            """
            {
              "RequestJson": "{\"rootDepartmentId\":\"od-test\",\"recursiveDepartment\":false,\"autoMatchUsers\":false}"
            }
            """);

        options.RootDepartmentId.ShouldBe("od-test");
        options.PageSize.ShouldBe(50);
        options.RecursiveDepartment.ShouldBeFalse();
        options.AutoMatchUsers.ShouldBeFalse();
        options.DepartmentIdType.ShouldBe("open_department_id");
        options.UserIdType.ShouldBe("user_id");
    }

    [Fact]
    public void ParseDepartmentItem_Should_Read_Feishu_Department_Fields()
    {
        using var document = JsonDocument.Parse("""
                                                {
                                                  "open_department_id": "od-child",
                                                  "parent_department_id": "od-parent",
                                                  "name": "交付中心"
                                                }
                                                """);

        var item = FeishuConnectorAdapter.ParseDepartmentItem(document.RootElement, "0");

        item.ExternalDepartmentId.ShouldBe("od-child");
        item.ParentExternalDepartmentId.ShouldBe("od-parent");
        item.Name.ShouldBe("交付中心");
        item.RawJson.ShouldContain("od-child");
    }

    [Fact]
    public void ParseUserItem_Should_Read_Feishu_User_Fields()
    {
        using var document = JsonDocument.Parse("""
                                                {
                                                  "user_id": "ou-user",
                                                  "union_id": "on-user",
                                                  "name": "王项目",
                                                  "mobile": "13800138000",
                                                  "email": "pm@general.local",
                                                  "department_ids": ["od-delivery"],
                                                  "avatar": {
                                                    "avatar_240": "https://avatar.example.com/pm.png"
                                                  }
                                                }
                                                """);

        var item = FeishuConnectorAdapter.ParseUserItem(document.RootElement, "0");

        item.ExternalUserId.ShouldBe("ou-user");
        item.ExternalUnionId.ShouldBe("on-user");
        item.DisplayName.ShouldBe("王项目");
        item.Mobile.ShouldBe("13800138000");
        item.Email.ShouldBe("pm@general.local");
        item.DepartmentExternalId.ShouldBe("od-delivery");
        item.AvatarUrl.ShouldBe("https://avatar.example.com/pm.png");
    }
}
