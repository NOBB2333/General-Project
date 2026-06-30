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

    [Fact]
    public void MarkAsVersion_Should_Set_Current_Version_Context()
    {
        var previousFileId = Guid.NewGuid();
        var file = CreateFile();

        file.MarkAsVersion(3, previousFileId);

        file.VersionNo.ShouldBe(3);
        file.PreviousVersionFileId.ShouldBe(previousFileId);
        file.ReplacedByFileId.ShouldBeNull();
        file.IsCurrentVersion.ShouldBeTrue();
    }

    [Fact]
    public void MarkReplacedBy_Should_Mark_File_As_Historical()
    {
        var replacingFileId = Guid.NewGuid();
        var file = CreateFile();

        file.MarkReplacedBy(replacingFileId);

        file.ReplacedByFileId.ShouldBe(replacingFileId);
        file.IsCurrentVersion.ShouldBeFalse();
    }

    [Fact]
    public void RestoreAsCurrentVersion_Should_Clear_Replaced_By_File()
    {
        var file = CreateFile();
        file.MarkReplacedBy(Guid.NewGuid());

        file.RestoreAsCurrentVersion();

        file.ReplacedByFileId.ShouldBeNull();
        file.IsCurrentVersion.ShouldBeTrue();
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
