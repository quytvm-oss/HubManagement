using HubManagement.BuildingBlock.Infrastructure.Authorization;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace HubManagement.Application.Features.Identities.Users.ToggleStatus;

public static class ToggleUserStatusEndpoint
{
    internal static RouteHandlerBuilder MapToggleUserStatusEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPatch("/users/{id:guid}", Handler)
            .WithName("ToggleUserStatus")
            .WithSummary("Toggle user status")
          //  .RequirePermission(IdentityPermissions.Users.Update)
            .WithDescription("Activate or deactivate a user account.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<Results<NoContent, BadRequest>> Handler(
        Guid id,
        [FromBody] ToggleUserStatusCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        command.UserId ??= id;

        await mediator.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }
}