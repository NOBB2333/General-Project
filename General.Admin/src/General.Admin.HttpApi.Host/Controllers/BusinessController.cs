using General.Admin.Infrastructure;
using General.Admin.PhaseOne;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Auditing;

namespace General.Admin.Controllers;

[ApiController]
[DisableAuditing]
[Authorize]
[Route("api/app/business")]
public class BusinessController : ControllerBase
{
    private readonly PhaseOneBusinessService _businessService;

    public BusinessController(PhaseOneBusinessService businessService)
    {
        _businessService = businessService;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<ApiResponse<PhaseOneBusinessOverviewDto>>> GetOverviewAsync()
    {
        return ApiResponse<PhaseOneBusinessOverviewDto>.Ok(await _businessService.GetOverviewAsync());
    }

    [HttpGet("projects")]
    public async Task<ActionResult<ApiResponse<List<PhaseOneBusinessProjectSummaryDto>>>> GetProjectsAsync([FromQuery] PhaseOneBusinessProjectListInput input)
    {
        return ApiResponse<List<PhaseOneBusinessProjectSummaryDto>>.Ok(await _businessService.GetProjectListAsync(input));
    }

    [HttpGet("projects/{projectId:guid}")]
    public async Task<ActionResult<ApiResponse<PhaseOneBusinessProjectDetailDto>>> GetProjectDetailAsync(Guid projectId)
    {
        return ApiResponse<PhaseOneBusinessProjectDetailDto>.Ok(await _businessService.GetProjectDetailAsync(projectId));
    }

    [HttpGet("reports")]
    public async Task<ActionResult<ApiResponse<PhaseOneBusinessReportDto>>> GetReportsAsync([FromQuery] PhaseOneBusinessReportQueryInput input)
    {
        return ApiResponse<PhaseOneBusinessReportDto>.Ok(await _businessService.GetReportAsync(input));
    }
}
