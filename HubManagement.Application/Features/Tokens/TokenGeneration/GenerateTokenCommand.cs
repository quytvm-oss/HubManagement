using HubManagement.Application.DTOs;
using Mediator;

namespace HubManagement.Application.Features.Tokens.TokenGeneration;

public record GenerateTokenCommand(
    string Email,
    string Password,
    string? TwoFactorCode = null) : ICommand<TokenResponse>;