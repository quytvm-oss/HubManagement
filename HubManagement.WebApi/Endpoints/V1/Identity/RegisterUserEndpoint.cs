using HubManagement.Application.Features.Identities.Users.RegisterUser;
using Mediator;
using Microsoft.AspNetCore.Authorization;

namespace HubManagement.WebApi.Endpoints.V1.Identity;

public static class RegisterUserEndpoint
{
    internal static RouteHandlerBuilder MapRegisterUserEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/register",
            [AllowAnonymous]    async (RegisterUserCommand command,
            HttpContext context,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var origin = $"{context.Request.Scheme}://{context.Request.Host.Value}{context.Request.PathBase.Value}";
            command.Origin = origin;
            var result = await mediator.Send(command, cancellationToken);
            return TypedResults.Created($"/api/v1/identity/users/{result.UserId}", result);
        })
        .WithName("RegisterUser")
        .WithSummary("Register user")
        //.RequirePermission(IdentityPermissions.Users.Create)
        //.WithIdempotency()
        .WithDescription("Create a new user account.")
        .Produces<RegisterUserResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status400BadRequest);
    }
}