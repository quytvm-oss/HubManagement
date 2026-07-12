using HubManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HubManagement.Application.Contracts;

public interface IApplicationDbContext
{
    DbSet<UserSession> UserSessions { get; }
    
    DbSet<UserDeviceToken> UserDeviceTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}