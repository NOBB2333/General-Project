namespace General.Admin.Platform;

public static class PlatformAuthorizationDefaults
{
    public const string AccountScopeAll = "all";
    public const string AccountScopeData = "data";
    public const string AccountScopeDataAndUsers = "data_and_users";
    public const string AccountScopeOnlyUsers = "only_users";

    public const string DataScopeAll = "all";
    public const string DataScopeCurrentOrganization = "current_org";
    public const string DataScopeCurrentOrganizationAndDescendants = "current_org_and_descendants";
    public const string DataScopeCustom = "custom";
    public const string DataScopeSelf = "self";
}
