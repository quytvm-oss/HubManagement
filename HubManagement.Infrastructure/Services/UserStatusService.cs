// using System.Net;
// using HubManagement.Application.Services;
// using HubManagement.BuildingBlock.Core.Abstractions;
// using HubManagement.BuildingBlock.Core.Exceptions;
// using HubManagement.BuildingBlock.Infrastructure.Authorization;
// using HubManagement.Domain.Entities;
// using Microsoft.AspNetCore.Identity;
// using Microsoft.EntityFrameworkCore;
//
// namespace HubManagement.Infrastructure.Services;
//
// public sealed class UserStatusService(
//     UserManager<ApplicationUser> userManager,
//     ICurrentUser currentUser) : IUserStatusService
// {
//     public async Task ToggleStatusAsync(bool activateUser, string userId, CancellationToken ct = default)
//     {
//         var context = await BuildToggleContextAsync(userId, activateUser, ct);
//
//         await ValidateTogglePermissionsAsync(context, ct);
//         ApplyStatusChange(context);
//         var result = await userManager.UpdateAsync(context.TargetUser);
//         if (!result.Succeeded)
//         {
//             throw new CustomException("Toggle status failed", result.Errors.Select(e => e.Description).ToList(), HttpStatusCode.BadRequest);
//         }
//     }
//
//     public Task DeleteAsync(string userId, CancellationToken ct = default)
//         => ToggleStatusAsync(activateUser: false, userId, ct);
//     
//     #region internals
//
//     private async Task<ToggleStatusContext> BuildToggleContextAsync(string userId, bool activateUser,
//         CancellationToken ct = default)
//     {
//         var actorId = currentUser.GetUserId();
//         if (actorId == Guid.Empty)
//         {
//             throw new UnauthorizedException("authenticated user required to toggle status");
//         }
//         
//         var actor = await userManager.FindByIdAsync(actorId.ToString())
//             ?? throw new NotFoundException("authenticated user not found");
//
//         var targetUser = await userManager.Users
//             .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken: ct) 
//             ?? throw new NotFoundException("user not found");
//         
//         return new ToggleStatusContext(
//             ActorId: actorId,
//             Actor: actor,
//             TargetUser: targetUser,
//             ActivateUser: activateUser);
//     }
//     
//     private static void ApplyStatusChange(ToggleStatusContext context)
//     {
//         if (context.ActivateUser)
//         {
//             context.TargetUser.Activate(context.ActorId.ToString());
//         }
//         else
//         {
//             context.TargetUser.Deactivate(context.ActorId.ToString(), "Status toggled by administrator");
//         }
//     }
//
//     private async Task ValidateTogglePermissionsAsync(ToggleStatusContext context, CancellationToken ct = default)
//     {
//         if (!await userManager.IsInRoleAsync(context.Actor, RoleConstants.Admin))
//         {
//             throw new ForbiddenException("Only administrators can change user status.");
//         }
//
//         if (!context.ActivateUser && context.ActorId.ToString() == context.TargetUser.Id)
//         {
//             throw new CustomException("Users cannot deactivate themselves.", Array.Empty<string>(), HttpStatusCode.BadRequest);
//         }
//
//         if (!context.ActivateUser && await userManager.IsInRoleAsync(context.TargetUser, RoleConstants.Admin))
//         {
//             throw new CustomException("Administrators cannot be deactivated.", Array.Empty<string>(), HttpStatusCode.BadRequest);
//         }
//     }
//     
//     private sealed record ToggleStatusContext(
//         Guid ActorId,
//         ApplicationUser Actor,
//         ApplicationUser TargetUser,
//         bool ActivateUser);
//
//     #endregion
// }