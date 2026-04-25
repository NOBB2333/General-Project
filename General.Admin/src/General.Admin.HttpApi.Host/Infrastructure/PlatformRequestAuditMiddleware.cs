using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Volo.Abp.DependencyInjection;

namespace General.Admin.Infrastructure;

public class PlatformRequestAuditMiddleware : IMiddleware, ITransientDependency
{
    private readonly PlatformRequestAuditStore _requestAuditStore;

    public PlatformRequestAuditMiddleware(PlatformRequestAuditStore requestAuditStore)
    {
        _requestAuditStore = requestAuditStore;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (!context.Request.Path.StartsWithSegments("/api/app"))
        {
            await next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        Exception? exception = null;

        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            exception = ex;
            throw;
        }
        finally
        {
            stopwatch.Stop();

            await _requestAuditStore.AppendAsync(new PlatformRequestAuditEntry
            {
                ActionSummary = ResolveActionSummary(context),
                BrowserInfo = context.Request.Headers.UserAgent.ToString(),
                ClientIpAddress = context.Connection.RemoteIpAddress?.ToString(),
                ExecutionDuration = (int)stopwatch.ElapsedMilliseconds,
                ExecutionTime = DateTime.UtcNow,
                ExceptionMessage = exception?.Message,
                HasException = exception != null || context.Response.StatusCode >= StatusCodes.Status500InternalServerError,
                HttpMethod = context.Request.Method,
                HttpStatusCode = context.Response.StatusCode,
                Id = Guid.NewGuid(),
                TenantName = context.User.FindFirst("tenant_name")?.Value,
                Url = $"{context.Request.Path}{context.Request.QueryString}",
                UserName = context.User.Identity?.IsAuthenticated == true
                    ? context.User.Identity?.Name
                    : "anonymous"
            });
        }
    }

    private static string? ResolveActionSummary(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        return endpoint?.DisplayName ?? $"{context.Request.Method} {context.Request.Path}";
    }
}
