namespace General.Admin.Project;

internal static class ProjectProjectionMapper
{
    internal const string HealthRuleVersion = "RuleBasedV1";
    internal const string HealthDataSource = "项目任务、RAID 风险、缺陷问题、任务完成率";

    internal static List<ProjectCalendarItemDto> BuildCalendarItems(
        IReadOnlyCollection<ProjectTask> tasks,
        IReadOnlyCollection<ProjectSprint> sprints,
        ProjectContext context)
    {
        var items = tasks
            .Where(item => item.PlannedStartTime.HasValue || item.PlannedEndTime.HasValue)
            .Select(item => new ProjectCalendarItemDto
            {
                ColorToken = ResolveCalendarColor(item.Priority, item.Status),
                EndDate = item.PlannedEndTime,
                Id = item.Id,
                OwnerName = context.UserNameMap.GetValueOrDefault(item.OwnerUserId) ?? "-",
                StartDate = item.PlannedStartTime ?? item.PlannedEndTime,
                Status = item.Status,
                Title = item.Title,
                Type = "任务"
            })
            .Concat(sprints.Select(item => new ProjectCalendarItemDto
            {
                ColorToken = "var(--ant-color-primary)",
                EndDate = item.EndDate,
                Id = item.Id,
                OwnerName = context.UserNameMap.GetValueOrDefault(item.OwnerUserId) ?? "-",
                StartDate = item.StartDate,
                Status = item.Status,
                Title = item.Name,
                Type = "Sprint"
            }))
            .OrderBy(item => item.StartDate ?? DateTime.MaxValue)
            .ToList();

        return items;
    }

    internal static List<ProjectTaskScheduleImpactDto> BuildTaskScheduleImpacts(
        IReadOnlyCollection<ProjectTask> tasks,
        IReadOnlyCollection<ProjectTaskDependency> dependencies)
    {
        var taskMap = tasks.ToDictionary(item => item.Id);
        return dependencies
            .Where(item => item.DependencyType == "FinishToStart")
            .Select(item =>
            {
                taskMap.TryGetValue(item.PredecessorTaskId, out var predecessor);
                taskMap.TryGetValue(item.SuccessorTaskId, out var successor);
                return new
                {
                    Predecessor = predecessor,
                    Successor = successor
                };
            })
            .Where(item => item.Predecessor != null && item.Successor != null)
            .Where(item =>
                IsOverdue(item.Predecessor!.PlannedEndTime, item.Predecessor.Status)
                || (item.Predecessor.PlannedEndTime.HasValue
                    && item.Successor!.PlannedStartTime.HasValue
                    && item.Successor.PlannedStartTime.Value.Date < item.Predecessor.PlannedEndTime.Value.Date))
            .Select(item => new ProjectTaskScheduleImpactDto
            {
                ImpactReason = IsOverdue(item.Predecessor!.PlannedEndTime, item.Predecessor.Status)
                    ? "前置任务已延期，可能影响后续任务启动。"
                    : "后续任务计划开始早于前置任务计划完成，存在 Sprint 冲突。",
                PredecessorPlannedEndTime = item.Predecessor.PlannedEndTime,
                PredecessorTaskId = item.Predecessor.Id,
                PredecessorTaskTitle = item.Predecessor.Title,
                SuccessorPlannedStartTime = item.Successor!.PlannedStartTime,
                SuccessorTaskId = item.Successor.Id,
                SuccessorTaskTitle = item.Successor.Title
            })
            .OrderBy(item => item.SuccessorPlannedStartTime ?? DateTime.MaxValue)
            .ThenBy(item => item.SuccessorTaskTitle)
            .ToList();
    }

    internal static ProjectSprintItemDto MapSprint(ProjectSprint item, ProjectContext context)
    {
        var milestones = context.Milestones
            .Where(milestone => milestone.SprintId == item.Id)
            .ToList();
        var tasks = context.Tasks
            .Where(task => task.SprintId == item.Id)
            .ToList();
        var sprintRaidItems = context.RaidItems
            .Where(raid => raid.ProjectId == item.ProjectId
                && raid.PlannedResolveDate.HasValue
                && IsInSprintRange(raid.PlannedResolveDate.Value, item.StartDate, item.EndDate)
                && !IsClosed(raid.Status))
            .ToList();
        var sprintIssues = context.Issues
            .Where(issue => issue.ProjectId == item.ProjectId
                && issue.DueDate.HasValue
                && IsInSprintRange(issue.DueDate.Value, item.StartDate, item.EndDate)
                && !IsClosed(issue.Status))
            .ToList();
        var sprintReports = context.Reports
            .Where(report => report.ProjectId == item.ProjectId
                && IsInSprintRange(report.PeriodEnd, item.StartDate, item.EndDate))
            .OrderByDescending(report => report.PeriodEnd)
            .ThenByDescending(report => report.CreationTime)
            .ToList();
        var legacySprintReports = context.Documents
            .Where(document => document.ProjectId == item.ProjectId
                && IsCommunicationDocument(document.Category)
                && IsInSprintRange(document.CreationTime, item.StartDate, item.EndDate))
            .OrderByDescending(document => document.CreationTime)
            .ToList();
        return new ProjectSprintItemDto
        {
            ClosedMilestoneCount = milestones.Count(milestone => IsClosed(milestone.Status)),
            ClosedTaskCount = tasks.Count(task => IsClosed(task.Status)),
            EndDate = item.EndDate,
            HighRiskCount = sprintRaidItems.Count(raid =>
                raid.Type == ProjectRaidTypes.Risk
                && (raid.Level == ProjectPriorities.P0 || raid.RiskScore >= 16)),
            Id = item.Id,
            LatestReportStatus = ResolveLatestReportStatus(sprintReports, legacySprintReports),
            MilestoneCount = milestones.Count,
            Name = item.Name,
            OwnerName = context.UserNameMap.GetValueOrDefault(item.OwnerUserId) ?? "-",
            OwnerUserId = item.OwnerUserId,
            OpenIssueCount = sprintIssues.Count,
            OverdueMilestoneCount = milestones.Count(milestone => milestone.PlannedCompletionDate.Date < DateTime.Today && !IsClosed(milestone.Status)),
            OverdueTaskCount = tasks.Count(task => IsOverdue(task.PlannedEndTime, task.Status)),
            Progress = item.Progress,
            ReportCount = sprintReports.Count + legacySprintReports.Count,
            RiskCount = sprintRaidItems.Count(raid => raid.Type == ProjectRaidTypes.Risk),
            SprintCode = item.SprintCode,
            StartDate = item.StartDate,
            Status = item.Status,
            Summary = item.Summary,
            TaskCount = tasks.Count
        };
    }

