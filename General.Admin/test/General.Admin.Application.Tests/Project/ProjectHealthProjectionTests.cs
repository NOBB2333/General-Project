using System;
using System.Linq;
using Shouldly;
using Xunit;

namespace General.Admin.Project;

public class ProjectHealthProjectionTests
{
    [Fact]
    public void MapWbsNode_Should_Summarize_Task_Count_Completion_And_Work_Hours()
    {
        var projectId = Guid.NewGuid();
        var organizationUnitId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var nodeId = Guid.NewGuid();
        var node = new ProjectWbsNode(
            nodeId,
            projectId,
            wbsCode: "1.1",
            name: "评分模块",
            type: "工作包",
            status: ProjectStatuses.InProgress,
            ownerUserId: userId);
        var completedTask = new ProjectTask(
            Guid.NewGuid(),
            projectId,
            organizationUnitId,
            taskCode: "TASK-001",
            title: "完成评分规则",
            ownerUserId: userId,
            status: ProjectStatuses.Completed,
            priority: ProjectPriorities.P0,
            plannedStartTime: null,
            plannedEndTime: null,
            estimatedWorkHours: 8,
            actualWorkHours: 5,
            wbsNodeId: nodeId);
        var inProgressTask = new ProjectTask(
            Guid.NewGuid(),
            projectId,
            organizationUnitId,
            taskCode: "TASK-002",
            title: "完善评分页面",
            ownerUserId: userId,
            status: ProjectStatuses.InProgress,
            priority: ProjectPriorities.P1,
            plannedStartTime: null,
            plannedEndTime: null,
            estimatedWorkHours: 4,
            actualWorkHours: null,
            wbsCode: "1.1");
        var context = new ProjectContext(
            OrganizationUnitNameMap: [],
            Files: [],
            Projects: [],
            ProjectNameMap: [],
            Milestones: [],
            Sprints: [],
            AcceptanceCriteria: [],
            AcceptanceRecords: [],
            AcceptanceRecordCriteria: [],
            AcceptanceRecordRequirements: [],
            Baselines: [],
            ChangeApprovals: [],
            ChangeImpacts: [],
            ChangeRequests: [],
            Charters: [],
            ApprovalRecords: [],
            WorkflowInstances: [],
            PerformanceSnapshots: [],
            Requirements: [],
            Reports: [],
            MeetingRecords: [],
            MeetingActionItems: [],
            QualityChecklists: [],
            QualityInspections: [],
            TaskDependencies: [],
            TaskParticipants: [],
            MemberRoles: [],
            RaidItems: [],
            Tasks: [completedTask, inProgressTask],
            WbsNodes: [node],
            Issues: [],
            Documents: [],
            Members: [],
            Worklogs: [],
            UserNameMap: [],
            MyMemberMap: [],
            CurrentUserId: null,
            CurrentUserIsAdmin: false,
            CurrentUserIsPmo: false);

        var dto = ProjectProjectionMapper.MapWbsNode(node, context);

        dto.TaskCount.ShouldBe(2);
        dto.CompletionRate.ShouldBe(50);
        dto.EstimatedWorkHours.ShouldBe(12);
        dto.ActualWorkHours.ShouldBe(5);
    }

