using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace General.Admin.Logging;

public sealed class LogChannelMiddleware : IMiddleware
{
    private readonly ILogger<LogChannelMiddleware> _logger;
    private readonly SwaggerGroupLogChannelMapper _channelMapper;

    public LogChannelMiddleware(
        ILogger<LogChannelMiddleware> logger,
        SwaggerGroupLogChannelMapper channelMapper)
    {
        _logger = logger;
        _channelMapper = channelMapper;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var channel = _channelMapper.ResolveChannel(context);
        var apiGroup = _channelMapper.ResolveGroupName(context) ?? channel;
        var routePattern = (context.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText ?? context.Request.Path.Value ?? "/";

        using var scope = _logger.BeginScope(new Dictionary<string, object?>
        {
            [LoggingChannelConstants.PropertyName] = channel,
            [LoggingChannelConstants.SourcePropertyName] = "api",
            [LoggingChannelConstants.ApiGroupPropertyName] = apiGroup,
            [LoggingChannelConstants.RoutePatternPropertyName] = routePattern,
            [LoggingChannelConstants.TraceIdPropertyName] = context.TraceIdentifier
        });

        await next(context);
    }
}
