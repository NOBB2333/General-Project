using System;

namespace General.Admin.Infrastructure;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class PlatformEndpointAttribute : Attribute
{
    public PlatformEndpointAttribute(string key)
    {
        Key = key?.Trim() ?? string.Empty;
    }

    public string Key { get; }
}
