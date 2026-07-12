using HubManagement.Application.Services;
using HubManagement.BuildingBlock.Core.Abstractions;
using HubManagement.Infrastructure.DataContext;
using HubManagement.Infrastructure.Services;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HubManagement.Infrastructure.Installers;

public static class ServicesExtensions
{
    public static IServiceCollection ServicesRegisterExtensions(this IServiceCollection services)
    {
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ICurrentUser>(sp => sp.GetRequiredService<ICurrentUserService>());
        services.AddScoped<ICurrentUserInitializer>(sp => sp.GetRequiredService<ICurrentUserService>());
        services.AddScoped<IRequestContextService, RequestContextService>();
        services.AddScoped<IRequestContext>(sp => sp.GetRequiredService<IRequestContextService>());
        
        // Facade for backward compatibility
        services.AddTransient<IUserService, UserService>();
        
        services.AddHealthChecks()
            .AddDbContextCheck<HubDbContext>(
                name: "db:hubmanagement",
                failureStatus: HealthStatus.Unhealthy);
        services.AddScoped<IDbInitializer, HubDbInitializer>();
        
        return services;
    }
}