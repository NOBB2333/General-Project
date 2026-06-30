using System;
using System.Linq;
using System.Threading.Tasks;
using General.Admin.Business;
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
public class ProjectCommunicationServiceTests : AdminEntityFrameworkCoreTestBase
{
    private static readonly Guid CurrentUserId = Guid.Parse("2e701e62-0953-4dd3-910b-dc6cc93ccb0d");
    private readonly IRepository<BusinessBudgetExecution, Guid> _budgetExecutionRepository;
    private readonly IRepository<BusinessContract, Guid> _businessContractRepository;
    private readonly IRepository<AppPlatformFile, Guid> _fileRepository;
    private readonly IRepository<BusinessInvoice, Guid> _invoiceRepository;
    private readonly IRepository<BusinessProcurement, Guid> _procurementRepository;
    private readonly IRepository<BusinessReceivable, Guid> _receivableRepository;
    private readonly IRepository<ProjectSprint, Guid> _projectSprintRepository;
    private readonly IRepository<ProjectIssue, Guid> _projectIssueRepository;
    private readonly IRepository<ProjectMeetingActionItem, Guid> _projectMeetingActionItemRepository;
    private readonly IRepository<ProjectMeetingRecord, Guid> _projectMeetingRecordRepository;
    private readonly IRepository<ProjectMilestone, Guid> _projectMilestoneRepository;
    private readonly ProjectQueryService _projectQueryService;
    private readonly IRepository<ProjectRaidItem, Guid> _projectRaidItemRepository;
    private readonly IRepository<ProjectReport, Guid> _projectReportRepository;
    private readonly IRepository<ProjectRequirement, Guid> _projectRequirementRepository;
    private readonly IRepository<ProjectEntity, Guid> _projectRepository;
    private readonly IRepository<ProjectTask, Guid> _projectTaskRepository;
    private readonly IRepository<ProjectWorklog, Guid> _projectWorklogRepository;
    private readonly ProjectCommunicationService _service;
    private readonly IRepository<IdentityUser, Guid> _userRepository;

    public ProjectCommunicationServiceTests()
    {
        _budgetExecutionRepository = GetRequiredService<IRepository<BusinessBudgetExecution, Guid>>();
        _businessContractRepository = GetRequiredService<IRepository<BusinessContract, Guid>>();
        _fileRepository = GetRequiredService<IRepository<AppPlatformFile, Guid>>();
        _invoiceRepository = GetRequiredService<IRepository<BusinessInvoice, Guid>>();
        _procurementRepository = GetRequiredService<IRepository<BusinessProcurement, Guid>>();
        _receivableRepository = GetRequiredService<IRepository<BusinessReceivable, Guid>>();
        _projectSprintRepository = GetRequiredService<IRepository<ProjectSprint, Guid>>();
        _projectIssueRepository = GetRequiredService<IRepository<ProjectIssue, Guid>>();
        _projectMeetingActionItemRepository = GetRequiredService<IRepository<ProjectMeetingActionItem, Guid>>();
        _projectMeetingRecordRepository = GetRequiredService<IRepository<ProjectMeetingRecord, Guid>>();
        _projectMilestoneRepository = GetRequiredService<IRepository<ProjectMilestone, Guid>>();
        _projectQueryService = GetRequiredService<ProjectQueryService>();
        _projectRaidItemRepository = GetRequiredService<IRepository<ProjectRaidItem, Guid>>();
        _projectReportRepository = GetRequiredService<IRepository<ProjectReport, Guid>>();
        _projectRequirementRepository = GetRequiredService<IRepository<ProjectRequirement, Guid>>();
        _projectRepository = GetRequiredService<IRepository<ProjectEntity, Guid>>();
        _projectTaskRepository = GetRequiredService<IRepository<ProjectTask, Guid>>();
        _projectWorklogRepository = GetRequiredService<IRepository<ProjectWorklog, Guid>>();
        _service = GetRequiredService<ProjectCommunicationService>();
        _userRepository = GetRequiredService<IRepository<IdentityUser, Guid>>();
    }

