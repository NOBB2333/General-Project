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
    private readonly ProjectService _projectService;

    public ProjectController(ProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet("workspace")]
    public async Task<ActionResult<ApiResponse<ProjectWorkspaceDto>>> GetWorkspaceAsync()
    {
        return ApiResponse<ProjectWorkspaceDto>.Ok(await _projectService.GetWorkspaceAsync());
    }

    [HttpGet("list")]
    public async Task<ActionResult<ApiResponse<List<ProjectListItemDto>>>> GetListAsync([FromQuery] ProjectListInput input)
    {
        return ApiResponse<List<ProjectListItemDto>>.Ok(await _projectService.GetProjectListAsync(input));
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
        return ApiResponse<ProjectDetailDto>.Ok(await _projectService.GetDetailAsync(projectId));
    }

    [HttpPost("{projectId:guid}/acceptance-criteria")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateAcceptanceCriteriaAsync(Guid projectId, [FromBody] ProjectAcceptanceCriteriaSaveInput input)
    {
        return ApiResponse<Guid>.Ok(await _projectService.CreateAcceptanceCriteriaAsync(projectId, input));
    }

    [HttpPost("{projectId:guid}/charter")]
    public async Task<ActionResult<ApiResponse<Guid>>> SaveCharterAsync(Guid projectId, [FromBody] ProjectCharterSaveInput input)
    {
        return ApiResponse<Guid>.Ok(await _projectService.SaveCharterAsync(projectId, input));
    }

    [HttpPost("{projectId:guid}/quality-checklists")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateQualityChecklistAsync(Guid projectId, [FromBody] ProjectQualityChecklistSaveInput input)
    {
        return ApiResponse<Guid>.Ok(await _projectService.CreateQualityChecklistAsync(projectId, input));
    }

    [HttpPost("{projectId:guid}/quality-inspections")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateQualityInspectionAsync(Guid projectId, [FromBody] ProjectQualityInspectionSaveInput input)
    {
        return ApiResponse<Guid>.Ok(await _projectService.CreateQualityInspectionAsync(projectId, input));
    }

    [HttpPost("{projectId:guid}/task-dependencies")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateTaskDependencyAsync(Guid projectId, [FromBody] ProjectTaskDependencySaveInput input)
    {
        return ApiResponse<Guid>.Ok(await _projectService.CreateTaskDependencyAsync(projectId, input));
    }

    [HttpPut("members/{projectMemberId:guid}/roles")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateMemberRolesAsync(Guid projectMemberId, [FromBody] ProjectMemberRolesUpdateInput input)
    {
        await _projectService.UpdateMemberRolesAsync(projectMemberId, input);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpPut("{projectId:guid}/status")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateProjectStatusAsync(Guid projectId, [FromBody] ProjectStatusUpdateInput input)
    {
        await _projectService.UpdateProjectStatusAsync(projectId, input);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpPut("issues/{issueId:guid}/quality-link")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateIssueQualityLinkAsync(Guid issueId, [FromBody] ProjectIssueQualityLinkInput input)
    {
        await _projectService.UpdateIssueQualityLinkAsync(issueId, input);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpPut("milestones/{milestoneId:guid}/status")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateMilestoneStatusAsync(Guid milestoneId, [FromBody] ProjectMilestoneStatusUpdateInput input)
    {
        await _projectService.UpdateMilestoneStatusAsync(milestoneId, input);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpPut("raid/{raidItemId:guid}/risk-assessment")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateRaidRiskAsync(Guid raidItemId, [FromBody] ProjectRaidRiskUpdateInput input)
    {
        await _projectService.UpdateRaidRiskAsync(raidItemId, input);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpPut("tasks/{taskId:guid}/wbs")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateTaskWbsAsync(Guid taskId, [FromBody] ProjectTaskWbsUpdateInput input)
    {
        await _projectService.UpdateTaskWbsAsync(taskId, input);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpGet("my-related")]
    public async Task<ActionResult<ApiResponse<ProjectMyRelatedDto>>> GetMyRelatedAsync()
    {
        return ApiResponse<ProjectMyRelatedDto>.Ok(await _projectService.GetMyRelatedAsync());
    }

    [HttpGet("task/list")]
    public async Task<ActionResult<ApiResponse<List<ProjectTaskItemDto>>>> GetTaskListAsync([FromQuery] ProjectTaskListInput input)
    {
        return ApiResponse<List<ProjectTaskItemDto>>.Ok(await _projectService.GetTaskListAsync(input));
    }

    [HttpGet("raid/list")]
    public async Task<ActionResult<ApiResponse<List<ProjectRaidItemDto>>>> GetRaidListAsync([FromQuery] ProjectRaidListInput input)
    {
        return ApiResponse<List<ProjectRaidItemDto>>.Ok(await _projectService.GetRaidListAsync(input));
    }
}
