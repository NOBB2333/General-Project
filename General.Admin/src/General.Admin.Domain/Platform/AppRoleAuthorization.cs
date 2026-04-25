using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.Platform;

public class AppRoleAuthorization : FullAuditedAggregateRoot<Guid>
{
    public string ApiBlacklist { get; private set; }

    public string AccountScopeMode { get; private set; }

    public string AllowedUserIds { get; private set; }

    public string CustomOrganizationUnitIds { get; private set; }

    public string DataScopeMode { get; private set; }

    public Guid RoleId { get; private set; }

    protected AppRoleAuthorization()
    {
        ApiBlacklist = "[]";
        AccountScopeMode = PlatformAuthorizationDefaults.AccountScopeDataAndUsers;
        AllowedUserIds = "[]";
        CustomOrganizationUnitIds = "[]";
        DataScopeMode = PlatformAuthorizationDefaults.DataScopeCurrentOrganizationAndDescendants;
    }

    public AppRoleAuthorization(
        Guid id,
        Guid roleId,
        string dataScopeMode,
        string accountScopeMode,
        string customOrganizationUnitIds,
        string allowedUserIds,
        string apiBlacklist) : base(id)
    {
        RoleId = roleId;
        DataScopeMode = NormalizeDataScopeMode(dataScopeMode);
        AccountScopeMode = NormalizeAccountScopeMode(accountScopeMode);
        CustomOrganizationUnitIds = NormalizeJson(customOrganizationUnitIds);
        AllowedUserIds = NormalizeJson(allowedUserIds);
        ApiBlacklist = NormalizeJson(apiBlacklist);
    }

    public void Update(
        string dataScopeMode,
        string accountScopeMode,
        string customOrganizationUnitIds,
        string allowedUserIds,
        string apiBlacklist)
    {
        DataScopeMode = NormalizeDataScopeMode(dataScopeMode);
        AccountScopeMode = NormalizeAccountScopeMode(accountScopeMode);
        CustomOrganizationUnitIds = NormalizeJson(customOrganizationUnitIds);
        AllowedUserIds = NormalizeJson(allowedUserIds);
        ApiBlacklist = NormalizeJson(apiBlacklist);
    }

    private static string NormalizeAccountScopeMode(string value)
    {
        var normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
        return normalized switch
        {
            PlatformAuthorizationDefaults.AccountScopeAll => normalized,
            PlatformAuthorizationDefaults.AccountScopeData => normalized,
            PlatformAuthorizationDefaults.AccountScopeOnlyUsers => normalized,
            PlatformAuthorizationDefaults.AccountScopeDataAndUsers => normalized,
            _ => PlatformAuthorizationDefaults.AccountScopeDataAndUsers
        };
    }

    private static string NormalizeDataScopeMode(string value)
    {
        var normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
        return normalized switch
        {
            PlatformAuthorizationDefaults.DataScopeAll => normalized,
            PlatformAuthorizationDefaults.DataScopeCurrentOrganization => normalized,
            PlatformAuthorizationDefaults.DataScopeCurrentOrganizationAndDescendants => normalized,
            PlatformAuthorizationDefaults.DataScopeCustom => normalized,
            PlatformAuthorizationDefaults.DataScopeSelf => normalized,
            _ => PlatformAuthorizationDefaults.DataScopeCurrentOrganizationAndDescendants
        };
    }

    private static string NormalizeJson(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "[]" : value.Trim();
    }
}
