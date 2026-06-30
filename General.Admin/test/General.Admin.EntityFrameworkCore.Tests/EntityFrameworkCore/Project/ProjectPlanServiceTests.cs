using System;
using System.Threading.Tasks;
using General.Admin.EntityFrameworkCore;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Xunit;
using ProjectEntity = General.Admin.Project.Project;

namespace General.Admin.Project;

[Collection(AdminTestConsts.CollectionDefinitionName)]
public class ProjectPlanServiceTests : AdminEntityFrameworkCoreTestBase
{
    private static readonly Guid CurrentUserId = Guid.Parse("2e701e62-0953-4dd3-910b-dc6cc93ccb0d");
    private readonly IRepository<ProjectAcceptanceRecord, Guid> _projectAcceptanceRecordRepository;
    private readonly IRepository<ProjectSprint, Guid> _projectSprintRepository;
    private readonly IRepository<ProjectMilestone, Guid> _projectMilestoneRepository;
    private readonly IRepository<ProjectEntity, Guid> _projectRepository;
    private readonly IRepository<ProjectReport, Guid> _projectReportRepository;
    private readonly IRepository<ProjectRequirement, Guid> _projectRequirementRepository;
    private readonly IRepository<ProjectTask, Guid> _projectTaskRepository;
    private readonly IRepository<ProjectWorklog, Guid> _projectWorklogRepository;
    private readonly IRepository<ProjectWbsNode, Guid> _projectWbsNodeRepository;
    private readonly ProjectPlanService _service;
    private readonly IRepository<IdentityUser, Guid> _userRepository;

    public ProjectPlanServiceTests()
    {
        _projectAcceptanceRecordRepository = GetRequiredService<IRepository<ProjectAcceptanceRecord, Guid>>();
        _projectSprintRepository = GetRequiredService<IRepository<ProjectSprint, Guid>>();
        _projectMilestoneRepository = GetRequiredService<IRepository<ProjectMilestone, Guid>>();
        _projectRepository = GetRequiredService<IRepository<ProjectEntity, Guid>>();
        _projectReportRepository = GetRequiredService<IRepository<ProjectReport, Guid>>();
        _projectRequirementRepository = GetRequiredService<IRepository<ProjectRequirement, Guid>>();
        _projectTaskRepository = GetRequiredService<IRepository<ProjectTask, Guid>>();
        _projectWorklogRepository = GetRequiredService<IRepository<ProjectWorklog, Guid>>();
        _projectWbsNodeRepository = GetRequiredService<IRepository<ProjectWbsNode, Guid>>();
        _service = GetRequiredService<ProjectPlanService>();
        _userRepository = GetRequiredService<IRepository<IdentityUser, Guid>>();
    }

    [Fact]
    public async Task CreateWbsNodeAsync_Should_Save_Node_With_Sprint_And_Milestone()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("WBS 节点创建测试项目");
            var sprintId = await CreateSprintAsync(projectId, "WBS Sprint");
            var milestoneId = await CreateMilestoneAsync(projectId, "WBS 里程碑");

            var nodeId = await _service.CreateWbsNodeAsync(
                projectId,
                new ProjectWbsNodeSaveInput
                {
                    SprintId = sprintId,
                    DeliveryDescription = "提交评分配置页面。",
                    EstimateSummary = "约 5 人日。",
                    MilestoneId = milestoneId,
                    Name = "评分规则配置",
                    OwnerUserId = CurrentUserId,
                    ScopeDescription = "完成评分规则的配置能力。",
                    SortOrder = 10,
                    Status = "规划中",
                    WbsCode = "1.1"
                });

