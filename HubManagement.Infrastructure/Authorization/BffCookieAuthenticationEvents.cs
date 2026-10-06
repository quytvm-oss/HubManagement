using HubManagement.Application.Contracts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace HubManagement.Infrastructure.Authorization;

public sealed class BffCookieAuthenticationEvents(
    IApplicationDbContext db,
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
        var sessionIsValid = await db.UserSessions
            .AsNoTracking()
            .AnyAsync(session => session.Id == sessionId
                                 && !session.IsRevoked
                                 && session.ExpiresAt > now,
                context.HttpContext.RequestAborted);

        if (!sessionIsValid)
        {
            await RejectAsync(context);
        }
    }

    private static async Task RejectAsync(CookieValidatePrincipalContext context)
    {
        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync(BffAuthenticationDefaults.Scheme);
    }
}
