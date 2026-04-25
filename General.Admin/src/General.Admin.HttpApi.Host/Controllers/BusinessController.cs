using General.Admin.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Auditing;

namespace General.Admin.Controllers;

[ApiController]
[Authorize]
[ApiExplorerSettings(GroupName = ApiDocGroups.Business)]
[Route("api/app/business")]
public class BusinessController : ControllerBase
{
    private readonly BusinessService _businessService;

    public BusinessController(BusinessService businessService)
    {
        _businessService = businessService;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<ApiResponse<BusinessOverviewDto>>> GetOverviewAsync()
    {
        return ApiResponse<BusinessOverviewDto>.Ok(await _businessService.GetOverviewAsync());
    }

    [HttpGet("projects")]
    public async Task<ActionResult<ApiResponse<List<BusinessProjectSummaryDto>>>> GetProjectsAsync([FromQuery] BusinessProjectListInput input)
    {
        return ApiResponse<List<BusinessProjectSummaryDto>>.Ok(await _businessService.GetProjectListAsync(input));
    }

    [HttpGet("projects/{projectId:guid}")]
    public async Task<ActionResult<ApiResponse<BusinessProjectDetailDto>>> GetProjectDetailAsync(Guid projectId)
    {
        return ApiResponse<BusinessProjectDetailDto>.Ok(await _businessService.GetProjectDetailAsync(projectId));
    }

    [HttpGet("reports")]
    public async Task<ActionResult<ApiResponse<BusinessReportDto>>> GetReportsAsync([FromQuery] BusinessReportQueryInput input)
    {
        return ApiResponse<BusinessReportDto>.Ok(await _businessService.GetReportAsync(input));
    }
}
