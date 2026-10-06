using HubManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HubManagement.Application.Contracts;

public interface IApplicationDbContext
{
    DbSet<UserSession> UserSessions { get; }
    
    DbSet<UserDeviceToken> UserDeviceTokens { get; }
    
    DbSet<EmailTemplate> EmailTemplates { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Runs <paramref name="operation"/> in a database transaction. Messages sent through
    /// the Rebus outbox participate in the same transaction.
    /// </summary>
    Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default);
}