    [Fact]
    public async Task GetReportDraftAsync_Should_Build_Draft_From_Project_Execution_Data()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("报告草稿测试项目");
            var periodStart = DateTime.Today.AddDays(-6);
            var periodEnd = DateTime.Today;
            var completedTaskId = await CreateTaskAsync(
                projectId,
                "TASK-DONE",
                "完成评分规则配置",
                ProjectStatuses.Completed,
                DateTime.Today.AddDays(-2),
                actualEndTime: DateTime.Today.AddDays(-1));
            await CreateTaskAsync(
                projectId,
                "TASK-NEXT",
                "开发评分报表",
                ProjectStatuses.InProgress,
                periodEnd.AddDays(3));
            await CreateWorklogAsync(projectId, completedTaskId, periodStart.AddDays(1), 6.5);
            var assistantUserId = await CreateUserAsync("project.communication.assistant", "协作成员");
            await CreateWorklogAsync(projectId, completedTaskId, periodStart.AddDays(2), 3.5, assistantUserId);
            await CreateWorklogAsync(
                projectId,
                completedTaskId,
                new DateTime(periodStart.Year, periodStart.Month, 1).AddDays(-5),
                2,
                CurrentUserId);
            await CreateMilestoneAsync(projectId, "规则配置验收", periodStart.AddDays(2));
            await CreateRiskAsync(projectId, "客户验收延期风险", periodEnd.AddDays(-1));
            await CreateIssueAsync(projectId, "测试环境阻塞", periodEnd.AddDays(-1));
            await CreateBusinessRecordsAsync(projectId, periodStart.AddDays(3));

            var draft = await _service.GetReportDraftAsync(
                projectId,
                new ProjectReportDraftInput
                {
                    PeriodStart = periodStart,
                    PeriodEnd = periodEnd,
                    ReportType = "周报"
                });

            draft.ReportType.ShouldBe("周报");
            draft.PeriodStart.ShouldBe(periodStart.Date);
            draft.PeriodEnd.ShouldBe(periodEnd.Date);
            draft.CompletedSummary.ShouldContain("完成 TASK-DONE 完成评分规则配置");
            draft.NextPlan.ShouldContain("TASK-NEXT 开发评分报表");
            draft.HoursSummary.ShouldContain("10 小时");
            draft.HoursSummary.ShouldContain("2 人");
            draft.MilestoneSummary.ShouldContain("规则配置验收");
            draft.RiskIssueSummary.ShouldContain("客户验收延期风险");
            draft.RiskIssueSummary.ShouldContain("测试环境阻塞");

