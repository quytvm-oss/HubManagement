using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using HubManagement.Application.Services;
using HubManagement.BuildingBlock.Core.Exceptions;
using HubManagement.BuildingBlock.Infrastructure.Authorization;
using HubManagement.Domain.Entities;
using HubManagement.Infrastructure.DataContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HubManagement.Infrastructure.Services;

public class IdentityService(
    ILogger<IdentityService> logger,
    TimeProvider timeProvider,
    HubDbContext dbContext,
    UserManager<ApplicationUser> userManager) : IIdentityService
{
    public async Task<(string Subject, IEnumerable<Claim> Claims)?> 
        ValidateCredentialsAsync(string email, string password, string? twoFactorCode = null,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(password);
        
        var user = await FindAndValidateUserByCredentialsAsync(email, password);
        ValidateUserStatus(user);
        
        if (user.TwoFactorEnabled)
        {
            await VerifyTwoFactorOrThrowAsync(user, twoFactorCode);
        }

        var claims = await BuildUserClaimsAsync(user, ct);
        return (user.Id, claims);
    }

    public async Task<(string Subject, IEnumerable<Claim> Claims)?> 
        ValidateRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var user = await FindUserByRefreshTokenAsync(refreshToken, ct);

        ValidateRefreshTokenExpiry(user);
        ValidateUserStatus(user);

        var claims = await BuildUserClaimsAsync(user, ct);
        return (user.Id, claims);
    }

    public async Task StoreRefreshTokenAsync(string subject, string refreshToken, DateTime expiresAtUtc, CancellationToken ct = default)
    {
        var hashRefreshToken = HashToken(refreshToken);

        var resultUpdated = await dbContext.Users
            .Where(u => u.Id == subject)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.RefreshToken, hashRefreshToken)
                .SetProperty(u => u.RefreshTokenExpireTime, expiresAtUtc), ct);
        
        if (resultUpdated == 0)
        {
            throw new UnauthorizedException("user not found");
        }

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Stored refresh token for user {UserId}. Token hash: {TokenHash}, Expires: {ExpiresAt}",
                subject, hashRefreshToken[..Math.Min(8, hashRefreshToken.Length)], expiresAtUtc);
        }
    }

    public async Task<(string Subject, IEnumerable<Claim> Claims)?> 
        BuildClaimsForUserAsync(string userId,  CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(userId);
        
        var user = await userManager.Users
            .FirstOrDefaultAsync(u => u.Id == userId, ct);
        
        if (user is null)
            return null;
        
        ValidateUserStatus(user);
        
        var claims =  await BuildUserClaimsAsync(user, ct);
        
        return (user.Id, claims);   
    }

    #region internals method

    private async Task<ApplicationUser> FindAndValidateUserByCredentialsAsync(string email, string password)
    {
        var user = await userManager.FindByEmailAsync(email.Trim().Normalize());
        if (user is null)
        {
            // Generic 401 — never confirm or deny account existence from this path.
            throw new UnauthorizedException();
        }
        
        if (userManager.SupportsUserLockout && await userManager.IsLockedOutAsync(user))
        {
            logger.LogWarning("Login attempted for locked account {UserId}", user.Id);
            throw new CustomException(
                "Account is temporarily locked due to too many failed login attempts. Try again later.",
                errors: null,
                HttpStatusCode.Locked);
        }
        
        if (!await userManager.CheckPasswordAsync(user, password))
        {
            if (userManager.SupportsUserLockout)
            {
                await userManager.AccessFailedAsync(user);
                if (await userManager.IsLockedOutAsync(user))
                {
                    logger.LogWarning(
                        "Account {UserId} locked out after exceeding failed login threshold.",
                        user.Id);
                }
            }
            throw new UnauthorizedException();
        }
        
        // Successful authentication resets the failed-attempt counter.
        if (userManager.SupportsUserLockout && await userManager.GetAccessFailedCountAsync(user) > 0)
        {
            await userManager.ResetAccessFailedCountAsync(user);
        }

        return user;
    }
    
    private void ValidateUserStatus(ApplicationUser user)
    {
        if (!user.IsActive)
        {
            throw new UnauthorizedException("user is deactivated");
        }

        if (!user.EmailConfirmed)
        {
            throw new UnauthorizedException("email not confirmed");
        }
    }

    private async Task VerifyTwoFactorOrThrowAsync(ApplicationUser user, string? twoFactorCode)
    {
        if (string.IsNullOrWhiteSpace(twoFactorCode))
        {
            throw new CustomException(
                "two_factor_required: An authenticator code is required to complete sign-in.",
                errors: null,
                HttpStatusCode.Unauthorized);
        }

        var valid = await userManager.VerifyTwoFactorTokenAsync(
            user,
            userManager.Options.Tokens.AuthenticatorTokenProvider,
            twoFactorCode);

        if (!valid)
        {
            logger.LogWarning("Invalid two-factor code for user {UserId}", user.Id);
            throw new UnauthorizedException("two_factor_invalid: The authenticator code is invalid or expired.");
        }
    }
    
    
    private async Task<ApplicationUser> FindUserByRefreshTokenAsync(string refreshToken,CancellationToken ct)
    {
        var hashedToken = HashToken(refreshToken);

        var user = await userManager.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == hashedToken, ct);

        if (user is null)
        {
            throw new UnauthorizedException("refresh token is invalid or expired");
        }

        return user;
    }
    
    private void ValidateRefreshTokenExpiry(ApplicationUser user)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (user.RefreshTokenExpireTime <= now)
        {
            logger.LogWarning(
                "Refresh token expired for user {UserId}. Expired at: {ExpiryTime}, Current time: {CurrentTime}",
                user.Id, user.RefreshTokenExpireTime, now);
            throw new UnauthorizedException("refresh token is invalid or expired");
        }
    }
    
    private static string HashToken(string token)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(token);
        var hash = System.Security.Cryptography.SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
    
    private async Task<IEnumerable<Claim>> BuildUserClaimsAsync(ApplicationUser user, CancellationToken ct)
    {
        var claims = CreateBasicClaims(user);
        await AddRoleClaimsAsync(claims, user, ct);
        return claims;
    }
    private static List<Claim> CreateBasicClaims(ApplicationUser user)
    {
        var fullName = $"{user.FirstName} {user.LastName}".Trim();
        return
        [
            new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Name, fullName.Length > 0 ? fullName : (user.Email ?? string.Empty)),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.FirstName ?? string.Empty),
            new(ClaimTypes.MobilePhone, user.PhoneNumber ?? string.Empty),
            new(ClaimConstants.Fullname, fullName),
            new(ClaimTypes.Surname, user.LastName ?? string.Empty),
            new(ClaimConstants.ImageUrl, user.ImageUrl?.ToString() ?? string.Empty)
        ];
    }

    private async Task AddRoleClaimsAsync(List<Claim> claims, ApplicationUser user, CancellationToken ct)
    {
        var directRoles = await userManager.GetRolesAsync(user);

        var allRoles = directRoles.Distinct();
        claims.AddRange(allRoles.Select(r => new Claim(ClaimTypes.Role, r)));
    }
    
    #endregion
}