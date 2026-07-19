using FluentValidation;
using Mediator;

namespace HubManagement.Application.Features.Identities.Users.ToggleStatus;

public class ToggleUserStatusCommand : ICommand<Unit>
{
    public bool ActivateUser { get; set; }
    public Guid? UserId { get; set; }
}

public sealed class ToggleUserStatusCommandValidator : AbstractValidator<ToggleUserStatusCommand>
{
    public ToggleUserStatusCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}