using System.Linq;
using Shouldly;
using Xunit;

namespace General.Admin.Platform;

public class PlatformFileBusinessLinkResolverTests
{
    [Fact]
    public void ResolveBusinessKeyAliases_Should_Keep_Legacy_Business_Type_Aliases()
    {
        var aliases = PlatformFileBusinessLinkResolver
            .ResolveBusinessKeyAliases("BusinessContract", "contract-001")
            .ToList();

        aliases.ShouldContain("BusinessContract:contract-001");
        aliases.ShouldContain("Contract:contract-001");
    }

    [Theory]
    [InlineData("Project/Charter", "overview")]
    [InlineData("Project/AcceptanceRecord", "delivery")]
    [InlineData("Project/AcceptanceCriteria", "delivery")]
    [InlineData("Project/Delivery", "delivery")]
    [InlineData("Project/Requirement", "tasks")]
    [InlineData("Project/Document", "docs")]
    [InlineData("Project/Report", "docs")]
    [InlineData("Project/Meeting", "docs")]
    public void ResolveProjectTabByCategory_Should_Map_Project_File_Categories_To_Detail_Tabs(
        string category,
        string expectedTab)
    {
        PlatformFileBusinessLinkResolver.ResolveProjectTabByCategory(category).ShouldBe(expectedTab);
    }

    [Theory]
    [InlineData("Project/AcceptanceCriteria", "验收标准文件")]
    [InlineData("Project/AcceptanceRecord", "验收记录文件")]
    [InlineData("Project/Charter", "项目章程")]
    [InlineData("Project/Delivery", "交付物文件")]
    [InlineData("Project/Meeting", "会议附件")]
    [InlineData("Project/Report", "项目报告附件")]
    [InlineData("Project/Requirement", "需求附件")]
    public void ResolveProjectObjectTypeNameByCategory_Should_Map_Project_File_Categories_To_Object_Type_Names(
        string category,
        string expectedTypeName)
    {
        PlatformFileBusinessLinkResolver.ResolveProjectObjectTypeNameByCategory(category).ShouldBe(expectedTypeName);
    }

    [Theory]
    [InlineData("Project/Charter", "北斗交付平台", "北斗交付平台 / 项目章程")]
    [InlineData("Project/AcceptanceRecord", "北斗交付平台", "北斗交付平台 / 验收记录")]
    [InlineData("Project/Report", "", "项目报告")]
    [InlineData("Project/Meeting", null, "会议记录")]
    public void ResolveProjectObjectNameByCategory_Should_Keep_Project_Context_When_Available(
        string category,
        string? projectName,
        string expectedName)
    {
        PlatformFileBusinessLinkResolver.ResolveProjectObjectNameByCategory(category, projectName).ShouldBe(expectedName);
    }
}
