using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Security.Claims;

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
            var isHostTenantOperation = string.Equals(
                context.User.FindFirst(PlatformTenantOperationClaimTypes.HostTenantOperation)?.Value,
                "true",
                StringComparison.OrdinalIgnoreCase);
            var hostOperatorUserIdText = context.User.FindFirst(PlatformTenantOperationClaimTypes.HostOperatorUserId)?.Value;
            var hostOperatorUserId = Guid.TryParse(hostOperatorUserIdText, out var parsedHostOperatorUserId)
                ? parsedHostOperatorUserId
                : (Guid?)null;
            var operationTenantIdText = context.User.FindFirst(PlatformTenantOperationClaimTypes.OperationTenantId)?.Value;
            var operationTenantId = Guid.TryParse(operationTenantIdText, out var parsedOperationTenantId)
                ? parsedOperationTenantId
                : (Guid?)null;
            var impersonatedUserIdText = context.User.FindFirst(PlatformTenantOperationClaimTypes.ImpersonatedUserId)?.Value;
            var impersonatedUserId = Guid.TryParse(impersonatedUserIdText, out var parsedImpersonatedUserId)
                ? parsedImpersonatedUserId
                : (Guid?)null;

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
                HostOperatorUserId = hostOperatorUserId,
                HostOperatorUserName = context.User.FindFirst(PlatformTenantOperationClaimTypes.HostOperatorUserName)?.Value,
                Id = Guid.NewGuid(),
                ImpersonatedUserId = impersonatedUserId,
                ImpersonatedUserName = context.User.FindFirst(PlatformTenantOperationClaimTypes.ImpersonatedUserName)?.Value,
                IsHostTenantOperation = isHostTenantOperation,
                OperationSessionId = context.User.FindFirst(PlatformTenantOperationClaimTypes.OperationSessionId)?.Value,
                OperationTenantId = operationTenantId,
                TenantName = context.User.FindFirst(PlatformTenantOperationClaimTypes.OperationTenantName)?.Value
                    ?? context.User.FindFirst("tenant_name")?.Value
                    ?? context.User.FindFirst(AbpClaimTypes.TenantId)?.Value,
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
