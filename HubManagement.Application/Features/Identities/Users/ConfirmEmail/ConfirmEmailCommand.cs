using Mediator;

namespace HubManagement.Application.Features.Identities.Users.ConfirmEmail;

public sealed record ConfirmEmailCommand(Guid UserId, string Code) : ICommand<string>;