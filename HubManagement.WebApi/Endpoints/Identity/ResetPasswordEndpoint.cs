using HubManagement.Application.Features.Identities.Users.ResetPassword;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace HubManagement.WebApi.Endpoints.Identity;

public static class ResetPasswordEndpoint
{
    internal static RouteHandlerBuilder MapResetPasswordEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/reset-password",
                async ([FromBody] ResetPasswordCommand command,
                    IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var result = await mediator.Send(command, cancellationToken);
                    return TypedResults.Ok(result);
                })
            .WithName("ResetPassword")
            .WithSummary("Reset password")
            .WithDescription("Reset the user's password using the provided verification token.")
            .AllowAnonymous()
            .Produces(StatusCodes.Status200OK);
    }
}