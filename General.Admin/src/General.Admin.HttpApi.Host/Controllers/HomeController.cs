using General.Admin.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Volo.Abp.AspNetCore.Mvc;

namespace General.Admin.Controllers;

public class HomeController : AbpController
{
    private readonly IConfiguration _configuration;

    public HomeController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public ActionResult Index()
    {
        var options = _configuration
            .GetSection(PlatformApiDocumentationOptions.SectionName)
            .Get<PlatformApiDocumentationOptions>() ?? new PlatformApiDocumentationOptions();

        if (options.UseScalar())
        {
            return Redirect($"~/{options.ScalarRoutePrefix.Trim('/')}");
        }

        if (options.UseSwagger())
        {
            return Redirect($"~/{options.SwaggerRoutePrefix.Trim('/')}");
        }

        return Redirect("~/health/live");
    }
}
