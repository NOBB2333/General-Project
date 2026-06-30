using General.Admin.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Auditing;

namespace General.Admin.Controllers;

[ApiController]
[Authorize]
[ApiExplorerSettings(GroupName = ApiDocGroups.Project)]
[Route("api/app/project")]
public class ProjectController : ControllerBase
{
    private readonly ProjectGovernanceService _projectGovernanceService;
    private readonly ProjectLifecycleService _projectLifecycleService;
    private readonly ProjectCommunicationService _projectCommunicationService;
    private readonly ProjectPlanService _projectPlanService;
    private readonly ProjectQualityService _projectQualityService;
    private readonly ProjectDashboardService _projectDashboardService;
    private readonly ProjectQueryService _projectQueryService;
    private readonly ProjectRequirementService _projectRequirementService;
    private readonly ProjectResourceService _projectResourceService;
    private readonly ProjectRiskService _projectRiskService;
    private readonly ProjectWorkflowService _projectWorkflowService;
    private readonly ProjectService _projectService;

    public ProjectController(
        ProjectGovernanceService projectGovernanceService,
        ProjectCommunicationService projectCommunicationService,
        ProjectLifecycleService projectLifecycleService,
        ProjectPlanService projectPlanService,
        ProjectQualityService projectQualityService,
        ProjectDashboardService projectDashboardService,
        ProjectQueryService projectQueryService,
        ProjectRequirementService projectRequirementService,
        ProjectResourceService projectResourceService,
        ProjectRiskService projectRiskService,
        ProjectWorkflowService projectWorkflowService,
        ProjectService projectService)
    {
        _projectGovernanceService = projectGovernanceService;
        _projectCommunicationService = projectCommunicationService;
        _projectLifecycleService = projectLifecycleService;
        _projectPlanService = projectPlanService;
        _projectQualityService = projectQualityService;
        _projectDashboardService = projectDashboardService;
        _projectQueryService = projectQueryService;
        _projectRequirementService = projectRequirementService;
        _projectResourceService = projectResourceService;
        _projectRiskService = projectRiskService;
        _projectWorkflowService = projectWorkflowService;
        _projectService = projectService;
    }

    [HttpGet("workspace")]
    public async Task<ActionResult<ApiResponse<ProjectWorkspaceDto>>> GetWorkspaceAsync()
    {
        return ApiResponse<ProjectWorkspaceDto>.Ok(await _projectQueryService.GetWorkspaceAsync());
    }

    [HttpGet("dashboard/my-related")]
    public async Task<ActionResult<ApiResponse<ProjectMyExecutionDashboardDto>>> GetMyRelatedDashboardAsync()
    {
        return ApiResponse<ProjectMyExecutionDashboardDto>.Ok(await _projectDashboardService.GetMyRelatedDashboardAsync());
    }

    [HttpGet("dashboard/pm")]
    public async Task<ActionResult<ApiResponse<ProjectPmDashboardDto>>> GetPmDashboardAsync()
    {
        return ApiResponse<ProjectPmDashboardDto>.Ok(await _projectDashboardService.GetPmDashboardAsync());
    }

    [HttpGet("dashboard/pmo")]
    public async Task<ActionResult<ApiResponse<ProjectPmoDashboardDto>>> GetPmoDashboardAsync()
    {
        return ApiResponse<ProjectPmoDashboardDto>.Ok(await _projectDashboardService.GetPmoDashboardAsync());
    }

    [HttpGet("list")]
    public async Task<ActionResult<ApiResponse<List<ProjectListItemDto>>>> GetListAsync([FromQuery] ProjectListInput input)
    {
        return ApiResponse<List<ProjectListItemDto>>.Ok(await _projectQueryService.GetProjectListAsync(input));
    }

