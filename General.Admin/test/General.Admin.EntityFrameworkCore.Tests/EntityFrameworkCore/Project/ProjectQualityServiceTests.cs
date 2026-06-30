using System;
using System.Linq;
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
public class ProjectQualityServiceTests : AdminEntityFrameworkCoreTestBase
{
    private static readonly Guid CurrentUserId = Guid.Parse("2e701e62-0953-4dd3-910b-dc6cc93ccb0d");
    private readonly IRepository<AppPlatformFile, Guid> _fileRepository;
    private readonly IRepository<ProjectAcceptanceCriteria, Guid> _projectAcceptanceCriteriaRepository;
    private readonly IRepository<ProjectAcceptanceRecord, Guid> _projectAcceptanceRecordRepository;
    private readonly IRepository<ProjectAcceptanceRecordCriteria, Guid> _projectAcceptanceRecordCriteriaRepository;
    private readonly IRepository<ProjectAcceptanceRecordRequirement, Guid> _projectAcceptanceRecordRequirementRepository;
    private readonly IRepository<ProjectIssue, Guid> _projectIssueRepository;
    private readonly IRepository<ProjectEntity, Guid> _projectRepository;
    private readonly IRepository<ProjectSprint, Guid> _projectSprintRepository;
    private readonly IRepository<ProjectMilestone, Guid> _projectMilestoneRepository;
    private readonly IRepository<ProjectQualityChecklist, Guid> _projectQualityChecklistRepository;
    private readonly IRepository<ProjectRequirement, Guid> _projectRequirementRepository;
    private readonly ProjectQueryService _projectQueryService;
    private readonly ProjectQualityService _service;
    private readonly IRepository<IdentityUser, Guid> _userRepository;

    public ProjectQualityServiceTests()
    {
        _fileRepository = GetRequiredService<IRepository<AppPlatformFile, Guid>>();
        _projectAcceptanceCriteriaRepository = GetRequiredService<IRepository<ProjectAcceptanceCriteria, Guid>>();
        _projectAcceptanceRecordRepository = GetRequiredService<IRepository<ProjectAcceptanceRecord, Guid>>();
        _projectAcceptanceRecordCriteriaRepository = GetRequiredService<IRepository<ProjectAcceptanceRecordCriteria, Guid>>();
        _projectAcceptanceRecordRequirementRepository = GetRequiredService<IRepository<ProjectAcceptanceRecordRequirement, Guid>>();
        _projectIssueRepository = GetRequiredService<IRepository<ProjectIssue, Guid>>();
        _projectRepository = GetRequiredService<IRepository<ProjectEntity, Guid>>();
        _projectSprintRepository = GetRequiredService<IRepository<ProjectSprint, Guid>>();
        _projectMilestoneRepository = GetRequiredService<IRepository<ProjectMilestone, Guid>>();
        _projectQualityChecklistRepository = GetRequiredService<IRepository<ProjectQualityChecklist, Guid>>();
        _projectRequirementRepository = GetRequiredService<IRepository<ProjectRequirement, Guid>>();
        _projectQueryService = GetRequiredService<ProjectQueryService>();
        _service = GetRequiredService<ProjectQualityService>();
        _userRepository = GetRequiredService<IRepository<IdentityUser, Guid>>();
    }

    [Fact]
    public async Task CreateAcceptanceRecordAsync_Should_Save_Record_With_Criteria_Milestone_And_File()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var fixture = await CreateAcceptanceFixtureAsync();
            var requirementId = await CreateRequirementAsync(fixture.ProjectId, "REQ-ACCEPT", "客户签收需求");

            var recordId = await _service.CreateAcceptanceRecordAsync(
                fixture.ProjectId,
                new ProjectAcceptanceRecordSaveInput
                {
                    AcceptanceCriteriaIds = [fixture.CriteriaId],
                    AcceptanceFileId = fixture.AcceptanceFileId,
                    AcceptanceResult = "通过",
                    AcceptanceTime = DateTime.Today,
                    AcceptorUserId = CurrentUserId,
                    MilestoneId = fixture.MilestoneId,
                    Remark = "客户签字确认。",
                    RequirementIds = [requirementId],
                    Title = "里程碑验收"
                });

            var record = await _projectAcceptanceRecordRepository.GetAsync(recordId);
            record.ProjectId.ShouldBe(fixture.ProjectId);
            record.MilestoneId.ShouldBe(fixture.MilestoneId);
            record.AcceptanceFileId.ShouldBe(fixture.AcceptanceFileId);

