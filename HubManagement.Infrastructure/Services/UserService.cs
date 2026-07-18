using System.Security.Claims;
using HubManagement.Application.DTOs;
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

internal sealed class UserService(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    HubDbContext db,
    ICacheService cache) : IUserService
{
    public async Task<bool> ExistsWithEmailAsync(string email, Guid? exceptId = null, CancellationToken ct = default)
    {
        return await userManager.FindByEmailAsync(email.Trim()) is { } user && user.Id != exceptId;
    }

    public async Task<bool> ExistsWithNameAsync(string name, CancellationToken ct = default)
    {
        return await userManager.FindByNameAsync(name) is not null;
    }

    public async Task<bool> ExistsWithPhoneNumberAsync(string phoneNumber, Guid? exceptId = null, CancellationToken ct = default)
    {
        var normalized = NormalizePhoneNumber(phoneNumber);
        return await userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == normalized, ct) is { } user && user.Id != exceptId;
    }

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

    #region internals

    // Minimal E.164-ish normalization so the same number in different formats
    // (e.g. "0901234567" vs "+84901234567") compares equal. Strips whitespace,
    // dashes, and parens; keeps a leading "+" if present.
    private static string NormalizePhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return phoneNumber;
        }

        var trimmed = phoneNumber.Trim();
        var hasLeadingPlus = trimmed.StartsWith('+');

        var digits = new string(trimmed.Where(char.IsDigit).ToArray());

        return hasLeadingPlus ? $"+{digits}" : digits;
    }

    #endregion
}