    [Fact]
    public void MapSprint_Should_Count_Open_Risks_And_Issues_In_Sprint_Date_Range()
    {
        var projectId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var sprint = new ProjectSprint(
            Guid.NewGuid(),
            projectId,
            "SPR-001",
            "第 1 Sprint",
            userId,
            DateTime.MinValue,
            DateTime.Today.AddDays(14),
            ProjectStatuses.InProgress,
            20);
        var inRangeRisk = new ProjectRaidItem(
            Guid.NewGuid(),
            projectId,
            ProjectRaidTypes.Risk,
            title: "Sprint 内高风险",
            level: ProjectPriorities.P0,
            ownerUserId: userId,
            status: ProjectStatuses.InProgress,
            plannedResolveDate: DateTime.Today.AddDays(3),
            probability: 5,
            impact: 4);
        var outRangeRisk = new ProjectRaidItem(
            Guid.NewGuid(),
            projectId,
            ProjectRaidTypes.Risk,
            title: "Sprint 外风险",
            level: ProjectPriorities.P0,
            ownerUserId: userId,
            status: ProjectStatuses.InProgress,
            plannedResolveDate: DateTime.Today.AddDays(30));
        var inRangeIssue = new ProjectIssue(
            Guid.NewGuid(),
            projectId,
            type: "问题",
            title: "Sprint 内问题",
            level: ProjectPriorities.P1,
            ownerUserId: userId,
            status: ProjectStatuses.InProgress,
            dueDate: DateTime.Today.AddDays(4));
        var closedIssue = new ProjectIssue(
            Guid.NewGuid(),
            projectId,
            type: "问题",
            title: "已关闭问题",
            level: ProjectPriorities.P1,
            ownerUserId: userId,
            status: ProjectStatuses.Closed,
            dueDate: DateTime.Today.AddDays(5));
        var inRangeReport = new ProjectDocument(
            Guid.NewGuid(),
            projectId,
            category: "周报",
            title: "Sprint 内周报",
            version: "v1.0",
            ownerUserId: userId,
            status: "草稿");
        var otherDocument = new ProjectDocument(
            Guid.NewGuid(),
            projectId,
            category: "其他资料",
            title: "非报告资料",
            version: "v1.0",
            ownerUserId: userId,
            status: "已发布");
        var context = new ProjectContext(
            OrganizationUnitNameMap: [],
            Files: [],
            Projects: [],
            ProjectNameMap: [],
            Milestones: [],
            Sprints: [sprint],
            AcceptanceCriteria: [],
            AcceptanceRecords: [],
            AcceptanceRecordCriteria: [],
            AcceptanceRecordRequirements: [],
            Baselines: [],
            ChangeApprovals: [],
            ChangeImpacts: [],
            ChangeRequests: [],
            Charters: [],
            ApprovalRecords: [],
            WorkflowInstances: [],
            PerformanceSnapshots: [],
            Requirements: [],
            Reports: [],
            MeetingRecords: [],
            MeetingActionItems: [],
            QualityChecklists: [],
            QualityInspections: [],
            TaskDependencies: [],
            TaskParticipants: [],
            MemberRoles: [],
            RaidItems: [inRangeRisk, outRangeRisk],
            Tasks: [],
            WbsNodes: [],
            Issues: [inRangeIssue, closedIssue],
            Documents: [inRangeReport, otherDocument],
            Members: [],
            Worklogs: [],
            UserNameMap: [],
            MyMemberMap: [],
            CurrentUserId: null,
            CurrentUserIsAdmin: false,
            CurrentUserIsPmo: false);

        var dto = ProjectProjectionMapper.MapSprint(sprint, context);

        dto.RiskCount.ShouldBe(1);
        dto.HighRiskCount.ShouldBe(1);
        dto.OpenIssueCount.ShouldBe(1);
        dto.ReportCount.ShouldBe(1);
        dto.LatestReportStatus.ShouldBe("有草稿");
    }

    [Fact]
    public void BuildHealthBreakdowns_Should_Return_Healthy_When_No_Deductions()
    {
        var breakdowns = ProjectProjectionMapper.BuildHealthBreakdowns(
            [],
            [],
            [],
            taskCompletionRate: 100);

        breakdowns.Sum(item => item.Score).ShouldBe(0);
        ProjectProjectionMapper.BuildHealthReasons(breakdowns)
            .ShouldContain("当前没有触发风险、逾期任务、未闭环问题或进度偏差扣分。");
        ProjectProjectionMapper.ResolveHealthStatus(breakdowns.Sum(item => item.Score)).ShouldBe("健康");
    }