            var node = await _projectWbsNodeRepository.GetAsync(nodeId);
            node.ProjectId.ShouldBe(projectId);
            node.SprintId.ShouldBe(sprintId);
            node.MilestoneId.ShouldBe(milestoneId);
            node.OwnerUserId.ShouldBe(CurrentUserId);
            node.WbsCode.ShouldBe("1.1");
        });
    }

    [Fact]
    public async Task CreateWbsNodeAsync_Should_Reject_Parent_From_Other_Project()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("WBS 父节点归属测试项目");
            var otherProjectId = await CreateProjectAsync("其他 WBS 项目");
            var otherParentId = await CreateWbsNodeAsync(otherProjectId, "9", "其他项目父节点");

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _service.CreateWbsNodeAsync(
                    projectId,
                    new ProjectWbsNodeSaveInput
                    {
                        ParentId = otherParentId,
                        Name = "跨项目子节点",
                        Status = "规划中",
                        WbsCode = "1.2"
                    }));

            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("父节点必须属于当前项目");
        });
    }

    [Fact]
    public async Task UpdateWbsNodeAsync_Should_Reject_Parent_Loop()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("WBS 循环测试项目");
            var rootId = await CreateWbsNodeAsync(projectId, "1", "根节点");
            var childId = await CreateWbsNodeAsync(projectId, "1.1", "子节点", rootId);

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _service.UpdateWbsNodeAsync(
                    rootId,
                    new ProjectWbsNodeSaveInput
                    {
                        ParentId = childId,
                        Name = "根节点",
                        Status = "规划中",
                        WbsCode = "1"
                    }));

            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("不能形成循环");
        });
    }

    [Fact]
    public async Task DeleteWbsNodeAsync_Should_Reject_Node_With_Children()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("WBS 子节点删除测试项目");
            var rootId = await CreateWbsNodeAsync(projectId, "1", "根节点");
            await CreateWbsNodeAsync(projectId, "1.1", "子节点", rootId);

            var exception = await Should.ThrowAsync<BusinessException>(() => _service.DeleteWbsNodeAsync(rootId));

            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("存在子节点");
        });
    }

    [Fact]
    public async Task DeleteWbsNodeAsync_Should_Reject_Node_With_Linked_Tasks()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("WBS 任务关联删除测试项目");
            var nodeId = await CreateWbsNodeAsync(projectId, "1.1", "任务关联节点");
            var taskId = await CreateTaskAsync(projectId);
            await _service.UpdateTaskWbsAsync(
                taskId,
                new ProjectTaskWbsUpdateInput
                {
                    SortOrder = 0,
                    WbsCode = "1.1"
                });

            var exception = await Should.ThrowAsync<BusinessException>(() => _service.DeleteWbsNodeAsync(nodeId));

            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("已关联任务");
        });
    }

    [Fact]
    public async Task DeleteWbsNodeAsync_Should_Reject_Node_With_Linked_Requirements()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("WBS 需求关联删除测试项目");
            var nodeId = await CreateWbsNodeAsync(projectId, "1.1", "需求关联节点");
            await _projectRequirementRepository.InsertAsync(
                new ProjectRequirement(
                    Guid.NewGuid(),
                    projectId,
                    NewCode("REQ"),
                    "关联 WBS 的需求",
                    "用于验证 WBS 删除保护。",
                    "草稿",
                    1,
                    ProjectPriorities.P1,
                    "内部",
                    "功能",
                    wbsNodeId: nodeId),
                autoSave: true);

            var exception = await Should.ThrowAsync<BusinessException>(() => _service.DeleteWbsNodeAsync(nodeId));

            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("已关联需求");
        });
    }

    [Fact]
    public async Task UpdateTaskWbsAsync_Should_Save_Sprint_When_Sprint_Belongs_To_Project()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("任务 Sprint 关联测试项目");
            var taskId = await CreateTaskAsync(projectId);
            var sprintId = await CreateSprintAsync(projectId, "第 1 Sprint");

            await _service.UpdateTaskWbsAsync(
                taskId,
                new ProjectTaskWbsUpdateInput
                {
                    SprintId = sprintId,
                    SortOrder = 10,
                    WbsCode = "1.1"
                });

            var task = await _projectTaskRepository.GetAsync(taskId);
            task.SprintId.ShouldBe(sprintId);
            task.WbsCode.ShouldBe("1.1");
            task.SortOrder.ShouldBe(10);
        });
    }

    [Fact]
    public async Task UpdateTaskAsync_Should_Save_Basic_Task_Info()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("任务基础信息更新测试项目");
            var taskId = await CreateTaskAsync(projectId);
            var sprintId = await CreateSprintAsync(projectId, "第 2 Sprint");
            var wbsNodeId = await CreateWbsNodeAsync(projectId, "1.2", "Sprint 需求包");

            await _service.UpdateTaskAsync(
                taskId,
                new ProjectTaskSaveInput
                {
                    SprintId = sprintId,
                    EstimatedWorkHours = 12.5,
                    OwnerUserId = CurrentUserId,
                    PlannedEndTime = DateTime.Today.AddDays(8),
                    PlannedStartTime = DateTime.Today.AddDays(2),
                    Priority = ProjectPriorities.P0,
                    Status = ProjectStatuses.InProgress,
                    TaskCode = "TASK-UPDATED-001",
                    Title = "更新后的 Sprint 需求",
                    WbsNodeId = wbsNodeId
                });

            var task = await _projectTaskRepository.GetAsync(taskId);
            task.TaskCode.ShouldBe("TASK-UPDATED-001");
            task.Title.ShouldBe("更新后的 Sprint 需求");
            task.SprintId.ShouldBe(sprintId);
            task.WbsNodeId.ShouldBe(wbsNodeId);
            task.WbsCode.ShouldBe("1.2");
            task.EstimatedWorkHours.ShouldBe(12.5);
            task.Priority.ShouldBe(ProjectPriorities.P0);
        });
    }

    [Fact]
    public async Task UpdateTaskAsync_Should_Reject_Duplicate_Task_Code()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("任务编号重复测试项目");
            var firstTaskId = await CreateTaskAsync(projectId);
            var secondTaskId = await CreateTaskAsync(projectId);
            var firstTask = await _projectTaskRepository.GetAsync(firstTaskId);

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _service.UpdateTaskAsync(
                    secondTaskId,
                    new ProjectTaskSaveInput
                    {
                        OwnerUserId = CurrentUserId,
                        Priority = ProjectPriorities.P1,
                        Status = ProjectStatuses.InProgress,
                        TaskCode = firstTask.TaskCode,
                        Title = "重复编号需求"
                    }));

            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("任务编号在当前项目中已存在");
        });
    }

    [Fact]
    public async Task CreateSprintAsync_Should_Save_Sprint_And_Generate_Code()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("Sprint 创建测试项目");

            var firstSprintId = await _service.CreateSprintAsync(
                projectId,
                new ProjectSprintSaveInput
                {
                    EndDate = DateTime.Today.AddDays(14),
                    Name = "第 1 Sprint",
                    OwnerUserId = CurrentUserId,
                    Progress = 130,
                    StartDate = DateTime.Today,
                    Status = "进行中",
                    Summary = "支撑 Sprint 维护。"
                });
            var secondSprintId = await _service.CreateSprintAsync(
                projectId,
                new ProjectSprintSaveInput
                {
                    EndDate = DateTime.Today.AddDays(28),
                    Name = "第 2 Sprint",
                    OwnerUserId = CurrentUserId,
                    StartDate = DateTime.Today.AddDays(15),
                    Status = "规划中"
                });

            var firstSprint = await _projectSprintRepository.GetAsync(firstSprintId);
            firstSprint.ProjectId.ShouldBe(projectId);
            firstSprint.SprintCode.ShouldBe("SPR-001");
            firstSprint.Name.ShouldBe("第 1 Sprint");
            firstSprint.OwnerUserId.ShouldBe(CurrentUserId);
            firstSprint.Progress.ShouldBe(100);
            firstSprint.Summary.ShouldBe("支撑 Sprint 维护。");
            var secondSprint = await _projectSprintRepository.GetAsync(secondSprintId);
            secondSprint.SprintCode.ShouldBe("SPR-002");
        });
    }

    [Fact]
    public async Task UpdateSprintAsync_Should_Update_Fields_And_Normalize_Progress()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("Sprint 更新测试项目");
            var sprintId = await CreateSprintAsync(projectId, "第 1 Sprint");

            await _service.UpdateSprintAsync(
                sprintId,
                new ProjectSprintSaveInput
                {
                    EndDate = DateTime.Today.AddDays(45),
                    Name = "第 1 Sprint 修订",
                    OwnerUserId = CurrentUserId,
                    Progress = -5,
                    StartDate = DateTime.Today.AddDays(15),
                    Status = "规划中",
                    Summary = "更新 Sprint 计划。"
                });

            var sprint = await _projectSprintRepository.GetAsync(sprintId);
            sprint.Name.ShouldBe("第 1 Sprint 修订");
            sprint.Progress.ShouldBe(0);
            sprint.Summary.ShouldBe("更新 Sprint 计划。");
        });
    }

    [Fact]
    public async Task UpdateSprintAsync_Should_Reject_Invalid_Date_Range()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("Sprint 日期校验测试项目");
            var sprintId = await CreateSprintAsync(projectId, "日期校验 Sprint");

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _service.UpdateSprintAsync(
                    sprintId,
                    new ProjectSprintSaveInput
                    {
                        EndDate = DateTime.Today,
                        Name = "日期错误 Sprint",
                        OwnerUserId = CurrentUserId,
                        StartDate = DateTime.Today.AddDays(1),
                        Status = "规划中"
                    }));

            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("开始时间不能晚于结束时间");
        });
    }

    [Fact]
    public async Task DeleteSprintAsync_Should_Delete_When_No_References()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("Sprint 删除测试项目");
            var sprintId = await CreateSprintAsync(projectId, "可删除 Sprint");

            await _service.DeleteSprintAsync(sprintId);

            (await _projectSprintRepository.FindAsync(sprintId)).ShouldBeNull();
        });
    }

    [Fact]
    public async Task DeleteSprintAsync_Should_Reject_Referenced_Sprint()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("Sprint 引用删除测试项目");
            var sprintId = await CreateSprintAsync(projectId, "有关联 Sprint");
            await _service.CreateWbsNodeAsync(
                projectId,
                new ProjectWbsNodeSaveInput
                {
                    SprintId = sprintId,
                    Name = "Sprint 引用工作包",
                    Status = "规划中",
                    WbsCode = "6.1"
                });

            var exception = await Should.ThrowAsync<BusinessException>(() => _service.DeleteSprintAsync(sprintId));

            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("已关联 WBS 节点");
        });
    }

    [Fact]
    public async Task DeleteSprintAsync_Should_Reject_Sprint_With_Acceptance_Record()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("Sprint 验收引用删除测试项目");
            var sprintId = await CreateSprintAsync(projectId, "验收 Sprint");
            await _projectAcceptanceRecordRepository.InsertAsync(
                new ProjectAcceptanceRecord(
                    Guid.NewGuid(),
                    projectId,
                    "Sprint 验收记录",
                    "通过",
                    CurrentUserId,
                    DateTime.Now,
                    sprintId: sprintId),
                autoSave: true);

            var exception = await Should.ThrowAsync<BusinessException>(() => _service.DeleteSprintAsync(sprintId));

            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("已关联验收记录");
        });
    }

    [Fact]
    public async Task DeleteSprintAsync_Should_Reject_Sprint_With_Project_Report()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("Sprint 报告引用删除测试项目");
            var sprintId = await CreateSprintAsync(projectId, "报告 Sprint");
            await _projectReportRepository.InsertAsync(
                new ProjectReport(
                    Guid.NewGuid(),
                    projectId,
                    "周报",
                    "报告 Sprint 周报",
                    CurrentUserId,
                    DateTime.Today.AddDays(-6),
                    DateTime.Today,
                    "已提交",
                    sprintId: sprintId),
                autoSave: true);

            var exception = await Should.ThrowAsync<BusinessException>(() => _service.DeleteSprintAsync(sprintId));

            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("已关联项目报告");
        });
    }

    [Fact]
    public async Task DeleteSprintAsync_Should_Reject_Sprint_With_Milestone()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("Sprint 里程碑引用删除测试项目");
            var sprintId = await CreateSprintAsync(projectId, "里程碑 Sprint");
            await _service.CreateMilestoneAsync(
                projectId,
                new ProjectMilestoneSaveInput
                {
                    Name = "Sprint 里程碑",
                    OwnerUserId = CurrentUserId,
                    PlannedCompletionDate = DateTime.Today.AddDays(10),
                    SprintId = sprintId,
                    Status = "进行中"
                });

            var exception = await Should.ThrowAsync<BusinessException>(() => _service.DeleteSprintAsync(sprintId));

            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("已关联里程碑");
        });
    }

    [Fact]
    public async Task CreateMilestoneAsync_Should_Save_With_Sprint_And_Owner()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("里程碑创建测试项目");
            var sprintId = await CreateSprintAsync(projectId, "里程碑所属 Sprint");

            var milestoneId = await _service.CreateMilestoneAsync(
                projectId,
                new ProjectMilestoneSaveInput
                {
                    Name = "需求确认完成",
                    OwnerUserId = CurrentUserId,
                    PlannedCompletionDate = DateTime.Today.AddDays(7),
                    SprintId = sprintId,
                    Status = "进行中"
                });

            var milestone = await _projectMilestoneRepository.GetAsync(milestoneId);
            milestone.ProjectId.ShouldBe(projectId);
            milestone.SprintId.ShouldBe(sprintId);
            milestone.Name.ShouldBe("需求确认完成");
            milestone.OwnerUserId.ShouldBe(CurrentUserId);
            milestone.Status.ShouldBe("进行中");
        });
    }

    [Fact]
    public async Task UpdateMilestoneAsync_Should_Update_Basic_Fields()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("里程碑更新测试项目");
            var milestoneId = await CreateMilestoneAsync(projectId, "原里程碑");
            var sprintId = await CreateSprintAsync(projectId, "更新后 Sprint");

            await _service.UpdateMilestoneAsync(
                milestoneId,
                new ProjectMilestoneSaveInput
                {
                    ActualCompletionDate = DateTime.Today.AddDays(9),
                    Name = "UAT 完成",
                    OwnerUserId = CurrentUserId,
                    PlannedCompletionDate = DateTime.Today.AddDays(10),
                    SprintId = sprintId,
                    Status = "进行中"
                });

            var milestone = await _projectMilestoneRepository.GetAsync(milestoneId);
            milestone.Name.ShouldBe("UAT 完成");
            milestone.SprintId.ShouldBe(sprintId);
            milestone.PlannedCompletionDate.ShouldBe(DateTime.Today.AddDays(10));
            milestone.ActualCompletionDate.ShouldBe(DateTime.Today.AddDays(9));
        });
    }

    [Fact]
    public async Task UpdateMilestoneAsync_Should_Reject_Sprint_From_Other_Project()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("里程碑 Sprint 归属测试项目");
            var otherProjectId = await CreateProjectAsync("其他里程碑 Sprint 项目");
            var milestoneId = await CreateMilestoneAsync(projectId, "归属校验里程碑");
            var otherSprintId = await CreateSprintAsync(otherProjectId, "其他项目 Sprint");

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _service.UpdateMilestoneAsync(
                    milestoneId,
                    new ProjectMilestoneSaveInput
                    {
                        SprintId = otherSprintId,
                        Name = "跨项目 Sprint 里程碑",
                        OwnerUserId = CurrentUserId,
                        PlannedCompletionDate = DateTime.Today.AddDays(10),
                        Status = "进行中"
                    }));

            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("Sprint 必须属于当前项目");
        });
    }

    [Fact]
    public async Task UpdateMilestoneAsync_Should_Reject_Closing_By_Basic_Edit()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("里程碑关闭入口测试项目");
            var milestoneId = await CreateMilestoneAsync(projectId, "关闭入口校验里程碑");

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _service.UpdateMilestoneAsync(
                    milestoneId,
                    new ProjectMilestoneSaveInput
                    {
                        Name = "关闭入口校验里程碑",
                        OwnerUserId = CurrentUserId,
                        PlannedCompletionDate = DateTime.Today.AddDays(10),
                        Status = ProjectStatuses.Completed
                    }));

            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("状态更新入口");
        });
    }

    [Fact]
    public async Task DeleteMilestoneAsync_Should_Delete_When_No_References()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("里程碑删除测试项目");
            var milestoneId = await CreateMilestoneAsync(projectId, "可删除里程碑");

            await _service.DeleteMilestoneAsync(milestoneId);

            (await _projectMilestoneRepository.FindAsync(milestoneId)).ShouldBeNull();
        });
    }

    [Fact]
    public async Task DeleteMilestoneAsync_Should_Reject_Referenced_Milestone()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("里程碑引用删除测试项目");
            var milestoneId = await CreateMilestoneAsync(projectId, "有关联里程碑");
            await _service.CreateWbsNodeAsync(
                projectId,
                new ProjectWbsNodeSaveInput
                {
                    MilestoneId = milestoneId,
                    Name = "里程碑引用工作包",
                    Status = "规划中",
                    WbsCode = "7.1"
                });

            var exception = await Should.ThrowAsync<BusinessException>(() => _service.DeleteMilestoneAsync(milestoneId));

            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("已关联 WBS 节点");
        });
    }

    [Fact]
    public async Task CreateTaskFromRequirementAsync_Should_Create_Task_With_Requirement_And_Wbs()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("需求拆任务测试项目");
            var wbsNodeId = await CreateWbsNodeAsync(projectId, "2.1", "需求拆任务工作包");
            var requirementId = await CreateRequirementAsync(projectId, wbsNodeId);

            var taskId = await _service.CreateTaskFromRequirementAsync(
                projectId,
                requirementId,
                new ProjectTaskFromRequirementInput
                {
                    EstimatedWorkHours = 16,
                    OwnerUserId = CurrentUserId,
                    PlannedStartTime = DateTime.Today,
                    PlannedEndTime = DateTime.Today.AddDays(2),
                    TaskCode = "TASK-FROM-REQ",
                    Title = "实现评分规则配置"
                });

            var task = await _projectTaskRepository.GetAsync(taskId);
            task.RequirementId.ShouldBe(requirementId);
            task.WbsNodeId.ShouldBe(wbsNodeId);
            task.WbsCode.ShouldBe("2.1");
            task.EstimatedWorkHours.ShouldBe(16);
            task.OwnerUserId.ShouldBe(CurrentUserId);
            task.Title.ShouldBe("实现评分规则配置");
        });
    }

    [Fact]
    public async Task CreateTaskFromRequirementAsync_Should_Reject_Requirement_From_Other_Project()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("需求拆任务归属测试项目");
            var otherProjectId = await CreateProjectAsync("其他需求拆任务项目");
            var otherRequirementId = await CreateRequirementAsync(otherProjectId);

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _service.CreateTaskFromRequirementAsync(
                    projectId,
                    otherRequirementId,
                    new ProjectTaskFromRequirementInput
                    {
                        OwnerUserId = CurrentUserId,
                        Title = "跨项目任务"
                    }));

            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("需求必须属于当前项目");
        });
    }

    [Fact]
    public async Task CreateRequirementFromWbsAsync_Should_Create_Requirement_With_Wbs_Defaults()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("WBS 生成需求测试项目");
            var sprintId = await CreateSprintAsync(projectId, "WBS 需求 Sprint");
            var milestoneId = await CreateMilestoneAsync(projectId, "WBS 需求里程碑");
            var wbsNodeId = await _service.CreateWbsNodeAsync(
                projectId,
                new ProjectWbsNodeSaveInput
                {
                    SprintId = sprintId,
                    DeliveryDescription = "完成规则配置验收。",
                    MilestoneId = milestoneId,
                    Name = "评分规则工作包",
                    OwnerUserId = CurrentUserId,
                    ScopeDescription = "支持配置评分规则。",
                    Status = "规划中",
                    WbsCode = "3.1"
                });

            var requirementId = await _service.CreateRequirementFromWbsAsync(
                projectId,
                wbsNodeId,
                new ProjectRequirementFromWbsInput
                {
                    RequirementCode = "REQ-FROM-WBS",
                    SizeEstimate = 13
                });

            var requirement = await _projectRequirementRepository.GetAsync(requirementId);
            requirement.RequirementCode.ShouldBe("REQ-FROM-WBS");
            requirement.Title.ShouldBe("评分规则工作包");
            requirement.Description.ShouldBe("支持配置评分规则。");
            requirement.AcceptanceCriteria.ShouldBe("完成规则配置验收。");
            requirement.WbsNodeId.ShouldBe(wbsNodeId);
            requirement.MilestoneId.ShouldBe(milestoneId);
            requirement.ProductOwnerUserId.ShouldBe(CurrentUserId);
            requirement.SizeEstimate.ShouldBe(13);
        });
    }

    [Fact]
    public async Task CreateRequirementFromWbsAsync_Should_Reject_Wbs_From_Other_Project()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("WBS 生成需求归属测试项目");
            var otherProjectId = await CreateProjectAsync("其他 WBS 生成需求项目");
            var otherWbsNodeId = await CreateWbsNodeAsync(otherProjectId, "9.1", "其他项目工作包");

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _service.CreateRequirementFromWbsAsync(
                    projectId,
                    otherWbsNodeId,
                    new ProjectRequirementFromWbsInput
                    {
                        Title = "跨项目需求"
                    }));

            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("WBS 节点必须属于当前项目");
        });
    }

    [Fact]
    public async Task CreateTaskFromWbsAsync_Should_Create_Task_With_Wbs_Defaults()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("WBS 生成任务测试项目");
            var sprintId = await CreateSprintAsync(projectId, "WBS 任务 Sprint");
            var wbsNodeId = await _service.CreateWbsNodeAsync(
                projectId,
                new ProjectWbsNodeSaveInput
                {
                    SprintId = sprintId,
                    Name = "评分规则工作包",
                    OwnerUserId = CurrentUserId,
                    Status = "规划中",
                    WbsCode = "4.1"
                });

            var taskId = await _service.CreateTaskFromWbsAsync(
                projectId,
                wbsNodeId,
                new ProjectTaskFromWbsInput
                {
                    EstimatedWorkHours = 12,
                    OwnerUserId = CurrentUserId,
                    TaskCode = "TASK-FROM-WBS"
                });

            var task = await _projectTaskRepository.GetAsync(taskId);
            task.TaskCode.ShouldBe("TASK-FROM-WBS");
            task.Title.ShouldBe("评分规则工作包");
            task.WbsNodeId.ShouldBe(wbsNodeId);
            task.WbsCode.ShouldBe("4.1");
            task.SprintId.ShouldBe(sprintId);
            task.RequirementId.ShouldBeNull();
            task.EstimatedWorkHours.ShouldBe(12);
            task.OwnerUserId.ShouldBe(CurrentUserId);
        });
    }

    [Fact]
    public async Task CreateTaskFromWbsAsync_Should_Use_Input_Sprint_When_Provided()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("WBS 生成任务指定 Sprint 项目");
            var defaultSprintId = await CreateSprintAsync(projectId, "WBS 默认 Sprint");
            var sprintSprintId = await CreateSprintAsync(projectId, "Sprint 1");
            var wbsNodeId = await _service.CreateWbsNodeAsync(
                projectId,
                new ProjectWbsNodeSaveInput
                {
                    SprintId = defaultSprintId,
                    Name = "评分规则工作包",
                    OwnerUserId = CurrentUserId,
                    Status = "规划中",
                    WbsCode = "4.2"
                });

            var taskId = await _service.CreateTaskFromWbsAsync(
                projectId,
                wbsNodeId,
                new ProjectTaskFromWbsInput
                {
                    SprintId = sprintSprintId,
                    OwnerUserId = CurrentUserId,
                    TaskCode = "TASK-FROM-WBS-SPRINT",
                    Title = "冲刺内任务"
                });

            var task = await _projectTaskRepository.GetAsync(taskId);
            task.SprintId.ShouldBe(sprintSprintId);
            task.WbsNodeId.ShouldBe(wbsNodeId);
        });
    }

    [Fact]
    public async Task CreateTaskFromWbsAsync_Should_Reject_Wbs_From_Other_Project()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("WBS 生成任务归属测试项目");
            var otherProjectId = await CreateProjectAsync("其他 WBS 生成任务项目");
            var otherWbsNodeId = await CreateWbsNodeAsync(otherProjectId, "8.1", "其他项目工作包");

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _service.CreateTaskFromWbsAsync(
                    projectId,
                    otherWbsNodeId,
                    new ProjectTaskFromWbsInput
                    {
                        OwnerUserId = CurrentUserId,
                        Title = "跨项目任务"
                    }));

            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("WBS 节点必须属于当前项目");
        });
    }

    [Fact]
    public async Task CreateFromWbsAsync_Should_Reject_Non_WorkPackage_Node()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("WBS 非工作包生成测试项目");
            var moduleNodeId = await CreateWbsNodeAsync(projectId, "5", "评分模块");

            var requirementException = await Should.ThrowAsync<BusinessException>(() =>
                _service.CreateRequirementFromWbsAsync(
                    projectId,
                    moduleNodeId,
                    new ProjectRequirementFromWbsInput()));
            requirementException.Code.ShouldNotBeNull();
            requirementException.Code.ShouldContain("只有工作包节点才能生成需求");

            var taskException = await Should.ThrowAsync<BusinessException>(() =>
                _service.CreateTaskFromWbsAsync(
                    projectId,
                    moduleNodeId,
                    new ProjectTaskFromWbsInput
                    {
                        OwnerUserId = CurrentUserId
                    }));
            taskException.Code.ShouldNotBeNull();
            taskException.Code.ShouldContain("只有工作包节点才能生成任务");
        });
    }

    [Fact]
    public async Task UpdateTaskWbsAsync_Should_Save_WbsNode_And_Sync_WbsCode()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("任务 WBS 节点关联测试项目");
            var taskId = await CreateTaskAsync(projectId);
            var wbsNodeId = await CreateWbsNodeAsync(projectId, "1.2", "评分规则工作包");

            await _service.UpdateTaskWbsAsync(
                taskId,
                new ProjectTaskWbsUpdateInput
                {
                    SortOrder = 20,
                    WbsCode = "手工输入会被节点编码覆盖",
                    WbsNodeId = wbsNodeId
                });

            var task = await _projectTaskRepository.GetAsync(taskId);
            task.WbsNodeId.ShouldBe(wbsNodeId);
            task.WbsCode.ShouldBe("1.2");
            task.SortOrder.ShouldBe(20);
        });
    }

    [Fact]
    public async Task UpdateTaskWbsAsync_Should_Save_Requirement_And_Inherit_WbsNode()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("任务需求关联测试项目");
            var taskId = await CreateTaskAsync(projectId);
            var wbsNodeId = await CreateWbsNodeAsync(projectId, "1.3", "需求工作包");
            var requirementId = await CreateRequirementAsync(projectId, wbsNodeId);

            await _service.UpdateTaskWbsAsync(
                taskId,
                new ProjectTaskWbsUpdateInput
                {
                    RequirementId = requirementId,
                    SortOrder = 30,
                    WbsCode = string.Empty
                });

            var task = await _projectTaskRepository.GetAsync(taskId);
            task.RequirementId.ShouldBe(requirementId);
            task.WbsNodeId.ShouldBe(wbsNodeId);
            task.WbsCode.ShouldBe("1.3");
            task.SortOrder.ShouldBe(30);
        });
    }

    [Fact]
    public async Task UpdateTaskWbsAsync_Should_Reject_Requirement_From_Other_Project()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("任务需求归属测试项目");
            var taskId = await CreateTaskAsync(projectId);
            var otherProjectId = await CreateProjectAsync("其他需求项目");
            var otherRequirementId = await CreateRequirementAsync(otherProjectId);

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _service.UpdateTaskWbsAsync(
                    taskId,
                    new ProjectTaskWbsUpdateInput
                    {
                        RequirementId = otherRequirementId,
                        SortOrder = 0,
                        WbsCode = "1.2"
                    }));

            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("需求必须属于当前项目");
        });
    }

    [Fact]
    public async Task UpdateTaskWbsAsync_Should_Reject_Mismatched_Requirement_WbsNode()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("任务需求 WBS 冲突测试项目");
            var taskId = await CreateTaskAsync(projectId);
            var requirementWbsNodeId = await CreateWbsNodeAsync(projectId, "1.4", "需求工作包");
            var otherWbsNodeId = await CreateWbsNodeAsync(projectId, "1.5", "其他工作包");
            var requirementId = await CreateRequirementAsync(projectId, requirementWbsNodeId);

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _service.UpdateTaskWbsAsync(
                    taskId,
                    new ProjectTaskWbsUpdateInput
                    {
                        RequirementId = requirementId,
                        SortOrder = 0,
                        WbsCode = "1.5",
                        WbsNodeId = otherWbsNodeId
                    }));

            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("需求与 WBS 节点不一致");
        });
    }

    [Fact]
    public async Task UpdateTaskWbsAsync_Should_Reject_WbsNode_From_Other_Project()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("任务 WBS 节点归属测试项目");
            var taskId = await CreateTaskAsync(projectId);
            var otherProjectId = await CreateProjectAsync("其他 WBS 节点项目");
            var otherWbsNodeId = await CreateWbsNodeAsync(otherProjectId, "9.1", "其他项目工作包");

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _service.UpdateTaskWbsAsync(
                    taskId,
                    new ProjectTaskWbsUpdateInput
                    {
                        SortOrder = 0,
                        WbsCode = "1.2",
                        WbsNodeId = otherWbsNodeId
                    }));

            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("WBS 节点必须属于当前项目");
        });
    }

    [Fact]
    public async Task UpdateTaskWbsAsync_Should_Reject_Sprint_From_Other_Project()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("任务 Sprint 归属测试项目");
            var taskId = await CreateTaskAsync(projectId);
            var otherProjectId = await CreateProjectAsync("其他 Sprint 项目");
            var otherSprintId = await CreateSprintAsync(otherProjectId, "其他项目 Sprint");

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _service.UpdateTaskWbsAsync(
                    taskId,
                    new ProjectTaskWbsUpdateInput
                    {
                        SprintId = otherSprintId,
                        SortOrder = 0,
                        WbsCode = "1.2"
                    }));

            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("Sprint 必须属于当前项目");
        });
    }

    [Fact]
    public async Task RecordTaskProgressAsync_Should_Create_Worklog_And_Update_Task_Execution()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("任务进展回填测试项目");
            var taskId = await CreateTaskAsync(projectId);
            var firstWorkDate = DateTime.Today.AddDays(-1);

            var worklogId = await _service.RecordTaskProgressAsync(
                taskId,
                new ProjectTaskProgressInput
                {
                    Hours = 2.5,
                    Status = ProjectStatuses.InProgress,
                    Summary = "完成接口联调。",
                    WorkDate = firstWorkDate
                });

            var worklog = await _projectWorklogRepository.GetAsync(worklogId);
            worklog.ProjectId.ShouldBe(projectId);
            worklog.TaskId.ShouldBe(taskId);
            worklog.UserId.ShouldBe(CurrentUserId);
            worklog.Hours.ShouldBe(2.5);
            worklog.Summary.ShouldBe("完成接口联调。");
            worklog.WorkDate.ShouldBe(firstWorkDate.Date);
            worklog.WeekStartDate.ShouldBe(ResolveWeekStartDate(firstWorkDate));

            var task = await _projectTaskRepository.GetAsync(taskId);
            task.Status.ShouldBe(ProjectStatuses.InProgress);
            task.ActualWorkHours.ShouldBe(2.5);
            task.ActualStartTime.ShouldBe(firstWorkDate.Date);
            task.ActualEndTime.ShouldBeNull();
            task.IsBlocked.ShouldBeFalse();

            var completedWorkDate = DateTime.Today;
            await _service.RecordTaskProgressAsync(
                taskId,
                new ProjectTaskProgressInput
                {
                    Hours = 1.5,
                    Status = ProjectStatuses.Completed,
                    Summary = "完成自测并关闭任务。",
                    WorkDate = completedWorkDate
                });

            task = await _projectTaskRepository.GetAsync(taskId);
            task.Status.ShouldBe(ProjectStatuses.Completed);
            task.ActualWorkHours.ShouldBe(4);
            task.ActualStartTime.ShouldBe(firstWorkDate.Date);
            task.ActualEndTime.ShouldBe(completedWorkDate.Date);
        });
    }

    [Fact]
    public async Task RecordTaskProgressAsync_Should_Reject_Invalid_Hours()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("任务工时校验测试项目");
            var taskId = await CreateTaskAsync(projectId);

            var zeroHourException = await Should.ThrowAsync<BusinessException>(() =>
                _service.RecordTaskProgressAsync(
                    taskId,
                    new ProjectTaskProgressInput
                    {
                        Hours = 0,
                        Status = ProjectStatuses.InProgress,
                        Summary = "无效工时。"
                    }));
            zeroHourException.Code.ShouldNotBeNull();
            zeroHourException.Code.ShouldContain("本次工时必须大于 0");

            var excessiveHourException = await Should.ThrowAsync<BusinessException>(() =>
                _service.RecordTaskProgressAsync(
                    taskId,
                    new ProjectTaskProgressInput
                    {
                        Hours = 25,
                        Status = ProjectStatuses.InProgress,
                        Summary = "无效工时。"
                    }));
            excessiveHourException.Code.ShouldNotBeNull();
            excessiveHourException.Code.ShouldContain("本次工时必须大于 0");
        });
    }

    [Fact]
    public async Task RecordTaskProgressAsync_Should_Reject_Blocked_Task_Without_Block_Reason()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("任务阻塞校验测试项目");
            var taskId = await CreateTaskAsync(projectId);

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _service.RecordTaskProgressAsync(
                    taskId,
                    new ProjectTaskProgressInput
                    {
                        Hours = 1,
                        IsBlocked = true,
                        Status = "阻塞",
                        Summary = "接口被外部依赖阻塞。"
                    }));

            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("阻塞原因");
        });
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
                "用于验证计划 Sprint 关联。"),
            autoSave: true);
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
            20,
            "用于验证任务 Sprint 关联。");
        await _projectSprintRepository.InsertAsync(sprint, autoSave: true);
        return sprint.Id;
    }

    private async Task<Guid> CreateMilestoneAsync(Guid projectId, string name)
    {
        var milestone = new ProjectMilestone(
            Guid.NewGuid(),
            projectId,
            name,
            CurrentUserId,
            DateTime.Today.AddDays(10),
            ProjectStatuses.InProgress);
        await _projectMilestoneRepository.InsertAsync(milestone, autoSave: true);
        return milestone.Id;
    }

    private async Task<Guid> CreateTaskAsync(Guid projectId)
    {
        var task = new ProjectTask(
            Guid.NewGuid(),
            projectId,
            Guid.NewGuid(),
            NewCode("TASK"),
            "实现计划 Sprint 关联",
            CurrentUserId,
            ProjectStatuses.InProgress,
            ProjectPriorities.P1,
            DateTime.Today,
            DateTime.Today.AddDays(3));
        await _projectTaskRepository.InsertAsync(task, autoSave: true);
        return task.Id;
    }

    private async Task<Guid> CreateRequirementAsync(Guid projectId, Guid? wbsNodeId = null)
    {
        var requirement = new ProjectRequirement(
            Guid.NewGuid(),
            projectId,
            NewCode("REQ"),
            "评分规则配置需求",
            "用于验证任务和需求的关联。",
            "草稿",
            1,
            ProjectPriorities.P1,
            "内部",
            "功能",
            wbsNodeId: wbsNodeId);
        await _projectRequirementRepository.InsertAsync(requirement, autoSave: true);
        return requirement.Id;
    }

    private async Task<Guid> CreateWbsNodeAsync(Guid projectId, string wbsCode, string name, Guid? parentId = null)
    {
        return await _service.CreateWbsNodeAsync(
            projectId,
            new ProjectWbsNodeSaveInput
            {
                Name = name,
                ParentId = parentId,
                Status = "规划中",
                Type = parentId.HasValue ? "工作包" : "模块",
                WbsCode = wbsCode
            });
    }

    private async Task EnsureCurrentUserAsync()
    {
        if (await _userRepository.AnyAsync(item => item.Id == CurrentUserId))
        {
            return;
        }

        var user = new IdentityUser(CurrentUserId, "project.plan.tester", "project.plan.tester@general.local");
        await _userRepository.InsertAsync(user, autoSave: true);
    }

    private static string NewCode(string prefix)
    {
        return $"{prefix}-{Guid.NewGuid():N}"[..32];
    }

    private static DateTime ResolveWeekStartDate(DateTime workDate)
    {
        return workDate.Date.AddDays(-((int)workDate.DayOfWeek + 6) % 7);
    }
}
