using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using General.Admin.Business;
using General.Admin.EntityFrameworkCore;
using General.Admin.Permissions;
using General.Admin.Project;
using NSubstitute;
using Shouldly;
using Volo.Abp.Authorization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Linq;
using Volo.Abp.Users;
using Xunit;
using ProjectEntity = General.Admin.Project.Project;

namespace General.Admin.Platform;

[Collection(AdminTestConsts.CollectionDefinitionName)]
public class PlatformFileAccessServiceTests : AdminEntityFrameworkCoreTestBase
{
    private static readonly Guid CurrentUserId = Guid.Parse("2e701e62-0953-4dd3-910b-dc6cc93ccb0d");
    private static readonly Guid OtherUserId = Guid.Parse("5b5e9a2e-7b0c-4c83-94de-09a1e8f4cc11");
    private readonly IRepository<AppPlatformFile, Guid> _fileRepository;
    private readonly IRepository<BusinessContract, Guid> _businessContractRepository;
    private readonly IRepository<ProjectMember, Guid> _projectMemberRepository;
    private readonly IRepository<ProjectEntity, Guid> _projectRepository;
    private readonly IRepository<ProjectTaskParticipant, Guid> _projectTaskParticipantRepository;
    private readonly IRepository<ProjectTask, Guid> _projectTaskRepository;
    private readonly IPermissionChecker _permissionChecker;
    private readonly ProjectAccessGuard _projectAccessGuard;
    private readonly IRepository<IdentityUser, Guid> _userRepository;

    public PlatformFileAccessServiceTests()
    {
        _businessContractRepository = GetRequiredService<IRepository<BusinessContract, Guid>>();
        _fileRepository = GetRequiredService<IRepository<AppPlatformFile, Guid>>();
        _permissionChecker = Substitute.For<IPermissionChecker>();
        _permissionChecker.IsGrantedAsync(Arg.Any<string>()).Returns(false);
        _projectMemberRepository = GetRequiredService<IRepository<ProjectMember, Guid>>();
        _projectRepository = GetRequiredService<IRepository<ProjectEntity, Guid>>();
        _projectTaskParticipantRepository = GetRequiredService<IRepository<ProjectTaskParticipant, Guid>>();
        _projectTaskRepository = GetRequiredService<IRepository<ProjectTask, Guid>>();
        _projectAccessGuard = GetRequiredService<ProjectAccessGuard>();
        _userRepository = GetRequiredService<IRepository<IdentityUser, Guid>>();
    }

    [Fact]
    public async Task EnsureCanReadAsync_Should_Allow_Project_Member_To_Read_Project_File()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync(managerUserId: OtherUserId);
            await _projectMemberRepository.InsertAsync(
                new ProjectMember(
                    Guid.NewGuid(),
                    projectId,
                    CurrentUserId,
                    Guid.NewGuid(),
                    ["研发"],
                    DateTime.Today),
                autoSave: true);
            var file = await CreateFileAsync("Project", projectId.ToString(), "Project/Charter", uploadedByUserId: OtherUserId);