    [Fact]
    public void BuildHealthBreakdowns_Should_Return_Rule_Metadata_And_Action_Keys()
    {
        var projectId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var overdueTask = new ProjectTask(
            Guid.NewGuid(),
            projectId,
            Guid.NewGuid(),
            taskCode: "TASK-001",
            title: "逾期任务",
            ownerUserId: userId,
            status: ProjectStatuses.InProgress,
            priority: ProjectPriorities.P0,
            plannedStartTime: DateTime.Now.AddDays(-10),
            plannedEndTime: DateTime.Now.AddDays(-2));
        var issue = new ProjectIssue(
            Guid.NewGuid(),
            projectId,
            type: "问题",
            title: "未闭环问题",
            level: ProjectPriorities.P0,
            ownerUserId: userId,
            status: ProjectStatuses.InProgress);
        var risk = new ProjectRaidItem(
            Guid.NewGuid(),
            projectId,
            ProjectRaidTypes.Risk,
            title: "开放风险",
            level: ProjectPriorities.P0,
            ownerUserId: userId,
            status: ProjectStatuses.InProgress,
            probability: 5,
            impact: 5);

        var breakdowns = ProjectProjectionMapper.BuildHealthBreakdowns(
            [overdueTask],
            [issue],
            [risk],
            taskCompletionRate: 60);

        breakdowns.ShouldAllBe(item => item.RuleVersion == ProjectProjectionMapper.HealthRuleVersion);
        breakdowns.ShouldAllBe(item => item.DataSource == ProjectProjectionMapper.HealthDataSource);
        breakdowns.Single(item => item.Code == "Risk").ActionKey.ShouldBe("issues");
        breakdowns.Single(item => item.Code == "OverdueTask").ActionKey.ShouldBe("tasks");
        breakdowns.Single(item => item.Code == "OpenIssue").ActionKey.ShouldBe("issues");
        breakdowns.Single(item => item.Code == "Schedule").ActionKey.ShouldBe("control");
        breakdowns.ShouldAllBe(item => !string.IsNullOrWhiteSpace(item.ActionLabel));
    }

    [Fact]
    public void BuildHealthBreakdowns_Should_Return_Watch_Status_For_Medium_Deductions()
    {
        var projectId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var issue = new ProjectIssue(
            Guid.NewGuid(),
            projectId,
            type: "缺陷",
            title: "接口字段不一致",
            level: ProjectPriorities.P1,
            ownerUserId: userId,
            status: ProjectStatuses.InProgress);
        var risk = new ProjectRaidItem(
            Guid.NewGuid(),
            projectId,
            ProjectRaidTypes.Risk,
            title: "依赖延期",
            level: ProjectPriorities.P1,
            ownerUserId: userId,
            status: ProjectStatuses.InProgress,
            probability: 3,
            impact: 3);

        var breakdowns = ProjectProjectionMapper.BuildHealthBreakdowns(
            [],
            [issue],
            [risk],
            taskCompletionRate: 60);
        var score = breakdowns.Sum(item => item.Score);

        score.ShouldBe(59);
        ProjectProjectionMapper.ResolveHealthStatus(score).ShouldBe("关注中");
        ProjectProjectionMapper.BuildHealthReasons(breakdowns)
            .ShouldContain(item => item.Contains("完成率偏差"));
    }

    [Fact]
    public void BuildHealthBreakdowns_Should_Return_High_Risk_And_Normalize_To_100()
    {
        var projectId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var overdueTask = new ProjectTask(
            Guid.NewGuid(),
            projectId,
            Guid.NewGuid(),
            taskCode: "TASK-001",
            title: "逾期任务",
            ownerUserId: userId,
            status: ProjectStatuses.InProgress,
            priority: ProjectPriorities.P0,
            plannedStartTime: DateTime.Now.AddDays(-10),
            plannedEndTime: DateTime.Now.AddDays(-2));
        var issues = Enumerable.Range(1, 4)
            .Select(index => new ProjectIssue(
                Guid.NewGuid(),
                projectId,
                type: "问题",
                title: $"未闭环问题 {index}",
                level: ProjectPriorities.P0,
                ownerUserId: userId,
                status: ProjectStatuses.InProgress))
            .ToList();
        var risks = Enumerable.Range(1, 4)
            .Select(index => new ProjectRaidItem(
                Guid.NewGuid(),
                projectId,
                ProjectRaidTypes.Risk,
                title: $"高风险 {index}",
                level: ProjectPriorities.P0,
                ownerUserId: userId,
                status: ProjectStatuses.InProgress,
                probability: 5,
                impact: 5))
            .ToList();

        var breakdowns = ProjectProjectionMapper.BuildHealthBreakdowns(
            [overdueTask],
            issues,
            risks,
            taskCompletionRate: 0);
        var score = breakdowns.Sum(item => item.Score);

        score.ShouldBe(100);
        ProjectProjectionMapper.ResolveHealthStatus(score).ShouldBe("高风险");
        breakdowns.ShouldContain(item => item.Code == "Risk" && item.Score > 0);
        breakdowns.ShouldContain(item => item.Code == "OpenIssue" && item.Score > 0);
        breakdowns.ShouldContain(item => item.Code == "OverdueTask" && item.Score > 0);
    }
}