    internal static ProjectAcceptanceCriteriaDto MapAcceptanceCriteria(ProjectAcceptanceCriteria item, ProjectContext context)
    {
        return new ProjectAcceptanceCriteriaDto
        {
            Description = item.Description,
            Id = item.Id,
            MilestoneId = item.MilestoneId,
            MilestoneName = item.MilestoneId.HasValue
                ? context.MilestoneNameMap.GetValueOrDefault(item.MilestoneId.Value) ?? "-"
                : "-",
            ProjectId = item.ProjectId,
            Status = item.Status,
            TaskId = item.TaskId,
            TaskTitle = item.TaskId.HasValue
                ? context.TaskTitleMap.GetValueOrDefault(item.TaskId.Value) ?? "-"
                : "-",
            Title = item.Title
        };
    }

    internal static ProjectAcceptanceRecordDto MapAcceptanceRecord(ProjectAcceptanceRecord item, ProjectContext context)
    {
        var criteriaLinks = context.AcceptanceRecordCriteriaMap.GetValueOrDefault(item.Id) ?? [];
        var requirementLinks = context.AcceptanceRecordRequirementMap.GetValueOrDefault(item.Id) ?? [];
        var acceptanceFile = item.AcceptanceFileId.HasValue
            ? context.FileMap.GetValueOrDefault(item.AcceptanceFileId.Value)
            : null;

        return new ProjectAcceptanceRecordDto
        {
            AcceptanceFileId = item.AcceptanceFileId,
            AcceptanceFileName = acceptanceFile?.FileName ?? "-",
            AcceptanceResult = item.AcceptanceResult,
            AcceptanceTime = item.AcceptanceTime,
            AcceptanceCriteriaIds = criteriaLinks.Select(link => link.AcceptanceCriteriaId).ToList(),
            AcceptanceCriteriaTitles = criteriaLinks
                .Select(link => context.AcceptanceCriteriaTitleMap.GetValueOrDefault(link.AcceptanceCriteriaId) ?? "-")
                .ToList(),
            AcceptorName = context.UserNameMap.GetValueOrDefault(item.AcceptorUserId) ?? "-",
            AcceptorUserId = item.AcceptorUserId,
            Id = item.Id,
            MilestoneId = item.MilestoneId,
            MilestoneName = item.MilestoneId.HasValue
                ? context.MilestoneNameMap.GetValueOrDefault(item.MilestoneId.Value) ?? "-"
                : "-",
            ProjectId = item.ProjectId,
            RectificationRequirement = item.RectificationRequirement,
            SprintId = item.SprintId,
            SprintName = item.SprintId.HasValue
                ? context.SprintNameMap.GetValueOrDefault(item.SprintId.Value) ?? "-"
                : "-",
            RequirementIds = requirementLinks.Select(link => link.RequirementId).ToList(),
            RequirementTitles = requirementLinks
                .Select(link => context.RequirementTitleMap.GetValueOrDefault(link.RequirementId) ?? "-")
                .ToList(),
            Remark = item.Remark,
            Title = item.Title
        };
    }

    internal static ProjectCharterDto MapCharter(ProjectCharter item, ProjectContext context)
    {
        var approvalWorkflow = (context.CharterWorkflowInstancesByProject.GetValueOrDefault(item.ProjectId) ?? [])
            .FirstOrDefault();

        return new ProjectCharterDto
        {
            ApprovalStatus = item.ApprovalStatus,
            ApprovalWorkflow = approvalWorkflow is null ? null : MapWorkflowInstance(approvalWorkflow, context),
            Assumptions = item.Assumptions,
            BusinessCase = item.BusinessCase,
            Constraints = item.Constraints,
            Id = item.Id,
            Objectives = item.Objectives,
            ProjectId = item.ProjectId,
            ScopeBoundary = item.ScopeBoundary,
            SuccessCriteria = item.SuccessCriteria
        };
    }

    private static PlatformWorkflowInstanceDto MapWorkflowInstance(PlatformWorkflowInstance item, ProjectContext context)
    {
        return new PlatformWorkflowInstanceDto
        {
            ApplicantUserId = item.ApplicantUserId,
            ApplicantUserName = item.ApplicantUserId.HasValue
                ? context.UserNameMap.GetValueOrDefault(item.ApplicantUserId.Value) ?? "-"
                : "-",
            ApprovalRecords = (context.ApprovalRecordsByInstance.GetValueOrDefault(item.Id) ?? [])
                .OrderBy(record => record.CreationTime)
                .Select(record => MapApprovalRecord(record, context))
                .ToList(),
            BusinessCode = item.BusinessCode,
            BusinessId = item.BusinessId,
            BusinessTitle = item.BusinessTitle,
            BusinessType = item.BusinessType,
            CompletedTime = item.CompletedTime,
            CreationTime = item.CreationTime,
            CurrentNodeName = item.CurrentNodeName,
            ElsaWorkflowInstanceId = item.ElsaWorkflowInstanceId,
            Id = item.Id,
            Status = item.Status,
            WorkflowDefinitionKey = item.WorkflowDefinitionKey,
            WorkflowDefinitionVersion = item.WorkflowDefinitionVersion
        };
    }

    private static PlatformApprovalRecordDto MapApprovalRecord(PlatformApprovalRecord item, ProjectContext context)
    {
        return new PlatformApprovalRecordDto
        {
            Action = item.Action,
            ActorUserId = item.ActorUserId,
            ActorUserName = item.ActorUserId.HasValue
                ? context.UserNameMap.GetValueOrDefault(item.ActorUserId.Value) ?? "-"
                : "-",
            ApprovalInstanceId = item.ApprovalInstanceId,
            Comment = item.Comment,
            CreationTime = item.CreationTime,
            FromStatus = item.FromStatus,
            Id = item.Id,
            NodeName = item.NodeName,
            ToStatus = item.ToStatus
        };
    }

    internal static ProjectBaselineDto MapBaseline(ProjectBaseline item, ProjectContext context)
    {
        return new ProjectBaselineDto
        {
            BudgetAtCompletion = item.BudgetAtCompletion,
            CostBaselineSummary = item.CostBaselineSummary,
            CreatedFromChangeRequestCode = item.CreatedFromChangeRequestId.HasValue
                ? context.ChangeRequestCodeMap.GetValueOrDefault(item.CreatedFromChangeRequestId.Value) ?? "-"
                : "-",
            CreatedFromChangeRequestId = item.CreatedFromChangeRequestId,
            EffectiveDate = item.EffectiveDate,
            Id = item.Id,
            IsActive = item.IsActive,
            ProjectId = item.ProjectId,
            ScheduleBaselineSummary = item.ScheduleBaselineSummary,
            ScopeBaselineSummary = item.ScopeBaselineSummary,
            VersionNo = item.VersionNo
        };
    }

