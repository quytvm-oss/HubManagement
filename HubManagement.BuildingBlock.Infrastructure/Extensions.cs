using HealthChecks.Redis;
using HubManagement.BuildingBlock.Infrastructure.Cache;
using HubManagement.BuildingBlock.Infrastructure.Monitoring;
using HubManagement.BuildingBlock.Infrastructure.Web.Cors;
using HubManagement.BuildingBlock.Infrastructure.Web.Exceptions;
using HubManagement.BuildingBlock.Infrastructure.Web.Mediator.Behaviors;
using HubManagement.BuildingBlock.Infrastructure.Web.OpenApi;
using HubManagement.BuildingBlock.Infrastructure.Web.Security;
using HubManagement.BuildingBlock.Infrastructure.Web.Versioning;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace HubManagement.BuildingBlock.Infrastructure;

public static class Extensions
{
    public static IHostApplicationBuilder AddPlatform(this IHostApplicationBuilder builder,
        Action<PlatformOptions>? configure = null)
    {
         ArgumentNullException.ThrowIfNull(builder);

        var options = new PlatformOptions();
        configure?.Invoke(options);

        //PermissionConstants.Register(SystemPermissions.All);

        builder.Services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
        });
        builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
        {
            options.Level = System.IO.Compression.CompressionLevel.Fastest;
        });

        builder.Services.AddLogging();
        if (options.EnableOpenTelemetry)
        {
            builder.Services.AddAppOpenTelemetry(builder.Configuration);
        }

        builder.Services.AddHttpContextAccessor();

        var corsEnabled = options.EnableCors && IsCorsEnabled(builder.Configuration);
        var openApiEnabled = options.EnableOpenApi && IsOpenApiEnabled(builder.Configuration);

        if (corsEnabled)
        {
            builder.Services.AddCorsPolicy(builder.Configuration);
        }

        builder.Services.AddVersioning();

        if (openApiEnabled)
        {
            builder.Services.AddAppOpenApi(builder.Configuration);
        }

        builder.Services.AddHealthChecks().AddCheck("self", () => HealthCheckResult.Healthy());
        
        // if (options.EnableMailing)
        // {
        //     builder.Services.AddAppMailing(builder.Configuration);
        // }

        if (options.EnableCaching)
        {
            builder.Services.AddCaching(builder.Configuration);
            var cacheConfig = builder.Configuration.GetSection(nameof(CachingOptions)).Get<CachingOptions>();
            if (cacheConfig is not null && !string.IsNullOrEmpty(cacheConfig.Redis))
            {
                builder.Services.AddHealthChecks().AddCheck<RedisHealthCheck>("redis");
            }
        }

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        builder.Services.AddProblemDetails();
        builder.Services.AddOptions<SecurityHeadersOptions>().BindConfiguration(nameof(SecurityHeadersOptions));

        return builder;
    }
    
    
     public static WebApplication UsePlatform(this WebApplication app, Action<PipelineOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(app);

        var options = new PipelineOptions();
        configure?.Invoke(options);

        var corsEnabled = options.UseCors && IsCorsEnabled(app.Configuration);
        var openApiEnabled = options.UseOpenApi && IsOpenApiEnabled(app.Configuration);

        app.UseExceptionHandler();
        app.UseResponseCompression();

        // CORS MUST run before UseHttpsRedirection: preflight OPTIONS can't follow an HTTP→HTTPS redirect, so
        // the browser would block the call. Safe before routing because we use one global policy (no [EnableCors]).
        if (corsEnabled)
        {
            app.UseCorsPolicy();
        }

        app.UseHttpsRedirection();

        app.UseSecurityHeaders();

        // Serve static files as early as possible to short-circuit pipeline
        if (options.ServeStaticFiles)
        {
            var assetsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            if (!Directory.Exists(assetsPath))
            {
                Directory.CreateDirectory(assetsPath);
            }

            app.UseStaticFiles();
        }
        app.UseRouting();

        if (openApiEnabled)
        {
            app.UseAppOpenApi();
        }

        //app.UseAuthentication();

        // app.UseAuthorization();
        return app;
    }
    
    
    private static bool IsCorsEnabled(IConfiguration configuration)
    {
        var allowAll = configuration.GetValue("CorsOptions:AllowAll", false);
        var origins = configuration.GetSection("CorsOptions:AllowedOrigins").Get<string[]>() ?? [];
        return allowAll || origins.Length > 0;
    }

    private static bool IsOpenApiEnabled(IConfiguration configuration)
    {
        return configuration.GetValue("OpenApiOptions:Enabled", true);
    }
}