using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;
using General.Admin.EntityFrameworkCore;
using General.Admin.Infrastructure;
using General.Admin.Logging;
using General.Admin.MultiTenancy;
using Scalar.AspNetCore;
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
        ConfigureRateLimiting(context, configuration);
        Configure<PlatformApiDocumentationOptions>(configuration.GetSection(PlatformApiDocumentationOptions.SectionName));
        ConfigureSwaggerServices(context, configuration);
        ConfigureLoggingServices(context);
    }

    private void ConfigureAuditing()
    {
        Configure<AbpAspNetCoreAuditingOptions>(options =>
        {
            options.IgnoredUrls.Add("/health");
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
        Configure<PlatformFileStorageOptions>(configuration.GetSection(PlatformFileStorageOptions.SectionName));

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
        context.Services.AddScoped<PlatformHealthProbeService>();
        context.Services.AddTransient<PlatformOpenApiSignatureMiddleware>();
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
                options.SwaggerDoc(ApiDocGroups.Platform, new OpenApiInfo { Title = "平台 API", Version = "v1" });
                options.SwaggerDoc(ApiDocGroups.Project, new OpenApiInfo { Title = "项目 API", Version = "v1" });
                options.SwaggerDoc(ApiDocGroups.Business, new OpenApiInfo { Title = "业务 API", Version = "v1" });
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

    private static void ConfigureRateLimiting(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.Configure<PlatformRateLimitOptions>(configuration.GetSection(PlatformRateLimitOptions.SectionName));
        var rateLimitOptions = configuration.GetSection(PlatformRateLimitOptions.SectionName).Get<PlatformRateLimitOptions>() ?? new PlatformRateLimitOptions();

        context.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var requestPath = httpContext.Request.Path;
                if (requestPath.StartsWithSegments("/health"))
                {
                    return RateLimitPartition.GetNoLimiter("health");
                }

                if (requestPath.StartsWithSegments("/api/open"))
                {
                    return BuildPartition("open-api", rateLimitOptions.OpenApi, ResolveClientIp(httpContext, rateLimitOptions));
                }

                if (requestPath.StartsWithSegments("/api/app/auth"))
                {
                    return BuildPartition("login", rateLimitOptions.Login, ResolveClientIp(httpContext, rateLimitOptions));
                }

                if (requestPath.StartsWithSegments("/api/app/file/upload"))
                {
                    return BuildPartition("file-upload", rateLimitOptions.FileUpload, ResolveUserPartitionKey(httpContext, rateLimitOptions));
                }

                return BuildPartition("general", rateLimitOptions.General, ResolveUserPartitionKey(httpContext, rateLimitOptions));
            });
        });
    }

    private static RateLimitPartition<string> BuildPartition(
        string category,
        RateLimitRule rule,
        string partitionKey)
    {
        return RateLimitPartition.GetFixedWindowLimiter(
            $"{category}:{partitionKey}",
            _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = Math.Max(1, rule.PermitLimit),
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(Math.Max(1, rule.WindowMinutes))
            });
    }

    private static string ResolveUserPartitionKey(HttpContext context, PlatformRateLimitOptions options)
    {
        return context.User.Identity?.IsAuthenticated == true && context.User.Identity.Name != null
            ? context.User.Identity.Name
            : ResolveClientIp(context, options);
    }

    private static string ResolveClientIp(HttpContext context, PlatformRateLimitOptions options)
    {
        var remoteIp = context.Connection.RemoteIpAddress;
        if (remoteIp != null && IsTrustedForwarder(remoteIp, options.TrustedProxies))
        {
            var forwardedFor = context.Request.Headers[options.ForwardedForHeader].ToString();
            var forwardedIp = forwardedFor
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .FirstOrDefault();

            if (IPAddress.TryParse(forwardedIp, out var parsedForwardedIp))
            {
                return parsedForwardedIp.ToString();
            }
        }

        return remoteIp?.ToString() ?? "unknown";
    }

    private static bool IsTrustedForwarder(IPAddress remoteIp, IEnumerable<string> trustedProxies)
    {
        if (IPAddress.IsLoopback(remoteIp))
        {
            return true;
        }

        return trustedProxies
            .Select(x => IPAddress.TryParse(x, out var ipAddress) ? ipAddress : null)
            .Any(x => x != null && x.Equals(remoteIp));
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
        app.UseRateLimiter();

        if (MultiTenancyConsts.IsEnabled)
        {
            app.UseMultiTenancy();
        }
        app.UseUnitOfWork();
        app.UseMiddleware<PlatformOpenApiSignatureMiddleware>();
        app.UseDynamicClaims();
        app.UseAuthorization();
        app.UseMiddleware<PlatformRequestAuditMiddleware>();
        app.UseMiddleware<PlatformUserActivityMiddleware>();

        var configuration = context.ServiceProvider.GetRequiredService<IConfiguration>();
        var apiDocumentationOptions = configuration
            .GetSection(PlatformApiDocumentationOptions.SectionName)
            .Get<PlatformApiDocumentationOptions>() ?? new PlatformApiDocumentationOptions();

        if (apiDocumentationOptions.IsEnabled())
        {
            app.UseSwagger();
        }

        if (apiDocumentationOptions.UseSwagger())
        {
            app.UseAbpSwaggerUI(c =>
            {
                c.RoutePrefix = NormalizeRoutePrefix(apiDocumentationOptions.SwaggerRoutePrefix, trimLeadingSlash: true);
                c.SwaggerEndpoint($"/swagger/{ApiDocGroups.Platform}/swagger.json", "平台 API");
                c.SwaggerEndpoint($"/swagger/{ApiDocGroups.Project}/swagger.json", "项目 API");
                c.SwaggerEndpoint($"/swagger/{ApiDocGroups.Business}/swagger.json", "业务 API");
                c.OAuthClientId(configuration["AuthServer:SwaggerClientId"]);
                c.OAuthScopes("Admin");
            });
        }

        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints(endpoints =>
        {
            if (apiDocumentationOptions.UseScalar())
            {
                endpoints.MapScalarApiReference(
                    "/scalar-assets",
                    ConfigureScalarAssetOptions);

                var scalarRoutePrefix = NormalizeRoutePrefix(apiDocumentationOptions.ScalarRoutePrefix, trimLeadingSlash: false);
                endpoints.MapGet(
                    scalarRoutePrefix,
                    () => Results.Content(BuildScalarHtml(apiDocumentationOptions), "text/html; charset=utf-8"));
            }
        });
    }

    private static void ConfigureScalarAssetOptions(ScalarOptions options)
    {
        options
            .DisableDefaultFonts()
            .DisableMcp()
            .DisableTelemetry();
    }

    private static string BuildScalarHtml(PlatformApiDocumentationOptions apiDocumentationOptions)
    {
        var sources = apiDocumentationOptions.ResolveDocuments()
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .Select(x => new Dictionary<string, object?>
            {
                ["title"] = string.IsNullOrWhiteSpace(x.Title) ? x.Name : x.Title,
                ["url"] = apiDocumentationOptions.OpenApiRoutePattern.Replace("{documentName}", x.Name),
                ["default"] = x.IsDefault
            })
            .ToList();

        var configuration = new Dictionary<string, object?>
        {
            ["withDefaultFonts"] = false,
            ["favicon"] = "/scalar-assets/favicon.svg",
            ["_integration"] = "dotnet",
            ["sources"] = sources,
            ["telemetry"] = false,
            ["mcp"] = new Dictionary<string, object?> { ["disabled"] = true }
        };

        var configurationJson = JsonSerializer.Serialize(configuration, new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
        var encodedRoutePrefix = Uri.EscapeDataString(NormalizeRoutePrefix(apiDocumentationOptions.ScalarRoutePrefix, trimLeadingSlash: false) + "/");

        return $$"""
<!doctype html>
<html lang="zh-CN">
<head>
    <title>{{HtmlEncoder.Default.Encode(apiDocumentationOptions.Title)}}</title>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
</head>
<body>
    <div id="app"></div>
    <script src="/scalar-assets/scalar.js"></script>
    <script type="module" src="/scalar-assets/scalar.aspnetcore.js"></script>
    <script type="module">
        import { initialize } from '/scalar-assets/scalar.aspnetcore.js'
        initialize('{{encodedRoutePrefix}}', false, {{configurationJson}}, '')
    </script>
</body>
</html>
""";
    }

    private static string NormalizeRoutePrefix(string routePrefix, bool trimLeadingSlash)
    {
        if (string.IsNullOrWhiteSpace(routePrefix))
        {
            return trimLeadingSlash ? string.Empty : "/";
        }

        var normalized = routePrefix.Trim().TrimEnd('/');
        if (trimLeadingSlash)
        {
            return normalized.TrimStart('/');
        }

        return normalized.StartsWith('/') ? normalized : $"/{normalized}";
    }
}
