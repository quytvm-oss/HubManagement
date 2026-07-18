using HubManagement.Application.DTOs;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace HubManagement.Application.Features.Tokens.TokenGeneration;

internal static class GenerateTokenEndpoint
{
    public static RouteHandlerBuilder MapGenerateToken(this IEndpointRouteBuilder builder)
    {
         return builder.MapPost("/token/issue",
                 [AllowAnonymous] 
                 async Task<Results<Ok<TokenResponse>, UnauthorizedHttpResult, ProblemHttpResult>>
                ([FromBody] GenerateTokenCommand command,
                    [FromServices] IMediator mediator,
                    CancellationToken ct) =>
                {

                    var token = await mediator.Send(command, ct);
                    return TypedResults.Ok(token);
                })
            .WithName("IssueJwtTokens")
            .WithSummary("Issue JWT access and refresh tokens")
            .WithDescription("Submit credentials to receive a JWT access token and a refresh token. Provide the 'tenant' header to select the tenant context (defaults to 'root'). The 'X-FSH-App' header (admin|dashboard) is used to enforce the SuperAdmin / dashboard boundary.")
            .Produces<TokenResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}