            await CreateService().EnsureCanReadAsync(file);
        });
    }

    [Fact]
    public async Task EnsureCanReadAsync_Should_Reject_Private_File_Without_Business_Link()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var file = await CreateFileAsync(null, null, "Platform/General", uploadedByUserId: OtherUserId);

            var exception = await Should.ThrowAsync<AbpAuthorizationException>(() =>
                CreateService().EnsureCanReadAsync(file));

            exception.Message.ShouldContain("无权访问该文件");
        });
    }

    [Fact]
    public async Task EnsureCanReadAsync_Should_Reject_Project_Charter_When_Project_Not_Accessible()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync(managerUserId: OtherUserId);
            var file = await CreateFileAsync("Project", projectId.ToString(), "Project/Charter", uploadedByUserId: OtherUserId);

            await Should.ThrowAsync<EntityNotFoundException>(() =>
                CreateService().EnsureCanReadAsync(file));
        });
    }

    [Fact]
    public async Task EnsureCanReadAsync_Should_Reject_Business_File_When_Report_Permission_Missing()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync();
            var contractId = Guid.NewGuid();
            await _businessContractRepository.InsertAsync(
                new BusinessContract(
                    contractId,
                    projectId,
                    NewCode("CTR"),
                    "经营附件合同",
                    "收入合同",
                    "测试客户",
                    100_000,
                    DateTime.Today,
                    "执行中",
                    true),
                autoSave: true);
            var file = await CreateFileAsync("BusinessContract", contractId.ToString(), "Business/Contract", uploadedByUserId: OtherUserId);

            var exception = await Should.ThrowAsync<AbpAuthorizationException>(() =>
                CreateService().EnsureCanReadAsync(file));

            exception.Message.ShouldContain("无权访问该文件");
        });
    }

    [Fact]
    public async Task EnsureCanUploadAsync_Should_Allow_Project_Member_To_Upload_Project_File()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync(managerUserId: OtherUserId);
            await _projectMemberRepository.InsertAsync(
                new ProjectMember(
                    Guid.NewGuid(),
                    projectId,
                    CurrentUserId,
                    Guid.NewGuid(),
                    ["研发"],
                    DateTime.Today),
                autoSave: true);

            await CreateService().EnsureCanUploadAsync(
                "Project",
                projectId.ToString(),
                "Project/Charter");
        });
    }

    [Fact]
    public async Task EnsureCanUploadAsync_Should_Reject_Project_File_When_Project_Not_Accessible()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync(managerUserId: OtherUserId);

            await Should.ThrowAsync<EntityNotFoundException>(() =>
                CreateService().EnsureCanUploadAsync(
                    "Project",
                    projectId.ToString(),
                    "Project/Charter"));
        });
    }

    [Fact]
    public async Task EnsureCanUploadAsync_Should_Allow_Project_Member_To_Upload_Task_Prd()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync(managerUserId: OtherUserId);
            await _projectMemberRepository.InsertAsync(
                new ProjectMember(
                    Guid.NewGuid(),
                    projectId,
                    CurrentUserId,
                    Guid.NewGuid(),
                    ["开发"],
                    DateTime.Today),
                autoSave: true);
            var taskId = await CreateTaskAsync(projectId);

            await CreateService().EnsureCanUploadAsync(
                "ProjectTask",
                taskId.ToString(),
                "Project/RequirementPrd");
        });
    }

    [Fact]
    public async Task EnsureCanUploadAsync_Should_Reject_Task_Prd_When_Project_Not_Accessible()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync(managerUserId: OtherUserId);
            var taskId = await CreateTaskAsync(projectId);

            await Should.ThrowAsync<EntityNotFoundException>(() =>
                CreateService().EnsureCanUploadAsync(
                    "ProjectTask",
                    taskId.ToString(),
                    "Project/RequirementPrd"));
        });
    }

    [Fact]
    public async Task EnsureCanUploadAsync_Should_Allow_Task_Participant_To_Upload_Task_Prd()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync(managerUserId: OtherUserId);
            var taskId = await CreateTaskAsync(projectId);
            await _projectTaskParticipantRepository.InsertAsync(
                new ProjectTaskParticipant(
                    Guid.NewGuid(),
                    projectId,
                    taskId,
                    CurrentUserId,
                    "开发"),
                autoSave: true);

            await CreateService().EnsureCanUploadAsync(
                "ProjectTask",
                taskId.ToString(),
                "Project/RequirementPrd");
        });
    }

    [Fact]
    public async Task ApplyReadFilterAsync_Should_Hide_Inaccessible_Project_File_From_List()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var parentPath = $"documents/list-access-{Guid.NewGuid():N}";
            var accessibleProjectId = await CreateProjectAsync();
            var inaccessibleProjectId = await CreateProjectAsync(managerUserId: OtherUserId);
            await CreateFileAsync("Project", accessibleProjectId.ToString(), "Project/Charter", OtherUserId, parentPath: parentPath);
            await CreateFileAsync("Project", inaccessibleProjectId.ToString(), "Project/Charter", OtherUserId, parentPath: parentPath);
            await CreateFileAsync(null, null, "Platform/General", OtherUserId, parentPath: parentPath);
            await CreateFileAsync(null, null, "Platform/General", OtherUserId, isPublic: true, parentPath: parentPath);

            var queryable = (await _fileRepository.GetQueryableAsync()).Where(item => item.ParentPath == parentPath);
            var visibleFiles = await GetRequiredService<IAsyncQueryableExecuter>()
                .ToListAsync(await CreateService().ApplyReadFilterAsync(queryable));

            visibleFiles.Count.ShouldBe(2);
            visibleFiles.ShouldContain(item => item.BusinessId == accessibleProjectId.ToString());
            visibleFiles.ShouldContain(item => item.IsPublic);
            visibleFiles.ShouldNotContain(item => item.BusinessId == inaccessibleProjectId.ToString());
            visibleFiles.ShouldNotContain(item => item.BusinessType == null && !item.IsPublic);
        });
    }

    [Fact]
    public async Task ApplyReadFilterAsync_Should_Filter_Business_File_By_Report_Permission()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var parentPath = $"documents/business-access-{Guid.NewGuid():N}";
            var projectId = await CreateProjectAsync();
            var contractId = Guid.NewGuid();
            await _businessContractRepository.InsertAsync(
                new BusinessContract(
                    contractId,
                    projectId,
                    NewCode("CTR"),
                    "经营附件合同",
                    "收入合同",
                    "测试客户",
                    100_000,
                    DateTime.Today,
                    "执行中",
                    true),
                autoSave: true);
            await CreateFileAsync("BusinessContract", contractId.ToString(), "Business/Contract", OtherUserId, parentPath: parentPath);

            var queryable = (await _fileRepository.GetQueryableAsync()).Where(item => item.ParentPath == parentPath);
            var visibleWithoutPermission = await GetRequiredService<IAsyncQueryableExecuter>()
                .ToListAsync(await CreateService().ApplyReadFilterAsync(queryable));
            visibleWithoutPermission.ShouldBeEmpty();

            _permissionChecker.IsGrantedAsync(AdminPermissions.Business.ReportView).Returns(true);
            var visibleWithPermission = await GetRequiredService<IAsyncQueryableExecuter>()
                .ToListAsync(await CreateService().ApplyReadFilterAsync(queryable));
            visibleWithPermission.Count.ShouldBe(1);
            visibleWithPermission[0].BusinessId.ShouldBe(contractId.ToString());
        });
    }

    private async Task<Guid> CreateProjectAsync(Guid? managerUserId = null)
    {
        await EnsureUserAsync(CurrentUserId, "platform.file.current");
        await EnsureUserAsync(OtherUserId, "platform.file.other");
        var projectId = Guid.NewGuid();
        await _projectRepository.InsertAsync(
            new ProjectEntity(
                projectId,
                NewCode("PRJ"),
                "文件权限测试项目",
                null,
                "软件项目",
                "测试客户",
                Guid.NewGuid(),
                managerUserId ?? CurrentUserId,
                managerUserId ?? CurrentUserId,
                ProjectPriorities.P1,
                ProjectStatuses.InProgress,
                DateTime.Today,
                DateTime.Today.AddDays(30),
                false,
                "用于验证文件预览和下载权限。"),
            autoSave: true);
        return projectId;
    }

    private PlatformFileAccessService CreateService()
    {
        return new PlatformFileAccessService(
            GetRequiredService<ICurrentUser>(),
            GetRequiredService<IAsyncQueryableExecuter>(),
            GetRequiredService<CurrentUserDataScopeService>(),
            GetRequiredService<IEnumerable<IFileBusinessLinkResolver>>(),
            _permissionChecker,
            _projectAccessGuard,
            GetRequiredService<IRepository<BusinessAcceptance, Guid>>(),
            GetRequiredService<IRepository<BusinessBudgetExecution, Guid>>(),
            GetRequiredService<IRepository<BusinessChain, Guid>>(),
            _businessContractRepository,
            GetRequiredService<IRepository<BusinessInvoice, Guid>>(),
            GetRequiredService<IRepository<BusinessProcurement, Guid>>(),
            GetRequiredService<IRepository<BusinessReceivable, Guid>>(),
            GetRequiredService<IRepository<ProjectSprint, Guid>>(),
            GetRequiredService<IRepository<ProjectMeetingRecord, Guid>>(),
            _projectMemberRepository,
            _projectRepository,
            GetRequiredService<IRepository<ProjectReport, Guid>>(),
            GetRequiredService<IRepository<ProjectRequirement, Guid>>(),
            _projectTaskParticipantRepository,
            _projectTaskRepository);
    }

    private async Task<AppPlatformFile> CreateFileAsync(
        string? businessType,
        string? businessId,
        string category,
        Guid uploadedByUserId,
        bool isPublic = false,
        string? parentPath = null)
    {
        var file = new AppPlatformFile(
            Guid.NewGuid(),
            $"project/{Guid.NewGuid():N}/file.pdf",
            "权限测试文件.pdf",
            "application/pdf",
            128,
            category,
            parentPath ?? "documents/test",
            $"project/{Guid.NewGuid():N}/file.pdf",
            PlatformFileStorageNames.Local,
            uploadedByUserId,
            isPublic: isPublic,
            businessType: businessType,
            businessId: businessId);
        await _fileRepository.InsertAsync(file, autoSave: true);
        return file;
    }

    private async Task<Guid> CreateTaskAsync(Guid projectId)
    {
        var task = new ProjectTask(
            Guid.NewGuid(),
            projectId,
            Guid.NewGuid(),
            NewCode("TASK"),
            "PRD 上传权限验证需求",
            OtherUserId,
            ProjectStatuses.InProgress,
            ProjectPriorities.P1,
            DateTime.Today,
            DateTime.Today.AddDays(5));
        await _projectTaskRepository.InsertAsync(task, autoSave: true);
        return task.Id;
    }

    private async Task EnsureUserAsync(Guid userId, string userName)
    {
        if (await _userRepository.AnyAsync(item => item.Id == userId))
        {
            return;
        }

        var user = new IdentityUser(userId, userName, $"{userName}@general.local");
        await _userRepository.InsertAsync(user, autoSave: true);
    }

    private static string NewCode(string prefix)
    {
        return $"{prefix}-{Guid.NewGuid():N}"[..32];
    }
}
