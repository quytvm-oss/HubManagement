using HubManagement.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HubManagement.Infrastructure.Installers;

public static class RequiredPermissionDefaults
{
    public const string PolicyName = "RequiredPermission";
}

public static class AuthenticationConstants
{
    public const string AuthenticationScheme = BffAuthenticationDefaults.Scheme;
}

public static class RequiredPermissionAuthorizationExtensions
{
    private static AuthorizationPolicyBuilder RequireRequiredPermissions(this AuthorizationPolicyBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddRequirements(new PermissionAuthorizationRequirement());
    }

    public static AuthorizationBuilder AddRequiredPermissionPolicy(this AuthorizationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddPolicy(RequiredPermissionDefaults.PolicyName, policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.AddAuthenticationSchemes(AuthenticationConstants.AuthenticationScheme);
            policy.RequireRequiredPermissions();
        });
        
        builder.Services.TryAddEnumerable(ServiceDescriptor.Scoped<IAuthorizationHandler, RequiredPermissionAuthorizationHandler>());

        return builder;
    }
}
