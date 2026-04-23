using General.Admin.Infrastructure;
using General.Admin.PhaseOne;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Auditing;

namespace General.Admin.Controllers;

[ApiController]
[Authorize]
[Route("api/app/project")]
public class ProjectController : ControllerBase
{
    private readonly PhaseOneProjectService _projectService;

    public ProjectController(PhaseOneProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet("workspace")]
    public async Task<ActionResult<ApiResponse<PhaseOneProjectWorkspaceDto>>> GetWorkspaceAsync()
    {
        return ApiResponse<PhaseOneProjectWorkspaceDto>.Ok(await _projectService.GetWorkspaceAsync());
    }

    [HttpGet("list")]
    public async Task<ActionResult<ApiResponse<List<PhaseOneProjectListItemDto>>>> GetListAsync([FromQuery] PhaseOneProjectListInput input)
    {
        return ApiResponse<List<PhaseOneProjectListItemDto>>.Ok(await _projectService.GetProjectListAsync(input));
    }

    [HttpGet("detail/{projectId:guid}")]
    public async Task<ActionResult<ApiResponse<PhaseOneProjectDetailDto>>> GetDetailAsync(Guid projectId)
    {
        return ApiResponse<PhaseOneProjectDetailDto>.Ok(await _projectService.GetDetailAsync(projectId));
    }

    [HttpGet("my-related")]
    public async Task<ActionResult<ApiResponse<PhaseOneProjectMyRelatedDto>>> GetMyRelatedAsync()
    {
        return ApiResponse<PhaseOneProjectMyRelatedDto>.Ok(await _projectService.GetMyRelatedAsync());
    }

    [HttpGet("task/list")]
    public async Task<ActionResult<ApiResponse<List<PhaseOneProjectTaskItemDto>>>> GetTaskListAsync([FromQuery] PhaseOneProjectTaskListInput input)
    {
        return ApiResponse<List<PhaseOneProjectTaskItemDto>>.Ok(await _projectService.GetTaskListAsync(input));
    }

    [HttpGet("raid/list")]
    public async Task<ActionResult<ApiResponse<List<PhaseOneProjectRaidItemDto>>>> GetRaidListAsync([FromQuery] PhaseOneProjectRaidListInput input)
    {
        return ApiResponse<List<PhaseOneProjectRaidItemDto>>.Ok(await _projectService.GetRaidListAsync(input));
    }
}
