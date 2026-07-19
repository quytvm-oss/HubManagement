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
    /// Chạy <paramref name="operation"/> trong 1 DB transaction, đảm bảo mọi
    /// SaveChanges + message publish (nếu có) bên trong đều atomic với nhau.
    /// </summary>
    Task ExecuteTransactionalAsync(Func<Task> operation, CancellationToken cancellationToken = default);
}