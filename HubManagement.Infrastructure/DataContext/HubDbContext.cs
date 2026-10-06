using HubManagement.Application.Contracts;
using HubManagement.BuildingBlock.Core.Common;
using HubManagement.BuildingBlock.Core.DbSettings;
using HubManagement.BuildingBlock.Core.Domain;
using HubManagement.Domain.Entities;
using HubManagement.Infrastructure.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Npgsql;
using Rebus.Config.Outbox;
using Rebus.Transport;

namespace HubManagement.Infrastructure.DataContext;

public class HubDbContext(
    DbContextOptions options,
    IHostEnvironment environment,
    IOptions<PostGreSqlSetting> settingOptions)
    : IdentityDbContext<
        ApplicationUser,
        ApplicationRole,
        Guid,
        IdentityUserClaim<Guid>,
        IdentityUserRole<Guid>,
        IdentityUserLogin<Guid>,
        ApplicationRoleClaim,
        IdentityUserToken<Guid>,
        IdentityUserPasskey<Guid>>(options), IApplicationDbContext
{
    private readonly IHostEnvironment _environment = environment;
    
    private readonly PostGreSqlSetting _settings = settingOptions.Value;

    public DbSet<UserSession> UserSessions => Set<UserSession>();
    
    public DbSet<UserDeviceToken> UserDeviceTokens => Set<UserDeviceToken>();

    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();

    /// <summary>
    /// Configures the model and its relationships by applying global filters, tenant isolation,
    /// and other customization logic during the model creation stage of the database context.
    /// </summary>
    /// <param name="modelBuilder">The <see cref="ModelBuilder"/> instance used to define the model
    /// configuration for the database context.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="modelBuilder"/> argument is null.</exception>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        base.OnModelCreating(modelBuilder);
        modelBuilder.AppendGlobalQueryFilter<ISoftDeletable>(QueryFilters.SoftDelete, s => !s.IsDeleted);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HubDbContext).Assembly);
    }
    
    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        await using var transaction = await Database.BeginTransactionAsync(cancellationToken);

        using var scope = new RebusTransactionScope();
        scope.UseOutbox(
            connection: (NpgsqlConnection)Database.GetDbConnection(),
            transaction: (NpgsqlTransaction)transaction.GetDbTransaction());

        try
        {
            await operation(cancellationToken);

            await scope.CompleteAsync();
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
