using System.Security.Claims;
using System.Security.Cryptography;
using HubManagement.Application.Services;
using HubManagement.Infrastructure.Authorization;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace HubManagement.WebApi.Endpoints.V1.Identity;

public static class BffAuthenticationEndpoints
{
    public sealed record LoginRequest(string Email, string Password, string? TwoFactorCode = null);

    public sealed record SessionResponse(Guid UserId, string? Email, string? Name,
        IReadOnlyCollection<string> Roles, IReadOnlyCollection<string> Permissions);

    internal static RouteHandlerBuilder MapBffLoginEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapPost("/login", LoginAsync)
            .AllowAnonymous()
            .WithName("BffLogin")
            .WithSummary("Sign in and create an HttpOnly BFF session")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized);

    internal static RouteHandlerBuilder MapBffSessionEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapGet("/session", GetSessionAsync)
            .RequireAuthorization()
            .WithName("GetBffSession")
            .WithSummary("Get the current browser session")
            .Produces<SessionResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

    internal static RouteHandlerBuilder MapBffLogoutEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapPost("/logout", LogoutAsync)
            .RequireAuthorization()
            .WithName("BffLogout")
            .WithSummary("Revoke the current BFF session and clear its cookie")
            .Produces(StatusCodes.Status204NoContent);

    internal static RouteHandlerBuilder MapCsrfTokenEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapGet("/csrf", (HttpContext context, IAntiforgery antiforgery) =>
            {
                var tokens = antiforgery.GetAndStoreTokens(context);
                return TypedResults.Ok(new { token = tokens.RequestToken });
            })
            .AllowAnonymous()
            .WithName("GetCsrfToken")
            .WithSummary("Issue the request token required by unsafe browser requests");

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        HttpContext context,
        IIdentityService identityService,
        ISessionService sessionService,
        IOptions<BffAuthenticationOptions> options,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var validated = await identityService.ValidateCredentialsAsync(
            request.Email, request.Password, request.TwoFactorCode, cancellationToken);
        if (validated is null) return TypedResults.Unauthorized();

        var (userId, sourceClaims) = validated.Value;
        var now = timeProvider.GetUtcNow();
        var expiresAt = now.AddHours(options.Value.SessionHours);
        var sessionKey = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var userAgent = context.Request.Headers.UserAgent.ToString();

        var session = await sessionService.CreateSessionAsync(
            userId,
            sessionKey,
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            string.IsNullOrWhiteSpace(userAgent) ? "unknown" : userAgent,
            expiresAt.UtcDateTime,
            cancellationToken);

        var claims = sourceClaims
            .Where(claim => claim.Type != BffAuthenticationDefaults.SessionIdClaim)
            .ToList();
        claims.Add(new Claim(BffAuthenticationDefaults.SessionIdClaim, session.Id.ToString()));

        var identity = new ClaimsIdentity(claims, BffAuthenticationDefaults.Scheme,
            ClaimTypes.Name, ClaimTypes.Role);
        await context.SignInAsync(
            BffAuthenticationDefaults.Scheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties
            {
                IsPersistent = true,
                AllowRefresh = true,
                IssuedUtc = now,
                ExpiresUtc = expiresAt
            });

        return TypedResults.NoContent();
    }

    private static async Task<IResult> GetSessionAsync(
        ClaimsPrincipal principal,
        IUserService userService,
        CancellationToken cancellationToken)
    {
        var userIdValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId)) return TypedResults.Unauthorized();

        var permissions = await userService.GetPermissionsAsync(userIdValue, cancellationToken) ?? [];
        var roles = principal.FindAll(ClaimTypes.Role)
            .Select(claim => claim.Value)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return TypedResults.Ok(new SessionResponse(userId,
            principal.FindFirstValue(ClaimTypes.Email), principal.Identity?.Name, roles, permissions));
    }

    private static async Task<IResult> LogoutAsync(
        ClaimsPrincipal principal,
        HttpContext context,
        ISessionService sessionService,
        CancellationToken cancellationToken)
    {
        var sessionIdValue = principal.FindFirstValue(BffAuthenticationDefaults.SessionIdClaim);
        var userIdValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(sessionIdValue, out var sessionId))
        {
            await sessionService.RevokeSessionAsync(sessionId, userIdValue ?? "unknown",
                "User signed out", cancellationToken);
        }

        await context.SignOutAsync(BffAuthenticationDefaults.Scheme);
        return TypedResults.NoContent();
    }
}
