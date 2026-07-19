using System.Net;
using HubManagement.BuildingBlock.Core.Abstractions;
using HubManagement.BuildingBlock.Core.Exceptions;
using HubManagement.Domain.Entities;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HubManagement.Application.Features.Identities.Users.ToggleStatus;

public class ToggleUserStatusCommandHandler(UserManager<ApplicationUser> userManager, ICurrentUser currentUser)
    : ICommandHandler<ToggleUserStatusCommand>
{
    public async ValueTask<Unit> Handle(ToggleUserStatusCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        
        var actorId = currentUser.GetUserId();
        if (actorId == Guid.Empty)
        {
            throw new UnauthorizedException("authenticated user required to toggle status");
        }
        
        var actor = await userManager.FindByIdAsync(actorId.ToString())
                    ?? throw new UnauthorizedException("current user not found");

        var targetUser = await userManager.Users
                             .Where(u => u.Id == command.UserId)
                             .FirstOrDefaultAsync(cancellationToken)
                         ?? throw new NotFoundException("User Not Found.");
        
        if (command.ActivateUser)
        {
            targetUser.Activate(actor.Id.ToString());
        }
        else
        {
            targetUser.Deactivate(actor.ToString(), "Status toggled by administrator");
        }
        
        var result = await userManager.UpdateAsync(targetUser);
        if (!result.Succeeded)
        {
            throw new CustomException("Toggle status failed", result.Errors
                .Select(e => e.Description).ToList(), HttpStatusCode.BadRequest);
        }
        
        return Unit.Value;
    }
}