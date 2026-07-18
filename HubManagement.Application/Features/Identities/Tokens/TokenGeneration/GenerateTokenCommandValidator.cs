using FluentValidation;

namespace HubManagement.Application.Features.Identities.Tokens.TokenGeneration;

public class GenerateTokenCommandValidator : AbstractValidator<GenerateTokenCommand>
{
    public GenerateTokenCommandValidator()
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .EmailAddress();
        
        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty();
    }
}