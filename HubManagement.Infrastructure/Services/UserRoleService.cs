using System.Net;
using HubManagement.Application.DTOs;
using HubManagement.Application.Services;
using HubManagement.BuildingBlock.Core.Abstractions;
using HubManagement.BuildingBlock.Core.Exceptions;
using HubManagement.BuildingBlock.Infrastructure.Authorization;
using HubManagement.Domain.Entities;
using HubManagement.Infrastructure.DataContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HubManagement.Infrastructure.Services;

internal sealed class UserRoleService(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    HubDbContext db,
    ICurrentUser currentUser,
    IUserPermissionService userPermissionService) : IUserRoleService
{
    public async Task<string> AssignRoleAsync(string userId, List<UserRoleDto> userRoles, CancellationToken ct = default)
    {
        var user = await userManager.Users.FirstOrDefaultAsync(x => x.Id == userId, ct)
            ?? throw new NotFoundException("user not found");

        await ValidateAdminRoleChangeAsync(user, userRoles);
        
        var assignedRoles = await ProcessRoleAssignmentsAsync(user, userRoles);
        
        // Any role mutation (add or remove) invalidates the cached permission set; flush
        // unconditionally rather than gating on assignedRoles, which only tracks additions.
        await userPermissionService.InvalidatePermissionCacheAsync(user.Id, ct).ConfigureAwait(false);
        
        return "User Roles Updated Successfully.";
    }

    public async Task<List<UserRoleDto>> GetUserRolesAsync(string userId, CancellationToken ct = default)
    {
        var user = await userManager.Users.FirstOrDefaultAsync(x => x.Id == userId, ct) 
            ?? throw new NotFoundException("user not found");
        
        var roles = await roleManager.Roles.AsNoTracking().ToListAsync(ct)
                    ?? throw new NotFoundException("roles not found");

        var memberships = await userManager.GetRolesAsync(user).ConfigureAwait(false);
        var membershipsSet = new HashSet<string>(memberships, StringComparer.OrdinalIgnoreCase);
        
        var userRoles = new List<UserRoleDto>();
        foreach (var role in roles)
        {
            userRoles.Add(new UserRoleDto()
            {
                RoleId =  role.Id,
                RoleName = role.Name,
                Description = role.Description,
                Enable = membershipsSet.Contains(role.Name!)
            });
        }
        
        return userRoles;
    }

    #region internal methods

    private async Task ValidateAdminRoleChangeAsync(ApplicationUser user, List<UserRoleDto> userRoles)
    {
        bool isRemovingAdminRole = userRoles.Exists(x => !x.Enable && x.RoleName == RoleConstants.Admin);
        if (!isRemovingAdminRole) return;
        
        bool userIsAdmin = await userManager.IsInRoleAsync(user, RoleConstants.Admin);
        if (!userIsAdmin) return;
        
        // Administrators cannot demote themselves — they would lose access immediately on the next request,
        // and would need another admin to restore them.
        var actorId = currentUser.GetUserId();
        if (actorId != Guid.Empty && string.Equals(actorId.ToString(), user.Id, StringComparison.Ordinal))
        {
            throw new CustomException(
                "Administrators cannot remove their own admin role.",
                Array.Empty<string>(),
                HttpStatusCode.BadRequest);
        }
        
        int adminCount = (await userManager.GetUsersInRoleAsync(RoleConstants.Admin)).Count();
        if (adminCount <= 1)
        {
            throw new CustomException(
                "Tenant must retain at least one administrator.",
                Array.Empty<string>(),
                HttpStatusCode.BadRequest);
        }
        
    }
    
    private async Task<List<string>> ProcessRoleAssignmentsAsync(ApplicationUser user, List<UserRoleDto> userRoles)
    {
        var assignedRoles = new List<string>();

        foreach (var userRole in userRoles)
        {
            if (await roleManager.FindByNameAsync(userRole.RoleName!) is null)
            {
                continue;
            }

            if (userRole.Enable)
            {
                if (!await userManager.IsInRoleAsync(user, userRole.RoleName!))
                {
                    await userManager.AddToRoleAsync(user, userRole.RoleName!);
                    assignedRoles.Add(userRole.RoleName!);
                }
            }
            else
            {
                await userManager.RemoveFromRoleAsync(user, userRole.RoleName!);
            }
        }

        return assignedRoles;
    }

    #endregion
}