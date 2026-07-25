using HubManagement.Application.Features.Identities.Users.ConfirmEmail;
using Mediator;

namespace HubManagement.WebApi.Endpoints.V1.Identity;

public static class ConfirmEmailEndpoint
{
    internal static RouteHandlerBuilder MapConfirmEmailEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/confirm-email", async (Guid userId, string code, IMediator mediator, CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(new ConfirmEmailCommand(userId, code), cancellationToken);
                return TypedResults.Ok(result);
            })
            .WithName("ConfirmEmail")
            .WithSummary("Confirm user email")
            .WithDescription("Confirm a user's email address.")
            .AllowAnonymous()
            .Produces(StatusCodes.Status200OK);
    }
}