using FluentValidation;
using Mediator;

namespace HubManagement.Application.Features.Identities.Users.ForgotPassword;

public class ForgotPasswordCommand : ICommand<string>
{
    public string Email { get; set; } = default!;
}

public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(p => p.Email).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .EmailAddress();
    }
}