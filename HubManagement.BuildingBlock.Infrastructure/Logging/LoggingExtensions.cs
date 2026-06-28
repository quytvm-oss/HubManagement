using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;

namespace HubManagement.BuildingBlock.Infrastructure.Logging;

public static class LoggingExtensions
{
    public static IHostApplicationBuilder AddAppLogging(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSerilog((services, config) =>
        {
            config
                .ReadFrom.Configuration(builder.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.WithExceptionDetails()
                .Enrich.WithProperty("Application", builder.Environment.ApplicationName)
                .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName);
        });

        return builder;
    }

    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.GetLevel = (ctx, _, ex) =>
            {
                if (ctx.Request.Path.StartsWithSegments("/health"))
                    return LogEventLevel.Verbose;

                return ex != null ? LogEventLevel.Error : LogEventLevel.Information;
            };

            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent);

                if (httpContext.User.Identity?.IsAuthenticated == true)
                {
                    diagnosticContext.Set("UserId", httpContext.User.FindFirst("sub")?.Value);
                }
            };
        });

        return app;
    }
}