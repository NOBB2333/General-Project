using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using General.Admin.Infrastructure;

namespace General.Admin.Logging;

public sealed class SwaggerGroupLogChannelMapper
{
    public string ResolveChannel(HttpContext httpContext)
    {
        return ResolveChannel(ResolveGroupName(httpContext));
    }

    public string ResolveChannel(string? groupName)
    {
        return groupName switch
        {
            ApiDocGroups.Platform => LoggingChannelConstants.Platform,
            ApiDocGroups.Project => LoggingChannelConstants.Project,
            ApiDocGroups.Business => LoggingChannelConstants.Business,
            _ => LoggingChannelConstants.Platform
        };
    }

    public string? ResolveGroupName(HttpContext httpContext)
    {
        var actionDescriptor = httpContext.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>();
        if (actionDescriptor == null)
        {
            return null;
        }

        var actionGroup = actionDescriptor.MethodInfo
            .GetCustomAttributes(inherit: true)
            .OfType<ApiExplorerSettingsAttribute>()
            .Select(x => x.GroupName)
            .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

        if (!string.IsNullOrWhiteSpace(actionGroup))
        {
            return actionGroup;
        }

        return actionDescriptor.ControllerTypeInfo
            .GetCustomAttributes(typeof(ApiExplorerSettingsAttribute), inherit: true)
            .OfType<ApiExplorerSettingsAttribute>()
            .Select(x => x.GroupName)
            .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
    }
}
