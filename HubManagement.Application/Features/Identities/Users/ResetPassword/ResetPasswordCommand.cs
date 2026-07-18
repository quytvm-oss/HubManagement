using Mediator;

namespace HubManagement.Application.Features.Identities.Users.ResetPassword;

public class ResetPasswordCommand : ICommand<string>
{
    public string Email { get; set; } = default!;

    public string Password { get; set; } = default!;

    public string Token { get; set; } = default!;
}