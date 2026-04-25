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
        adminGroup.AddPermission(AdminPermissions.Platform.TenantManage, L("Permission:Platform.Tenant.Manage"));
        adminGroup.AddPermission(AdminPermissions.Platform.MenuManage, L("Permission:Platform.Menu.Manage"));
        adminGroup.AddPermission(AdminPermissions.Platform.FileManage, L("Permission:Platform.File.Manage"));
        adminGroup.AddPermission(AdminPermissions.Project.Create, L("Permission:Project.Project.Create"));
        adminGroup.AddPermission(AdminPermissions.Project.TaskManage, L("Permission:Project.Task.Manage"));
        adminGroup.AddPermission(AdminPermissions.Business.BudgetSensitive, L("Permission:Business.Budget.Sensitive"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<AdminResource>(name);
    }
}
