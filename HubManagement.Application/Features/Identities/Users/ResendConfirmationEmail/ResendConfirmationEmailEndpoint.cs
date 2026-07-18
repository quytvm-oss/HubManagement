using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace HubManagement.Application.Features.Identities.Users.ResendConfirmationEmail;

public static class ResendConfirmationEmailEndpoint
{
    internal static RouteHandlerBuilder MapResendConfirmationEmailEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/users/{id:guid}/resend-confirmation-email", Handler)
            .WithName("ResendConfirmationEmail")
            .WithSummary("Resend a user's email confirmation (admin)")
            .AllowAnonymous()
            .WithDescription("Re-sends the email-confirmation link to a user who has not confirmed their address yet.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<NoContent> Handler(
        Guid id,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        // Build the confirmation-link base URL from the request, same as the registration endpoint.
        var origin = $"{context.Request.Scheme}://{context.Request.Host.Value}{context.Request.PathBase.Value}";
        await mediator.Send(new ResendConfirmationEmailCommand(id, origin), cancellationToken);
        return TypedResults.NoContent();
    }
}
