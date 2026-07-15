// using System.Data;
// using System.Net;
// using HubManagement.Application.DTOs;
// using HubManagement.Application.Services;
// using HubManagement.BuildingBlock.Core.Abstractions;
// using HubManagement.BuildingBlock.Core.Exceptions;
// using HubManagement.BuildingBlock.Infrastructure.Authorization;
// using HubManagement.Domain.Entities;
// using HubManagement.Infrastructure.DataContext;
// using Microsoft.AspNetCore.Identity;
// using Microsoft.EntityFrameworkCore;
//
// namespace HubManagement.Infrastructure.Services;
//
// internal sealed class UserRoleService(
//     UserManager<ApplicationUser> userManager,
//     RoleManager<ApplicationRole> roleManager,
//     HubDbContext db,
//     ICurrentUser currentUser,
//     IUserPermissionService userPermissionService) : IUserRoleService
// {
//     public async Task<string> AssignRoleAsync(string userId, List<UserRoleDto> userRoles, CancellationToken ct = default)
//     {
//         var user = await userManager.Users.FirstOrDefaultAsync(x => x.Id == userId, ct)
//             ?? throw new NotFoundException("user not found");
//
//         // Wrap validate + apply in one transaction so a concurrent request touching the
//         // same tenant's Admin role can't slip past the "at least one active admin" check
//         // (TOCTOU: without this, two parallel demotions could each read count=2 and both commit).
//         await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
//
//         await ValidateAdminRoleChangeAsync(user, userRoles);
//
//         await ProcessRoleAssignmentsAsync(user, userRoles);
//
//         await transaction.CommitAsync(ct);
//
//         // Any role mutation (add or remove) invalidates the cached permission set; flush
//         // unconditionally rather than gating on assignedRoles, which only tracks additions.
//         await userPermissionService.InvalidatePermissionCacheAsync(user.Id, ct).ConfigureAwait(false);
//
//         return "User Roles Updated Successfully.";
//     }
//
//     public async Task<List<UserRoleDto>> GetUserRolesAsync(string userId, CancellationToken ct = default)
//     {
//         var user = await userManager.Users.FirstOrDefaultAsync(x => x.Id == userId, ct) 
//             ?? throw new NotFoundException("user not found");
//         
//         var roles = await roleManager.Roles.AsNoTracking().ToListAsync(ct);
//
//         var memberships = await userManager.GetRolesAsync(user).ConfigureAwait(false);
//         var membershipsSet = new HashSet<string>(memberships, StringComparer.OrdinalIgnoreCase);
//         
//         var userRoles = new List<UserRoleDto>();
//         foreach (var role in roles)
//         {
//             userRoles.Add(new UserRoleDto()
//             {
//                 RoleId =  role.Id,
//                 RoleName = role.Name,
//                 Description = role.Description,
//                 Enable = membershipsSet.Contains(role.Name!)
//             });
//         }
//         
//         return userRoles;
//     }
//
//     #region internal methods
//
//     private async Task ValidateAdminRoleChangeAsync(ApplicationUser user, List<UserRoleDto> userRoles)
//     {
//         bool isRemovingAdminRole = userRoles.Exists(x => !x.Enable && x.RoleName == RoleConstants.Admin);
//         if (!isRemovingAdminRole) return;
//         
//         bool userIsAdmin = await userManager.IsInRoleAsync(user, RoleConstants.Admin);
//         if (!userIsAdmin) return;
//         
//         // Administrators cannot demote themselves — they would lose access immediately on the next request,
//         // and would need another admin to restore them.
//         var actorId = currentUser.GetUserId();
//         if (actorId != Guid.Empty && string.Equals(actorId.ToString(), user.Id, StringComparison.Ordinal))
//         {
//             throw new CustomException(
//                 "Administrators cannot remove their own admin role.",
//                 Array.Empty<string>(),
//                 HttpStatusCode.BadRequest);
//         }
//         
//         var admins = await userManager.GetUsersInRoleAsync(RoleConstants.Admin);
//         var activeAdminCount = admins.Count(u => u.IsActive);
//         if (activeAdminCount <= 1)
//         {
//             throw new CustomException(
//                 "Tenant must retain at least one active administrator.",
//                 Array.Empty<string>(),
//                 HttpStatusCode.BadRequest);
//         }
//     }
//     
//     private async Task<List<string>> ProcessRoleAssignmentsAsync(ApplicationUser user, List<UserRoleDto> userRoles)
//     {
//         var assignedRoles = new List<string>();
//         var errors = new List<string>();
//
//         foreach (var userRole in userRoles)
//         {
//             if (await roleManager.FindByNameAsync(userRole.RoleName!) is null)
//             {
//                 errors.Add($"Role '{userRole.RoleName}' does not exist.");
//                 continue;
//             }
//
//             if (userRole.Enable)
//             {
//                 if (!await userManager.IsInRoleAsync(user, userRole.RoleName!))
//                 {
//                     var result = await userManager.AddToRoleAsync(user, userRole.RoleName!);
//                     if (!result.Succeeded)
//                     {
//                         errors.AddRange(result.Errors.Select(e => $"{userRole.RoleName}: {e.Description}"));
//                         continue;
//                     }
//                     assignedRoles.Add(userRole.RoleName!);
//                 }
//             }
//             else
//             {
//                 var result = await userManager.RemoveFromRoleAsync(user, userRole.RoleName!);
//                 if (!result.Succeeded)
//                 {
//                     errors.AddRange(result.Errors.Select(e => $"{userRole.RoleName}: {e.Description}"));
//                 }
//             }
//         }
//
//         if (errors.Count > 0)
//         {
//             throw new CustomException("Some role assignments failed.", errors, HttpStatusCode.BadRequest);
//         }
//
//         return assignedRoles;
//     }
//
//     #endregion
// }