    internal static ProjectChangeApprovalDto MapChangeApproval(ProjectChangeApproval item, ProjectContext context)
    {
        return new ProjectChangeApprovalDto
        {
            Action = item.Action,
            ApproverName = context.UserNameMap.GetValueOrDefault(item.ApproverUserId) ?? "-",
            ApproverUserId = item.ApproverUserId,
            Comment = item.Comment,
            DecisionTime = item.DecisionTime,
            Id = item.Id
        };
    }

    internal static ProjectChangeImpactDto MapChangeImpact(ProjectChangeImpact item, ProjectContext context)
    {
        return new ProjectChangeImpactDto
        {
            Analysis = item.Analysis,
            EstimatedCostDelta = item.EstimatedCostDelta,
            EstimatedScheduleDeltaDays = item.EstimatedScheduleDeltaDays,
            Id = item.Id,
            ImpactType = item.ImpactType,
            RelatedBusinessCode = item.RelatedBusinessCode,
            RelatedBudgetCode = item.RelatedBudgetCode,
            RelatedContractCode = item.RelatedContractCode,
            RelatedMilestoneId = item.RelatedMilestoneId,
            RelatedMilestoneName = item.RelatedMilestoneId.HasValue
                ? context.MilestoneNameMap.GetValueOrDefault(item.RelatedMilestoneId.Value) ?? "-"
                : "-",
            RelatedTaskId = item.RelatedTaskId,
            RelatedTaskTitle = item.RelatedTaskId.HasValue
                ? context.TaskTitleMap.GetValueOrDefault(item.RelatedTaskId.Value) ?? "-"
                : "-",
            Severity = item.Severity
        };
    }

    internal static ProjectChangeRequestDto MapChangeRequest(ProjectChangeRequest item, ProjectContext context)
    {
        var approvalWorkflow = (context.ChangeWorkflowInstancesByChangeRequest.GetValueOrDefault(item.Id) ?? [])
            .FirstOrDefault();

        return new ProjectChangeRequestDto
        {
            ApplicantName = context.UserNameMap.GetValueOrDefault(item.ApplicantUserId) ?? "-",
            ApplicantUserId = item.ApplicantUserId,
            ApprovalWorkflow = approvalWorkflow is null ? null : MapWorkflowInstance(approvalWorkflow, context),
            Approvals = context.ChangeApprovals
                .Where(approval => approval.ProjectChangeRequestId == item.Id)
                .OrderByDescending(approval => approval.DecisionTime)
                .Select(approval => MapChangeApproval(approval, context))
                .ToList(),
            ChangeType = item.ChangeType,
            ExpectedResult = item.ExpectedResult,
            Id = item.Id,
            ImpactScope = item.ImpactScope,
            Impacts = context.ChangeImpacts
                .Where(impact => impact.ProjectChangeRequestId == item.Id)
                .OrderBy(impact => impact.ImpactType)
                .Select(impact => MapChangeImpact(impact, context))
                .ToList(),
            ProjectId = item.ProjectId,
            Reason = item.Reason,
            RequestCode = item.RequestCode,
            Status = item.Status,
            Title = item.Title
        };
    }

    internal static ProjectDocumentItemDto MapDocument(ProjectDocument item, ProjectContext context)
    {
        return new ProjectDocumentItemDto
        {
            Category = item.Category,
            Id = item.Id,
            OwnerName = context.UserNameMap.GetValueOrDefault(item.OwnerUserId) ?? "-",
            Status = item.Status,
            Summary = item.Summary,
            Title = item.Title,
            Version = item.Version
        };
    }

    internal static ProjectReportDto MapReport(ProjectReport item, ProjectContext context)
    {
        var attachmentFile = item.AttachmentFileId.HasValue
            ? context.FileMap.GetValueOrDefault(item.AttachmentFileId.Value)
            : null;

        return new ProjectReportDto
        {
            AttachmentFileId = item.AttachmentFileId,
            AttachmentFileName = attachmentFile?.FileName ?? string.Empty,
            AuthorName = context.UserNameMap.GetValueOrDefault(item.AuthorUserId) ?? "-",
            AuthorUserId = item.AuthorUserId,
            BusinessSummary = item.BusinessSummary,
            CompletedSummary = item.CompletedSummary,
            SprintId = item.SprintId,
            SprintName = item.SprintId.HasValue
                ? context.SprintNameMap.GetValueOrDefault(item.SprintId.Value) ?? "-"
                : "-",
            HoursSummary = item.HoursSummary,
            Id = item.Id,
            MilestoneSummary = item.MilestoneSummary,
            NextPlan = item.NextPlan,
            PeriodEnd = item.PeriodEnd,
            PeriodStart = item.PeriodStart,
            ProjectId = item.ProjectId,
            ReportType = item.ReportType,
            RiskIssueSummary = item.RiskIssueSummary,
            Status = item.Status,
            Title = item.Title
        };
    }

    internal static ProjectMeetingRecordDto MapMeetingRecord(ProjectMeetingRecord item, ProjectContext context)
    {
        var attachmentFile = item.AttachmentFileId.HasValue
            ? context.FileMap.GetValueOrDefault(item.AttachmentFileId.Value)
            : null;
        var participantUserIds = item.GetParticipantUserIds();

        return new ProjectMeetingRecordDto
        {
            ActionItems = (context.MeetingActionItemsByRecord.GetValueOrDefault(item.Id) ?? [])
                .OrderBy(action => action.DueDate ?? DateTime.MaxValue)
                .ThenBy(action => action.Title)
                .Select(action => MapMeetingActionItem(action, context))
                .ToList(),
            AttachmentFileId = item.AttachmentFileId,
            AttachmentFileName = attachmentFile?.FileName ?? string.Empty,
            Decisions = item.Decisions,
            FacilitatorName = context.UserNameMap.GetValueOrDefault(item.FacilitatorUserId) ?? "-",
            FacilitatorUserId = item.FacilitatorUserId,
            Id = item.Id,
            MeetingTime = item.MeetingTime,
            Minutes = item.Minutes,
            ParticipantUserIds = participantUserIds,
            ParticipantNames = participantUserIds
                .Select(userId => context.UserNameMap.GetValueOrDefault(userId) ?? "-")
                .ToList(),
            ProjectId = item.ProjectId,
            Status = item.Status,
            Topic = item.Topic
        };
    }

