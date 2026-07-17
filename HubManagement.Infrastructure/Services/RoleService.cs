using System.Net;
using System.Security.Claims;
using HubManagement.Application.DTOs;
using HubManagement.Application.Services;
using HubManagement.BuildingBlock.Core.Abstractions;
using HubManagement.BuildingBlock.Core.Common;
using HubManagement.BuildingBlock.Core.Exceptions;
using HubManagement.BuildingBlock.Infrastructure.Authorization;
using HubManagement.Domain.Entities;
using HubManagement.Infrastructure.Authorization.Constants;
using HubManagement.Infrastructure.DataContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace HubManagement.Infrastructure.Services;

public sealed class RoleService(
    RoleManager<ApplicationRole> roleManager,
    HubDbContext context,
    ICurrentUser currentUser,
    IUserPermissionService userPermissionService) : IRoleService
{
    public async Task<PagedResponse<RoleDto>> GetRolesAsync(int pageNumber = 1, int pageSize = 20,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, pageNumber);
        var size = Math.Clamp(pageSize, 1, 200);

        var query = roleManager.Roles.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower().Trim();
            query = query.Where(r => r.Name != null && r.Name.ToLower().Contains(term)
                                     || (r.Description != null && r.Description.ToLower().Contains(term)));
        }

        var totalCount = await query.LongCountAsync(cancellationToken);
        var roles = await query.OrderBy(r => r.Name)
            .Skip((page - 1) * size).Take(size)
            .Select(r => new RoleDto() { Id = r.Id, Name = r.Name!, Description = r.Description, })
            .ToListAsync(cancellationToken);

        return new PagedResponse<RoleDto>
        {
            Items = roles,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = size,
            TotalPages = (int)Math.Ceiling(totalCount / (double)size)
        };
    }

    public async Task<RoleDto?> GetRoleAsync(string id, CancellationToken cancellationToken = default)
    {
        var role = await roleManager.Roles.FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
                   ?? throw new NotFoundException("Role not found.");

        return new RoleDto() { Id = role.Id, Name = role.Name!, Description = role.Description, };
    }

    public async Task<RoleDto> CreateOrUpdateRoleAsync(string roleId, string name, string description,
        CancellationToken cancellationToken = default)
    {
        ApplicationRole? role = string.IsNullOrEmpty(roleId)
            ? null
            : await roleManager.FindByIdAsync(roleId);

        if (role is not null)
        {
            // Protect the EXISTING role's identity — a system role must never be
            // renamed/edited, regardless of what `name` the caller passes in.
            EnsureNotSystemRole(role.Name, "System roles cannot be created or updated.");

            role.Name = name;
            role.Description = description;

            var updateResult = await roleManager.UpdateAsync(role);
            if (!updateResult.Succeeded)
            {
                throw new CustomException("Update role failed",
                    updateResult.Errors.Select(e => e.Description).ToList(), HttpStatusCode.BadRequest);
            }
        }
        else
        {
            // Protect against creating a NEW role that collides with a system role's name.
            EnsureNotSystemRole(name, "Cannot create a role using a system role's name.");

            role = new ApplicationRole(name, description);

            var createResult = await roleManager.CreateAsync(role);
            if (!createResult.Succeeded)
            {
                throw new CustomException("Create role failed",
                    createResult.Errors.Select(e => e.Description).ToList(), HttpStatusCode.BadRequest);
            }
        }

        return new RoleDto() { Id = role.Id, Name = role.Name!, Description = role.Description };
    }

    public async Task DeleteRoleAsync(string id, CancellationToken cancellationToken = default)
    {
        ApplicationRole? role = await roleManager.FindByIdAsync(id)
                                 ?? throw new NotFoundException("role not found");

        EnsureNotSystemRole(role.Name, "System roles cannot be deleted.");

        var deleteResult = await roleManager.DeleteAsync(role);
        if (!deleteResult.Succeeded)
        {
            throw new CustomException("Delete role failed",
                deleteResult.Errors.Select(e => e.Description).ToList(), HttpStatusCode.BadRequest);
        }

        // Only invalidate caches after the role is actually gone.
        await InvalidateAffectedUsersAsync(id, cancellationToken).ConfigureAwait(false);
    }

    public async Task<RoleDto> GetWithPermissionsAsync(string id, CancellationToken cancellationToken = default)
    {
        var role = await GetRoleAsync(id, cancellationToken);

        role.Permissions = await context.RoleClaims
            .AsNoTracking()
            .Where(rc => rc.RoleId == id && rc.ClaimType == ClaimConstants.Permission)
            .Select(rc => rc.ClaimValue!)
            .ToListAsync(cancellationToken);

        return role;
    }

    public async Task<string> UpdatePermissionsAsync(string roleId, List<string> permissions,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(permissions);

        var role = await roleManager.FindByIdAsync(roleId)
                   ?? throw new NotFoundException("role not found");

        EnsureNotSystemRole(role.Name, "System role permissions are managed by the framework and cannot be modified.");

        var currentClaims = await roleManager.GetClaimsAsync(role);
        await RemoveRevokedPermissionsAsync(role, currentClaims, permissions, cancellationToken);
        await AddNewPermissionsAsync(role, currentClaims, permissions, cancellationToken);

        await InvalidateAffectedUsersAsync(roleId, cancellationToken).ConfigureAwait(false);

        return "Permissions updated successfully.";
    }

    #region internals

    private static void EnsureNotSystemRole(string? roleName, string message)
    {
        if (!string.IsNullOrEmpty(roleName) && RoleConstants.IsDefault(roleName))
        {
            throw new CustomException(message, Array.Empty<string>(), HttpStatusCode.BadRequest);
        }
    }

    // Invalidate every direct holder of this role (AspNetUserRoles) whose effective
    // permissions may have shifted from a role mutation (permission change or role deletion).
    private async Task InvalidateAffectedUsersAsync(string roleId, CancellationToken cancellationToken)
    {
        var directUserIds = await context.UserRoles
            .Where(ur => ur.RoleId == roleId)
            .Select(ur => ur.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var userId in directUserIds)
        {
            await userPermissionService.InvalidatePermissionCacheAsync(userId, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task AddNewPermissionsAsync(ApplicationRole role, IList<Claim> currentClaims,
        List<string> permissions, CancellationToken cancellationToken = default)
    {
        var newPermissions = permissions.Where(p => !string.IsNullOrEmpty(p) && currentClaims.All(c => c.Value != p))
            .ToList();

        foreach (var permission in newPermissions)
        {
            context.RoleClaims.Add(new ApplicationRoleClaim()
            {
                RoleId = role.Id,
                ClaimType = ClaimConstants.Permission,
                ClaimValue = permission,
                CreatedBy = currentUser.GetUserId().ToString(),
            });
        }

        if (newPermissions.Count > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task RemoveRevokedPermissionsAsync(ApplicationRole role, IList<Claim> currentClaims,
        List<string> permissions, CancellationToken cancellationToken = default)
    {
        var claimsToRemove = currentClaims.Where(c => !permissions.Exists(p => p == c.Value));

        foreach (var claim in claimsToRemove)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await roleManager.RemoveClaimAsync(role, claim);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(error => error.Description).ToList();
                throw new CustomException("operation failed", errors);
            }
        }
    }

    #endregion
}