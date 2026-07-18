using System.Text;
using HubManagement.Application.Contracts;
using HubManagement.BuildingBlock.Core.Exceptions;
using HubManagement.Domain.Entities;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace HubManagement.Application.Features.Identities.Users.ResetPassword;

public class ResetPasswordCommandHandler(UserManager<ApplicationUser> userManager)
    : ICommandHandler<ResetPasswordCommand, string>
{
    public async ValueTask<string> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        
        var user = await userManager.FindByEmailAsync(command.Email);
        if (user == null)
        {
            throw new NotFoundException("user not found");
        }

        var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(command.Token));
        var result = await userManager.ResetPasswordAsync(user, token, command.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            throw new CustomException("error resetting password", errors);
        }
        return "Password has been reset.";
    }
}