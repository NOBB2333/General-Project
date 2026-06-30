using System;
using System.Linq;
using System.Threading.Tasks;
using General.Admin.EntityFrameworkCore;
using General.Admin.Project;
using ProjectEntity = General.Admin.Project.Project;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;
using Xunit;

namespace General.Admin.Platform;

[Collection(AdminTestConsts.CollectionDefinitionName)]
public class PlatformFileVersionServiceTests : AdminEntityFrameworkCoreTestBase
{
    private readonly IRepository<AppPlatformFile, Guid> _fileRepository;
    private readonly IRepository<ProjectEntity, Guid> _projectRepository;
    private readonly IRepository<ProjectAcceptanceCriteria, Guid> _projectAcceptanceCriteriaRepository;
    private readonly IRepository<ProjectCharter, Guid> _projectCharterRepository;

    public PlatformFileVersionServiceTests()
    {
        _fileRepository = GetRequiredService<IRepository<AppPlatformFile, Guid>>();
        _projectRepository = GetRequiredService<IRepository<ProjectEntity, Guid>>();
        _projectAcceptanceCriteriaRepository = GetRequiredService<IRepository<ProjectAcceptanceCriteria, Guid>>();
        _projectCharterRepository = GetRequiredService<IRepository<ProjectCharter, Guid>>();
    }

    [Fact]
    public async Task ApplyVersionChainAsync_Should_Mark_Previous_Charter_As_Historical()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = Guid.NewGuid().ToString();
            var previousFile = CreateFile(projectId, "Project/Charter", "章程-v1.pdf");
            await _fileRepository.InsertAsync(previousFile, autoSave: true);

            var currentFile = CreateFile(projectId, "Project/Charter", "章程-v2.pdf");
            await GetRequiredService<PlatformFileVersionService>().ApplyVersionChainAsync(currentFile);
            await _fileRepository.InsertAsync(currentFile, autoSave: true);

            previousFile.IsCurrentVersion.ShouldBeFalse();
            previousFile.ReplacedByFileId.ShouldBe(currentFile.Id);
            currentFile.IsCurrentVersion.ShouldBeTrue();
            currentFile.VersionNo.ShouldBe(2);
            currentFile.PreviousVersionFileId.ShouldBe(previousFile.Id);

            var files = await GetRequiredService<IAsyncQueryableExecuter>()
                .ToListAsync((await _fileRepository.GetQueryableAsync()).Where(x => x.BusinessId == projectId));
            files.Count(x => x.IsCurrentVersion).ShouldBe(1);
            files.Single(x => x.IsCurrentVersion).Id.ShouldBe(currentFile.Id);
        });
    }

    [Fact]
    public async Task RestorePreviousCurrentVersionAsync_Should_Restore_Previous_File_When_Current_Is_Deleted()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = Guid.NewGuid().ToString();
            var previousFile = CreateFile(projectId, "Project/AcceptanceCriteria", "验收标准-v1.pdf");
            await _fileRepository.InsertAsync(previousFile, autoSave: true);

            var currentFile = CreateFile(projectId, "Project/AcceptanceCriteria", "验收标准-v2.pdf");
            var service = GetRequiredService<PlatformFileVersionService>();
            await service.ApplyVersionChainAsync(currentFile);
            await _fileRepository.InsertAsync(currentFile, autoSave: true);

            previousFile.IsCurrentVersion.ShouldBeFalse();

            await service.RestorePreviousCurrentVersionAsync(currentFile);

            previousFile.IsCurrentVersion.ShouldBeTrue();
            previousFile.ReplacedByFileId.ShouldBeNull();
        });
    }

    [Fact]
    public async Task ApplyVersionChainAsync_Should_Not_Version_Normal_Project_Documents()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = Guid.NewGuid().ToString();
            var firstFile = CreateFile(projectId, "Project/Document", "资料-v1.pdf");
            await _fileRepository.InsertAsync(firstFile, autoSave: true);

            var secondFile = CreateFile(projectId, "Project/Document", "资料-v2.pdf");
            await GetRequiredService<PlatformFileVersionService>().ApplyVersionChainAsync(secondFile);

            firstFile.IsCurrentVersion.ShouldBeTrue();
            firstFile.ReplacedByFileId.ShouldBeNull();
            secondFile.IsCurrentVersion.ShouldBeTrue();
            secondFile.VersionNo.ShouldBe(1);
            secondFile.PreviousVersionFileId.ShouldBeNull();
        });
    }

    [Fact]
    public async Task ApplyVersionChainAsync_Should_Reject_Replacing_Approved_Charter_File()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("章程锁定测试项目");
            var previousFile = CreateFile(projectId.ToString(), "Project/Charter", "章程-v1.pdf");
            await _fileRepository.InsertAsync(previousFile, autoSave: true);
            await _projectCharterRepository.InsertAsync(
                new ProjectCharter(
                    Guid.NewGuid(),
                    projectId,
                    "商业论证",
                    "项目目标",
                    "范围边界",
                    "假设条件",
                    "制约因素",
                    "成功标准",
                    "已批准"),
                autoSave: true);

            var currentFile = CreateFile(projectId.ToString(), "Project/Charter", "章程-v2.pdf");

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                GetRequiredService<PlatformFileVersionService>().ApplyVersionChainAsync(currentFile));
            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("已批准的项目章程文件不能直接替换");
            previousFile.IsCurrentVersion.ShouldBeTrue();
            previousFile.ReplacedByFileId.ShouldBeNull();
        });
    }

    [Fact]
    public async Task ApplyVersionChainAsync_Should_Reject_Replacing_Confirmed_Acceptance_Criteria_File()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("验收标准锁定测试项目");
            var previousFile = CreateFile(projectId.ToString(), "Project/AcceptanceCriteria", "验收标准-v1.pdf");
            await _fileRepository.InsertAsync(previousFile, autoSave: true);
            await _projectAcceptanceCriteriaRepository.InsertAsync(
                new ProjectAcceptanceCriteria(
                    Guid.NewGuid(),
                    projectId,
                    "验收标准",
                    "按验收标准文件执行",
                    "已确认"),
                autoSave: true);

            var currentFile = CreateFile(projectId.ToString(), "Project/AcceptanceCriteria", "验收标准-v2.pdf");

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                GetRequiredService<PlatformFileVersionService>().ApplyVersionChainAsync(currentFile));
            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("已确认的验收标准文件不能直接替换");
            previousFile.IsCurrentVersion.ShouldBeTrue();
            previousFile.ReplacedByFileId.ShouldBeNull();
        });
    }

    private async Task<Guid> CreateProjectAsync(string name)
    {
        var projectId = Guid.NewGuid();
        await _projectRepository.InsertAsync(
            new ProjectEntity(
                projectId,
                $"PRJ-{Guid.NewGuid():N}"[..12],
                name,
                null,
                "软件项目",
                "测试客户",
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                ProjectPriorities.P1,
                ProjectStatuses.InProgress,
                DateTime.Today,
                DateTime.Today.AddDays(30),
                false,
                "用于验证正式文件版本锁定。"),
            autoSave: true);
        return projectId;
    }

    private static AppPlatformFile CreateFile(string businessId, string category, string fileName)
    {
        return new AppPlatformFile(
            Guid.NewGuid(),
            $"project/{Guid.NewGuid():N}/{fileName}",
            fileName,
            "application/pdf",
            128,
            category,
            "documents/test",
            $"project/{Guid.NewGuid():N}/{fileName}",
            PlatformFileStorageNames.Local,
            Guid.NewGuid(),
            businessType: "Project",
            businessId: businessId);
    }
}
