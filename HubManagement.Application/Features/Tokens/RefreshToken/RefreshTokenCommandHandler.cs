using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HubManagement.Application.Services;
using HubManagement.BuildingBlock.Core.Abstractions;
using HubManagement.BuildingBlock.Core.Exceptions;
using Mediator;
using Microsoft.Extensions.Logging;

namespace HubManagement.Application.Features.Tokens.RefreshToken;

public class RefreshTokenCommandHandler(
    ITokenService tokenService,
    IIdentityService identityService,
    IRequestContext requestContext,
    ISessionService sessionService,
    ILogger<RefreshTokenCommandHandler> logger)
    : ICommandHandler<RefreshTokenCommand, RefreshTokenCommandResponse>
{

    public async ValueTask<RefreshTokenCommandResponse> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var clientId = requestContext.ClientId;
        
        var validated = await identityService.ValidateRefreshTokenAsync(command.RefreshToken, cancellationToken);

        if (validated is null)
        {
            throw new UnauthorizedException("Invalid refresh token.");
        }
        
        var (subject, claims) = validated.Value;
        var refreshTokenHash = Sha256Short(command.RefreshToken);
        var isSessionValid = await sessionService.ValidateSessionAsync(refreshTokenHash, cancellationToken);
        if (!isSessionValid)
        {
            throw new UnauthorizedException("Session has been revoked.");
        }
        
        // Optionally, cross-check the provided access token subject
        var handler = new JwtSecurityTokenHandler();
        JwtSecurityToken? parsedAccessToken = null;
        try
        {
            parsedAccessToken = handler.ReadJwtToken(command.Token);
        }
        catch (Exception e)
        {
            logger.LogDebug(e, "Failed to parse access token during refresh; relying on refresh-token validation only");
        }

        if (parsedAccessToken is not null)
        {
            var accessTokenSubject = parsedAccessToken.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                 ?? parsedAccessToken.Subject;

            if (!string.IsNullOrEmpty(accessTokenSubject) &&
                !string.Equals(accessTokenSubject, subject.ToString(), StringComparison.Ordinal))
            {
                throw new UnauthorizedException("Access token subject mismatch.");
            }
        }
        
        
        // Issue new tokens
        var newToken = await tokenService.IssueAsync(subject.ToString(), claims, cancellationToken);
        
        // Persist rotated refresh token for this user
        await identityService.StoreRefreshTokenAsync(subject.ToString(), newToken.RefreshToken, newToken.RefreshTokenExpiresAt, cancellationToken);
        
        // Update the session with the new refresh token hash
        var newRefreshTokenHash = Sha256Short(newToken.RefreshToken);
        await sessionService.UpdateSessionRefreshTokenAsync(
            refreshTokenHash,
            newRefreshTokenHash,
            newToken.RefreshTokenExpiresAt,
            cancellationToken);
        
        return new RefreshTokenCommandResponse(
            AccessToken: newToken.AccessToken,
            RefreshToken: newToken.RefreshToken,
            RefreshTokenExpiresAt: newToken.RefreshTokenExpiresAt);
    }
    
    private static string Sha256Short(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hash.AsSpan(0, 8));
    }
}