    private static ProjectMeetingActionItemDto MapMeetingActionItem(ProjectMeetingActionItem item, ProjectContext context)
    {
        return new ProjectMeetingActionItemDto
        {
            ActionType = item.ActionType,
            AssigneeName = item.AssigneeUserId.HasValue
                ? context.UserNameMap.GetValueOrDefault(item.AssigneeUserId.Value) ?? "-"
                : "-",
            AssigneeUserId = item.AssigneeUserId,
            DueDate = item.DueDate,
            Id = item.Id,
            LinkedIssueId = item.LinkedIssueId,
            LinkedIssueTitle = item.LinkedIssueId.HasValue
                ? context.Issues.FirstOrDefault(issue => issue.Id == item.LinkedIssueId.Value)?.Title ?? "-"
                : "-",
            LinkedRaidItemId = item.LinkedRaidItemId,
            LinkedRaidItemTitle = item.LinkedRaidItemId.HasValue
                ? context.RaidItems.FirstOrDefault(raid => raid.Id == item.LinkedRaidItemId.Value)?.Title ?? "-"
                : "-",
            LinkedRequirementId = item.LinkedRequirementId,
            LinkedRequirementTitle = item.LinkedRequirementId.HasValue
                ? context.RequirementTitleMap.GetValueOrDefault(item.LinkedRequirementId.Value) ?? "-"
                : "-",
            LinkedTaskId = item.LinkedTaskId,
            LinkedTaskTitle = item.LinkedTaskId.HasValue
                ? context.TaskTitleMap.GetValueOrDefault(item.LinkedTaskId.Value) ?? "-"
                : "-",
            Status = item.Status,
            Title = item.Title
        };
    }

    internal static ProjectIssueItemDto MapIssue(ProjectIssue item, ProjectContext context)
    {
        return new ProjectIssueItemDto
        {
            AcceptanceCriteriaId = item.AcceptanceCriteriaId,
            AcceptanceCriteriaTitle = item.AcceptanceCriteriaId.HasValue
                ? context.AcceptanceCriteriaTitleMap.GetValueOrDefault(item.AcceptanceCriteriaId.Value) ?? "-"
                : "-",
            CreationTime = item.CreationTime,
            DeveloperOwnerName = item.DeveloperOwnerUserId.HasValue
                ? item.DeveloperOwnerName ?? context.UserNameMap.GetValueOrDefault(item.DeveloperOwnerUserId.Value) ?? "-"
                : item.DeveloperOwnerName,
            DeveloperOwnerUserId = item.DeveloperOwnerUserId,
            DueDate = item.DueDate,
            Id = item.Id,
            IsOverdue = item.DueDate.HasValue && item.DueDate.Value.Date < DateTime.Today && !IsClosed(item.Status),
            Level = item.Level,
            OwnerName = context.UserNameMap.GetValueOrDefault(item.OwnerUserId) ?? "-",
            OwnerUserId = item.OwnerUserId,
            ProductOwnerName = item.ProductOwnerUserId.HasValue
                ? item.ProductOwnerName ?? context.UserNameMap.GetValueOrDefault(item.ProductOwnerUserId.Value) ?? "-"
                : item.ProductOwnerName,
            ProductOwnerUserId = item.ProductOwnerUserId,
            QualityChecklistId = item.QualityChecklistId,
            QualityChecklistTitle = item.QualityChecklistId.HasValue
                ? context.QualityChecklistTitleMap.GetValueOrDefault(item.QualityChecklistId.Value) ?? "-"
                : "-",
            ProjectId = item.ProjectId,
            ProjectName = context.ProjectNameMap.GetValueOrDefault(item.ProjectId) ?? "-",
            RequirementTitle = item.RequirementTitle,
            Status = item.Status,
            TesterOwnerName = item.TesterOwnerUserId.HasValue
                ? item.TesterOwnerName ?? context.UserNameMap.GetValueOrDefault(item.TesterOwnerUserId.Value) ?? "-"
                : item.TesterOwnerName,
            TesterOwnerUserId = item.TesterOwnerUserId,
            Title = item.Title,
            Type = item.Type
        };
    }

    internal static ProjectMemberItemDto MapMember(ProjectMember item, ProjectContext context)
    {
        var roleNames = ResolveMemberRoles(item, context);
        return new ProjectMemberItemDto
        {
            AccessLevel = item.IsActive() ? "编辑" : item.AllowHistoricalRead ? "历史只读" : "已移除",
            Id = item.Id,
            IsActive = item.IsActive(),
            JoinDate = item.JoinDate,
            LeaveDate = item.LeaveDate,
            OrganizationUnitName = context.OrganizationUnitNameMap.GetValueOrDefault(item.OrganizationUnitId) ?? "-",
            RoleNames = roleNames,
            UserId = item.UserId,
            UserName = context.UserNameMap.GetValueOrDefault(item.UserId) ?? "-"
        };
    }

    internal static ProjectQualityChecklistDto MapQualityChecklist(ProjectQualityChecklist item, ProjectContext context)
    {
        return new ProjectQualityChecklistDto
        {
            Category = item.Category,
            Description = item.Description,
            Id = item.Id,
            InspectionCount = context.QualityInspections.Count(inspection => inspection.QualityChecklistId == item.Id),
            ProjectId = item.ProjectId,
            Title = item.Title
        };
    }

    internal static ProjectQualityInspectionDto MapQualityInspection(ProjectQualityInspection item, ProjectContext context)
    {
        return new ProjectQualityInspectionDto
        {
            FindingSummary = item.FindingSummary,
            Id = item.Id,
            InspectionResult = item.InspectionResult,
            InspectionTime = item.InspectionTime,
            InspectorName = context.UserNameMap.GetValueOrDefault(item.InspectorUserId) ?? "-",
            ProjectId = item.ProjectId,
            QualityChecklistId = item.QualityChecklistId,
            QualityChecklistTitle = context.QualityChecklistTitleMap.GetValueOrDefault(item.QualityChecklistId) ?? "-",
            RectificationRequirement = item.RectificationRequirement
        };
    }

