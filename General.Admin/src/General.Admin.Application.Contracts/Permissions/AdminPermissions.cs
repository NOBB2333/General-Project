namespace General.Admin.Permissions;

public static class AdminPermissions
{
    public const string GroupName = "Admin";

    public static readonly string[] All =
    [
        Platform.OrganizationManage,
        Platform.UserManage,
        Platform.RoleManage,
        Platform.TenantView,
        Platform.TenantManage,
        Platform.MenuManage,
        Platform.ConfigManage,
        Platform.CacheManage,
        Platform.DictManage,
        Platform.OpenApiManage,
        Platform.RecycleBinManage,
        Platform.FileManage,
        Platform.AuditLogView,
        Platform.SystemMonitorView,
        Platform.SchedulerView,
        Platform.SchedulerManage,
        Platform.SchedulerExecute,
        Project.Create,
        Project.TaskManage,
        Business.BudgetSensitive,
        Business.ReportView,
        Business.ReportExport
    ];

    public static class Platform
    {
        public const string OrganizationManage = "Platform.Organization.Manage";
        public const string UserManage = "Platform.User.Manage";
        public const string RoleManage = "Platform.Role.Manage";
        public const string TenantView = "Platform.Tenant.View";
        public const string TenantManage = "Platform.Tenant.Manage";
        public const string MenuManage = "Platform.Menu.Manage";
        public const string ConfigManage = "Platform.Config.Manage";
        public const string CacheManage = "Platform.Cache.Manage";
        public const string DictManage = "Platform.Dict.Manage";
        public const string OpenApiManage = "Platform.OpenApi.Manage";
        public const string RecycleBinManage = "Platform.RecycleBin.Manage";
        public const string FileManage = "Platform.File.Manage";
        public const string AuditLogView = "Platform.AuditLog.View";
        public const string SystemMonitorView = "Platform.SystemMonitor.View";
        public const string SchedulerView = "Platform.Scheduler.View";
        public const string SchedulerManage = "Platform.Scheduler.Manage";
        public const string SchedulerExecute = "Platform.Scheduler.Execute";
    }

    public static class Project
    {
        public const string Create = "Project.Project.Create";
        public const string TaskManage = "Project.Task.Manage";
    }

    public static class Business
    {
        public const string BudgetSensitive = "Business.Budget.Sensitive";
        public const string ReportView = "Business.Report.View";
        public const string ReportExport = "Business.Report.Export";
    }
}
