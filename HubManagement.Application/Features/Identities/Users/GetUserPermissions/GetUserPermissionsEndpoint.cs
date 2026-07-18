using System.Security.Claims;
using HubManagement.BuildingBlock.Core.Exceptions;
using HubManagement.BuildingBlock.Infrastructure.Authorization;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace HubManagement.Application.Features.Identities.Users.GetUserPermissions;

public static class GetUserPermissionsEndpoint
{
    internal static RouteHandlerBuilder MapGetCurrentUserPermissionsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/permissions", async (ClaimsPrincipal user, IMediator mediator, CancellationToken cancellationToken) =>
            {
                if (user.GetUserId() is not { } userId || string.IsNullOrEmpty(userId))
                {
                    throw new UnauthorizedException();
                }

                return TypedResults.Ok(await mediator.Send(new GetCurrentUserPermissionsQuery(userId), cancellationToken));
            })
            .WithName("GetCurrentUserPermissions")
            .WithSummary("Get current user permissions")
            .WithDescription("Retrieve permissions for the authenticated user. Requires authentication only — every signed-in user can read their own grants.")
            .Produces<IEnumerable<string>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
    }
}