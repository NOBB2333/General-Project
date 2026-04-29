using General.Admin.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Auditing;

namespace General.Admin.Controllers;

[ApiController]
[Authorize]
[ApiExplorerSettings(GroupName = ApiDocGroups.Platform)]
[Route("api/app/platform/scheduler")]
public class PlatformSchedulerController : ControllerBase
{
    private readonly PlatformSchedulerService _schedulerService;

    public PlatformSchedulerController(PlatformSchedulerService schedulerService)
    {
        _schedulerService = schedulerService;
    }

    [HttpGet("list")]
    public async Task<ActionResult<ApiResponse<List<PlatformScheduledJobDto>>>> GetListAsync()
    {
        return ApiResponse<List<PlatformScheduledJobDto>>.Ok(await _schedulerService.GetListAsync());
    }

    [HttpGet("handlers")]
    public async Task<ActionResult<ApiResponse<List<PlatformScheduledJobHandlerDto>>>> GetHandlersAsync()
    {
        return ApiResponse<List<PlatformScheduledJobHandlerDto>>.Ok(await _schedulerService.GetHandlersAsync());
    }

    [HttpGet("cluster-nodes")]
    public async Task<ActionResult<ApiResponse<List<PlatformScheduledJobClusterNodeDto>>>> GetClusterNodesAsync()
    {
        return ApiResponse<List<PlatformScheduledJobClusterNodeDto>>.Ok(await _schedulerService.GetClusterNodesAsync());
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PlatformScheduledJobDto>>> CreateAsync([FromBody] PlatformScheduledJobSaveInput input)
    {
        return ApiResponse<PlatformScheduledJobDto>.Ok(await _schedulerService.CreateAsync(input));
    }

    [HttpPut("{jobKey}")]
    public async Task<ActionResult<ApiResponse<PlatformScheduledJobDto>>> UpdateAsync(
        string jobKey,
        [FromBody] PlatformScheduledJobSaveInput input)
    {
        return ApiResponse<PlatformScheduledJobDto>.Ok(await _schedulerService.UpdateAsync(jobKey, input));
    }

    [HttpGet("{jobKey}/records")]
    public async Task<ActionResult<ApiResponse<List<PlatformScheduledJobRecordDto>>>> GetRecordsAsync(
        string jobKey,
        [FromQuery] PlatformScheduledJobRecordQueryInput input)
    {
        return ApiResponse<List<PlatformScheduledJobRecordDto>>.Ok(
            await _schedulerService.GetRecordsAsync(jobKey, input));
    }

    [HttpGet("{jobKey}/triggers")]
    public async Task<ActionResult<ApiResponse<List<PlatformScheduledJobTriggerDto>>>> GetTriggersAsync(string jobKey)
    {
        return ApiResponse<List<PlatformScheduledJobTriggerDto>>.Ok(await _schedulerService.GetTriggersAsync(jobKey));
    }

    [HttpPost("{jobKey}/triggers")]
    public async Task<ActionResult<ApiResponse<PlatformScheduledJobTriggerDto>>> CreateTriggerAsync(
        string jobKey,
        [FromBody] PlatformScheduledJobTriggerSaveInput input)
    {
        return ApiResponse<PlatformScheduledJobTriggerDto>.Ok(await _schedulerService.CreateTriggerAsync(jobKey, input));
    }

    [HttpPut("{jobKey}/triggers/{triggerKey}")]
    public async Task<ActionResult<ApiResponse<PlatformScheduledJobTriggerDto>>> UpdateTriggerAsync(
        string jobKey,
        string triggerKey,
        [FromBody] PlatformScheduledJobTriggerSaveInput input)
    {
        return ApiResponse<PlatformScheduledJobTriggerDto>.Ok(
            await _schedulerService.UpdateTriggerAsync(jobKey, triggerKey, input));
    }

    [HttpPost("{jobKey}/triggers/{triggerKey}/toggle")]
    public async Task<ActionResult<ApiResponse<string>>> ToggleTriggerAsync(
        string jobKey,
        string triggerKey,
        [FromBody] PlatformScheduledJobToggleInput input)
    {
        await _schedulerService.ToggleTriggerAsync(jobKey, triggerKey, input.IsEnabled);
        return ApiResponse<string>.Ok("ok");
    }

    [HttpDelete("{jobKey}/triggers/{triggerKey}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteTriggerAsync(string jobKey, string triggerKey)
    {
        await _schedulerService.DeleteTriggerAsync(jobKey, triggerKey);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpPost("{jobKey}/run")]
    public async Task<ActionResult<ApiResponse<string>>> RunAsync(string jobKey)
    {
        return ApiResponse<string>.Ok(await _schedulerService.RunAsync(jobKey));
    }

    [HttpPost("{jobKey}/cancel")]
    public async Task<ActionResult<ApiResponse<string>>> CancelAsync(string jobKey)
    {
        return ApiResponse<string>.Ok(await _schedulerService.CancelAsync(jobKey));
    }

    [HttpPost("{jobKey}/toggle")]
    public async Task<ActionResult<ApiResponse<string>>> ToggleAsync(string jobKey, [FromBody] PlatformScheduledJobToggleInput input)
    {
        await _schedulerService.ToggleAsync(jobKey, input.IsEnabled);
        return ApiResponse<string>.Ok("ok");
    }

    [HttpDelete("{jobKey}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteAsync(string jobKey)
    {
        await _schedulerService.DeleteAsync(jobKey);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpDelete("{jobKey}/records")]
    public async Task<ActionResult<ApiResponse<bool>>> ClearRecordsAsync(
        string jobKey,
        [FromQuery] int keepLastN = 0)
    {
        await _schedulerService.ClearRecordsAsync(jobKey, keepLastN);
        return ApiResponse<bool>.Ok(true);
    }
}
