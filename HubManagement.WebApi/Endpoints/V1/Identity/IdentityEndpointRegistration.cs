using HubManagement.BuildingBlock.Infrastructure.Web.MinimalApis;

namespace HubManagement.WebApi.Endpoints.V1.Identity;

public class IdentityEndpointRegistration : IMinimalEndpointDefinition
{
    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder builder)
    {
        var group = builder
            .MapGroup("/identity")
            .WithTags("Identity")
            .AddEndpointFilter<HubManagement.WebApi.Authentication.BffAntiforgeryFilter>();

        group.MapCsrfTokenEndpoint();
        group.MapBffLoginEndpoint();
        group.MapBffSessionEndpoint();
        group.MapBffLogoutEndpoint();
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