    internal static ProjectListItemDto MapProject(Project project, ProjectContext context)
    {
        var projectTasks = context.TasksByProject.GetValueOrDefault(project.Id) ?? [];
        var projectMilestones = context.MilestonesByProject.GetValueOrDefault(project.Id) ?? [];
        var raidItems = context.RaidItemsByProject.GetValueOrDefault(project.Id) ?? [];
        var members = context.MembersByProject.GetValueOrDefault(project.Id) ?? [];

        return new ProjectListItemDto
        {
            BlockedTaskCount = projectTasks.Count(item => item.IsBlocked && !IsClosed(item.Status)),
            CompletedTaskCount = projectTasks.Count(item => item.Status == ProjectStatuses.Completed || item.Status == ProjectStatuses.Closed),
            HighRiskCount = raidItems.Count(item => item.Type == ProjectRaidTypes.Risk && item.Level == ProjectPriorities.P0 && !IsClosed(item.Status)),
            Id = project.Id,
            IsKeyProject = project.IsKeyProject,
            ManagerName = context.UserNameMap.GetValueOrDefault(project.ManagerUserId) ?? "-",
            MemberCount = members.Count(item => item.IsActive()),
            MilestoneTotalCount = projectMilestones.Count,
            MyRelation = ResolveMyRelation(project, context),
            Name = project.Name,
            OrganizationUnitName = context.OrganizationUnitNameMap.GetValueOrDefault(project.OrganizationUnitId) ?? "-",
            OverdueMilestoneCount = projectMilestones.Count(item => item.PlannedCompletionDate.Date < DateTime.Today && !IsClosed(item.Status)),
            OverdueTaskCount = projectTasks.Count(item => IsOverdue(item.PlannedEndTime, item.Status)),
            PlannedEndDate = project.PlannedEndDate,
            PlannedStartDate = project.PlannedStartDate,
            Priority = project.Priority,
            ProjectCode = project.ProjectCode,
            Status = project.Status,
            TaskTotalCount = projectTasks.Count
        };
    }

    internal static ProjectMilestoneItemDto MapMilestone(ProjectMilestone item, ProjectContext context)
    {
        return new ProjectMilestoneItemDto
        {
            ActualCompletionDate = item.ActualCompletionDate,
            SprintId = item.SprintId,
            SprintName = item.SprintId.HasValue
                ? context.SprintNameMap.GetValueOrDefault(item.SprintId.Value) ?? "-"
                : "-",
            Id = item.Id,
            Name = item.Name,
            OwnerName = context.UserNameMap.GetValueOrDefault(item.OwnerUserId) ?? "-",
            OwnerUserId = item.OwnerUserId,
            PlannedCompletionDate = item.PlannedCompletionDate,
            ProjectId = item.ProjectId,
            ProjectName = context.ProjectNameMap.GetValueOrDefault(item.ProjectId) ?? "-",
            Status = item.Status
        };
    }

    internal static ProjectTaskItemDto MapTask(ProjectTask item, ProjectContext context)
    {
        var productParticipants = ResolveTaskParticipants(item, context, "产品");
        var developerParticipants = ResolveTaskParticipants(item, context, "开发");
        var testerParticipants = ResolveTaskParticipants(item, context, "测试");
        return new ProjectTaskItemDto
        {
            ActualEndTime = item.ActualEndTime,
            ActualStartTime = item.ActualStartTime,
            ActualWorkHours = item.ActualWorkHours,
            BlockReason = item.BlockReason,
            ContractClause = item.ContractClause,
            SprintId = item.SprintId,
            SprintName = item.SprintId.HasValue
                ? context.SprintNameMap.GetValueOrDefault(item.SprintId.Value) ?? "-"
                : "-",
            DeveloperOwnerName = developerParticipants.Names.Count > 0
                ? string.Join("、", developerParticipants.Names)
                : item.DeveloperOwnerUserId.HasValue
                ? item.DeveloperOwnerName ?? context.UserNameMap.GetValueOrDefault(item.DeveloperOwnerUserId.Value) ?? "-"
                : item.DeveloperOwnerName,
            DeveloperOwnerNames = developerParticipants.Names,
            DeveloperOwnerUserId = item.DeveloperOwnerUserId,
            DeveloperOwnerUserIds = developerParticipants.UserIds,
            Description = item.Description,
            EstimatedWorkHours = item.EstimatedWorkHours,
            Id = item.Id,
            IsBlocked = item.IsBlocked,
            IsOutOfContract = item.IsOutOfContract,
            OrganizationUnitName = context.OrganizationUnitNameMap.GetValueOrDefault(item.OrganizationUnitId) ?? "-",
            OwnerName = context.UserNameMap.GetValueOrDefault(item.OwnerUserId) ?? "-",
            OwnerUserId = item.OwnerUserId,
            ParentTaskId = item.ParentTaskId,
            BaselineEndTime = item.BaselineEndTime,
            BaselineStartTime = item.BaselineStartTime,
            PlannedEndTime = item.PlannedEndTime,
            PlannedStartTime = item.PlannedStartTime,
            Priority = item.Priority,
            ProductOwnerName = productParticipants.Names.Count > 0
                ? string.Join("、", productParticipants.Names)
                : item.ProductOwnerUserId.HasValue
                ? item.ProductOwnerName ?? context.UserNameMap.GetValueOrDefault(item.ProductOwnerUserId.Value) ?? "-"
                : item.ProductOwnerName,
            ProductOwnerNames = productParticipants.Names,
            ProductOwnerUserId = item.ProductOwnerUserId,
            ProductOwnerUserIds = productParticipants.UserIds,
            ProjectId = item.ProjectId,
            ProjectName = context.ProjectNameMap.GetValueOrDefault(item.ProjectId) ?? "-",
            RequirementId = item.RequirementId,
            RequirementTitle = item.RequirementId.HasValue
                ? context.RequirementTitleMap.GetValueOrDefault(item.RequirementId.Value) ?? "-"
                : "-",
            Status = item.Status,
            TaskCode = item.TaskCode,
            TesterOwnerName = testerParticipants.Names.Count > 0
                ? string.Join("、", testerParticipants.Names)
                : item.TesterOwnerUserId.HasValue
                ? item.TesterOwnerName ?? context.UserNameMap.GetValueOrDefault(item.TesterOwnerUserId.Value) ?? "-"
                : item.TesterOwnerName,
            TesterOwnerNames = testerParticipants.Names,
            TesterOwnerUserId = item.TesterOwnerUserId,
            TesterOwnerUserIds = testerParticipants.UserIds,
            Title = item.Title,
            SortOrder = item.SortOrder,
            WbsCode = item.WbsCode,
            WbsNodeId = item.WbsNodeId,
            WbsNodeName = item.WbsNodeId.HasValue
                ? context.WbsNodeNameMap.GetValueOrDefault(item.WbsNodeId.Value) ?? "-"
                : "-"
        };
    }

