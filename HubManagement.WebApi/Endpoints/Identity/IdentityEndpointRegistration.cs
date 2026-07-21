using HubManagement.BuildingBlock.Infrastructure.Web.MinimalApis;

namespace HubManagement.WebApi.Endpoints.Identity;

public class IdentityEndpointRegistration : IMinimalEndpointDefinition
{
    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder builder)
    {
        var group = builder
            .MapGroup("/identity")
            .WithTags("Identity");

        group.MapGenerateToken();
        group.MapRefreshTokenEndpoint();
        group.MapRegisterUserEndpoint();
        group.MapConfirmEmailEndpoint();
        group.MapForgotPasswordEndpoint();
        group.MapChangePasswordEndpoint();
        group.MapResetPasswordEndpoint();
        group.MapResendConfirmationEmailEndpoint();
        group.MapGetCurrentUserPermissionsEndpoint();
        group.MapToggleUserStatusEndpoint();

        return builder;
    }
}