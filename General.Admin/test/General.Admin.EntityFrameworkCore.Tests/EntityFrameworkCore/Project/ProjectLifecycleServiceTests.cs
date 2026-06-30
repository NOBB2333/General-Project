using System;
using System.Threading.Tasks;
using General.Admin.EntityFrameworkCore;
using General.Admin.Platform;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Xunit;
using ProjectEntity = General.Admin.Project.Project;

namespace General.Admin.Project;

[Collection(AdminTestConsts.CollectionDefinitionName)]
public class ProjectLifecycleServiceTests : AdminEntityFrameworkCoreTestBase
{
    private static readonly Guid CurrentUserId = Guid.Parse("2e701e62-0953-4dd3-910b-dc6cc93ccb0d");
    private readonly IRepository<AppPlatformFile, Guid> _fileRepository;
    private readonly IRepository<ProjectAcceptanceCriteria, Guid> _projectAcceptanceCriteriaRepository;
    private readonly IRepository<ProjectAcceptanceRecord, Guid> _projectAcceptanceRecordRepository;
    private readonly IRepository<ProjectAcceptanceRecordCriteria, Guid> _projectAcceptanceRecordCriteriaRepository;
    private readonly IRepository<ProjectEntity, Guid> _projectRepository;
    private readonly IRepository<ProjectSprint, Guid> _projectSprintRepository;
    private readonly IRepository<ProjectMilestone, Guid> _projectMilestoneRepository;
    private readonly IRepository<ProjectQualityChecklist, Guid> _projectQualityChecklistRepository;
    private readonly IRepository<ProjectQualityInspection, Guid> _projectQualityInspectionRepository;
    private readonly ProjectLifecycleService _service;
    private readonly IRepository<IdentityUser, Guid> _userRepository;

    public ProjectLifecycleServiceTests()
    {
        _fileRepository = GetRequiredService<IRepository<AppPlatformFile, Guid>>();
        _projectAcceptanceCriteriaRepository = GetRequiredService<IRepository<ProjectAcceptanceCriteria, Guid>>();
        _projectAcceptanceRecordRepository = GetRequiredService<IRepository<ProjectAcceptanceRecord, Guid>>();
        _projectAcceptanceRecordCriteriaRepository = GetRequiredService<IRepository<ProjectAcceptanceRecordCriteria, Guid>>();
        _projectRepository = GetRequiredService<IRepository<ProjectEntity, Guid>>();
        _projectSprintRepository = GetRequiredService<IRepository<ProjectSprint, Guid>>();
        _projectMilestoneRepository = GetRequiredService<IRepository<ProjectMilestone, Guid>>();
        _projectQualityChecklistRepository = GetRequiredService<IRepository<ProjectQualityChecklist, Guid>>();
        _projectQualityInspectionRepository = GetRequiredService<IRepository<ProjectQualityInspection, Guid>>();
        _service = GetRequiredService<ProjectLifecycleService>();
        _userRepository = GetRequiredService<IRepository<IdentityUser, Guid>>();
    }

