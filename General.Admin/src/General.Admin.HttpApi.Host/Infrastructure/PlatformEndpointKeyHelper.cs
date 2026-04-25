using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace General.Admin.Infrastructure;

public static class PlatformEndpointKeyHelper
{
    public static List<string> GetBlacklistKeys(ActionDescriptor actionDescriptor, HttpRequest request)
    {
        var keys = new List<string>();
        var actionKey = GetActionKey(actionDescriptor);
        if (!string.IsNullOrWhiteSpace(actionKey))
        {
            keys.Add(actionKey);
        }
        else
        {
            keys.Add($"{request.Method}:{request.Path.Value}".Trim());
        }

        var capabilityKey = GetCapabilityKey(actionDescriptor);
        if (!string.IsNullOrWhiteSpace(capabilityKey))
        {
            keys.Add(capabilityKey);
        }

        return keys
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static string? GetActionKey(ActionDescriptor actionDescriptor)
    {
        if (actionDescriptor is not ControllerActionDescriptor controllerAction)
        {
            return null;
        }

        var routeTemplate = controllerAction.AttributeRouteInfo?.Template?.Trim();
        if (string.IsNullOrWhiteSpace(routeTemplate))
        {
            return null;
        }

        var method = controllerAction.ActionConstraints
            ?.OfType<HttpMethodActionConstraint>()
            .FirstOrDefault()
            ?.HttpMethods
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(method))
        {
            return null;
        }

        return $"{method.ToUpperInvariant()}:{NormalizePath(routeTemplate)}";
    }

    public static string? GetCapabilityKey(ActionDescriptor actionDescriptor)
    {
        return actionDescriptor.EndpointMetadata
            .OfType<PlatformEndpointAttribute>()
            .FirstOrDefault()
            ?.Key;
    }

    private static string NormalizePath(string routeTemplate)
    {
        return "/" + routeTemplate.TrimStart('/');
    }
}
