using System;
using System.Threading.Tasks;
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
        var endpointKeys = PlatformEndpointKeyHelper.GetBlacklistKeys(
            context.ActionDescriptor,
            context.HttpContext.Request);

        if (endpointKeys.Count > 0 && await _authorizationService.IsBlockedAsync(endpointKeys))
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
