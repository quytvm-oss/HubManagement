using HubManagement.Application.Features.Identities.Users.ForgotPassword;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace HubManagement.WebApi.Endpoints.V1.Identity;

public static class ForgotPasswordEndpoint
{
    internal static RouteHandlerBuilder MapForgotPasswordEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/forgot-password", async (
                [FromBody] ForgotPasswordCommand command,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(command, cancellationToken);
                return TypedResults.Ok(result);
            })
            .WithName("RequestPasswordReset")
            .WithSummary("Request password reset")
            .WithDescription("Generate a password reset token and send it via email.")
            .AllowAnonymous()
            .Produces(StatusCodes.Status200OK);
    }
}