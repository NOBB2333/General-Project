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
