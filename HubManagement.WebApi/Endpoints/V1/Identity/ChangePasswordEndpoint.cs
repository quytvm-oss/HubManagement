using HubManagement.Application.Features.Identities.Users.ChangePassword;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace HubManagement.WebApi.Endpoints.V1.Identity;

public static class ChangePasswordEndpoint
{
    internal static RouteHandlerBuilder MapChangePasswordEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/change-password", async (
                [FromBody] ChangePasswordCommand command,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(command, cancellationToken);
                return TypedResults.Ok(result);
            })
            .WithName("ChangePassword")
            .WithSummary("Change password")
            .WithDescription("Change the current user's password.")
            .RequireAuthorization()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest);
    }
}