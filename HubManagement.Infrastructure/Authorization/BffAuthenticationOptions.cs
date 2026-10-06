using System.ComponentModel.DataAnnotations;

namespace HubManagement.Infrastructure.Authorization;

public static class BffAuthenticationDefaults
{
    public const string Scheme = "BffCookie";
    public const string SessionIdClaim = "session_id";
    public const string CookieName = "__Host-hub.session";
}

public sealed class BffAuthenticationOptions
{
    [Range(1, 720)]
    public int SessionHours { get; init; } = 8;
}
