using HubManagement.Application.Services;
using HubManagement.BuildingBlock.Core.Exceptions;
using HubManagement.BuildingBlock.Infrastructure.Authorization;
using HubManagement.BuildingBlock.Infrastructure.Cache;
using HubManagement.BuildingBlock.Infrastructure.Cache.Abstractions;
using HubManagement.Domain.Entities;
using HubManagement.Infrastructure.DataContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HubManagement.Infrastructure.Services;

internal sealed class UserPermissionService(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    HubDbContext db,
    ICacheService cache)  : IUserPermissionService
{
    public async Task<List<string>?> GetPermissionsAsync(string userId, CancellationToken ct = default)
    {
        var permissions = await cache.GetOrSetAsync(
            GetPermissionCacheKey(userId),
            async () =>
            {
                var user = await userManager.FindByIdAsync(userId);

                _ = user ?? throw new UnauthorizedException();

                var userRoles = await userManager.GetRolesAsync(user);
                var permissions = new List<string>();
                foreach (var role in await roleManager.Roles
                             .Where(r => userRoles.Contains(r.Name!))
                             .ToListAsync(ct))
                {
                    permissions.AddRange(await db.RoleClaims
                        .Where(rc => rc.RoleId == role.Id && rc.ClaimType == ClaimConstants.Permission)
                        .Select(rc => rc.ClaimValue!)
                        .ToListAsync(ct));
                }
                return permissions.Distinct().ToList();
            },
            cancellationToken: ct);

        return permissions;
    }

    public async Task<bool> HasPermissionAsync(string userId, string permissionName, CancellationToken ct = default)
    {
        var permissions = await GetPermissionsAsync(userId, ct);

        return permissions?.Contains(permissionName) ?? false;
    }

    public Task InvalidatePermissionCacheAsync(string userId, CancellationToken cancellationToken)
    {
        return cache.RemoveItemAsync(GetPermissionCacheKey(userId), cancellationToken);
    }
    
    public static string GetPermissionCacheKey(string userId)
    {
        return $"perm:{userId}";
    }
}