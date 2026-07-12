using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace HubManagement.BuildingBlock.Infrastructure.Monitoring;

public static class MonitoringExtensions
{
    private const string SectionName = "OpenTelemetry";
    
    public static IServiceCollection AddAppOpenTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        var openTelemetryOptions = configuration.GetSection(SectionName).Get<OpenTelemetryOptions>();
        
        var serviceName = openTelemetryOptions?.ServiceName;
        var otlpEndpoint = openTelemetryOptions?.Endpoint!;

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName!)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = 
                        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
                }))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation(options =>
                {
                    // Không trace health check — noise
                    options.Filter = ctx =>
                        !ctx.Request.Path.StartsWithSegments("/health");
                })
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()  // nếu muốn trace EF queries
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otlpEndpoint);
                }))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()  // GC, thread pool, memory
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otlpEndpoint);
                }));

        return services;
    }
}