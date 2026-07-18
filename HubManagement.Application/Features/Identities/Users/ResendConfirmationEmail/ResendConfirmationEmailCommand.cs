using FluentValidation;
using Mediator;

namespace HubManagement.Application.Features.Identities.Users.ResendConfirmationEmail;

public sealed record ResendConfirmationEmailCommand(Guid UserId, string Origin) : ICommand<Unit>;

public sealed class ResendConfirmationEmailCommandValidator : AbstractValidator<ResendConfirmationEmailCommand>
{
    public ResendConfirmationEmailCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}