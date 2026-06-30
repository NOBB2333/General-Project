using System;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace General.Admin.Platform;

public class PlatformFilePathPolicyTests
{
    [Theory]
    [InlineData(null, "default")]
    [InlineData("", "default")]
    [InlineData(" project ", "project")]
    [InlineData(" Project / Charter ", "Project/Charter")]
    [InlineData("Project\\AcceptanceRecord", "Project/AcceptanceRecord")]
    public void NormalizeCategory_Should_Default_And_Trim(string? input, string expected)
    {
        PlatformFilePathPolicy.NormalizeCategory(input).ShouldBe(expected);
    }

    [Theory]
    [InlineData("../secret")]
    [InlineData("/absolute")]
    [InlineData("C:/temp")]
    [InlineData("a//b")]
    public void NormalizeCategory_Should_Reject_Unsafe_Value(string input)
    {
        Should.Throw<BusinessException>(() => PlatformFilePathPolicy.NormalizeCategory(input));
    }

    [Fact]
    public void NormalizeUploadTarget_Should_Default_Platform_File_To_General_Folder()
    {
        var result = PlatformFilePathPolicy.NormalizeUploadTarget(null, null, null, null);

        result.Category.ShouldBe("Platform/General");
        result.ParentPath.ShouldBe("files");
    }

    [Fact]
    public void NormalizeUploadTarget_Should_Reject_Business_File_Without_Category()
    {
        Should.Throw<BusinessException>(() =>
            PlatformFilePathPolicy.NormalizeUploadTarget(null, null, "Project", Guid.NewGuid().ToString()));
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData(" docs / contracts ", "docs/contracts")]
    [InlineData("docs\\contracts", "docs/contracts")]
    public void NormalizeParentPath_Should_Normalize_Relative_Path(string? input, string? expected)
    {
        PlatformFilePathPolicy.NormalizeParentPath(input).ShouldBe(expected);
    }

    [Theory]
    [InlineData("../secret")]
    [InlineData("/absolute")]
    [InlineData("C:/temp")]
    [InlineData("a//b")]
    [InlineData("a/../b")]
    public void NormalizeParentPath_Should_Reject_Unsafe_Path(string input)
    {
        Should.Throw<BusinessException>(() => PlatformFilePathPolicy.NormalizeParentPath(input));
    }

    [Theory]
    [InlineData(" 项目章程.pdf ", "项目章程.pdf")]
    [InlineData("验收标准 V1.docx", "验收标准 V1.docx")]
    public void NormalizeFileName_Should_Trim_Valid_File_Name(string input, string expected)
    {
        PlatformFilePathPolicy.NormalizeFileName(input).ShouldBe(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("../secret.pdf")]
    [InlineData("/absolute.pdf")]
    [InlineData("C:/temp.pdf")]
    [InlineData("a//b.pdf")]
    [InlineData("a/b.pdf")]
    [InlineData("a\\b.pdf")]
    [InlineData(".")]
    [InlineData("..")]
    public void NormalizeFileName_Should_Reject_Unsafe_File_Name(string input)
    {
        Should.Throw<BusinessException>(() => PlatformFilePathPolicy.NormalizeFileName(input));
    }
}
