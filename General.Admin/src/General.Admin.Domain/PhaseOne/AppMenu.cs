using General.Admin.PhaseOne;
using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.PhaseOne;

public class AppMenu : FullAuditedAggregateRoot<Guid>
{
    public string AppCode { get; private set; }

    public Guid? ParentId { get; private set; }

    public string Name { get; private set; }

    public string Path { get; private set; }

    public string? Component { get; private set; }

    public string? Redirect { get; private set; }

    public PhaseOneMenuType Type { get; private set; }

    public string Title { get; private set; }

    public string? Icon { get; private set; }

    public string? PermissionCode { get; private set; }

    public string? Link { get; private set; }

    public bool AffixTab { get; private set; }

    public bool KeepAlive { get; private set; }

    public bool HideInBreadcrumb { get; private set; }

    public bool HideInMenu { get; private set; }

    public bool HideInTab { get; private set; }

    public bool MenuVisibleWithForbidden { get; private set; }

    public int Order { get; private set; }

    public bool IsEnabled { get; private set; }

    protected AppMenu()
    {
        AppCode = string.Empty;
        Name = string.Empty;
        Path = string.Empty;
        Title = string.Empty;
    }

    public AppMenu(
        Guid id,
        string appCode,
        Guid? parentId,
        string name,
        string path,
        string? component,
        string? redirect,
        PhaseOneMenuType type,
        string title,
        string? icon,
        string? permissionCode,
        string? link,
        bool affixTab,
        bool keepAlive,
        bool hideInBreadcrumb,
        bool hideInMenu,
        bool hideInTab,
        bool menuVisibleWithForbidden,
        int order,
        bool isEnabled = true) : base(id)
    {
        AppCode = NormalizeAppCode(appCode);
        ParentId = parentId;
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 64);
        Path = Check.NotNullOrWhiteSpace(path, nameof(path), 256);
        Component = component;
        Redirect = redirect;
        Type = type;
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), 128);
        Icon = icon;
        PermissionCode = permissionCode;
        Link = link;
        AffixTab = affixTab;
        KeepAlive = keepAlive;
        HideInBreadcrumb = hideInBreadcrumb;
        HideInMenu = hideInMenu;
        HideInTab = hideInTab;
        MenuVisibleWithForbidden = menuVisibleWithForbidden;
        Order = order;
        IsEnabled = isEnabled;
    }

    public void SetAppCode(string appCode)
    {
        AppCode = NormalizeAppCode(appCode);
    }

    public void Update(
        string appCode,
        Guid? parentId,
        string name,
        string path,
        string? component,
        string? redirect,
        PhaseOneMenuType type,
        string title,
        string? icon,
        string? permissionCode,
        string? link,
        bool affixTab,
        bool keepAlive,
        bool hideInBreadcrumb,
        bool hideInMenu,
        bool hideInTab,
        bool menuVisibleWithForbidden,
        int order,
        bool isEnabled)
    {
        AppCode = NormalizeAppCode(appCode);
        ParentId = parentId;
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 64);
        Path = Check.NotNullOrWhiteSpace(path, nameof(path), 256);
        Component = component;
        Redirect = redirect;
        Type = type;
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), 128);
        Icon = icon;
        PermissionCode = permissionCode;
        Link = link;
        AffixTab = affixTab;
        KeepAlive = keepAlive;
        HideInBreadcrumb = hideInBreadcrumb;
        HideInMenu = hideInMenu;
        HideInTab = hideInTab;
        MenuVisibleWithForbidden = menuVisibleWithForbidden;
        Order = order;
        IsEnabled = isEnabled;
    }

    private static string NormalizeAppCode(string appCode)
    {
        return Check.NotNullOrWhiteSpace(appCode, nameof(appCode), 32)
            .Trim()
            .ToLowerInvariant();
    }
}
