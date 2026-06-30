using General.Admin.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace General.Admin.Permissions;

public class AdminPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var adminGroup = context.AddGroup(AdminPermissions.GroupName);

        adminGroup.AddPermission(AdminPermissions.Platform.OrganizationManage, L("Permission:Platform.Organization.Manage"));
        adminGroup.AddPermission(AdminPermissions.Platform.UserManage, L("Permission:Platform.User.Manage"));
        adminGroup.AddPermission(AdminPermissions.Platform.RoleManage, L("Permission:Platform.Role.Manage"));
        adminGroup.AddPermission(AdminPermissions.Platform.TenantView, L("Permission:Platform.Tenant.View"));
        adminGroup.AddPermission(AdminPermissions.Platform.TenantManage, L("Permission:Platform.Tenant.Manage"));
        adminGroup.AddPermission(AdminPermissions.Platform.MenuManage, L("Permission:Platform.Menu.Manage"));
        adminGroup.AddPermission(AdminPermissions.Platform.ConfigManage, L("Permission:Platform.Config.Manage"));
        adminGroup.AddPermission(AdminPermissions.Platform.CacheManage, L("Permission:Platform.Cache.Manage"));
        adminGroup.AddPermission(AdminPermissions.Platform.DictManage, L("Permission:Platform.Dict.Manage"));
        adminGroup.AddPermission(AdminPermissions.Platform.OpenApiManage, L("Permission:Platform.OpenApi.Manage"));
        adminGroup.AddPermission(AdminPermissions.Platform.RecycleBinManage, L("Permission:Platform.RecycleBin.Manage"));
        adminGroup.AddPermission(AdminPermissions.Platform.FileManage, L("Permission:Platform.File.Manage"));
        adminGroup.AddPermission(AdminPermissions.Platform.AuditLogView, L("Permission:Platform.AuditLog.View"));
        adminGroup.AddPermission(AdminPermissions.Platform.SystemMonitorView, L("Permission:Platform.SystemMonitor.View"));
        adminGroup.AddPermission(AdminPermissions.Platform.SchedulerView, L("Permission:Platform.Scheduler.View"));
        adminGroup.AddPermission(AdminPermissions.Platform.SchedulerManage, L("Permission:Platform.Scheduler.Manage"));
        adminGroup.AddPermission(AdminPermissions.Platform.SchedulerExecute, L("Permission:Platform.Scheduler.Execute"));
        adminGroup.AddPermission(AdminPermissions.Platform.WorkflowManage, L("Permission:Platform.Workflow.Manage"));
        adminGroup.AddPermission(AdminPermissions.Project.Create, L("Permission:Project.Project.Create"));
        adminGroup.AddPermission(AdminPermissions.Project.TaskManage, L("Permission:Project.Task.Manage"));
        adminGroup.AddPermission(AdminPermissions.Project.WorkflowApprove, L("Permission:Project.Workflow.Approve"));
        adminGroup.AddPermission(AdminPermissions.Business.BudgetSensitive, L("Permission:Business.Budget.Sensitive"));
        adminGroup.AddPermission(AdminPermissions.Business.ReportView, L("Permission:Business.Report.View"));
        adminGroup.AddPermission(AdminPermissions.Business.ReportExport, L("Permission:Business.Report.Export"));
        adminGroup.AddPermission(AdminPermissions.Ai.DashboardView, L("Permission:Ai.Dashboard.View"));
        adminGroup.AddPermission(AdminPermissions.Ai.ConversationUse, L("Permission:Ai.Conversation.Use"));
        adminGroup.AddPermission(AdminPermissions.Ai.ProviderManage, L("Permission:Ai.Provider.Manage"));
        adminGroup.AddPermission(AdminPermissions.Ai.AssistantManage, L("Permission:Ai.Assistant.Manage"));
        adminGroup.AddPermission(AdminPermissions.Ai.PromptManage, L("Permission:Ai.Prompt.Manage"));
        adminGroup.AddPermission(AdminPermissions.Ai.AgentManage, L("Permission:Ai.Agent.Manage"));
        adminGroup.AddPermission(AdminPermissions.Ai.ArtifactManage, L("Permission:Ai.Artifact.Manage"));
        adminGroup.AddPermission(AdminPermissions.Ai.WebSearchManage, L("Permission:Ai.WebSearch.Manage"));
        adminGroup.AddPermission(AdminPermissions.Ai.McpManage, L("Permission:Ai.Mcp.Manage"));
        adminGroup.AddPermission(AdminPermissions.Ai.MemoryManage, L("Permission:Ai.Memory.Manage"));
        adminGroup.AddPermission(AdminPermissions.Ai.JobManage, L("Permission:Ai.Job.Manage"));
        adminGroup.AddPermission(AdminPermissions.Ai.JobRun, L("Permission:Ai.Job.Run"));
        adminGroup.AddPermission(AdminPermissions.Ai.UsageView, L("Permission:Ai.Usage.View"));
        adminGroup.AddPermission(AdminPermissions.Ai.WritebackView, L("Permission:Ai.Writeback.View"));
        adminGroup.AddPermission(AdminPermissions.Ai.WritebackApprove, L("Permission:Ai.Writeback.Approve"));
        adminGroup.AddPermission(AdminPermissions.Integration.DashboardView, L("Permission:Integration.Dashboard.View"));
        adminGroup.AddPermission(AdminPermissions.Integration.ConnectorView, L("Permission:Integration.Connector.View"));
        adminGroup.AddPermission(AdminPermissions.Integration.ConnectorManage, L("Permission:Integration.Connector.Manage"));
        adminGroup.AddPermission(AdminPermissions.Integration.ConnectorRotateSecret, L("Permission:Integration.Connector.RotateSecret"));
        adminGroup.AddPermission(AdminPermissions.Integration.ConnectorTest, L("Permission:Integration.Connector.Test"));
        adminGroup.AddPermission(AdminPermissions.Integration.DeliveryView, L("Permission:Integration.Delivery.View"));
        adminGroup.AddPermission(AdminPermissions.Integration.DeliveryManage, L("Permission:Integration.Delivery.Manage"));
        adminGroup.AddPermission(AdminPermissions.Integration.DeliveryExport, L("Permission:Integration.Delivery.Export"));
        adminGroup.AddPermission(AdminPermissions.Integration.BindingView, L("Permission:Integration.Binding.View"));
        adminGroup.AddPermission(AdminPermissions.Integration.BindingManage, L("Permission:Integration.Binding.Manage"));
        adminGroup.AddPermission(AdminPermissions.Integration.BindingImport, L("Permission:Integration.Binding.Import"));
        adminGroup.AddPermission(AdminPermissions.Integration.TemplateView, L("Permission:Integration.Template.View"));
        adminGroup.AddPermission(AdminPermissions.Integration.TemplateManage, L("Permission:Integration.Template.Manage"));
        adminGroup.AddPermission(AdminPermissions.Integration.TemplateTest, L("Permission:Integration.Template.Test"));
        adminGroup.AddPermission(AdminPermissions.Integration.RuleView, L("Permission:Integration.Rule.View"));
        adminGroup.AddPermission(AdminPermissions.Integration.RuleManage, L("Permission:Integration.Rule.Manage"));
        adminGroup.AddPermission(AdminPermissions.Integration.SyncView, L("Permission:Integration.Sync.View"));
        adminGroup.AddPermission(AdminPermissions.Integration.SyncManage, L("Permission:Integration.Sync.Manage"));
        adminGroup.AddPermission(AdminPermissions.Integration.SyncRun, L("Permission:Integration.Sync.Run"));
        adminGroup.AddPermission(AdminPermissions.Integration.WebhookView, L("Permission:Integration.Webhook.View"));
        adminGroup.AddPermission(AdminPermissions.Integration.WebhookManage, L("Permission:Integration.Webhook.Manage"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<AdminResource>(name);
    }
}
