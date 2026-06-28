using HealthChecks.UI.Client;
using HubManagement.BuildingBlock.Core.DbSettings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HubManagement.BuildingBlock.Infrastructure.HealthCheck;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddHeroHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var dbSettings = configuration.GetSection(nameof(PostGreSqlSetting)).Get<PostGreSqlSetting>();
        
        services
            .AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
            .AddNpgSql(
                dbSettings?.ConnectionString!,
                name: "postgresql",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["ready", "db"])
            .AddRedis(
                configuration.GetConnectionString("Redis")!,
                name: "redis",
                failureStatus: HealthStatus.Degraded, 
                tags: ["ready", "cache"]);

        return services;
    }

    public static IApplicationBuilder UseHeroHealthChecks(this IApplicationBuilder app)
    {
        app.UseHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        
        app.UseHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        return app;
    }
}