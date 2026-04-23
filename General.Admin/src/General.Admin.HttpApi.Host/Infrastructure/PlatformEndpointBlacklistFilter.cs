using System;
using System.Threading.Tasks;
using General.Admin.PhaseOne;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace General.Admin.Infrastructure;

public class PlatformEndpointBlacklistFilter : IAsyncActionFilter
{
    private readonly CurrentUserAuthorizationService _authorizationService;

    public PlatformEndpointBlacklistFilter(CurrentUserAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var endpointKey = context.ActionDescriptor.EndpointMetadata
            .OfType<PlatformEndpointAttribute>()
            .FirstOrDefault()
            ?.Key
            ?? $"{context.HttpContext.Request.Method}:{context.HttpContext.Request.Path.Value}".Trim();

        if (!string.IsNullOrWhiteSpace(endpointKey) && await _authorizationService.IsBlockedAsync(endpointKey))
        {
            context.Result = new ObjectResult(new ApiResponse<bool>
            {
                Code = -1,
                Data = false,
                Error = "Access to this endpoint is blocked by role or tenant policy.",
                Message = "Access to this endpoint is blocked by role or tenant policy."
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
            return;
        }

        await next();
    }
}
