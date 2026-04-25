using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;
using General.Admin.EntityFrameworkCore;
using General.Admin.Infrastructure;
using General.Admin.Logging;
using General.Admin.MultiTenancy;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite.Bundling;
using Microsoft.OpenApi;
using Volo.Abp;
using Volo.Abp.Account;
using Volo.Abp.Account.Web;
using Volo.Abp.AspNetCore.Auditing;
using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.Libs;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.Security.Claims;
using Volo.Abp.Swashbuckle;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.VirtualFileSystem;
using Volo.Abp.Auditing;

namespace General.Admin;

[DependsOn(
    typeof(AdminHttpApiModule),
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreMultiTenancyModule),
    typeof(AdminApplicationModule),
    typeof(AdminEntityFrameworkCoreModule),
    typeof(AbpAspNetCoreMvcUiLeptonXLiteThemeModule),
    typeof(AbpAccountWebOpenIddictModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpSwashbuckleModule)
)]
public class AdminHttpApiHostModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        context.Services.AddHttpContextAccessor();
        ConfigureAuditing();
        ConfigureAuthentication(context);
        ConfigureMvcLibs();
        ConfigureBundles();
        ConfigureUrls(configuration);
        ConfigureConventionalControllers();
        ConfigureVirtualFileSystem(context);
        ConfigureCors(context, configuration);
        ConfigureSwaggerServices(context, configuration);
        ConfigureLoggingServices(context);
    }

    private void ConfigureAuditing()
    {
        Configure<AbpAspNetCoreAuditingOptions>(options =>
        {
            options.IgnoredUrls.Add("/api/health");
            options.IgnoredUrls.Add("/api/app");
        });

        Configure<AbpAuditingOptions>(options =>
        {
            options.IsEnabledForGetRequests = false;
            options.HideErrors = true;
        });
    }

    private void ConfigureMvcLibs()
    {
        Configure<AbpMvcLibsOptions>(options =>
        {
            options.CheckLibs = false;
        });

        Configure<MvcOptions>(options =>
        {
            options.Filters.AddService<PlatformEndpointBlacklistFilter>();
        });
    }

    private void ConfigureAuthentication(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecurityKey));

        context.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidIssuer = jwtOptions.Issuer,
                    NameClaimType = AbpClaimTypes.UserName,
                    RoleClaimType = AbpClaimTypes.Role,
                    IssuerSigningKey = signingKey,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        context.Services.Configure<AbpClaimsPrincipalFactoryOptions>(options =>
        {
            options.IsDynamicClaimsEnabled = true;
        });
    }

    private void ConfigureBundles()
    {
        Configure<AbpBundlingOptions>(options =>
        {
            options.StyleBundles.Configure(
                LeptonXLiteThemeBundles.Styles.Global,
                bundle =>
                {
                    bundle.AddFiles("/global-styles.css");
                }
            );
        });
    }

    private void ConfigureUrls(IConfiguration configuration)
    {
        Configure<AppUrlOptions>(options =>
        {
            options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"];
            options.RedirectAllowedUrls.AddRange(configuration["App:RedirectAllowedUrls"]?.Split(',') ?? Array.Empty<string>());

            options.Applications["Angular"].RootUrl = configuration["App:ClientUrl"];
            options.Applications["Angular"].Urls[AccountUrlNames.PasswordReset] = "account/reset-password";
        });
    }

    private void ConfigureVirtualFileSystem(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        if (hostingEnvironment.IsDevelopment())
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.ReplaceEmbeddedByPhysical<AdminDomainSharedModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}General.Admin.Domain.Shared"));
                options.FileSets.ReplaceEmbeddedByPhysical<AdminDomainModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}General.Admin.Domain"));
                options.FileSets.ReplaceEmbeddedByPhysical<AdminApplicationContractsModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}General.Admin.Application.Contracts"));
                options.FileSets.ReplaceEmbeddedByPhysical<AdminApplicationModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}General.Admin.Application"));
            });
        }
    }

    private void ConfigureConventionalControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(AdminApplicationModule).Assembly);
        });
    }

    private static void ConfigureSwaggerServices(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddScoped<PlatformEndpointBlacklistFilter>();
        context.Services.AddTransient<PlatformRequestAuditMiddleware>();
        context.Services.AddTransient<PlatformUserActivityMiddleware>();
        context.Services.AddHostedService<PlatformRequestAuditFlushBackgroundService>();
        context.Services.AddHostedService<PlatformSchedulerBackgroundService>();
        context.Services.AddAbpSwaggerGenWithOAuth(
            configuration["AuthServer:Authority"]!,
            new Dictionary<string, string>
            {
                    {"Admin", "Admin API"}
            },
            options =>
            {
                options.SwaggerDoc(ApiDocGroups.Platform, new OpenApiInfo { Title = "Admin API - Platform", Version = "v1" });
                options.SwaggerDoc(ApiDocGroups.Project, new OpenApiInfo { Title = "Admin API - Project", Version = "v1" });
                options.SwaggerDoc(ApiDocGroups.Business, new OpenApiInfo { Title = "Admin API - Business", Version = "v1" });
                options.DocInclusionPredicate((docName, description) =>
                {
                    var groupName = description.GroupName;
                    if (string.IsNullOrWhiteSpace(groupName))
                    {
                        groupName = ApiDocGroups.Platform;
                    }

                    return string.Equals(docName, groupName, StringComparison.OrdinalIgnoreCase);
                });
                options.CustomSchemaIds(type => type.FullName);
            });
    }

    private static void ConfigureLoggingServices(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<SwaggerGroupLogChannelMapper>();
        context.Services.AddTransient<LogChannelMiddleware>();
    }

    private void ConfigureCors(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .WithOrigins(configuration["App:CorsOrigins"]?
                        .Split(",", StringSplitOptions.RemoveEmptyEntries)
                        .Select(o => o.RemovePostFix("/"))
                        .ToArray() ?? Array.Empty<string>())
                    .WithAbpExposedHeaders()
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseAbpRequestLocalization();

        if (!env.IsDevelopment())
        {
            app.UseErrorPage();
        }

        app.UseCorrelationId();
        app.MapAbpStaticAssets();
        app.UseRouting();
        app.UseMiddleware<LogChannelMiddleware>();
        app.UseCors();
        app.UseAuthentication();

        if (MultiTenancyConsts.IsEnabled)
        {
            app.UseMultiTenancy();
        }
        app.UseUnitOfWork();
        app.UseDynamicClaims();
        app.UseAuthorization();
        app.UseMiddleware<PlatformRequestAuditMiddleware>();
        app.UseMiddleware<PlatformUserActivityMiddleware>();

        app.UseSwagger();
        app.UseAbpSwaggerUI(c =>
        {
            c.SwaggerEndpoint($"/swagger/{ApiDocGroups.Platform}/swagger.json", "Admin API - Platform");
            c.SwaggerEndpoint($"/swagger/{ApiDocGroups.Project}/swagger.json", "Admin API - Project");
            c.SwaggerEndpoint($"/swagger/{ApiDocGroups.Business}/swagger.json", "Admin API - Business");

            var configuration = context.ServiceProvider.GetRequiredService<IConfiguration>();
            c.OAuthClientId(configuration["AuthServer:SwaggerClientId"]);
            c.OAuthScopes("Admin");
        });

        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();
    }
}
