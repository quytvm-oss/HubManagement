namespace HubManagement.Infrastructure.Authorization;

internal static class SessionValidationCache
{
    internal static string Key(Guid sessionId) => $"auth:session:{sessionId:N}";
}

internal sealed record SessionValidationCacheEntry(Guid UserId, DateTime ExpiresAt);
