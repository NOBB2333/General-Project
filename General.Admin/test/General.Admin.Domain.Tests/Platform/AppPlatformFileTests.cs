using System;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace General.Admin.Platform;

public class AppPlatformFileTests
{
    [Fact]
    public void UpdateMetadata_Should_Reject_Too_Long_Business_Type()
    {
        var file = CreateFile();

        var exception = Should.Throw<BusinessException>(() =>
            file.UpdateMetadata("demo.txt", false, new string('a', 65), null));

        exception.Code.ShouldBe("Platform:FileMetadataTooLong");
    }

    [Fact]
    public void UpdateMetadata_Should_Reject_Too_Long_Business_Id()
    {
        var file = CreateFile();

        var exception = Should.Throw<BusinessException>(() =>
            file.UpdateMetadata("demo.txt", false, null, new string('a', 129)));

        exception.Code.ShouldBe("Platform:FileMetadataTooLong");
    }

    private static AppPlatformFile CreateFile()
    {
        return new AppPlatformFile(
            Guid.NewGuid(),
            "files/demo.txt",
            "demo.txt",
            "text/plain",
            10,
            "default",
            null,
            "files/demo.txt",
            PlatformFileStorageNames.Local,
            null);
    }
}