    private static (List<Guid> UserIds, List<string> Names) ResolveTaskParticipants(
        ProjectTask item,
        ProjectContext context,
        string roleName)
    {
        var participants = context.TaskParticipantsByTask
            .GetValueOrDefault(item.Id)?
            .Where(participant => participant.RoleName == roleName)
            .OrderBy(participant => context.UserNameMap.GetValueOrDefault(participant.UserId) ?? string.Empty)
            .ToList() ?? [];
        var userIds = participants.Select(participant => participant.UserId).Distinct().ToList();
        var names = userIds
            .Select(userId => context.UserNameMap.GetValueOrDefault(userId) ?? "-")
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToList();

        return (userIds, names);
    }

    internal static ProjectRequirementDto MapRequirement(ProjectRequirement item, ProjectContext context)
    {
        return new ProjectRequirementDto
        {
            AcceptanceCriteria = item.AcceptanceCriteria,
            BusinessOwnerName = item.BusinessOwnerUserId.HasValue
                ? context.UserNameMap.GetValueOrDefault(item.BusinessOwnerUserId.Value) ?? "-"
                : "-",
            BusinessOwnerUserId = item.BusinessOwnerUserId,
            ChangeReason = item.ChangeReason,
            Description = item.Description,
            Id = item.Id,
            ImpactSummary = item.ImpactSummary,
            IsCurrent = item.IsCurrent,
            MilestoneId = item.MilestoneId,
            MilestoneName = item.MilestoneId.HasValue
                ? context.MilestoneNameMap.GetValueOrDefault(item.MilestoneId.Value) ?? "-"
                : "-",
            Priority = item.Priority,
            ProductOwnerName = item.ProductOwnerUserId.HasValue
                ? context.UserNameMap.GetValueOrDefault(item.ProductOwnerUserId.Value) ?? "-"
                : "-",
            ProductOwnerUserId = item.ProductOwnerUserId,
            ProjectId = item.ProjectId,
            ReplacesRequirementId = item.ReplacesRequirementId,
            ReplacesRequirementTitle = item.ReplacesRequirementId.HasValue
                ? context.RequirementTitleMap.GetValueOrDefault(item.ReplacesRequirementId.Value) ?? "-"
                : "-",
            RequirementCode = item.RequirementCode,
            SizeEstimate = item.SizeEstimate,
            Source = item.Source,
            Status = item.Status,
            Title = item.Title,
            Type = item.Type,
            VersionNo = item.VersionNo,
            WbsNodeId = item.WbsNodeId,
            WbsNodeName = item.WbsNodeId.HasValue
                ? context.WbsNodeNameMap.GetValueOrDefault(item.WbsNodeId.Value) ?? "-"
                : "-"
        };
    }

    internal static ProjectWbsNodeDto MapWbsNode(ProjectWbsNode item, ProjectContext context)
    {
        var tasks = context.Tasks
            .Where(task =>
                task.ProjectId == item.ProjectId &&
                (task.WbsNodeId == item.Id ||
                 (!task.WbsNodeId.HasValue && task.WbsCode == item.WbsCode)))
            .ToList();
        var requirements = context.Requirements
            .Where(requirement => requirement.ProjectId == item.ProjectId && requirement.WbsNodeId == item.Id)
            .ToList();
        var taskCount = tasks.Count;
        var closedTaskCount = tasks.Count(task => ProjectStatuses.IsClosed(task.Status));
        var estimatedWorkHours = tasks.Sum(task => task.EstimatedWorkHours ?? 0);
        var actualWorkHours = tasks.Sum(task => task.ActualWorkHours ?? 0);
        return new ProjectWbsNodeDto
        {
            ActualWorkHours = Math.Round(actualWorkHours, 1),
            CompletionRate = taskCount == 0
                ? 0
                : (int)Math.Round((double)closedTaskCount / taskCount * 100),
            SprintId = item.SprintId,
            SprintName = item.SprintId.HasValue
                ? context.SprintNameMap.GetValueOrDefault(item.SprintId.Value) ?? "-"
                : "-",
            DeliveryDescription = item.DeliveryDescription,
            EstimateSummary = item.EstimateSummary,
            EstimatedWorkHours = Math.Round(estimatedWorkHours, 1),
            Id = item.Id,
            MilestoneId = item.MilestoneId,
            MilestoneName = item.MilestoneId.HasValue
                ? context.MilestoneNameMap.GetValueOrDefault(item.MilestoneId.Value) ?? "-"
                : "-",
            Name = item.Name,
            OwnerName = item.OwnerUserId.HasValue
                ? context.UserNameMap.GetValueOrDefault(item.OwnerUserId.Value) ?? "-"
                : "-",
            OwnerUserId = item.OwnerUserId,
            ParentId = item.ParentId,
            ProjectId = item.ProjectId,
            RequirementCount = requirements.Count,
            ScopeDescription = item.ScopeDescription,
            SortOrder = item.SortOrder,
            Status = item.Status,
            TaskCount = taskCount,
            Type = item.Type,
            WbsCode = item.WbsCode
        };
    }

    internal static ProjectTaskDependencyDto MapTaskDependency(ProjectTaskDependency item, ProjectContext context)
    {
        return new ProjectTaskDependencyDto
        {
            DependencyType = item.DependencyType,
            Id = item.Id,
            PredecessorTaskId = item.PredecessorTaskId,
            PredecessorTaskTitle = context.TaskTitleMap.GetValueOrDefault(item.PredecessorTaskId) ?? "-",
            ProjectId = item.ProjectId,
            SuccessorTaskId = item.SuccessorTaskId,
            SuccessorTaskTitle = context.TaskTitleMap.GetValueOrDefault(item.SuccessorTaskId) ?? "-"
        };
    }

    internal static ProjectPerformanceSnapshotDto MapPerformanceSnapshot(ProjectPerformanceSnapshot item)
    {
        return new ProjectPerformanceSnapshotDto
        {
            ActualCost = item.ActualCost,
            CostPerformanceIndex = item.CostPerformanceIndex,
            EarnedValue = item.EarnedValue,
            Id = item.Id,
            PlannedValue = item.PlannedValue,
            ProjectBaselineId = item.ProjectBaselineId,
            ProjectId = item.ProjectId,
            SchedulePerformanceIndex = item.SchedulePerformanceIndex,
            SnapshotDate = item.SnapshotDate,
            SnapshotPeriod = item.SnapshotPeriod
        };
    }

