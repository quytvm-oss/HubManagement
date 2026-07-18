using HubManagement.Application.Constants;
using HubManagement.BuildingBlock.Core.Abstractions;
using HubManagement.BuildingBlock.Infrastructure.Authorization;
using HubManagement.Domain.Entities;
using HubManagement.Infrastructure.SeedData;
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

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        await SeedRolesAsync(cancellationToken);
        await SeedAdminUserAsync(cancellationToken);
    }

    private async Task SeedRolesAsync(CancellationToken cancellationToken)
    {
        foreach (string roleName in RoleConstants.DefaultRoles)
        {
            if (await roleManager.Roles.SingleOrDefaultAsync(r => r.Name == roleName, cancellationToken)
                is not { } role)
            {
                role = new ApplicationRole(roleName, $"Role default");
                await roleManager.CreateAsync(role);
            }
            
            // Assign permissions
            if (roleName == RoleConstants.Basic)
            {
                await AssignPermissionsToRoleAsync(context, SystemPermissions.Basic, role, cancellationToken);
            }
            else if (roleName == RoleConstants.Admin)
            {
                await AssignPermissionsToRoleAsync(context, SystemPermissions.Admin, role, cancellationToken);
            }
        }
    }
    
    private async Task AssignPermissionsToRoleAsync(HubDbContext dbContext, IReadOnlyList<Permission> permissions, ApplicationRole role, CancellationToken cancellationToken = default)
    {
        var currentClaims = await roleManager.GetClaimsAsync(role);
        var newClaims = permissions.Where(permission => !currentClaims.Any(c => c.Type == ClaimConstants.Permission && c.Value == permission.Name))
            .Select(permission => new ApplicationRoleClaim()
            {
                RoleId = role.Id,
                ClaimType = ClaimConstants.Permission,
                Description = permission.Description,
                ClaimValue = permission.Name,
                CreatedBy = "application",
                CreatedOn = timeProvider.GetUtcNow()
            }).ToList();

        foreach (var claim in newClaims)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Seeding {Role} Permission '{Permission}' ", role.Name, claim.ClaimValue);
            }
            await dbContext.RoleClaims.AddAsync(claim, cancellationToken);
        }
        
        // Save changes to the database context
        if (newClaims.Count != 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
    
    private async Task SeedAdminUserAsync(CancellationToken cancellationToken)
    {
        var systemAccount = await userManager.Users
            .IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == SystemPermissions.AdminId, cancellationToken);

        if (systemAccount != null) return;

        systemAccount = new ApplicationUser()
        {
            Id = SystemPermissions.AdminId,
            UserName = "admin",
            FirstName = "Admin",
            LastName = "System",
            Email = "admin@mail.com",
            PhoneNumberConfirmed = true,
            EmailConfirmed = true,
            IsActive = true,
            SecurityStamp = Guid.NewGuid().ToString(),
        };

        logger.LogInformation("Seeding system account");
        var result = await userManager.CreateAsync(systemAccount, "123456789Aa@");

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(systemAccount, RoleConstants.Admin);

            await userManager.SetLockoutEnabledAsync(systemAccount, false);

            logger.LogInformation("Seed system account success");
            
        }
    }
}