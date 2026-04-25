using Volo.Abp.Domain.Entities;

namespace General.Admin.Platform;

public class AppRoleMenu : Entity<Guid>
{
    public Guid RoleId { get; private set; }

    public Guid MenuId { get; private set; }

    protected AppRoleMenu()
    {
    }

    public AppRoleMenu(Guid id, Guid roleId, Guid menuId) : base(id)
    {
        RoleId = roleId;
        MenuId = menuId;
    }
}
