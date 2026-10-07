using HubManagement.Application.Contracts;
using HubManagement.BuildingBlock.Infrastructure.Cache.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HubManagement.Infrastructure.Authorization;

public sealed class BffCookieAuthenticationEvents(
    IApplicationDbContext db,
    ICacheService cache,
    TimeProvider timeProvider) : CookieAuthenticationEvents
{
    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var sessionValue = context.Principal?.FindFirst(BffAuthenticationDefaults.SessionIdClaim)?.Value;
        if (!Guid.TryParse(sessionValue, out var sessionId))
        {
            await RejectAsync(context);
            return;
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var cancellationToken = context.HttpContext.RequestAborted;
        var cacheKey = SessionValidationCache.Key(sessionId);
        var cachedSession = await cache.GetItemAsync<SessionValidationCacheEntry>(cacheKey, cancellationToken);

        if (cachedSession is not null && cachedSession.ExpiresAt > now)
        {
            return;
        }

        if (cachedSession is not null)
        {
            await cache.RemoveItemAsync(cacheKey, cancellationToken);
        }

        var session = await db.UserSessions
            .AsNoTracking()
            .Where(session => session.Id == sessionId
                              && !session.IsRevoked
                              && session.ExpiresAt > now)
            .Select(session => new SessionValidationCacheEntry(session.UserId, session.ExpiresAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (session is null)
        {
            await RejectAsync(context);
            return;
        }

        await cache.SetItemAsync(cacheKey, session, session.ExpiresAt - now, cancellationToken);
    }

    private static async Task RejectAsync(CookieValidatePrincipalContext context)
    {
        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync(BffAuthenticationDefaults.Scheme);
    }
    
    public override async Task RedirectToLogin(
        RedirectContext<CookieAuthenticationOptions> context)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;

        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Detail = "Authentication is required to access this resource."
        });
    }

    public override async Task RedirectToAccessDenied(
        RedirectContext<CookieAuthenticationOptions> context)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;

        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Forbidden",
            Detail = "You do not have permission to access this resource."
        });
    }
}
