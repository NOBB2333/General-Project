using General.Admin.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace General.Admin.Controllers;

[ApiController]
[Authorize]
[ApiExplorerSettings(GroupName = ApiDocGroups.Platform)]
[Route("api/app/platform/notification")]
public class NotificationController : ControllerBase
{
    private readonly PlatformNotificationService _notificationService;

    public NotificationController(PlatformNotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet("list")]
    public async Task<ActionResult<ApiResponse<List<PlatformNotificationDto>>>> GetListAsync(
        [FromQuery] PlatformNotificationListInput input)
    {
        return ApiResponse<List<PlatformNotificationDto>>.Ok(await _notificationService.GetListAsync(input));
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<ApiResponse<PlatformNotificationUnreadCountDto>>> GetUnreadCountAsync()
    {
        return ApiResponse<PlatformNotificationUnreadCountDto>.Ok(await _notificationService.GetUnreadCountAsync());
    }

    [HttpPost("{id:guid}/read")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkReadAsync(Guid id)
    {
        await _notificationService.MarkReadAsync(id);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpPost("read-all")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkAllReadAsync()
    {
        await _notificationService.MarkAllReadAsync();
        return ApiResponse<bool>.Ok(true);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveAsync(Guid id)
    {
        await _notificationService.RemoveAsync(id);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpDelete("clear")]
    public async Task<ActionResult<ApiResponse<bool>>> ClearAsync()
    {
        await _notificationService.ClearAsync();
        return ApiResponse<bool>.Ok(true);
    }

    [Authorize(Roles = PlatformRoleNames.Admin)]
    [HttpPost("send")]
    public async Task<ActionResult<ApiResponse<Guid?>>> SendAsync([FromBody] PlatformNotificationSendInput input)
    {
        return ApiResponse<Guid?>.Ok(await _notificationService.SendAsync(input));
    }
}
