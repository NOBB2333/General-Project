using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.PhaseOne;

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
        AccountScopeMode = PhaseOneAuthorizationDefaults.AccountScopeDataAndUsers;
        AllowedUserIds = "[]";
        CustomOrganizationUnitIds = "[]";
        DataScopeMode = PhaseOneAuthorizationDefaults.DataScopeCurrentOrganizationAndDescendants;
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
            PhaseOneAuthorizationDefaults.AccountScopeAll => normalized,
            PhaseOneAuthorizationDefaults.AccountScopeData => normalized,
            PhaseOneAuthorizationDefaults.AccountScopeOnlyUsers => normalized,
            PhaseOneAuthorizationDefaults.AccountScopeDataAndUsers => normalized,
            _ => PhaseOneAuthorizationDefaults.AccountScopeDataAndUsers
        };
    }

    private static string NormalizeDataScopeMode(string value)
    {
        var normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
        return normalized switch
        {
            PhaseOneAuthorizationDefaults.DataScopeAll => normalized,
            PhaseOneAuthorizationDefaults.DataScopeCurrentOrganization => normalized,
            PhaseOneAuthorizationDefaults.DataScopeCurrentOrganizationAndDescendants => normalized,
            PhaseOneAuthorizationDefaults.DataScopeCustom => normalized,
            PhaseOneAuthorizationDefaults.DataScopeSelf => normalized,
            _ => PhaseOneAuthorizationDefaults.DataScopeCurrentOrganizationAndDescendants
        };
    }

    private static string NormalizeJson(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "[]" : value.Trim();
    }
}
