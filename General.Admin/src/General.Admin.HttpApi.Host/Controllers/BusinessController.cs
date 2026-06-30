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
    private readonly BusinessMutationService _businessMutationService;
    private readonly BusinessService _businessService;

    public BusinessController(BusinessMutationService businessMutationService, BusinessService businessService)
    {
        _businessMutationService = businessMutationService;
        _businessService = businessService;
    }

    [HttpGet("overview")]
    [Authorize(AdminPermissions.Business.ReportView)]
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

    [HttpPost("projects/{projectId:guid}/budgets")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateBudgetExecutionAsync(Guid projectId, [FromBody] BusinessBudgetExecutionSaveInput input)
    {
        return ApiResponse<Guid>.Ok(await _businessMutationService.CreateBudgetExecutionAsync(projectId, input));
    }

    [HttpPut("projects/{projectId:guid}/budgets/{budgetExecutionId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateBudgetExecutionAsync(Guid projectId, Guid budgetExecutionId, [FromBody] BusinessBudgetExecutionSaveInput input)
    {
        await _businessMutationService.UpdateBudgetExecutionAsync(projectId, budgetExecutionId, input);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpDelete("projects/{projectId:guid}/budgets/{budgetExecutionId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteBudgetExecutionAsync(Guid projectId, Guid budgetExecutionId)
    {
        await _businessMutationService.DeleteBudgetExecutionAsync(projectId, budgetExecutionId);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpPost("projects/{projectId:guid}/acceptances")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateAcceptanceAsync(Guid projectId, [FromBody] BusinessAcceptanceSaveInput input)
    {
        return ApiResponse<Guid>.Ok(await _businessMutationService.CreateAcceptanceAsync(projectId, input));
    }

    [HttpPut("projects/{projectId:guid}/acceptances/{acceptanceId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateAcceptanceAsync(Guid projectId, Guid acceptanceId, [FromBody] BusinessAcceptanceSaveInput input)
    {
        await _businessMutationService.UpdateAcceptanceAsync(projectId, acceptanceId, input);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpDelete("projects/{projectId:guid}/acceptances/{acceptanceId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteAcceptanceAsync(Guid projectId, Guid acceptanceId)
    {
        await _businessMutationService.DeleteAcceptanceAsync(projectId, acceptanceId);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpPost("projects/{projectId:guid}/chains")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateBusinessChainAsync(Guid projectId, [FromBody] BusinessChainSaveInput input)
    {
        return ApiResponse<Guid>.Ok(await _businessMutationService.CreateBusinessChainAsync(projectId, input));
    }

    [HttpPut("projects/{projectId:guid}/chains/{chainId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateBusinessChainAsync(Guid projectId, Guid chainId, [FromBody] BusinessChainSaveInput input)
    {
        await _businessMutationService.UpdateBusinessChainAsync(projectId, chainId, input);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpDelete("projects/{projectId:guid}/chains/{chainId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteBusinessChainAsync(Guid projectId, Guid chainId)
    {
        await _businessMutationService.DeleteBusinessChainAsync(projectId, chainId);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpPost("projects/{projectId:guid}/contracts")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateContractAsync(Guid projectId, [FromBody] BusinessContractSaveInput input)
    {
        return ApiResponse<Guid>.Ok(await _businessMutationService.CreateContractAsync(projectId, input));
    }

    [HttpPut("projects/{projectId:guid}/contracts/{contractId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateContractAsync(Guid projectId, Guid contractId, [FromBody] BusinessContractSaveInput input)
    {
        await _businessMutationService.UpdateContractAsync(projectId, contractId, input);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpDelete("projects/{projectId:guid}/contracts/{contractId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteContractAsync(Guid projectId, Guid contractId)
    {
        await _businessMutationService.DeleteContractAsync(projectId, contractId);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpPost("projects/{projectId:guid}/procurements")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateProcurementAsync(Guid projectId, [FromBody] BusinessProcurementSaveInput input)
    {
        return ApiResponse<Guid>.Ok(await _businessMutationService.CreateProcurementAsync(projectId, input));
    }

    [HttpPut("projects/{projectId:guid}/procurements/{procurementId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateProcurementAsync(Guid projectId, Guid procurementId, [FromBody] BusinessProcurementSaveInput input)
    {
        await _businessMutationService.UpdateProcurementAsync(projectId, procurementId, input);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpDelete("projects/{projectId:guid}/procurements/{procurementId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteProcurementAsync(Guid projectId, Guid procurementId)
    {
        await _businessMutationService.DeleteProcurementAsync(projectId, procurementId);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpPost("projects/{projectId:guid}/invoices")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateInvoiceAsync(Guid projectId, [FromBody] BusinessInvoiceSaveInput input)
    {
        return ApiResponse<Guid>.Ok(await _businessMutationService.CreateInvoiceAsync(projectId, input));
    }

    [HttpPut("projects/{projectId:guid}/invoices/{invoiceId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateInvoiceAsync(Guid projectId, Guid invoiceId, [FromBody] BusinessInvoiceSaveInput input)
    {
        await _businessMutationService.UpdateInvoiceAsync(projectId, invoiceId, input);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpDelete("projects/{projectId:guid}/invoices/{invoiceId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteInvoiceAsync(Guid projectId, Guid invoiceId)
    {
        await _businessMutationService.DeleteInvoiceAsync(projectId, invoiceId);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpPost("projects/{projectId:guid}/receivables")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateReceivableAsync(Guid projectId, [FromBody] BusinessReceivableSaveInput input)
    {
        return ApiResponse<Guid>.Ok(await _businessMutationService.CreateReceivableAsync(projectId, input));
    }

    [HttpPut("projects/{projectId:guid}/receivables/{receivableId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateReceivableAsync(Guid projectId, Guid receivableId, [FromBody] BusinessReceivableSaveInput input)
    {
        await _businessMutationService.UpdateReceivableAsync(projectId, receivableId, input);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpDelete("projects/{projectId:guid}/receivables/{receivableId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteReceivableAsync(Guid projectId, Guid receivableId)
    {
        await _businessMutationService.DeleteReceivableAsync(projectId, receivableId);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpGet("reports")]
    [Authorize(AdminPermissions.Business.ReportView)]
    public async Task<ActionResult<ApiResponse<BusinessReportDto>>> GetReportsAsync([FromQuery] BusinessReportQueryInput input)
    {
        return ApiResponse<BusinessReportDto>.Ok(await _businessService.GetReportAsync(input));
    }

    [HttpGet("reports/export")]
    [Authorize(AdminPermissions.Business.ReportExport)]
    [DisableAuditing]
    public async Task<IActionResult> ExportReportsAsync([FromQuery] BusinessReportExportInput input)
    {
        var result = await _businessService.ExportReportAsync(input);
        return File(result.Content, result.ContentType, result.FileName);
    }
}