            var links = await _projectAcceptanceRecordCriteriaRepository.GetListAsync(item =>
                item.AcceptanceRecordId == recordId);
            links.Count.ShouldBe(1);
            links[0].AcceptanceCriteriaId.ShouldBe(fixture.CriteriaId);
            var requirementLinks = await _projectAcceptanceRecordRequirementRepository.GetListAsync(item =>
                item.AcceptanceRecordId == recordId);
            requirementLinks.Count.ShouldBe(1);
            requirementLinks[0].RequirementId.ShouldBe(requirementId);

            var detail = await _projectQueryService.GetDetailAsync(fixture.ProjectId);
            var dto = detail.AcceptanceRecords.Single(item => item.Id == recordId);
            dto.MilestoneName.ShouldBe("客户验收");
            dto.AcceptanceFileName.ShouldBe("验收报告.pdf");
            dto.AcceptanceCriteriaIds.ShouldBe([fixture.CriteriaId]);
            dto.AcceptanceCriteriaTitles.ShouldBe(["客户签收标准"]);
            dto.RequirementIds.ShouldBe([requirementId]);
            dto.RequirementTitles.ShouldBe(["客户签收需求"]);
        });
    }

    [Fact]
    public async Task CreateAcceptanceRecordAsync_Should_Reject_Requirement_From_Other_Project()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var fixture = await CreateAcceptanceFixtureAsync();
            var otherProjectId = Guid.NewGuid();
            await _projectRepository.InsertAsync(
                new ProjectEntity(
                    otherProjectId,
                    NewCode("PRJ"),
                    "其他需求项目",
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
                    "用于验证跨项目需求拒绝。"),
                autoSave: true);
            var otherRequirementId = await CreateRequirementAsync(otherProjectId, "REQ-OTHER", "其他项目需求");

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _service.CreateAcceptanceRecordAsync(
                    fixture.ProjectId,
                    new ProjectAcceptanceRecordSaveInput
                    {
                        AcceptanceCriteriaIds = [fixture.CriteriaId],
                        AcceptanceFileId = fixture.AcceptanceFileId,
                        AcceptanceResult = "通过",
                        AcceptorUserId = CurrentUserId,
                        RequirementIds = [otherRequirementId],
                        Title = "跨项目需求验收"
                    }));

            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("关联的需求必须属于当前项目");
        });
    }

    [Fact]
    public async Task CreateAcceptanceRecordAsync_Should_Save_Record_With_Sprint_And_Delivery_File()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var fixture = await CreateAcceptanceFixtureAsync();
            var sprintId = await CreateSprintAsync(fixture.ProjectId, "第 1 Sprint");
            var deliveryFileId = await CreateAcceptanceFileAsync(fixture.ProjectId, "Project/Delivery", sprintId);

            var recordId = await _service.CreateAcceptanceRecordAsync(
                fixture.ProjectId,
                new ProjectAcceptanceRecordSaveInput
                {
                    AcceptanceCriteriaIds = [fixture.CriteriaId],
                    AcceptanceFileId = deliveryFileId,
                    AcceptanceResult = "通过",
                    AcceptorUserId = CurrentUserId,
                    MilestoneId = fixture.MilestoneId,
                    SprintId = sprintId,
                    Title = "Sprint 验收"
                });

            var record = await _projectAcceptanceRecordRepository.GetAsync(recordId);
            record.SprintId.ShouldBe(sprintId);
            record.AcceptanceFileId.ShouldBe(deliveryFileId);

            var detail = await _projectQueryService.GetDetailAsync(fixture.ProjectId);
            var dto = detail.AcceptanceRecords.Single(item => item.Id == recordId);
            dto.SprintId.ShouldBe(sprintId);
            dto.SprintName.ShouldBe("第 1 Sprint");
            dto.AcceptanceFileName.ShouldBe("验收报告.pdf");
        });
    }

    [Fact]
    public async Task CreateAcceptanceRecordAsync_Should_Reject_Sprint_From_Other_Project()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var fixture = await CreateAcceptanceFixtureAsync();
            var otherProjectId = Guid.NewGuid();
            await _projectRepository.InsertAsync(
                new ProjectEntity(
                    otherProjectId,
                    NewCode("PRJ"),
                    "其他 Sprint 项目",
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
                    "用于验证跨项目 Sprint 拒绝。"),
                autoSave: true);
            var otherSprintId = await CreateSprintAsync(otherProjectId, "其他项目 Sprint");

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _service.CreateAcceptanceRecordAsync(
                    fixture.ProjectId,
                    new ProjectAcceptanceRecordSaveInput
                    {
                        AcceptanceCriteriaIds = [fixture.CriteriaId],
                        AcceptanceFileId = fixture.AcceptanceFileId,
                        AcceptanceResult = "通过",
                        AcceptorUserId = CurrentUserId,
                        SprintId = otherSprintId,
                        Title = "跨项目 Sprint 验收"
                    }));

            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("Sprint 必须属于当前项目");
        });
    }

    [Fact]
    public async Task CreateAcceptanceRecordAsync_Should_Reject_File_From_Other_Project()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var fixture = await CreateAcceptanceFixtureAsync();
            var otherProjectFileId = await CreateAcceptanceFileAsync(Guid.NewGuid(), "Project/AcceptanceRecord");

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _service.CreateAcceptanceRecordAsync(
                    fixture.ProjectId,
                    new ProjectAcceptanceRecordSaveInput
                    {
                        AcceptanceCriteriaIds = [fixture.CriteriaId],
                        AcceptanceFileId = otherProjectFileId,
                        AcceptanceResult = "通过",
                        AcceptorUserId = CurrentUserId,
                        MilestoneId = fixture.MilestoneId,
                        Title = "错误文件验收"
                    }));

            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("当前项目或 Sprint 的验收记录或交付物文件");
        });
    }

    [Fact]
    public async Task CreateAcceptanceRecordAsync_Should_Reject_Wrong_File_Category()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var fixture = await CreateAcceptanceFixtureAsync();
            var charterFileId = await CreateAcceptanceFileAsync(fixture.ProjectId, "Project/Charter");

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _service.CreateAcceptanceRecordAsync(
                    fixture.ProjectId,
                    new ProjectAcceptanceRecordSaveInput
                    {
                        AcceptanceCriteriaIds = [fixture.CriteriaId],
                        AcceptanceFileId = charterFileId,
                        AcceptanceResult = "通过",
                        AcceptorUserId = CurrentUserId,
                        MilestoneId = fixture.MilestoneId,
                        Title = "错误分类验收"
                    }));

            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("当前项目或 Sprint 的验收记录或交付物文件");
        });
    }

    [Fact]
    public async Task CreateAcceptanceRecordAsync_Should_Require_Rectification_When_Result_Failed()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var fixture = await CreateAcceptanceFixtureAsync();

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _service.CreateAcceptanceRecordAsync(
                    fixture.ProjectId,
                    new ProjectAcceptanceRecordSaveInput
                    {
                        AcceptanceCriteriaIds = [fixture.CriteriaId],
                        AcceptanceFileId = fixture.AcceptanceFileId,
                        AcceptanceResult = "不通过",
                        AcceptorUserId = CurrentUserId,
                        MilestoneId = fixture.MilestoneId,
                        Title = "未通过验收"
                    }));

            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("整改要求");
        });
    }

    [Fact]
    public async Task CreateAcceptanceRecordAsync_Should_Create_Issue_When_Rejected_And_Requested()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var fixture = await CreateAcceptanceFixtureAsync();

            await _service.CreateAcceptanceRecordAsync(
                fixture.ProjectId,
                new ProjectAcceptanceRecordSaveInput
                {
                    AcceptanceCriteriaIds = [fixture.CriteriaId],
                    AcceptanceFileId = fixture.AcceptanceFileId,
                    AcceptanceResult = "不通过",
                    AcceptorUserId = CurrentUserId,
                    CreateIssueWhenRejected = true,
                    IssueDueDate = DateTime.Today.AddDays(3),
                    IssueLevel = ProjectPriorities.P0,
                    IssueOwnerUserId = CurrentUserId,
                    MilestoneId = fixture.MilestoneId,
                    RectificationRequirement = "补齐客户签字页。",
                    Title = "未通过验收"
                });

            var issues = await _projectIssueRepository.GetListAsync(item => item.ProjectId == fixture.ProjectId);
            issues.Count.ShouldBe(1);
            var issue = issues.Single();
            issue.Type.ShouldBe("验收问题");
            issue.Title.ShouldBe("验收不通过：未通过验收");
            issue.Status.ShouldBe("待处理");
            issue.Level.ShouldBe(ProjectPriorities.P0);
            issue.DueDate.ShouldBe(DateTime.Today.AddDays(3));
            issue.AcceptanceCriteriaId.ShouldBe(fixture.CriteriaId);
            issue.RequirementTitle.ShouldBe("来源验收记录：未通过验收");
        });
    }

    [Fact]
    public async Task CreateAcceptanceRecordAsync_Should_Reject_Issue_Creation_When_Result_Passed()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var fixture = await CreateAcceptanceFixtureAsync();

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _service.CreateAcceptanceRecordAsync(
                    fixture.ProjectId,
                    new ProjectAcceptanceRecordSaveInput
                    {
                        AcceptanceCriteriaIds = [fixture.CriteriaId],
                        AcceptanceFileId = fixture.AcceptanceFileId,
                        AcceptanceResult = "通过",
                        AcceptorUserId = CurrentUserId,
                        CreateIssueWhenRejected = true,
                        IssueOwnerUserId = CurrentUserId,
                        MilestoneId = fixture.MilestoneId,
                        Title = "通过验收"
                    }));

            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("只有验收不通过");
        });
    }

    [Fact]
    public async Task CreateIssueAsync_Should_Save_Issue()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var fixture = await CreateAcceptanceFixtureAsync();

            var issueId = await _service.CreateIssueAsync(
                fixture.ProjectId,
                new ProjectIssueSaveInput
                {
                    AcceptanceCriteriaId = fixture.CriteriaId,
                    DueDate = DateTime.Today.AddDays(2),
                    Level = ProjectPriorities.P0,
                    OwnerUserId = CurrentUserId,
                    RequirementTitle = "登录失败反馈",
                    Status = "待处理",
                    TesterOwnerUserId = CurrentUserId,
                    Title = "登录按钮无响应",
                    Type = "缺陷"
                });

            var issue = await _projectIssueRepository.GetAsync(issueId);
            issue.ProjectId.ShouldBe(fixture.ProjectId);
            issue.Title.ShouldBe("登录按钮无响应");
            issue.Type.ShouldBe("缺陷");
            issue.Level.ShouldBe(ProjectPriorities.P0);
            issue.Status.ShouldBe("待处理");
            issue.AcceptanceCriteriaId.ShouldBe(fixture.CriteriaId);
            issue.TesterOwnerUserId.ShouldBe(CurrentUserId);
            issue.TesterOwnerName.ShouldBe("project.quality.tester");
        });
    }

    [Fact]
    public async Task UpdateIssueAsync_Should_Update_Issue()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var fixture = await CreateAcceptanceFixtureAsync();
            var issueId = await _service.CreateIssueAsync(
                fixture.ProjectId,
                new ProjectIssueSaveInput
                {
                    Level = ProjectPriorities.P1,
                    OwnerUserId = CurrentUserId,
                    Status = "待处理",
                    Title = "旧标题",
                    Type = "问题"
                });

            await _service.UpdateIssueAsync(
                issueId,
                new ProjectIssueSaveInput
                {
                    DueDate = DateTime.Today.AddDays(5),
                    Level = ProjectPriorities.P3,
                    OwnerUserId = CurrentUserId,
                    Status = "处理中",
                    Title = "新标题",
                    Type = "改进"
                });

            var issue = await _projectIssueRepository.GetAsync(issueId);
            issue.Title.ShouldBe("新标题");
            issue.Type.ShouldBe("改进");
            issue.Level.ShouldBe(ProjectPriorities.P3);
            issue.Status.ShouldBe("处理中");
            issue.DueDate.ShouldBe(DateTime.Today.AddDays(5));
        });
    }

    [Fact]
    public async Task CreateIssueAsync_Should_Reject_QualityChecklist_From_Other_Project()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var fixture = await CreateAcceptanceFixtureAsync();
            var otherProjectId = await CreateProjectAsync("其他质量检查单项目");
            var checklist = new ProjectQualityChecklist(
                Guid.NewGuid(),
                otherProjectId,
                "其他项目检查单",
                "功能测试",
                "跨项目校验");
            await _projectQualityChecklistRepository.InsertAsync(checklist, autoSave: true);

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _service.CreateIssueAsync(
                    fixture.ProjectId,
                    new ProjectIssueSaveInput
                    {
                        Level = ProjectPriorities.P0,
                        OwnerUserId = CurrentUserId,
                        QualityChecklistId = checklist.Id,
                        Status = "待处理",
                        Title = "跨项目问题",
                        Type = "问题"
                    }));

            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("质量检查单必须属于当前项目");
        });
    }

    [Fact]
    public async Task DeleteIssueAsync_Should_Delete_Issue()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var fixture = await CreateAcceptanceFixtureAsync();
            var issueId = await _service.CreateIssueAsync(
                fixture.ProjectId,
                new ProjectIssueSaveInput
                {
                    Level = ProjectPriorities.P1,
                    OwnerUserId = CurrentUserId,
                    Status = "待处理",
                    Title = "待删除问题",
                    Type = "问题"
                });

            await _service.DeleteIssueAsync(issueId);

            (await _projectIssueRepository.FindAsync(issueId)).ShouldBeNull();
        });
    }

    private async Task<AcceptanceFixture> CreateAcceptanceFixtureAsync()
    {
        await EnsureCurrentUserAsync();

        var projectId = Guid.NewGuid();
        var milestoneId = Guid.NewGuid();
        var criteriaId = Guid.NewGuid();

        await _projectRepository.InsertAsync(
            new ProjectEntity(
                projectId,
                NewCode("PRJ"),
                "验收记录测试项目",
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
                "用于验证验收记录关联标准、里程碑和文件。"),
            autoSave: true);
        await _projectMilestoneRepository.InsertAsync(
            new ProjectMilestone(
                milestoneId,
                projectId,
                "客户验收",
                CurrentUserId,
                DateTime.Today.AddDays(7),
                ProjectStatuses.InProgress),
            autoSave: true);
        await _projectAcceptanceCriteriaRepository.InsertAsync(
            new ProjectAcceptanceCriteria(
                criteriaId,
                projectId,
                "客户签收标准",
                "客户确认关键交付件后签收。",
                "已确认",
                milestoneId),
            autoSave: true);

        var acceptanceFileId = await CreateAcceptanceFileAsync(projectId, "Project/AcceptanceRecord");

        return new AcceptanceFixture(projectId, milestoneId, criteriaId, acceptanceFileId);
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
            DateTime.Today.AddDays(7),
            ProjectStatuses.InProgress,
            0);
        await _projectSprintRepository.InsertAsync(sprint, autoSave: true);
        return sprint.Id;
    }

    private async Task<Guid> CreateAcceptanceFileAsync(Guid projectId, string category, Guid? sprintId = null)
    {
        var fileId = Guid.NewGuid();
        await _fileRepository.InsertAsync(
            new AppPlatformFile(
                fileId,
                $"project/{Guid.NewGuid():N}/acceptance-report.pdf",
                "验收报告.pdf",
                "application/pdf",
                128,
                category,
                category == "Project/Delivery" ? "documents/delivery" : "documents/acceptance/records",
                $"project/{Guid.NewGuid():N}/acceptance-report.pdf",
                PlatformFileStorageNames.Local,
                CurrentUserId,
                businessType: sprintId.HasValue ? "ProjectSprint" : "Project",
                businessId: sprintId?.ToString() ?? projectId.ToString()),
            autoSave: true);
        return fileId;
    }

    private async Task<Guid> CreateRequirementAsync(Guid projectId, string requirementCode, string title)
    {
        var requirement = new ProjectRequirement(
            Guid.NewGuid(),
            projectId,
            requirementCode,
            title,
            "用于验证验收记录关联需求。",
            "待验收",
            1,
            ProjectPriorities.P1,
            "客户",
            "功能",
            acceptanceCriteria: "客户确认后通过验收。");
        await _projectRequirementRepository.InsertAsync(requirement, autoSave: true);
        return requirement.Id;
    }

    private async Task<Guid> CreateProjectAsync(string name)
    {
        await EnsureCurrentUserAsync();

        var projectId = Guid.NewGuid();
        await _projectRepository.InsertAsync(
            new ProjectEntity(
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
                "用于验证问题记录。"),
            autoSave: true);
        return projectId;
    }

    private async Task EnsureCurrentUserAsync()
    {
        if (await _userRepository.AnyAsync(item => item.Id == CurrentUserId))
        {
            return;
        }

        var user = new IdentityUser(CurrentUserId, "project.quality.tester", "project.quality.tester@general.local");
        await _userRepository.InsertAsync(user, autoSave: true);
    }

    private static string NewCode(string prefix)
    {
        return $"{prefix}-{Guid.NewGuid():N}"[..32];
    }

    private sealed record AcceptanceFixture(Guid ProjectId, Guid MilestoneId, Guid CriteriaId, Guid AcceptanceFileId);
}
