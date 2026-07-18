using Mediator;

namespace HubManagement.Application.Features.Tokens.RefreshToken;

public record RefreshTokenCommand(string? Token, string RefreshToken) : ICommand<RefreshTokenCommandResponse>;

public sealed record RefreshTokenCommandResponse(
    string AccessToken,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt);