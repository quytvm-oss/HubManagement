using System.Globalization;
using System.Text;
using HubManagement.BuildingBlock.Core.Exceptions;
using HubManagement.Domain.Entities;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace HubManagement.Application.Features.Identities.Users.ConfirmEmail;

public class ConfirmEmailCommandHandler(UserManager<ApplicationUser> userManager)
    : ICommandHandler<ConfirmEmailCommand, string>
{
    public async ValueTask<string> Handle(ConfirmEmailCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        
        var user = await userManager.Users
            .Where(u => u.Id == command.UserId && !u.EmailConfirmed)
            .FirstOrDefaultAsync(cancellationToken);
        
        _ = user ?? throw new CustomException("An error occurred while confirming E-Mail.");
        var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(command.Code));
        var result = await userManager.ConfirmEmailAsync(user, code);
        
        return result.Succeeded
            ? string.Format(CultureInfo.InvariantCulture, "Account Confirmed for E-Mail {0}. You can now use the /api/tokens endpoint to generate JWT.", user.Email)
            : throw new CustomException(string.Format(CultureInfo.InvariantCulture, "An error occurred while confirming {0}", user.Email));
    }
}