    internal static ProjectRaidItemDto MapRaidItem(ProjectRaidItem item, ProjectContext context)
    {
        return new ProjectRaidItemDto
        {
            ContingencyPlan = item.ContingencyPlan,
            Id = item.Id,
            Impact = item.Impact,
            IsOverdue = item.PlannedResolveDate.HasValue && item.PlannedResolveDate.Value.Date < DateTime.Today && !IsClosed(item.Status),
            Level = item.Level,
            OwnerName = context.UserNameMap.GetValueOrDefault(item.OwnerUserId) ?? "-",
            PlannedResolveDate = item.PlannedResolveDate,
            Probability = item.Probability,
            ProjectId = item.ProjectId,
            ProjectName = context.ProjectNameMap.GetValueOrDefault(item.ProjectId) ?? "-",
            ResponseStrategy = item.ResponseStrategy,
            RiskScore = item.RiskScore,
            Status = item.Status,
            Title = item.Title,
            TriggerCondition = item.TriggerCondition,
            Type = item.Type
        };
    }

    internal static ProjectWorklogItemDto MapWorklog(ProjectWorklog item, ProjectContext context)
    {
        return new ProjectWorklogItemDto
        {
            Hours = item.Hours,
            Id = item.Id,
            ProjectId = item.ProjectId,
            ProjectName = context.ProjectNameMap.GetValueOrDefault(item.ProjectId) ?? "-",
            Summary = item.Summary,
            TaskId = item.TaskId,
            TaskTitle = item.TaskId.HasValue
                ? context.TaskTitleMap.GetValueOrDefault(item.TaskId.Value) ?? "-"
                : "-",
            UserId = item.UserId,
            UserName = context.UserNameMap.GetValueOrDefault(item.UserId) ?? "-",
            WeekStartDate = item.WeekStartDate,
            WorkDate = item.WorkDate
        };
    }

    internal static bool IsClosed(string status) => ProjectStatuses.IsClosed(status);

    private static bool IsCommunicationDocument(string category)
    {
        return category is "日报" or "周报" or "月报" or "阶段报告" or "发布报告" or "项目总结" or "会议纪要" or "会议记录";
    }

    private static string ResolveLatestReportStatus(
        IReadOnlyCollection<ProjectReport> reports,
        IReadOnlyCollection<ProjectDocument> legacyReports)
    {
        if (reports.Count == 0 && legacyReports.Count == 0)
        {
            return "未归档";
        }

        if (reports.Any(report => report.Status == "草稿" || report.Status == "待提交")
            || legacyReports.Any(report => report.Status == "草稿" || report.Status == "待提交"))
        {
            return "有草稿";
        }

        return reports.Count > 0
            ? reports.First().Status
            : legacyReports.First().Status;
    }

    internal static bool IsInSprintRange(DateTime value, DateTime? startDate, DateTime? endDate)
    {
        if (!startDate.HasValue && !endDate.HasValue)
        {
            return false;
        }

        var date = value.Date;
        if (startDate.HasValue && date < startDate.Value.Date)
        {
            return false;
        }

        if (endDate.HasValue && date > endDate.Value.Date)
        {
            return false;
        }

        return true;
    }

    internal static int CalculateHealthRiskScore(
        IReadOnlyCollection<ProjectTask> tasks,
        IReadOnlyCollection<ProjectIssue> issues,
        IReadOnlyCollection<ProjectRaidItem> raidItems,
        int taskCompletionRate)
    {
        return BuildHealthBreakdowns(tasks, issues, raidItems, taskCompletionRate).Sum(item => item.Score);
    }

    internal static List<ProjectHealthBreakdownDto> BuildHealthBreakdowns(
        IReadOnlyCollection<ProjectTask> tasks,
        IReadOnlyCollection<ProjectIssue> issues,
        IReadOnlyCollection<ProjectRaidItem> raidItems,
        int taskCompletionRate)
    {
        var openRisks = raidItems
            .Where(item => item.Type == ProjectRaidTypes.Risk && !IsClosed(item.Status))
            .ToList();
        var quantifiedRiskScore = openRisks.Sum(item =>
            item.RiskScore > 0
                ? item.RiskScore
                : item.Level switch
                {
                    ProjectPriorities.P0 => 15,
                    ProjectPriorities.P1 => 8,
                    ProjectPriorities.P2 => 5,
                    ProjectPriorities.P3 => 3,
                    _ => 3
                });
        var overdueTaskCount = tasks.Count(item => IsOverdue(item.PlannedEndTime, item.Status));
        var overdueTaskPenalty = overdueTaskCount * 8;
        var openIssueCount = issues.Count(item => !IsClosed(item.Status));
        var openIssuePenalty = openIssueCount * 10;
        var schedulePenalty = Math.Max(0, 100 - taskCompletionRate);
        var totalScore = quantifiedRiskScore + overdueTaskPenalty + openIssuePenalty + schedulePenalty;

        if (totalScore <= 100)
        {
            return
            [
                new ProjectHealthBreakdownDto
                {
                    ActionKey = "issues",
                    ActionLabel = "处理风险",
                    Code = "Risk",
                    DataSource = HealthDataSource,
                    Label = "开放风险",
                    Reason = openRisks.Count == 0
                        ? "当前没有未关闭风险。"
                        : $"{openRisks.Count} 个未关闭风险，按风险分或风险等级折算。",
                    RuleVersion = HealthRuleVersion,
                    Score = quantifiedRiskScore
                },
                new ProjectHealthBreakdownDto
                {
                    ActionKey = "tasks",
                    ActionLabel = "处理逾期任务",
                    Code = "OverdueTask",
                    DataSource = HealthDataSource,
                    Label = "逾期任务",
                    Reason = overdueTaskCount == 0
                        ? "当前没有逾期任务。"
                        : $"{overdueTaskCount} 个逾期任务，每项扣 8 分。",
                    RuleVersion = HealthRuleVersion,
                    Score = overdueTaskPenalty
                },
                new ProjectHealthBreakdownDto
                {
                    ActionKey = "issues",
                    ActionLabel = "处理问题",
                    Code = "OpenIssue",
                    DataSource = HealthDataSource,
                    Label = "未闭环问题",
                    Reason = openIssueCount == 0
                        ? "当前没有未闭环问题。"
                        : $"{openIssueCount} 个未闭环问题，每项扣 10 分。",
                    RuleVersion = HealthRuleVersion,
                    Score = openIssuePenalty
                },
                new ProjectHealthBreakdownDto
                {
                    ActionKey = "control",
                    ActionLabel = "查看项目控制",
                    Code = "Schedule",
                    DataSource = HealthDataSource,
                    Label = "完成率偏差",
                    Reason = taskCompletionRate >= 100
                        ? "任务完成率已达到 100%。"
                        : $"任务完成率 {taskCompletionRate}%，按剩余完成率折算。",
                    RuleVersion = HealthRuleVersion,
                    Score = schedulePenalty
                }
            ];
        }

        var ratio = 100d / totalScore;
        var normalizedItems = new List<ProjectHealthBreakdownDto>
        {
            new ProjectHealthBreakdownDto
            {
                ActionKey = "issues",
                ActionLabel = "处理风险",
                Code = "Risk",
                DataSource = HealthDataSource,
                Label = "开放风险",
                Reason = openRisks.Count == 0
                    ? "当前没有未关闭风险。"
                    : $"{openRisks.Count} 个未关闭风险，按风险分或风险等级折算。",
                RuleVersion = HealthRuleVersion,
                Score = (int)Math.Round(quantifiedRiskScore * ratio, MidpointRounding.AwayFromZero)
            },
            new ProjectHealthBreakdownDto
            {
                ActionKey = "tasks",
                ActionLabel = "处理逾期任务",
                Code = "OverdueTask",
                DataSource = HealthDataSource,
                Label = "逾期任务",
                Reason = overdueTaskCount == 0
                    ? "当前没有逾期任务。"
                    : $"{overdueTaskCount} 个逾期任务，每项扣 8 分。",
                RuleVersion = HealthRuleVersion,
                Score = (int)Math.Round(overdueTaskPenalty * ratio, MidpointRounding.AwayFromZero)
            },
            new ProjectHealthBreakdownDto
            {
                ActionKey = "issues",
                ActionLabel = "处理问题",
                Code = "OpenIssue",
                DataSource = HealthDataSource,
                Label = "未闭环问题",
                Reason = openIssueCount == 0
                    ? "当前没有未闭环问题。"
                    : $"{openIssueCount} 个未闭环问题，每项扣 10 分。",
                RuleVersion = HealthRuleVersion,
                Score = (int)Math.Round(openIssuePenalty * ratio, MidpointRounding.AwayFromZero)
            },
            new ProjectHealthBreakdownDto
            {
                ActionKey = "control",
                ActionLabel = "查看项目控制",
                Code = "Schedule",
                DataSource = HealthDataSource,
                Label = "完成率偏差",
                Reason = taskCompletionRate >= 100
                    ? "任务完成率已达到 100%。"
                    : $"任务完成率 {taskCompletionRate}%，按剩余完成率折算。",
                RuleVersion = HealthRuleVersion,
                Score = Math.Max(0, 100 - (int)Math.Round((quantifiedRiskScore + overdueTaskPenalty + openIssuePenalty) * ratio, MidpointRounding.AwayFromZero))
            }
        };

        var overflow = normalizedItems.Sum(item => item.Score) - 100;
        if (overflow > 0)
        {
            var largestItem = normalizedItems.OrderByDescending(item => item.Score).First();
            largestItem.Score = Math.Max(0, largestItem.Score - overflow);
        }
        else if (overflow < 0)
        {
            var largestItem = normalizedItems.OrderByDescending(item => item.Score).First();
            largestItem.Score += Math.Abs(overflow);
        }

        return normalizedItems;
    }

