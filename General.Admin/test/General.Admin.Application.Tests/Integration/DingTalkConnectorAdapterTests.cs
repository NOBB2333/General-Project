using System.Text.Json;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace General.Admin.Integration;

public class DingTalkConnectorAdapterTests
{
    [Fact]
    public void BuildWorkMessagePayload_Should_Build_Markdown_Work_Notice()
    {
        var payload = DingTalkConnectorAdapter.BuildWorkMessagePayload(
            "10001",
            "Markdown",
            "项目 **风险** 提醒",
            new DingTalkConnectorAdapter.DingTalkWorkMessageTarget("user-1,user-2", null, false));

        using var document = JsonDocument.Parse(payload);
        document.RootElement.GetProperty("agent_id").GetString().ShouldBe("10001");
        document.RootElement.GetProperty("userid_list").GetString().ShouldBe("user-1,user-2");
        document.RootElement.TryGetProperty("dept_id_list", out _).ShouldBeFalse();
        document.RootElement.GetProperty("msg").GetProperty("msgtype").GetString().ShouldBe("markdown");
        document.RootElement.GetProperty("msg").GetProperty("markdown").GetProperty("text").GetString()
            .ShouldBe("项目 **风险** 提醒");
    }

    [Fact]
    public void BuildRobotMessagePayload_Should_Build_Text_Robot_Message()
    {
        var payload = DingTalkConnectorAdapter.BuildRobotMessagePayload("Text", "普通通知");

        using var document = JsonDocument.Parse(payload);
        document.RootElement.GetProperty("msgtype").GetString().ShouldBe("text");
        document.RootElement.GetProperty("text").GetProperty("content").GetString().ShouldBe("普通通知");
    }

    [Fact]
    public void BuildSyncOptions_Should_Merge_Config_And_Run_Request()
    {
        var options = DingTalkConnectorAdapter.BuildSyncOptions(
            """
            {
              "rootDepartmentId": "1",
              "pageSize": 200,
              "autoMatchUsers": true
            }
            """,
            """
            {
              "RequestJson": "{\"rootDepartmentId\":\"42\",\"recursiveDepartment\":false,\"autoMatchUsers\":false}"
            }
            """);

        options.RootDepartmentId.ShouldBe("42");
        options.PageSize.ShouldBe(100);
        options.RecursiveDepartment.ShouldBeFalse();
        options.AutoMatchUsers.ShouldBeFalse();
        options.Language.ShouldBe("zh_CN");
    }

    [Fact]
    public void ParseDepartmentItem_Should_Read_DingTalk_Department_Fields()
    {
        using var document = JsonDocument.Parse("""
                                                {
                                                  "dept_id": 42,
                                                  "parent_id": 1,
                                                  "name": "交付中心"
                                                }
                                                """);

        var item = DingTalkConnectorAdapter.ParseDepartmentItem(document.RootElement, "1");

        item.ExternalDepartmentId.ShouldBe("42");
        item.ParentExternalDepartmentId.ShouldBe("1");
        item.Name.ShouldBe("交付中心");
        item.RawJson.ShouldContain("交付中心");
    }

    [Fact]
    public void ParseUserItem_Should_Read_DingTalk_User_Fields()
    {
        using var document = JsonDocument.Parse("""
                                                {
                                                  "userid": "dt-user",
                                                  "unionid": "union-user",
                                                  "name": "王项目",
                                                  "mobile": "13800138000",
                                                  "email": "pm@general.local",
                                                  "dept_id_list": [42],
                                                  "avatar": "https://avatar.example.com/pm.png"
                                                }
                                                """);

        var item = DingTalkConnectorAdapter.ParseUserItem(document.RootElement, "1");

        item.ExternalUserId.ShouldBe("dt-user");
        item.ExternalUnionId.ShouldBe("union-user");
        item.DisplayName.ShouldBe("王项目");
        item.Mobile.ShouldBe("13800138000");
        item.Email.ShouldBe("pm@general.local");
        item.DepartmentExternalId.ShouldBe("42");
        item.AvatarUrl.ShouldBe("https://avatar.example.com/pm.png");
    }

    [Fact]
    public void GenerateRobotSignature_Should_Use_DingTalk_Hmac_Rule()
    {
        var sign = DingTalkConnectorAdapter.GenerateRobotSignature("SECabc", 1_234_567_890);

        sign.ShouldBe("BgiIAARG0zIa5lDrfiS7I0fwY3PdIm8sGO4DKEOupWI=");
    }

    [Fact]
    public void BuildSyncOptions_Should_Reject_Invalid_Json()
    {
        var exception = Should.Throw<UserFriendlyException>(() =>
            DingTalkConnectorAdapter.BuildSyncOptions("{", "{}"));

        exception.Message.ShouldBe("钉钉同步配置 JSON 格式不正确。");
    }
}
