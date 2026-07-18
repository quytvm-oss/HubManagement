using System.Globalization;
using HubManagement.Application.IntegrationEvents;
using HubManagement.BuildingBlock.Core.Exceptions;
using HubManagement.Domain.Entities;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rebus.Bus;

namespace HubManagement.Application.Features.Identities.Users.ResendConfirmationEmail;

public class ResendConfirmationEmailCommandHandler(UserManager<ApplicationUser> userManager, IBus bus)
    : ICommandHandler<ResendConfirmationEmailCommand>
{

    public async ValueTask<Unit> Handle(ResendConfirmationEmailCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.Users
                       .Where(u => u.Id == command.UserId)
                       .FirstOrDefaultAsync(cancellationToken)
                   ?? throw new NotFoundException($"User {command.UserId} was not found.");

        if (user.EmailConfirmed)
        {
            throw new CustomException(string.Format(
                CultureInfo.InvariantCulture,
                "The email for {0} is already confirmed.",
                user.Email));
        }
        
        await bus.Send(new SendConfirmationEmailEvent()
        {
            UserId = user.Id,
            Email = user.Email,
            Origin = command.Origin ?? string.Empty
        });
        
        return Unit.Value;
    }
}