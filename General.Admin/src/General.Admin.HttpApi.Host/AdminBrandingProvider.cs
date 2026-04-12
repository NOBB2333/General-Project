using Microsoft.Extensions.Localization;
using General.Admin.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace General.Admin;

[Dependency(ReplaceServices = true)]
public class AdminBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<AdminResource> _localizer;

    public AdminBrandingProvider(IStringLocalizer<AdminResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
