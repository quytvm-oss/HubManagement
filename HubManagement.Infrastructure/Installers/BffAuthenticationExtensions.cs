using HubManagement.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace HubManagement.Infrastructure.Installers;

public static class BffAuthenticationExtensions
{
    public static IServiceCollection ConfigureBffAuthentication(this IServiceCollection services)
    {
        services.AddOptions<BffAuthenticationOptions>()
            .BindConfiguration(nameof(BffAuthenticationOptions))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<BffCookieAuthenticationEvents>();

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = BffAuthenticationDefaults.Scheme;
                options.DefaultChallengeScheme = BffAuthenticationDefaults.Scheme;
                options.DefaultSignInScheme = BffAuthenticationDefaults.Scheme;
            })
            .AddCookie(BffAuthenticationDefaults.Scheme, options =>
            {
                options.Cookie.Name = BffAuthenticationDefaults.CookieName;
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.Path = "/";
                // Keep the protected cookie and database session on the same fixed lifetime.
                options.SlidingExpiration = false;
                options.EventsType = typeof(BffCookieAuthenticationEvents);

                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
            });

        services.AddAuthorizationBuilder().AddRequiredPermissionPolicy();
        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = options.GetPolicy(RequiredPermissionDefaults.PolicyName)!;
            options.FallbackPolicy = options.GetPolicy(RequiredPermissionDefaults.PolicyName);
        });
        services.AddSingleton<IAuthorizationMiddlewareResultHandler, PathAwareAuthorizationHandler>();

        services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";
            options.Cookie.Name = "__Host-hub.csrf";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.Path = "/";
        });

        return services;
    }
}
