using General.Admin.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace General.Admin.Controllers;

[ApiController]
[Authorize]
[ApiExplorerSettings(GroupName = ApiDocGroups.Platform)]
[Route("api/app/platform/workspace")]
public class PlatformWorkspaceController : ControllerBase
{
    private readonly PlatformWorkspaceService _workspaceService;

    public PlatformWorkspaceController(PlatformWorkspaceService workspaceService)
    {
        _workspaceService = workspaceService;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<PlatformWorkspaceSummaryDto>>> GetSummaryAsync()
    {
        return ApiResponse<PlatformWorkspaceSummaryDto>.Ok(await _workspaceService.GetSummaryAsync());
    }
}