            var monthlyDraft = await _service.GetReportDraftAsync(
                projectId,
                new ProjectReportDraftInput
                {
                    PeriodStart = new DateTime(periodStart.Year, periodStart.Month, 1),
                    PeriodEnd = new DateTime(periodStart.Year, periodStart.Month, 1).AddMonths(1).AddDays(-1),
                    ReportType = "月报"
                });
            monthlyDraft.BusinessSummary.ShouldContain("经营摘要已按权限脱敏");
            monthlyDraft.BusinessSummary.ShouldNotContain("120,000");
            monthlyDraft.RiskIssueSummary.ShouldContain("本月新增风险");
            monthlyDraft.RiskIssueSummary.ShouldContain("新增问题");
            monthlyDraft.RiskIssueSummary.ShouldContain("当前待闭环风险");
            monthlyDraft.RiskIssueSummary.ShouldContain("高等级风险");
            monthlyDraft.RiskIssueSummary.ShouldContain("重点跟进");
            monthlyDraft.RiskIssueSummary.ShouldContain("客户验收延期风险");
            monthlyDraft.RiskIssueSummary.ShouldContain("测试环境阻塞");
            monthlyDraft.HoursSummary.ShouldContain("本月累计工时 10 小时");
            monthlyDraft.HoursSummary.ShouldContain("参与成员 2 人");
            monthlyDraft.HoursSummary.ShouldContain("较上期工时 +8 小时");
            monthlyDraft.HoursSummary.ShouldContain("有任务关联的工时 10 小时");
            monthlyDraft.HoursSummary.ShouldContain("协作成员 3.5 小时");
            monthlyDraft.HoursSummary.ShouldContain("不作为个人绩效评价");
            monthlyDraft.CompletedSummary.ShouldNotBeEmpty();
        });
    }

    [Fact]
    public async Task CreateReportAsync_Should_Save_Report_And_Project_Detail_Projection()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("报告保存测试项目");
            var sprintId = await CreateSprintAsync(projectId, "第 1 Sprint");
            var attachmentFileId = await CreateFileAsync(projectId, "Project/Report", "documents/reports", "report.pdf");

            var reportId = await _service.CreateReportAsync(
                projectId,
                new ProjectReportSaveInput
                {
                    AttachmentFileId = attachmentFileId,
                    AuthorUserId = CurrentUserId,
                    BusinessSummary = "经营摘要已脱敏。",
                    CompletedSummary = "本周完成评分配置。",
                    SprintId = sprintId,
                    HoursSummary = "本周工时 12 小时。",
                    MilestoneSummary = "里程碑按计划推进。",
                    NextPlan = "下周完成评分报表。",
                    PeriodStart = DateTime.Today.AddDays(-6),
                    PeriodEnd = DateTime.Today,
                    ReportType = "周报",
                    RiskIssueSummary = "暂无高风险。",
                    Status = "已提交",
                    Title = "评分系统周报"
                });

            var report = await _projectReportRepository.GetAsync(reportId);
            report.ProjectId.ShouldBe(projectId);
            report.SprintId.ShouldBe(sprintId);
            report.AttachmentFileId.ShouldBe(attachmentFileId);
            report.BusinessSummary.ShouldBe("经营摘要已脱敏。");
            report.Title.ShouldBe("评分系统周报");

            var detail = await _projectQueryService.GetDetailAsync(projectId);
            var reportDto = detail.Reports.Single(item => item.Id == reportId);
            reportDto.SprintName.ShouldBe("第 1 Sprint");
            reportDto.AttachmentFileName.ShouldBe("report.pdf");
            reportDto.AuthorName.ShouldNotBeEmpty();
            reportDto.BusinessSummary.ShouldBe("经营摘要已脱敏。");
        });
    }

    [Fact]
    public async Task CreateMeetingRecordAsync_Should_Save_Record_Action_Items_And_Detail_Projection()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("会议记录测试项目");
            var taskId = await CreateTaskAsync(projectId, "TASK-MEETING", "补充会议行动项任务", ProjectStatuses.InProgress, DateTime.Today.AddDays(2));
            var riskId = await CreateRiskAsync(projectId, "需求范围风险", DateTime.Today.AddDays(5));
            var issueId = await CreateIssueAsync(projectId, "接口联调问题", DateTime.Today.AddDays(4));
            var requirementId = await CreateRequirementAsync(projectId, "REQ-MEETING", "会议确认的评分需求");
            var attachmentFileId = await CreateFileAsync(projectId, "Project/Meeting", "documents/meetings", "meeting.docx");

            var meetingRecordId = await _service.CreateMeetingRecordAsync(
                projectId,
                new ProjectMeetingRecordSaveInput
                {
                    ActionItems =
                    [
                        new ProjectMeetingActionItemSaveInput
                        {
                            ActionType = "转任务",
                            AssigneeUserId = CurrentUserId,
                            DueDate = DateTime.Today.AddDays(2),
                            EstimatedWorkHours = 4,
                            Priority = ProjectPriorities.P0,
                            TaskCode = "TASK-ACTION",
                            Status = "进行中",
                            Title = "补充评分规则验收用例"
                        },
                        new ProjectMeetingActionItemSaveInput
                        {
                            ActionType = "关联任务",
                            LinkedTaskId = taskId,
                            Status = "进行中",
                            Title = "跟踪既有会议行动项任务"
                        },
                        new ProjectMeetingActionItemSaveInput
                        {
                            ActionType = "关联风险",
                            LinkedRaidItemId = riskId,
                            Status = "未开始",
                            Title = "跟踪需求范围风险"
                        },
                        new ProjectMeetingActionItemSaveInput
                        {
                            ActionType = "关联问题",
                            LinkedIssueId = issueId,
                            Status = "未开始",
                            Title = "推进接口联调问题"
                        },
                        new ProjectMeetingActionItemSaveInput
                        {
                            ActionType = "关联需求",
                            LinkedRequirementId = requirementId,
                            Status = "未开始",
                            Title = "跟踪会议确认的评分需求"
                        },
                        new ProjectMeetingActionItemSaveInput
                        {
                            ActionType = "生成需求",
                            AssigneeUserId = CurrentUserId,
                            DueDate = DateTime.Today.AddDays(9),
                            Priority = ProjectPriorities.P0,
                            Status = "未开始",
                            Title = "新增评分结果导出需求"
                        },
                        new ProjectMeetingActionItemSaveInput
                        {
                            ActionType = "生成风险",
                            AssigneeUserId = CurrentUserId,
                            DueDate = DateTime.Today.AddDays(6),
                            GeneratedRiskResponseStrategy = "安排接口人每日同步。",
                            Priority = ProjectPriorities.P0,
                            Status = "未开始",
                            Title = "客户接口人响应不稳定"
                        },
                        new ProjectMeetingActionItemSaveInput
                        {
                            ActionType = "生成问题",
                            AssigneeUserId = CurrentUserId,
                            DueDate = DateTime.Today.AddDays(3),
                            GeneratedIssueType = "需求问题",
                            Priority = ProjectPriorities.P1,
                            Status = "未开始",
                            Title = "会议确认的验收口径缺失"
                        }
                    ],
                    AttachmentFileId = attachmentFileId,
                    Decisions = "确认下周优先完成验收用例。",
                    FacilitatorUserId = CurrentUserId,
                    MeetingTime = DateTime.Today.AddHours(10),
                    Minutes = "讨论评分配置范围和验收节点。",
                    ParticipantUserIds = [CurrentUserId],
                    Status = "已确认",
                    Topic = "评分规则推进会"
                });

            var meetingRecord = await _projectMeetingRecordRepository.GetAsync(meetingRecordId);
            meetingRecord.ProjectId.ShouldBe(projectId);
            meetingRecord.AttachmentFileId.ShouldBe(attachmentFileId);
            meetingRecord.GetParticipantUserIds().ShouldBe([CurrentUserId]);

            var actionItems = await _projectMeetingActionItemRepository.GetListAsync(item => item.MeetingRecordId == meetingRecordId);
            actionItems.Count.ShouldBe(8);
            var convertedTask = (await _projectTaskRepository.GetListAsync(item =>
                item.ProjectId == projectId && item.TaskCode == "TASK-ACTION")).Single();
            convertedTask.Title.ShouldBe("补充评分规则验收用例");
            convertedTask.OwnerUserId.ShouldBe(CurrentUserId);
            convertedTask.PlannedEndTime.ShouldBe(DateTime.Today.AddDays(2));
            convertedTask.EstimatedWorkHours.ShouldBe(4);
            var generatedRisk = (await _projectRaidItemRepository.GetListAsync(item =>
                item.ProjectId == projectId && item.Title == "客户接口人响应不稳定")).Single();
            generatedRisk.Type.ShouldBe(ProjectRaidTypes.Risk);
            generatedRisk.OwnerUserId.ShouldBe(CurrentUserId);
            generatedRisk.Level.ShouldBe(ProjectPriorities.P0);
            generatedRisk.PlannedResolveDate.ShouldBe(DateTime.Today.AddDays(6));
            generatedRisk.ResponseStrategy.ShouldBe("安排接口人每日同步。");
            var generatedIssue = (await _projectIssueRepository.GetListAsync(item =>
                item.ProjectId == projectId && item.Title == "会议确认的验收口径缺失")).Single();
            generatedIssue.Type.ShouldBe("需求问题");
            generatedIssue.OwnerUserId.ShouldBe(CurrentUserId);
            generatedIssue.Level.ShouldBe(ProjectPriorities.P1);
            generatedIssue.DueDate.ShouldBe(DateTime.Today.AddDays(3));
            generatedIssue.RequirementTitle.ShouldBe("来源会议行动项");
            var generatedRequirement = (await _projectRequirementRepository.GetListAsync(item =>
                item.ProjectId == projectId && item.Title == "新增评分结果导出需求")).Single();
            generatedRequirement.Source.ShouldBe("会议行动项");
            generatedRequirement.Status.ShouldBe("草稿");
            generatedRequirement.Priority.ShouldBe(ProjectPriorities.P0);
            generatedRequirement.ProductOwnerUserId.ShouldBe(CurrentUserId);
            generatedRequirement.AcceptanceCriteria.ShouldBe("待补充");
            generatedRequirement.ImpactSummary.ShouldContain(DateTime.Today.AddDays(9).ToString("yyyy-MM-dd"));
            actionItems.ShouldContain(item => item.ActionType == "转任务" && item.LinkedTaskId == convertedTask.Id);
            actionItems.ShouldContain(item => item.LinkedTaskId == taskId);
            actionItems.ShouldContain(item => item.ActionType == "关联需求" && item.LinkedRequirementId == requirementId);
            actionItems.ShouldContain(item => item.ActionType == "生成需求" && item.LinkedRequirementId == generatedRequirement.Id);
            actionItems.ShouldContain(item => item.LinkedRaidItemId == riskId);
            actionItems.ShouldContain(item => item.LinkedIssueId == issueId);
            actionItems.ShouldContain(item => item.ActionType == "生成风险" && item.LinkedRaidItemId == generatedRisk.Id);
            actionItems.ShouldContain(item => item.ActionType == "生成问题" && item.LinkedIssueId == generatedIssue.Id);

            var detail = await _projectQueryService.GetDetailAsync(projectId);
            var meetingDto = detail.MeetingRecords.Single(item => item.Id == meetingRecordId);
            meetingDto.AttachmentFileName.ShouldBe("meeting.docx");
            meetingDto.FacilitatorName.ShouldNotBeEmpty();
            meetingDto.ParticipantNames.ShouldContain(item => !string.IsNullOrWhiteSpace(item));
            meetingDto.ActionItems.Count.ShouldBe(8);
            meetingDto.ActionItems.ShouldContain(item => item.LinkedTaskTitle == "补充评分规则验收用例");
            meetingDto.ActionItems.ShouldContain(item => item.LinkedTaskTitle == "补充会议行动项任务");
            meetingDto.ActionItems.ShouldContain(item => item.LinkedRequirementTitle == "会议确认的评分需求");
            meetingDto.ActionItems.ShouldContain(item => item.LinkedRequirementTitle == "新增评分结果导出需求");
            meetingDto.ActionItems.ShouldContain(item => item.LinkedRaidItemTitle == "需求范围风险");
            meetingDto.ActionItems.ShouldContain(item => item.LinkedIssueTitle == "接口联调问题");
            meetingDto.ActionItems.ShouldContain(item => item.LinkedRaidItemTitle == "客户接口人响应不稳定");
            meetingDto.ActionItems.ShouldContain(item => item.LinkedIssueTitle == "会议确认的验收口径缺失");
        });
    }

    [Fact]
    public async Task CreateMeetingRecordAsync_Should_Reject_Action_Item_Link_From_Other_Project()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("会议行动项归属测试项目");
            var otherProjectId = await CreateProjectAsync("其他会议行动项项目");
            var otherTaskId = await CreateTaskAsync(otherProjectId, "TASK-OTHER", "其他项目任务", ProjectStatuses.InProgress, DateTime.Today.AddDays(2));
            var otherRequirementId = await CreateRequirementAsync(otherProjectId, "REQ-OTHER", "其他项目需求");

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _service.CreateMeetingRecordAsync(
                    projectId,
                    new ProjectMeetingRecordSaveInput
                    {
                        ActionItems =
                        [
                            new ProjectMeetingActionItemSaveInput
                            {
                                ActionType = "关联任务",
                                LinkedTaskId = otherTaskId,
                                Status = "未开始",
                                Title = "跨项目行动项"
                            }
                        ],
                        FacilitatorUserId = CurrentUserId,
                        Status = "草稿",
                        Topic = "跨项目行动项测试"
                    }));

            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("会议行动项关联任务必须属于当前项目");

            var requirementException = await Should.ThrowAsync<BusinessException>(() =>
                _service.CreateMeetingRecordAsync(
                    projectId,
                    new ProjectMeetingRecordSaveInput
                    {
                        ActionItems =
                        [
                            new ProjectMeetingActionItemSaveInput
                            {
                                ActionType = "关联需求",
                                LinkedRequirementId = otherRequirementId,
                                Status = "未开始",
                                Title = "跨项目需求行动项"
                            }
                        ],
                        FacilitatorUserId = CurrentUserId,
                        Status = "草稿",
                        Topic = "跨项目需求行动项测试"
                    }));

            requirementException.Code.ShouldNotBeNull();
            requirementException.Code.ShouldContain("会议行动项关联需求必须属于当前项目");
        });
    }

    [Fact]
    public async Task CreateMeetingRecordAsync_Should_Require_Assignee_When_Converting_Action_Item_To_Task()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("会议行动项转任务校验项目");

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _service.CreateMeetingRecordAsync(
                    projectId,
                    new ProjectMeetingRecordSaveInput
                    {
                        ActionItems =
                        [
                            new ProjectMeetingActionItemSaveInput
                            {
                                ActionType = "转任务",
                                Status = "未开始",
                                Title = "缺少负责人的转任务行动项"
                            }
                        ],
                        FacilitatorUserId = CurrentUserId,
                        Status = "草稿",
                        Topic = "转任务校验"
                    }));

            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("转任务行动项必须指定负责人");
        });
    }

    [Fact]
    public async Task CreateMeetingRecordAsync_Should_Require_Assignee_When_Generating_Risk_Or_Issue()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("会议行动项生成风险问题校验项目");

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _service.CreateMeetingRecordAsync(
                    projectId,
                    new ProjectMeetingRecordSaveInput
                    {
                        ActionItems =
                        [
                            new ProjectMeetingActionItemSaveInput
                            {
                                ActionType = "生成风险",
                                Status = "未开始",
                                Title = "缺少负责人的风险行动项"
                            }
                        ],
                        FacilitatorUserId = CurrentUserId,
                        Status = "草稿",
                        Topic = "生成风险校验"
                    }));

            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("生成风险或问题的行动项必须指定负责人");
        });
    }

    [Fact]
    public async Task CreateReportAsync_Should_Reject_Attachment_From_Other_Project()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("报告附件归属测试项目");
            var otherProjectId = await CreateProjectAsync("其他报告附件项目");
            var otherAttachmentFileId = await CreateFileAsync(otherProjectId, "Project/Report", "documents/reports", "other-report.pdf");

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _service.CreateReportAsync(
                    projectId,
                    new ProjectReportSaveInput
                    {
                        AttachmentFileId = otherAttachmentFileId,
                        AuthorUserId = CurrentUserId,
                        ReportType = "周报",
                        Status = "草稿",
                        Title = "跨项目附件周报"
                    }));

            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("附件文件必须属于当前项目");
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
                DateTime.Today.AddDays(-10),
                DateTime.Today.AddDays(30),
                false,
                "用于验证项目报告和会议记录。"),
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
            DateTime.Today.AddDays(-6),
            DateTime.Today,
            ProjectStatuses.InProgress,
            50);
        await _projectSprintRepository.InsertAsync(sprint, autoSave: true);
        return sprint.Id;
    }

    private async Task<Guid> CreateTaskAsync(
        Guid projectId,
        string taskCode,
        string title,
        string status,
        DateTime plannedEndTime,
        DateTime? actualEndTime = null)
    {
        var task = new ProjectTask(
            Guid.NewGuid(),
            projectId,
            Guid.NewGuid(),
            taskCode,
            title,
            CurrentUserId,
            status,
            ProjectPriorities.P1,
            plannedEndTime.AddDays(-2),
            plannedEndTime,
            estimatedWorkHours: 8,
            actualWorkHours: actualEndTime.HasValue ? 8 : null,
            actualStartTime: actualEndTime?.AddDays(-2),
            actualEndTime: actualEndTime);
        await _projectTaskRepository.InsertAsync(task, autoSave: true);
        return task.Id;
    }

    private async Task CreateWorklogAsync(Guid projectId, Guid taskId, DateTime workDate, double hours, Guid? userId = null)
    {
        await _projectWorklogRepository.InsertAsync(
            new ProjectWorklog(
                Guid.NewGuid(),
                projectId,
                userId ?? CurrentUserId,
                workDate.AddDays(-((int)workDate.DayOfWeek + 6) % 7),
                workDate,
                hours,
                "推进项目报告自动草稿。",
                taskId),
            autoSave: true);
    }

    private async Task<Guid> CreateMilestoneAsync(Guid projectId, string name, DateTime plannedCompletionDate)
    {
        var milestone = new ProjectMilestone(
            Guid.NewGuid(),
            projectId,
            name,
            CurrentUserId,
            plannedCompletionDate,
            ProjectStatuses.InProgress);
        await _projectMilestoneRepository.InsertAsync(milestone, autoSave: true);
        return milestone.Id;
    }

    private async Task<Guid> CreateRiskAsync(Guid projectId, string title, DateTime plannedResolveDate)
    {
        var risk = new ProjectRaidItem(
            Guid.NewGuid(),
            projectId,
            ProjectRaidTypes.Risk,
            title,
            ProjectPriorities.P0,
            CurrentUserId,
            ProjectStatuses.InProgress,
            plannedResolveDate,
            4,
            4);
        await _projectRaidItemRepository.InsertAsync(risk, autoSave: true);
        return risk.Id;
    }

    private async Task<Guid> CreateIssueAsync(Guid projectId, string title, DateTime dueDate)
    {
        var issue = new ProjectIssue(
            Guid.NewGuid(),
            projectId,
            "执行问题",
            title,
            ProjectPriorities.P0,
            CurrentUserId,
            "待处理",
            dueDate);
        await _projectIssueRepository.InsertAsync(issue, autoSave: true);
        return issue.Id;
    }

    private async Task<Guid> CreateRequirementAsync(Guid projectId, string requirementCode, string title)
    {
        var requirement = new ProjectRequirement(
            Guid.NewGuid(),
            projectId,
            requirementCode,
            title,
            "用于验证会议行动项关联需求。",
            "草稿",
            1,
            ProjectPriorities.P1,
            "会议",
            "功能",
            acceptanceCriteria: "满足会议确认的验收口径。");
        await _projectRequirementRepository.InsertAsync(requirement, autoSave: true);
        return requirement.Id;
    }

    private async Task CreateBusinessRecordsAsync(Guid projectId, DateTime businessDate)
    {
        await _businessContractRepository.InsertAsync(
            new BusinessContract(
                Guid.NewGuid(),
                projectId,
                NewCode("CON"),
                "月报收入合同",
                "收入合同",
                "测试客户",
                120_000m,
                businessDate,
                "已签约",
                isRevenueContract: true),
            autoSave: true);
        await _procurementRepository.InsertAsync(
            new BusinessProcurement(
                Guid.NewGuid(),
                projectId,
                NewCode("PUR"),
                "月报采购",
                "测试供应商",
                30_000m,
                businessDate,
                "合同签订",
                "执行中"),
            autoSave: true);
        await _receivableRepository.InsertAsync(
            new BusinessReceivable(
                Guid.NewGuid(),
                projectId,
                NewCode("REC"),
                "月报回款",
                businessDate,
                80_000m,
                50_000m,
                "部分回款"),
            autoSave: true);
        await _invoiceRepository.InsertAsync(
            new BusinessInvoice(
                Guid.NewGuid(),
                projectId,
                NewCode("INV"),
                "月报开票",
                businessDate,
                60_000m,
                3_600m,
                "已开票"),
            autoSave: true);
        await _budgetExecutionRepository.InsertAsync(
            new BusinessBudgetExecution(
                Guid.NewGuid(),
                projectId,
                NewCode("BUD"),
                "实施成本",
                70_000m,
                20_000m,
                50_000m,
                1),
            autoSave: true);
    }

    private async Task<Guid> CreateFileAsync(Guid projectId, string category, string parentPath, string fileName)
    {
        var fileId = Guid.NewGuid();
        await _fileRepository.InsertAsync(
            new AppPlatformFile(
                fileId,
                $"project/{Guid.NewGuid():N}/{fileName}",
                fileName,
                "application/pdf",
                128,
                category,
                parentPath,
                $"project/{Guid.NewGuid():N}/{fileName}",
                PlatformFileStorageNames.Local,
                CurrentUserId,
                businessType: "Project",
                businessId: projectId.ToString()),
            autoSave: true);
        return fileId;
    }

    private async Task EnsureCurrentUserAsync()
    {
        if (await _userRepository.AnyAsync(item => item.Id == CurrentUserId))
        {
            return;
        }

        var user = new IdentityUser(CurrentUserId, "project.communication.tester", "project.communication.tester@general.local");
        await _userRepository.InsertAsync(user, autoSave: true);
    }

    private async Task<Guid> CreateUserAsync(string userName, string displayName)
    {
        var userId = Guid.NewGuid();
        var user = new IdentityUser(userId, userName, $"{userName}@general.local")
        {
            Name = displayName
        };
        await _userRepository.InsertAsync(user, autoSave: true);
        return userId;
    }

    private static string NewCode(string prefix)
    {
        return $"{prefix}-{Guid.NewGuid():N}"[..32];
    }
}
