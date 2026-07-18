using System.Net;
using HubManagement.Application.Constants;
using HubManagement.Application.IntegrationEvents;
using HubManagement.BuildingBlock.Core.Exceptions;
using HubManagement.Domain.Entities;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Rebus.Bus;

namespace HubManagement.Application.Features.Identities.Users.RegisterUser;

public class RegisterUserCommandHandler(UserManager<ApplicationUser> userManager, IBus bus)
    : ICommandHandler<RegisterUserCommand, RegisterUserResponse>
{
    public async ValueTask<RegisterUserResponse> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        ValidatePasswordMatch(command.Password, command.ConfirmPassword);
        
        var user = new ApplicationUser
        {
            Email = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            UserName = command.UserName,
            PhoneNumber = command.PhoneNumber,
            IsActive = true,
            EmailConfirmed = false,
            PhoneNumberConfirmed = false,
        };

        var result = await userManager.CreateAsync(user, command.Password);
        if (!result.Succeeded)
        {
            // Identity create failures (duplicate email/username, password policy, …) are
            // client-input errors, not server faults — surface them as 400 with the specific
            // reasons so the caller sees *why* registration failed, not a bare 500.
            var errors = result.Errors.Select(error => error.Description).ToList();
            throw new CustomException(
                "Unable to register the user.",
                errors,
                HttpStatusCode.BadRequest);
        }
        
        await userManager.AddToRoleAsync(user, RoleConstants.Basic);

        await bus.Send(new SendConfirmationEmailEvent()
        {
            UserId = user.Id,
            Email = user.Email,
            Origin = command.Origin ?? string.Empty
        });
        
        return new RegisterUserResponse(user.Id);
    }
    
    private static void ValidatePasswordMatch(string password, string confirmPassword)
    {
        if (password != confirmPassword)
        {
            throw new CustomException(
                "Passwords do not match.",
                errors: null,
                HttpStatusCode.BadRequest);
        }
    }
}