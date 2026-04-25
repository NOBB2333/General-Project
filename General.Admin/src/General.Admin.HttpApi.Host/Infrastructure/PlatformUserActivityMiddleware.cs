using Microsoft.AspNetCore.Http;

namespace General.Admin.Infrastructure;

public class PlatformUserActivityMiddleware : IMiddleware
{
    private readonly PlatformUserActivityService _userActivityService;

    public PlatformUserActivityMiddleware(PlatformUserActivityService userActivityService)
    {
        _userActivityService = userActivityService;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.User.Identity?.IsAuthenticated == true &&
            context.Request.Path.StartsWithSegments("/api/app"))
        {
            if (await _userActivityService.IsCurrentUserForcedLogoutAsync())
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
        }

        await next(context);

        if (context.Response.StatusCode >= StatusCodes.Status400BadRequest)
        {
            return;
        }

        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            return;
        }

        if (!context.Request.Path.StartsWithSegments("/api/app"))
        {
            return;
        }

        await _userActivityService.TouchCurrentUserAsync();
    }
}