    internal static List<string> BuildHealthReasons(IReadOnlyCollection<ProjectHealthBreakdownDto> breakdowns)
    {
        var reasons = breakdowns
            .Where(item => item.Score > 0)
            .OrderByDescending(item => item.Score)
            .Select(item => $"{item.Label}：{item.Reason}")
            .ToList();

        return reasons.Count == 0
            ? ["当前没有触发风险、逾期任务、未闭环问题或进度偏差扣分。"]
            : reasons;
    }

    internal static string ResolveHealthStatus(int healthRiskScore)
    {
        if (healthRiskScore >= 70)
        {
            return "高风险";
        }

        return healthRiskScore >= 35 ? "关注中" : "健康";
    }

    internal static bool IsDueSoon(DateTime? plannedEndTime)
    {
        if (!plannedEndTime.HasValue)
        {
            return false;
        }

        var today = DateTime.Today;
        return plannedEndTime.Value.Date >= today && plannedEndTime.Value.Date <= today.AddDays(7);
    }

    internal static bool IsMyRelatedProject(Project project, ProjectContext context)
    {
        if (!context.CurrentUserId.HasValue)
        {
            return false;
        }

        var currentUserId = context.CurrentUserId.Value;
        return project.ManagerUserId == currentUserId
            || project.SponsorUserId == currentUserId
            || context.MyMemberMap.ContainsKey(project.Id)
            || context.MyTaskProjectIds.Contains(project.Id)
            || context.MyIssueProjectIds.Contains(project.Id)
            || context.MyWorklogProjectIds.Contains(project.Id);
    }

    internal static bool IsOverdue(DateTime? plannedEndTime, string status)
    {
        return plannedEndTime.HasValue && plannedEndTime.Value < DateTime.Now && !IsClosed(status);
    }

    internal static DateTime GetWeekStart(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.Date.AddDays(-diff);
    }

    private static string ResolveCalendarColor(string priority, string status)
    {
        if (IsClosed(status))
        {
            return "var(--ant-color-success)";
        }

        return priority == ProjectPriorities.P0
            ? "var(--ant-color-error)"
            : "var(--ant-color-primary)";
    }

    internal static string ResolveMyRelation(Project project, ProjectContext context)
    {
        if (context.CurrentUserId.HasValue)
        {
            if (project.ManagerUserId == context.CurrentUserId.Value)
            {
                return "项目经理";
            }

            if (project.SponsorUserId == context.CurrentUserId.Value)
            {
                return "项目发起人";
            }
        }

        if (context.MyMemberMap.TryGetValue(project.Id, out var member))
        {
            var roleLabel = string.Join(" / ", ResolveMemberRoles(member, context));
            if (!member.IsActive())
            {
                return string.IsNullOrWhiteSpace(roleLabel)
                    ? "历史只读"
                    : $"{roleLabel} / 历史只读";
            }

            return string.IsNullOrWhiteSpace(roleLabel) ? "项目成员" : roleLabel;
        }

        if (context.CurrentUserIsAdmin)
        {
            return "管理员";
        }

        if (context.CurrentUserIsPmo)
        {
            return "PMO";
        }

        return "组织可见";
    }

    private static List<string> ResolveMemberRoles(ProjectMember member, ProjectContext context)
    {
        var structuredRoles = context.MemberRoles
            .Where(item => item.ProjectMemberId == member.Id)
            .Select(item => item.RoleName)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return structuredRoles.Count > 0
            ? structuredRoles
            : member.GetRoleList().ToList();
    }
}