    [Authorize(AdminPermissions.Project.Create)]
    [PlatformEndpoint("Project.Project.Create")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateAsync([FromBody] ProjectSaveInput input)
    {
        return ApiResponse<Guid>.Ok(await _projectService.CreateAsync(input));
    }

    [HttpGet("detail/{projectId:guid}")]
    public async Task<ActionResult<ApiResponse<ProjectDetailDto>>> GetDetailAsync(Guid projectId)
    {
        return ApiResponse<ProjectDetailDto>.Ok(await _projectQueryService.GetDetailAsync(projectId));
    }

    [HttpGet("detail/{section}/{projectId:guid}")]
    public async Task<ActionResult<ApiResponse<ProjectDetailDto>>> GetDetailSectionAsync(string section, Guid projectId)
    {
        return ApiResponse<ProjectDetailDto>.Ok(await _projectQueryService.GetDetailSectionAsync(projectId, section));
    }

    [HttpPost("{projectId:guid}/acceptance-criteria")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateAcceptanceCriteriaAsync(Guid projectId, [FromBody] ProjectAcceptanceCriteriaSaveInput input)
    {
        return ApiResponse<Guid>.Ok(await _projectQualityService.CreateAcceptanceCriteriaAsync(projectId, input));
    }

    [HttpPost("{projectId:guid}/acceptance-records")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateAcceptanceRecordAsync(Guid projectId, [FromBody] ProjectAcceptanceRecordSaveInput input)
    {
        return ApiResponse<Guid>.Ok(await _projectQualityService.CreateAcceptanceRecordAsync(projectId, input));
    }

    [HttpPost("{projectId:guid}/baselines")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateBaselineAsync(Guid projectId, [FromBody] ProjectBaselineSaveInput input)
    {
        return ApiResponse<Guid>.Ok(await _projectGovernanceService.CreateBaselineAsync(projectId, input));
    }

    [HttpPost("change-requests/{changeRequestId:guid}/approval/submit")]
    public async Task<ActionResult<ApiResponse<Guid>>> SubmitChangeApprovalAsync(
        Guid changeRequestId,
        [FromBody] ProjectWorkflowActionInput input)
    {
        return ApiResponse<Guid>.Ok(await _projectWorkflowService.SubmitChangeApprovalAsync(changeRequestId, input));
    }

    [Authorize(AdminPermissions.Project.WorkflowApprove)]
    [PlatformEndpoint("Project.Workflow.Approve")]
    [HttpPost("change-requests/{changeRequestId:guid}/approval/approve")]
    public async Task<ActionResult<ApiResponse<bool>>> ApproveChangeAsync(
        Guid changeRequestId,
        [FromBody] ProjectWorkflowActionInput input)
    {
        await _projectWorkflowService.ApproveChangeAsync(changeRequestId, input);
        return ApiResponse<bool>.Ok(true);
    }

    [Authorize(AdminPermissions.Project.WorkflowApprove)]
    [PlatformEndpoint("Project.Workflow.Approve")]
    [HttpPost("change-requests/{changeRequestId:guid}/approval/reject")]
    public async Task<ActionResult<ApiResponse<bool>>> RejectChangeAsync(
        Guid changeRequestId,
        [FromBody] ProjectWorkflowActionInput input)
    {
        await _projectWorkflowService.RejectChangeAsync(changeRequestId, input);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpPost("change-requests/{changeRequestId:guid}/approval/withdraw")]
    public async Task<ActionResult<ApiResponse<bool>>> WithdrawChangeApprovalAsync(
        Guid changeRequestId,
        [FromBody] ProjectWorkflowActionInput input)
    {
        await _projectWorkflowService.WithdrawChangeApprovalAsync(changeRequestId, input);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpPost("{projectId:guid}/change-requests")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateChangeRequestAsync(Guid projectId, [FromBody] ProjectChangeRequestSaveInput input)
    {
        return ApiResponse<Guid>.Ok(await _projectGovernanceService.CreateChangeRequestAsync(projectId, input));
    }

    [HttpPost("{projectId:guid}/charter")]
    public async Task<ActionResult<ApiResponse<Guid>>> SaveCharterAsync(Guid projectId, [FromBody] ProjectCharterSaveInput input)
    {
        return ApiResponse<Guid>.Ok(await _projectGovernanceService.SaveCharterAsync(projectId, input));
    }

    [HttpPost("{projectId:guid}/charter/approval/submit")]
    public async Task<ActionResult<ApiResponse<Guid>>> SubmitCharterApprovalAsync(
        Guid projectId,
        [FromBody] ProjectCharterApprovalActionInput input)
    {
        return ApiResponse<Guid>.Ok(await _projectWorkflowService.SubmitCharterApprovalAsync(projectId, input));
    }

    [Authorize(AdminPermissions.Project.WorkflowApprove)]
    [PlatformEndpoint("Project.Workflow.Approve")]
    [HttpPost("{projectId:guid}/charter/approval/approve")]
    public async Task<ActionResult<ApiResponse<bool>>> ApproveCharterAsync(
        Guid projectId,
        [FromBody] ProjectCharterApprovalActionInput input)
    {
        await _projectWorkflowService.ApproveCharterAsync(projectId, input);
        return ApiResponse<bool>.Ok(true);
    }

    [Authorize(AdminPermissions.Project.WorkflowApprove)]
    [PlatformEndpoint("Project.Workflow.Approve")]
    [HttpPost("{projectId:guid}/charter/approval/reject")]
    public async Task<ActionResult<ApiResponse<bool>>> RejectCharterAsync(
        Guid projectId,
        [FromBody] ProjectCharterApprovalActionInput input)
    {
        await _projectWorkflowService.RejectCharterAsync(projectId, input);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpPost("{projectId:guid}/charter/approval/withdraw")]
    public async Task<ActionResult<ApiResponse<bool>>> WithdrawCharterApprovalAsync(
        Guid projectId,
        [FromBody] ProjectCharterApprovalActionInput input)
    {
        await _projectWorkflowService.WithdrawCharterApprovalAsync(projectId, input);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpPost("{projectId:guid}/sprints")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateSprintAsync(Guid projectId, [FromBody] ProjectSprintSaveInput input)
    {
        return ApiResponse<Guid>.Ok(await _projectPlanService.CreateSprintAsync(projectId, input));
    }

    [HttpPost("{projectId:guid}/milestones")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateMilestoneAsync(Guid projectId, [FromBody] ProjectMilestoneSaveInput input)
    {
        return ApiResponse<Guid>.Ok(await _projectPlanService.CreateMilestoneAsync(projectId, input));
    }

    [HttpPost("{projectId:guid}/performance-snapshots")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreatePerformanceSnapshotAsync(Guid projectId, [FromBody] ProjectPerformanceSnapshotSaveInput input)
    {
        return ApiResponse<Guid>.Ok(await _projectGovernanceService.CreatePerformanceSnapshotAsync(projectId, input));
    }

    [HttpGet("{projectId:guid}/reports/draft")]
    public async Task<ActionResult<ApiResponse<ProjectReportDraftDto>>> GetReportDraftAsync(
        Guid projectId,
        [FromQuery] ProjectReportDraftInput input)
    {
        return ApiResponse<ProjectReportDraftDto>.Ok(await _projectCommunicationService.GetReportDraftAsync(projectId, input));
    }

    [HttpPost("{projectId:guid}/reports")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateReportAsync(
        Guid projectId,
        [FromBody] ProjectReportSaveInput input)
    {
        return ApiResponse<Guid>.Ok(await _projectCommunicationService.CreateReportAsync(projectId, input));
    }

    [HttpPost("{projectId:guid}/meeting-records")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateMeetingRecordAsync(
        Guid projectId,
        [FromBody] ProjectMeetingRecordSaveInput input)
    {
        return ApiResponse<Guid>.Ok(await _projectCommunicationService.CreateMeetingRecordAsync(projectId, input));
    }

    [HttpPost("{projectId:guid}/quality-checklists")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateQualityChecklistAsync(Guid projectId, [FromBody] ProjectQualityChecklistSaveInput input)
    {
        return ApiResponse<Guid>.Ok(await _projectQualityService.CreateQualityChecklistAsync(projectId, input));
    }

    [HttpPost("{projectId:guid}/quality-inspections")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateQualityInspectionAsync(Guid projectId, [FromBody] ProjectQualityInspectionSaveInput input)
    {
        return ApiResponse<Guid>.Ok(await _projectQualityService.CreateQualityInspectionAsync(projectId, input));
    }

    [HttpPost("{projectId:guid}/issues")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateIssueAsync(Guid projectId, [FromBody] ProjectIssueSaveInput input)
    {
        return ApiResponse<Guid>.Ok(await _projectQualityService.CreateIssueAsync(projectId, input));
    }

    [HttpPost("{projectId:guid}/requirements")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateRequirementAsync(Guid projectId, [FromBody] ProjectRequirementSaveInput input)
    {
        return ApiResponse<Guid>.Ok(await _projectRequirementService.CreateRequirementAsync(projectId, input));
    }

    [HttpPost("{projectId:guid}/task-dependencies")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateTaskDependencyAsync(Guid projectId, [FromBody] ProjectTaskDependencySaveInput input)
    {
        return ApiResponse<Guid>.Ok(await _projectPlanService.CreateTaskDependencyAsync(projectId, input));
    }

    [HttpPost("{projectId:guid}/requirements/{requirementId:guid}/tasks")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateTaskFromRequirementAsync(
        Guid projectId,
        Guid requirementId,
        [FromBody] ProjectTaskFromRequirementInput input)
    {
        return ApiResponse<Guid>.Ok(await _projectPlanService.CreateTaskFromRequirementAsync(projectId, requirementId, input));
    }

    [HttpPost("{projectId:guid}/wbs-nodes/{wbsNodeId:guid}/requirements")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateRequirementFromWbsAsync(
        Guid projectId,
        Guid wbsNodeId,
        [FromBody] ProjectRequirementFromWbsInput input)
    {
        return ApiResponse<Guid>.Ok(await _projectPlanService.CreateRequirementFromWbsAsync(projectId, wbsNodeId, input));
    }

    [HttpPost("{projectId:guid}/wbs-nodes/{wbsNodeId:guid}/tasks")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateTaskFromWbsAsync(
        Guid projectId,
        Guid wbsNodeId,
        [FromBody] ProjectTaskFromWbsInput input)
    {
        return ApiResponse<Guid>.Ok(await _projectPlanService.CreateTaskFromWbsAsync(projectId, wbsNodeId, input));
    }

    [HttpPost("{projectId:guid}/wbs-nodes")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateWbsNodeAsync(Guid projectId, [FromBody] ProjectWbsNodeSaveInput input)
    {
        return ApiResponse<Guid>.Ok(await _projectPlanService.CreateWbsNodeAsync(projectId, input));
    }

    [HttpPut("wbs-nodes/{wbsNodeId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateWbsNodeAsync(Guid wbsNodeId, [FromBody] ProjectWbsNodeSaveInput input)
    {
        await _projectPlanService.UpdateWbsNodeAsync(wbsNodeId, input);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpDelete("wbs-nodes/{wbsNodeId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteWbsNodeAsync(Guid wbsNodeId)
    {
        await _projectPlanService.DeleteWbsNodeAsync(wbsNodeId);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpPut("sprints/{sprintId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateSprintAsync(Guid sprintId, [FromBody] ProjectSprintSaveInput input)
    {
        await _projectPlanService.UpdateSprintAsync(sprintId, input);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpDelete("sprints/{sprintId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteSprintAsync(Guid sprintId)
    {
        await _projectPlanService.DeleteSprintAsync(sprintId);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpPut("milestones/{milestoneId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateMilestoneAsync(Guid milestoneId, [FromBody] ProjectMilestoneSaveInput input)
    {
        await _projectPlanService.UpdateMilestoneAsync(milestoneId, input);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpDelete("milestones/{milestoneId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteMilestoneAsync(Guid milestoneId)
    {
        await _projectPlanService.DeleteMilestoneAsync(milestoneId);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpPut("members/{projectMemberId:guid}/roles")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateMemberRolesAsync(Guid projectMemberId, [FromBody] ProjectMemberRolesUpdateInput input)
    {
        await _projectResourceService.UpdateMemberRolesAsync(projectMemberId, input);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpPut("{projectId:guid}/status")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateProjectStatusAsync(Guid projectId, [FromBody] ProjectStatusUpdateInput input)
    {
        await _projectLifecycleService.UpdateProjectStatusAsync(projectId, input);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpPut("issues/{issueId:guid}/quality-link")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateIssueQualityLinkAsync(Guid issueId, [FromBody] ProjectIssueQualityLinkInput input)
    {
        await _projectQualityService.UpdateIssueQualityLinkAsync(issueId, input);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpPut("issues/{issueId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateIssueAsync(Guid issueId, [FromBody] ProjectIssueSaveInput input)
    {
        await _projectQualityService.UpdateIssueAsync(issueId, input);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpDelete("issues/{issueId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteIssueAsync(Guid issueId)
    {
        await _projectQualityService.DeleteIssueAsync(issueId);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpPut("milestones/{milestoneId:guid}/status")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateMilestoneStatusAsync(Guid milestoneId, [FromBody] ProjectMilestoneStatusUpdateInput input)
    {
        await _projectLifecycleService.UpdateMilestoneStatusAsync(milestoneId, input);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpPut("raid/{raidItemId:guid}/risk-assessment")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateRaidRiskAsync(Guid raidItemId, [FromBody] ProjectRaidRiskUpdateInput input)
    {
        await _projectRiskService.UpdateRaidRiskAsync(raidItemId, input);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpPut("requirements/{requirementId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateRequirementAsync(Guid requirementId, [FromBody] ProjectRequirementSaveInput input)
    {
        await _projectRequirementService.UpdateRequirementAsync(requirementId, input);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpPut("tasks/{taskId:guid}/wbs")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateTaskWbsAsync(Guid taskId, [FromBody] ProjectTaskWbsUpdateInput input)
    {
        await _projectPlanService.UpdateTaskWbsAsync(taskId, input);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpPut("tasks/{taskId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateTaskAsync(Guid taskId, [FromBody] ProjectTaskSaveInput input)
    {
        await _projectPlanService.UpdateTaskAsync(taskId, input);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpPost("tasks/{taskId:guid}/progress")]
    public async Task<ActionResult<ApiResponse<Guid>>> RecordTaskProgressAsync(Guid taskId, [FromBody] ProjectTaskProgressInput input)
    {
        return ApiResponse<Guid>.Ok(await _projectPlanService.RecordTaskProgressAsync(taskId, input));
    }

    [HttpGet("my-related")]
    public async Task<ActionResult<ApiResponse<ProjectMyRelatedDto>>> GetMyRelatedAsync()
    {
        return ApiResponse<ProjectMyRelatedDto>.Ok(await _projectQueryService.GetMyRelatedAsync());
    }

    [HttpGet("task/list")]
    public async Task<ActionResult<ApiResponse<List<ProjectTaskItemDto>>>> GetTaskListAsync([FromQuery] ProjectTaskListInput input)
    {
        return ApiResponse<List<ProjectTaskItemDto>>.Ok(await _projectQueryService.GetTaskListAsync(input));
    }

    [HttpGet("raid/list")]
    public async Task<ActionResult<ApiResponse<List<ProjectRaidItemDto>>>> GetRaidListAsync([FromQuery] ProjectRaidListInput input)
    {
        return ApiResponse<List<ProjectRaidItemDto>>.Ok(await _projectQueryService.GetRaidListAsync(input));
    }
}
