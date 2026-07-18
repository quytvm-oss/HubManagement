using HubManagement.Application.DTOs;

namespace HubManagement.Application.Services;

public interface ISessionService
{
    Task<UserSessionDto> CreateSessionAsync(
        Guid userId,
        string refreshTokenHash,
        string ipAddress,
        string userAgent,
        DateTime expiresAt,
        CancellationToken cancellationToken = default);

    Task<List<UserSessionDto>> GetUserSessionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<List<UserSessionDto>> GetUserSessionsForAdminAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
    
    Task<(List<UserSessionDto> Items, long TotalCount)> GetTenantSessionsAsync(
        bool includeInactive,
        string? search,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<UserSessionDto?> GetSessionAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);

    Task<bool> RevokeSessionAsync(
        Guid sessionId,
        string revokedBy,
        string? reason = null,
        CancellationToken cancellationToken = default);

    Task<int> RevokeAllSessionsAsync(
        Guid userId,
        string revokedBy,
        Guid? exceptSessionId = null,
        string? reason = null,
        CancellationToken cancellationToken = default);

    Task<int> RevokeAllSessionsForAdminAsync(
        Guid userId,
        string revokedBy,
        string? reason = null,
        CancellationToken cancellationToken = default);

    Task<bool> RevokeSessionForAdminAsync(
        Guid sessionId,
        string revokedBy,
        string? reason = null,
        CancellationToken cancellationToken = default);

    Task UpdateSessionActivityAsync(
        string refreshTokenHash,
        CancellationToken cancellationToken = default);

    Task UpdateSessionRefreshTokenAsync(
        string oldRefreshTokenHash,
        string newRefreshTokenHash,
        DateTime newExpiresAt,
        CancellationToken cancellationToken = default);

    Task<bool> ValidateSessionAsync(
        string refreshTokenHash,
        CancellationToken cancellationToken = default);

    Task<Guid?> GetSessionIdByRefreshTokenAsync(
        string refreshTokenHash,
        CancellationToken cancellationToken = default);

    Task CleanupExpiredSessionsAsync(
        CancellationToken cancellationToken = default);
}