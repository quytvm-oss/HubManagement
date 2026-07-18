using HubManagement.BuildingBlock.Core.Abstractions;
using HubManagement.BuildingBlock.Core.Exceptions;
using HubManagement.Domain.Entities;
using Mediator;
using Microsoft.AspNetCore.Identity;

namespace HubManagement.Application.Features.Identities.Users.ChangePassword;

public class ChangePasswordCommandHandler(ICurrentUser currentUser, UserManager<ApplicationUser> userManager)
    : ICommandHandler<ChangePasswordCommand, string>
{
    public async ValueTask<string> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (!currentUser.IsAuthenticated())
        {
            throw new InvalidOperationException("User is not authenticated.");
        }

        var userId = currentUser.GetUserId().ToString();
        
        var user = await userManager.FindByIdAsync(userId);

        _ = user ?? throw new NotFoundException("user not found");

        var result = await userManager.ChangePasswordAsync(user, command.Password, command.ConfirmNewPassword);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            throw new CustomException("failed to change password", errors);
        }
        
        return "password reset email sent";
    }
}