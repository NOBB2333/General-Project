namespace General.Admin.Integration;

public static class IntegrationBindingStatus
{
    public const string Bound = "Bound";
    public const string Conflict = "Conflict";
    public const string Disabled = "Disabled";
    public const string Pending = "Pending";
}

public static class IntegrationBindingMatchTypes
{
    public const string Email = "Email";
    public const string Manual = "Manual";
    public const string Mobile = "Mobile";
    public const string MobileAndEmail = "MobileAndEmail";
    public const string SelfService = "SelfService";
    public const string Sync = "Sync";
}
