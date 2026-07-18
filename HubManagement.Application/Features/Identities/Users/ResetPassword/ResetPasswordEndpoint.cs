using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace HubManagement.Application.Features.Identities.Users.ResetPassword;

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