using HubManagement.Application.DTOs;
using HubManagement.Application.Services;
using HubManagement.Domain.Entities;
using HubManagement.Infrastructure.Authorizations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace HubManagement.Infrastructure.Services;

public class PasswordExpiryService(
    UserManager<ApplicationUser> userManager,
    IOptions<PasswordPolicyOptions> passwordPolicyOptions,
    TimeProvider timeProvider)
    : IPasswordExpiryService
{
    private readonly PasswordPolicyOptions _passwordPolicyOptions = passwordPolicyOptions.Value;

    public async Task<bool> IsPasswordExpiredAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await  userManager.FindByIdAsync(userId);
        
        if (user is null)
            return false;

        return IsPasswordExpired(user);
    }

    public async Task<int> GetDaysUntilExpiryAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await  userManager.FindByIdAsync(userId);
        
        if (user is null)
            return int.MaxValue;
        
        return GetDaysUntilExpiry(user);
    }

    public async Task<bool> IsPasswordExpiringWithinWarningPeriodAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return false;
        }

        return IsPasswordExpiringWithinWarningPeriod(user);
    }

    public async Task<PasswordExpiryStatusDto> GetPasswordExpiryStatusAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return new PasswordExpiryStatusDto
            {
                IsExpired = false,
                IsExpiringWithinWarningPeriod = false,
                DaysUntilExpiry = int.MaxValue,
                ExpiryDate = null
            };
        }

        return GetPasswordExpiryStatus(user);
    }

    public async Task UpdateLastPasswordChangeDateAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is not null)
        {
            user.LastPasswordChangeDateTime = timeProvider.GetUtcNow().UtcDateTime;
            await userManager.UpdateAsync(user);
        }
    }

    #region internals

    private int GetDaysUntilExpiry(ApplicationUser user)
    {
        if (!_passwordPolicyOptions.EnforcePasswordExpiry)
        {
            return int.MaxValue;
        }  
        
        var expiryDate = user.LastPasswordChangeDateTime.AddDays(_passwordPolicyOptions.PasswordExpiryDays);
        return (int)Math.Ceiling((expiryDate - timeProvider.GetUtcNow().UtcDateTime).TotalDays);
    }

    private bool IsPasswordExpired(ApplicationUser user)
    {
        if (!_passwordPolicyOptions.EnforcePasswordExpiry)
        {
            return false;
        }

        var expiryDate = user.LastPasswordChangeDateTime.AddDays(_passwordPolicyOptions.PasswordExpiryDays);
        return timeProvider.GetUtcNow().UtcDateTime > expiryDate;
    }

    private bool IsPasswordExpiringWithinWarningPeriod(ApplicationUser user)
    {
        if (!_passwordPolicyOptions.EnforcePasswordExpiry)
        {
            return false;
        }
        
        var daysUntilExpiry = GetDaysUntilExpiry(user);
        return daysUntilExpiry >= 0 && daysUntilExpiry <= _passwordPolicyOptions.PasswordExpiryWarningDays;
    }

    private PasswordExpiryStatusDto GetPasswordExpiryStatus(ApplicationUser user)
    {
        var expiryDate = user.LastPasswordChangeDateTime.AddDays(_passwordPolicyOptions.PasswordExpiryDays);
        var daysUntilExpiry = GetDaysUntilExpiry(user);
        var isExpired = IsPasswordExpired(user);
        var isExpiringWithinWarningPeriod = IsPasswordExpiringWithinWarningPeriod(user);

        return new PasswordExpiryStatusDto
        {
            IsExpired = isExpired,
            IsExpiringWithinWarningPeriod = isExpiringWithinWarningPeriod,
            DaysUntilExpiry = daysUntilExpiry,
            ExpiryDate = _passwordPolicyOptions.EnforcePasswordExpiry ? expiryDate : null
        };
    }

    #endregion
}