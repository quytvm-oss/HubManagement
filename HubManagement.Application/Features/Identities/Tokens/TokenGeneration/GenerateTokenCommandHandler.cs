using System.Security.Cryptography;
using System.Text;
using HubManagement.Application.DTOs;
using HubManagement.Application.Services;
using HubManagement.BuildingBlock.Core.Abstractions;
using Mediator;
using Microsoft.Extensions.Logging;

namespace HubManagement.Application.Features.Identities.Tokens.TokenGeneration;

public class GenerateTokenCommandHandler(
    IIdentityService identityService,
    ITokenService tokenService,
    IRequestContext requestContext,
    ISessionService sessionService,
    ILogger<GenerateTokenCommandHandler> logger)
    : ICommandHandler<GenerateTokenCommand, TokenResponse>
{

    public async ValueTask<TokenResponse> Handle(GenerateTokenCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var ip = requestContext.IpAddress ?? "unknown";
        var ua = requestContext.UserAgent ?? "unknown";

        var identityResult = await identityService
            .ValidateCredentialsAsync(command.Email, command.Password, command.TwoFactorCode, cancellationToken);

        if (identityResult is null)
        {
            
            throw new UnauthorizedAccessException("Invalid credentials.");
        }
        
        var (subject, claims) = identityResult.Value;
        
        
        // Issue token
        var token = await tokenService.IssueAsync(subject.ToString(), claims, cancellationToken);
        
        await identityService.StoreRefreshTokenAsync(subject.ToString(), token.RefreshToken, token.RefreshTokenExpiresAt, cancellationToken);
        
        // Create user session for session management (non-blocking, fail gracefully)
        try
        {
            var refreshTokenHash = Sha256Short(token.RefreshToken);
            await sessionService.CreateSessionAsync(
                subject,
                refreshTokenHash, 
                ip,
                ua,
                token.RefreshTokenExpiresAt, 
                cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Failed to create user session for user {UserId}. Login will continue without session tracking.", subject);
        }
        
        return token;
    }

    private static string Sha256Short(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hash.AsSpan(0, 8));
    }
}