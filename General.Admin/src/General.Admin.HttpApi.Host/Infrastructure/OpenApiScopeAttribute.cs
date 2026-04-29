namespace General.Admin.Infrastructure;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class OpenApiScopeAttribute : Attribute
{
    public OpenApiScopeAttribute(string scope)
    {
        Scope = scope;
    }

    public string Scope { get; }
}