    [Fact]
    public async Task UpdateMilestoneStatusAsync_Should_Reject_Close_When_Acceptance_File_Missing()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var milestoneId = await CreateMilestoneReadyForCloseAsync();

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _service.UpdateMilestoneStatusAsync(
                    milestoneId,
                    new ProjectMilestoneStatusUpdateInput
                    {
                        ActualCompletionDate = DateTime.Today,
                        Status = ProjectStatuses.Closed
                    }));

            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("验收记录或交付物文件");
        });
    }

    [Fact]
    public async Task UpdateMilestoneStatusAsync_Should_Close_When_Acceptance_File_Exists()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var milestoneId = await CreateMilestoneReadyForCloseAsync();
            var milestone = await _projectMilestoneRepository.GetAsync(milestoneId);
            await _fileRepository.InsertAsync(
                new AppPlatformFile(
                    Guid.NewGuid(),
                    $"project/{Guid.NewGuid():N}/acceptance-report.pdf",
                    "验收报告.pdf",
                    "application/pdf",
                    128,
                    "Project/AcceptanceRecord",
                    "documents/acceptance/records",
                    $"project/{Guid.NewGuid():N}/acceptance-report.pdf",
                    PlatformFileStorageNames.Local,
                    CurrentUserId,
                    businessType: "Project",
                    businessId: milestone.ProjectId.ToString()),
                autoSave: true);

            await _service.UpdateMilestoneStatusAsync(
                milestoneId,
                new ProjectMilestoneStatusUpdateInput
                {
                    ActualCompletionDate = DateTime.Today,
                    Status = ProjectStatuses.Closed
                });

            var updatedMilestone = await _projectMilestoneRepository.GetAsync(milestoneId);
            updatedMilestone.Status.ShouldBe(ProjectStatuses.Closed);
            updatedMilestone.ActualCompletionDate.ShouldBe(DateTime.Today);
        });
    }

    [Fact]
    public async Task UpdateProjectStatusAsync_Should_Reject_Close_When_Acceptance_Record_File_Missing()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectReadyForCloseAsync();

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _service.UpdateProjectStatusAsync(
                    projectId,
                    new ProjectStatusUpdateInput
                    {
                        Status = ProjectStatuses.Closed
                    }));

            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("补齐通过的验收记录和验收文件");
        });
    }

    [Fact]
    public async Task UpdateProjectStatusAsync_Should_Close_When_Acceptance_Record_File_Exists()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectReadyForCloseAsync();
            var criteria = await _projectAcceptanceCriteriaRepository.FirstOrDefaultAsync(item => item.ProjectId == projectId);
            criteria.ShouldNotBeNull();
            var fileId = await CreateAcceptanceFileAsync(projectId);
            var record = new ProjectAcceptanceRecord(
                Guid.NewGuid(),
                projectId,
                "项目最终验收",
                "通过",
                CurrentUserId,
                DateTime.Today,
                acceptanceFileId: fileId);
            await _projectAcceptanceRecordRepository.InsertAsync(record, autoSave: true);
            await _projectAcceptanceRecordCriteriaRepository.InsertAsync(
                new ProjectAcceptanceRecordCriteria(
                    Guid.NewGuid(),
                    projectId,
                    record.Id,
                    criteria.Id),
                autoSave: true);

            await _service.UpdateProjectStatusAsync(
                projectId,
                new ProjectStatusUpdateInput
                {
                    Status = ProjectStatuses.Closed
                });

            var updatedProject = await _projectRepository.GetAsync(projectId);
            updatedProject.Status.ShouldBe(ProjectStatuses.Closed);
        });
    }

    [Fact]
    public async Task UpdateMilestoneStatusAsync_Should_Update_Sprint_When_Sprint_Belongs_To_Project()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var milestoneId = await CreateMilestoneReadyForCloseAsync();
            var milestone = await _projectMilestoneRepository.GetAsync(milestoneId);
            var sprintId = await CreateSprintAsync(milestone.ProjectId, "第 1 Sprint");

            await _service.UpdateMilestoneStatusAsync(
                milestoneId,
                new ProjectMilestoneStatusUpdateInput
                {
                    SprintId = sprintId,
                    Status = ProjectStatuses.InProgress
                });

            var updatedMilestone = await _projectMilestoneRepository.GetAsync(milestoneId);
            updatedMilestone.SprintId.ShouldBe(sprintId);
            updatedMilestone.Status.ShouldBe(ProjectStatuses.InProgress);
        });
    }

    [Fact]
    public async Task UpdateMilestoneStatusAsync_Should_Reject_Sprint_From_Other_Project()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var milestoneId = await CreateMilestoneReadyForCloseAsync();
            var otherProjectId = await CreateProjectAsync("Sprint 归属校验其他项目");
            var otherSprintId = await CreateSprintAsync(otherProjectId, "其他项目 Sprint");

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _service.UpdateMilestoneStatusAsync(
                    milestoneId,
                    new ProjectMilestoneStatusUpdateInput
                    {
                        SprintId = otherSprintId,
                        Status = ProjectStatuses.InProgress
                    }));

            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("Sprint 必须属于当前项目");
        });
    }

    private async Task<Guid> CreateMilestoneReadyForCloseAsync()
    {
        await EnsureCurrentUserAsync();
        var projectId = await CreateProjectAsync("里程碑关闭校验测试项目");
        var milestoneId = Guid.NewGuid();
        var checklistId = Guid.NewGuid();
        await _projectMilestoneRepository.InsertAsync(
            new ProjectMilestone(
                milestoneId,
                projectId,
                "完成验收",
                CurrentUserId,
                DateTime.Today.AddDays(7),
                ProjectStatuses.InProgress),
            autoSave: true);
        await _projectAcceptanceCriteriaRepository.InsertAsync(
            new ProjectAcceptanceCriteria(
                Guid.NewGuid(),
                projectId,
                "验收标准",
                "按客户确认的验收标准执行。",
                "已确认",
                milestoneId),
            autoSave: true);
        await _projectQualityChecklistRepository.InsertAsync(
            new ProjectQualityChecklist(
                checklistId,
                projectId,
                "验收前质量检查",
                "验收",
                "确认关键交付项已经检查。"),
            autoSave: true);
        await _projectQualityInspectionRepository.InsertAsync(
            new ProjectQualityInspection(
                Guid.NewGuid(),
                projectId,
                checklistId,
                CurrentUserId,
                DateTime.Today,
                "通过",
                "检查通过。",
                string.Empty),
            autoSave: true);

        return milestoneId;
    }

    private async Task<Guid> CreateProjectAsync(string name)
    {
        await EnsureCurrentUserAsync();
        var projectId = Guid.NewGuid();
        var project = new ProjectEntity(
            projectId,
            NewCode("PRJ"),
            name,
            null,
            "软件项目",
            "测试客户",
            Guid.NewGuid(),
            CurrentUserId,
            CurrentUserId,
            ProjectPriorities.P1,
            ProjectStatuses.InProgress,
            DateTime.Today,
            DateTime.Today.AddDays(30),
            false,
            "用于验证项目生命周期规则");
        await _projectRepository.InsertAsync(project, autoSave: true);
        return projectId;
    }

    private async Task<Guid> CreateSprintAsync(Guid projectId, string name)
    {
        var sprint = new ProjectSprint(
            Guid.NewGuid(),
            projectId,
            NewCode("SPR"),
            name,
            CurrentUserId,
            DateTime.Today,
            DateTime.Today.AddDays(14),
            ProjectStatuses.InProgress,
            30,
            "用于验证里程碑和 Sprint 关联。");
        await _projectSprintRepository.InsertAsync(sprint, autoSave: true);
        return sprint.Id;
    }

    private async Task<Guid> CreateProjectReadyForCloseAsync()
    {
        var milestoneId = await CreateMilestoneReadyForCloseAsync();
        var milestone = await _projectMilestoneRepository.GetAsync(milestoneId);
        milestone.UpdateStatus(ProjectStatuses.Closed, DateTime.Today);
        await _projectMilestoneRepository.UpdateAsync(milestone, autoSave: true);
        return milestone.ProjectId;
    }

    private async Task<Guid> CreateAcceptanceFileAsync(Guid projectId)
    {
        var file = new AppPlatformFile(
            Guid.NewGuid(),
            $"project/{Guid.NewGuid():N}/acceptance-report.pdf",
            "验收报告.pdf",
            "application/pdf",
            128,
            "Project/AcceptanceRecord",
            "documents/acceptance/records",
            $"project/{Guid.NewGuid():N}/acceptance-report.pdf",
            PlatformFileStorageNames.Local,
            CurrentUserId,
            businessType: "Project",
            businessId: projectId.ToString());
        await _fileRepository.InsertAsync(file, autoSave: true);
        return file.Id;
    }

    private async Task EnsureCurrentUserAsync()
    {
        if (await _userRepository.AnyAsync(item => item.Id == CurrentUserId))
        {
            return;
        }

        var user = new IdentityUser(CurrentUserId, "project.lifecycle.tester", "project.lifecycle.tester@general.local");
        await _userRepository.InsertAsync(user, autoSave: true);
    }

    private static string NewCode(string prefix)
    {
        return $"{prefix}-{Guid.NewGuid():N}"[..32];
    }
}
