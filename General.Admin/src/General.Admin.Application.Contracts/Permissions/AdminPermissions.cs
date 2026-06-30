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
        Platform.WorkflowManage,
        Project.Create,
        Project.TaskManage,
        Project.WorkflowApprove,
        Business.BudgetSensitive,
        Business.ReportView,
        Business.ReportExport,
        Ai.DashboardView,
        Ai.ConversationUse,
        Ai.ProviderManage,
        Ai.AssistantManage,
        Ai.PromptManage,
        Ai.AgentManage,
        Ai.ArtifactManage,
        Ai.WebSearchManage,
        Ai.McpManage,
        Ai.MemoryManage,
        Ai.JobManage,
        Ai.JobRun,
        Ai.UsageView,
        Ai.WritebackView,
        Ai.WritebackApprove,
        Integration.DashboardView,
        Integration.ConnectorView,
        Integration.ConnectorManage,
        Integration.ConnectorRotateSecret,
        Integration.ConnectorTest,
        Integration.DeliveryView,
        Integration.DeliveryManage,
        Integration.DeliveryExport,
        Integration.BindingView,
        Integration.BindingManage,
        Integration.BindingImport,
        Integration.TemplateView,
        Integration.TemplateManage,
        Integration.TemplateTest,
        Integration.RuleView,
        Integration.RuleManage,
        Integration.SyncView,
        Integration.SyncManage,
        Integration.SyncRun,
        Integration.WebhookView,
        Integration.WebhookManage
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
        public const string WorkflowManage = "Platform.Workflow.Manage";
    }

    public static class Project
    {
        public const string Create = "Project.Project.Create";
        public const string TaskManage = "Project.Task.Manage";
        public const string WorkflowApprove = "Project.Workflow.Approve";
    }

    public static class Business
    {
        public const string BudgetSensitive = "Business.Budget.Sensitive";
        public const string ReportView = "Business.Report.View";
        public const string ReportExport = "Business.Report.Export";
    }

    public static class Ai
    {
        public const string DashboardView = "Ai.Dashboard.View";
        public const string ConversationUse = "Ai.Conversation.Use";
        public const string ProviderManage = "Ai.Provider.Manage";
        public const string AssistantManage = "Ai.Assistant.Manage";
        public const string PromptManage = "Ai.Prompt.Manage";
        public const string AgentManage = "Ai.Agent.Manage";
        public const string ArtifactManage = "Ai.Artifact.Manage";
        public const string WebSearchManage = "Ai.WebSearch.Manage";
        public const string McpManage = "Ai.Mcp.Manage";
        public const string MemoryManage = "Ai.Memory.Manage";
        public const string JobManage = "Ai.Job.Manage";
        public const string JobRun = "Ai.Job.Run";
        public const string UsageView = "Ai.Usage.View";
        public const string WritebackView = "Ai.Writeback.View";
        public const string WritebackApprove = "Ai.Writeback.Approve";
    }

    public static class Integration
    {
        public const string DashboardView = "Integration.Dashboard.View";
        public const string ConnectorView = "Integration.Connector.View";
        public const string ConnectorManage = "Integration.Connector.Manage";
        public const string ConnectorRotateSecret = "Integration.Connector.RotateSecret";
        public const string ConnectorTest = "Integration.Connector.Test";
        public const string DeliveryView = "Integration.Delivery.View";
        public const string DeliveryManage = "Integration.Delivery.Manage";
        public const string DeliveryExport = "Integration.Delivery.Export";
        public const string BindingView = "Integration.Binding.View";
        public const string BindingManage = "Integration.Binding.Manage";
        public const string BindingImport = "Integration.Binding.Import";
        public const string TemplateView = "Integration.Template.View";
        public const string TemplateManage = "Integration.Template.Manage";
        public const string TemplateTest = "Integration.Template.Test";
        public const string RuleView = "Integration.Rule.View";
        public const string RuleManage = "Integration.Rule.Manage";
        public const string SyncView = "Integration.Sync.View";
        public const string SyncManage = "Integration.Sync.Manage";
        public const string SyncRun = "Integration.Sync.Run";
        public const string WebhookView = "Integration.Webhook.View";
        public const string WebhookManage = "Integration.Webhook.Manage";
    }
}
