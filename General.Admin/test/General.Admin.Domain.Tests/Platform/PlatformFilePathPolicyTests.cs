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
    public void NormalizeCategory_Should_Default_And_Trim(string? input, string expected)
    {
        PlatformFilePathPolicy.NormalizeCategory(input).ShouldBe(expected);
    }

    [Theory]
    [InlineData("a/b")]
    [InlineData("../secret")]
    [InlineData("/absolute")]
    [InlineData("C:/temp")]
    [InlineData("a//b")]
    public void NormalizeCategory_Should_Reject_Unsafe_Value(string input)
    {
        Should.Throw<BusinessException>(() => PlatformFilePathPolicy.NormalizeCategory(input));
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
}
