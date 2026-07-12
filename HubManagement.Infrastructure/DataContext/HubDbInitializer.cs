using HubManagement.BuildingBlock.Core.Abstractions;
using HubManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HubManagement.Infrastructure.DataContext;

public class HubDbInitializer(
    ILogger<HubDbInitializer> logger,
    HubDbContext context,
    RoleManager<ApplicationRole> roleManager,
    UserManager<ApplicationUser> userManager,
    TimeProvider timeProvider,
    IConfiguration configuration) : IDbInitializer
{
    public async Task MigrateAsync(CancellationToken cancellationToken)
    {
        if ((await context.Database.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false)).Any())
        {
            await context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("applied database migrations for identity module");
            }
        }
    }

    public Task SeedAsync(CancellationToken cancellationToken)
    => Task.CompletedTask;
}