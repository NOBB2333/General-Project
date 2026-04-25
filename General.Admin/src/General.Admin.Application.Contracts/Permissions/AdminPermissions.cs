namespace General.Admin.Permissions;

public static class AdminPermissions
{
    public const string GroupName = "Admin";

    public static readonly string[] All =
    [
        Platform.OrganizationManage,
        Platform.UserManage,
        Platform.RoleManage,
        Platform.TenantManage,
        Platform.MenuManage,
        Platform.FileManage,
        Project.Create,
        Project.TaskManage,
        Business.BudgetSensitive
    ];

    public static class Platform
    {
        public const string OrganizationManage = "Platform.Organization.Manage";
        public const string UserManage = "Platform.User.Manage";
        public const string RoleManage = "Platform.Role.Manage";
        public const string TenantManage = "Platform.Tenant.Manage";
        public const string MenuManage = "Platform.Menu.Manage";
        public const string FileManage = "Platform.File.Manage";
    }

    public static class Project
    {
        public const string Create = "Project.Project.Create";
        public const string TaskManage = "Project.Task.Manage";
    }

    public static class Business
    {
        public const string BudgetSensitive = "Business.Budget.Sensitive";
    }
}
