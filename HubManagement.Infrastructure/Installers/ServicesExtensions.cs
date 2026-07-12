using HubManagement.Application.Services;
using HubManagement.BuildingBlock.Core.Abstractions;
using HubManagement.Infrastructure.DataContext;
using HubManagement.Infrastructure.Services;
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
        
        // User services - focused single-responsibility services
        services.AddTransient<IUserRegistrationService, UserRegistrationService>();
        services.AddTransient<IUserProfileService, UserProfileService>();
        services.AddTransient<IUserStatusService, UserStatusService>();
        services.AddTransient<IUserRoleService, UserRoleService>();
        services.AddTransient<IUserPasswordService, UserPasswordService>();
        services.AddTransient<IUserPermissionService, UserPermissionService>();
        
        // Facade for backward compatibility
        services.AddTransient<IUserService, UserService>();
        
        services.AddTransient<IRoleService, RoleService>();
        
        // Register password expiry service
        services.AddScoped<IPasswordExpiryService, PasswordExpiryService>();

        // Register session service and background cleanup
        services.AddScoped<ISessionService, SessionService>();
        
        services.AddHealthChecks()
            .AddDbContextCheck<HubDbContext>(
                name: "db:hubmanagement",
                failureStatus: HealthStatus.Unhealthy);
        services.AddScoped<IDbInitializer, HubDbInitializer>();
        
        return services;
    }
}