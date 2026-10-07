using HubManagement.Application.DTOs;
using HubManagement.Application.Helpers;
using HubManagement.Application.Services;
using HubManagement.BuildingBlock.Core.Abstractions;
using HubManagement.BuildingBlock.Infrastructure.Cache.Abstractions;
using HubManagement.Domain.Entities;
using HubManagement.Infrastructure.Authorization;
using HubManagement.Infrastructure.DataContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UAParser;

namespace HubManagement.Infrastructure.Services;

public class SessionService(
    HubDbContext db,
    ICurrentUser currentUser,
    ICacheService cache,
    ILogger<SessionService> logger,
    TimeProvider timeProvider)
    : ISessionService
{
    private readonly Parser _uaParser = Parser.GetDefault();

    public async Task<UserSessionDto> CreateSessionAsync(Guid userId, string refreshTokenHash, string ipAddress, string userAgent, DateTime expiresAt,
        CancellationToken cancellationToken = default)
    {
        var clientInfo = _uaParser.Parse(userAgent);
        
        var session = UserSession.Create(userId: userId,
            refreshTokenHash: refreshTokenHash,
            ipAddress: ipAddress,
            userAgent: userAgent,
            expiresAt: expiresAt,
            deviceType:DeviceTypeClassifier.Classify(clientInfo.Device.Family),
            browser: clientInfo.UA.Family,
            browserVersion: clientInfo.UA.Major,
            operatingSystem: clientInfo.OS.Family,
            osVersion: clientInfo.OS.Major);
        
        db.UserSessions.Add(session);
        
        await db.SaveChangesAsync(cancellationToken);

        await CacheSessionAsync(session, cancellationToken);
        
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Created session {SessionId} for user {UserId}", session.Id, userId);
        }

        return MapToDto(session, isCurrentSession: false);
    }

    public async Task<List<UserSessionDto>> GetUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {

        EnsureOwnsSession(userId);
        
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var sessions = await db.UserSessions
            .Where(s => s.UserId == userId && !s.IsRevoked && s.ExpiresAt > now)
            .OrderByDescending(s => s.LastActivityAt)
            .ToListAsync(cancellationToken);
        
        return sessions.Select(x => MapToDto(x,isCurrentSession: false)).ToList();
    }

    public async Task<List<UserSessionDto>> GetUserSessionsForAdminAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var sessions = await db.UserSessions
            .AsNoTracking()
            .Include(s => s.User)
            .Where(s => s.UserId == userId && !s.IsRevoked && s.ExpiresAt > now)  
            .OrderByDescending(s => s.LastActivityAt)
            .ToListAsync(cancellationToken);
            
        return sessions.Select(s => MapToDto(s, isCurrentSession: false)).ToList();
    }

    public async Task<(List<UserSessionDto> Items, long TotalCount)> GetTenantSessionsAsync(bool includeInactive, string? search, int skip, int take,
        CancellationToken cancellationToken = default)
    {
        
        if (take is < 1 or > 200) take = 50;
        if (skip < 0) skip = 0;
        
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var query = db.UserSessions
            .AsNoTracking().Include(s => s.User)
            .AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(s => !s.IsRevoked && s.ExpiresAt > now);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            string term = search.Trim();
            string escapedTerm = term.Replace("\\", "\\\\")
                .Replace("%", "\\%")
                .Replace("_", "\\_");
            string pattern = $"%{escapedTerm}%";
            query = query.Where(s =>
                (s.User != null && s.User.UserName != null && EF.Functions.ILike(s.User.UserName, pattern, "\\"))
                || (s.User != null && s.User.Email != null && EF.Functions.ILike(s.User.Email, pattern, "\\"))
                || (!string.IsNullOrEmpty(s.IpAddress) && EF.Functions.ILike(s.IpAddress, pattern, "\\")));
        }
        
        long totalCount = query.LongCount();
        
        var items = await query
            .OrderByDescending(s => s.LastActivityAt)
            .Skip(skip).Take(take)
            .ToListAsync(cancellationToken);
        
        return (items.Select(s => MapToDto(s, isCurrentSession: false)).ToList(), totalCount);
    }

    public async Task<UserSessionDto?> GetSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {

        var session = await db.UserSessions
            .AsNoTracking()
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);
        
        if (session is null) return null;

        EnsureOwnsSession(session.UserId);

        return MapToDto(session, isCurrentSession: false);
    }

    public async Task<bool> RevokeSessionAsync(Guid sessionId, string revokedBy, string? reason = null,
        CancellationToken cancellationToken = default)
    {
        
        var session = await db.UserSessions.FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);
        
        if (session is null) return false;
        
        EnsureOwnsSession(session.UserId);
        
        session.Revoke(revokedBy,reason ?? "User requested");
        
        await db.SaveChangesAsync(cancellationToken);
        await RemoveSessionFromCacheAsync(session.Id, cancellationToken);
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Session {SessionId} revoked by {RevokedBy}", sessionId, revokedBy);
        }

        return true;
    }

    public async Task<int> RevokeAllSessionsAsync(Guid userId, string revokedBy, Guid? exceptSessionId = null, string? reason = null,
        CancellationToken cancellationToken = default)
    {

        EnsureOwnsSession(userId);

        var query = db.UserSessions
            .Where(s => s.UserId == userId && !s.IsRevoked);

        if (exceptSessionId.HasValue)
        {
            query = query.Where(s => s.Id != exceptSessionId.Value);
        }

        var sessions = await query.ToListAsync(cancellationToken);
        
        foreach (var session in sessions)
        {
            session.Revoke(revokedBy, reason ?? "User requested logout from all devices");
        }

        await db.SaveChangesAsync(cancellationToken);

        await RemoveSessionsFromCacheAsync(sessions, cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Revoked {Count} sessions for user {UserId}", sessions.Count, userId);
        }

        return sessions.Count;
    }

    public async Task<int> RevokeAllSessionsForAdminAsync(Guid userId, string revokedBy, string? reason = null,
        CancellationToken cancellationToken = default)
    {

        var sessions = await db.UserSessions
            .Where(s => s.UserId == userId && !s.IsRevoked)
            .ToListAsync(cancellationToken);
        
        foreach (var session in sessions)
        {
            session.Revoke(revokedBy, reason ?? "Admin requested");
        }

        await db.SaveChangesAsync(cancellationToken);

        await RemoveSessionsFromCacheAsync(sessions, cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Admin {AdminId} revoked {Count} sessions for user {UserId}",
                revokedBy, sessions.Count, userId);
        }

        return sessions.Count;
    }

    public async Task<bool> RevokeSessionForAdminAsync(Guid sessionId, string revokedBy, string? reason = null,
        CancellationToken cancellationToken = default)
    {

        var session = await db.UserSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && !s.IsRevoked, cancellationToken);

        if (session is null)
        {
            return false;
        }
        
        session.Revoke(revokedBy, reason ?? "Admin requested");

        await db.SaveChangesAsync(cancellationToken);
        await RemoveSessionFromCacheAsync(session.Id, cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Admin {AdminId} revoked session {SessionId}", revokedBy, sessionId);
        }

        return true;
    }

    public async Task UpdateSessionActivityAsync(string refreshTokenHash, CancellationToken cancellationToken = default)
    {

        var session = await db.UserSessions
            .FirstOrDefaultAsync(s => s.RefreshTokenHash == refreshTokenHash && !s.IsRevoked, cancellationToken);

        if (session is not null)
        {
            session.UpdateActivity();
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UpdateSessionRefreshTokenAsync(string oldRefreshTokenHash, string newRefreshTokenHash, DateTime newExpiresAt,
        CancellationToken cancellationToken = default)
    {

        var session = await db.UserSessions
            .FirstOrDefaultAsync(s => s.RefreshTokenHash == oldRefreshTokenHash && !s.IsRevoked, cancellationToken);

        if (session is not null)
        {
            session.UpdateRefreshToken(newRefreshTokenHash, newExpiresAt);
            await db.SaveChangesAsync(cancellationToken);
            await CacheSessionAsync(session, cancellationToken);

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Updated session {SessionId} with new refresh token", session.Id);
            }
        }
    }

    public async Task<bool> ValidateSessionAsync(string refreshTokenHash, CancellationToken cancellationToken = default)
    {
        var session = await db.UserSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.RefreshTokenHash == refreshTokenHash, cancellationToken);

        if (session is null)
        {
            return true;
        }

        return !session.IsRevoked && session.ExpiresAt > timeProvider.GetUtcNow().UtcDateTime;
    }

    public async Task<Guid?> GetSessionIdByRefreshTokenAsync(string refreshTokenHash, CancellationToken cancellationToken = default)
    {

        var session = await db.UserSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.RefreshTokenHash == refreshTokenHash && !s.IsRevoked, cancellationToken);

        return session?.Id;
    }

    public async Task CleanupExpiredSessionsAsync(CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var cutoffDate = now.AddDays(-30); // Keep revoked sessions for 30 days for audit
        var deleted = await db.UserSessions
            .Where(s => s.ExpiresAt < now && s.ExpiresAt < cutoffDate)
            .ExecuteDeleteAsync(cancellationToken);

        if (deleted > 0 && logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Cleaned up {Count} expired sessions", deleted);
        }
    }

    #region internals
    
    
    private void EnsureOwnsSession(Guid sessionOwnerUserId)
    {
        if (sessionOwnerUserId != currentUser.GetUserId())
        {
            throw new UnauthorizedAccessException("Cannot access sessions for another user.");
        }
    }

    private Task CacheSessionAsync(UserSession session, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (session.IsRevoked || session.ExpiresAt <= now)
        {
            return RemoveSessionFromCacheAsync(session.Id, cancellationToken);
        }

        var entry = new SessionValidationCacheEntry(session.UserId, session.ExpiresAt);
        return cache.SetItemAsync(
            SessionValidationCache.Key(session.Id),
            entry,
            session.ExpiresAt - now,
            cancellationToken);
    }

    private Task RemoveSessionFromCacheAsync(Guid sessionId, CancellationToken cancellationToken) =>
        cache.RemoveItemAsync(SessionValidationCache.Key(sessionId), cancellationToken);

    private async Task RemoveSessionsFromCacheAsync(
        IEnumerable<UserSession> sessions,
        CancellationToken cancellationToken)
    {
        foreach (var session in sessions)
        {
            await RemoveSessionFromCacheAsync(session.Id, cancellationToken);
        }
    }
    
    private UserSessionDto MapToDto(UserSession session, bool isCurrentSession)
    {
        return new UserSessionDto
        {
            Id = session.Id,
            UserId = session.UserId,
            UserName = session.User?.UserName,
            UserEmail = session.User?.Email,
            IpAddress = session.IpAddress,
            DeviceType = session.DeviceType,
            Browser = session.Browser,
            BrowserVersion = session.BrowserVersion,
            OperatingSystem = session.OperatingSystem,
            OsVersion = session.OsVersion,
            CreatedAt = session.CreatedAt,
            LastActivityAt = session.LastActivityAt,
            ExpiresAt = session.ExpiresAt,
            IsActive = !session.IsRevoked && session.ExpiresAt > timeProvider.GetUtcNow().UtcDateTime,
            IsCurrentSession = isCurrentSession
        };
    }

    #endregion
}
