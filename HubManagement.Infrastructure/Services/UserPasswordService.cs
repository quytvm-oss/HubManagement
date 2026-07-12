using System.Text;
using HubManagement.Application.Services;
using HubManagement.BuildingBlock.Core.Exceptions;
using HubManagement.Domain.Entities;
using HubManagement.Infrastructure.DataContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace HubManagement.Infrastructure.Services;

internal sealed class UserPasswordService(
    UserManager<ApplicationUser> userManager,
    HubDbContext db,
    IPasswordExpiryService passwordExpiryService)  : IUserPasswordService
{
    public async Task ForgotPasswordAsync(string email, string origin, CancellationToken ct = default)
    {
        var user =  await userManager.FindByEmailAsync(email);
        
        if (user is null || string.IsNullOrWhiteSpace(user.Email))
        {
            return;
        }
        
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        
        var resetPasswordUri = QueryHelpers.AddQueryString(
            $"{origin.TrimEnd('/')}/reset-password",
            new Dictionary<string, string?>
            {
                ["token"] = token,
                ["email"] = email,
            });
    }

    public async Task ResetPasswordAsync(string email, string password, string token, CancellationToken ct = default)
    { 
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            throw new NotFoundException("user not found");
        }
        
        token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        var result = await userManager.ResetPasswordAsync(user, token, password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            throw new CustomException("error resetting password", errors);
        }
    }

    public async Task ChangePasswordAsync(string password, string newPassword, string confirmNewPassword, string userId,
        CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId);

        _ = user ?? throw new NotFoundException("user not found");

        var result = await userManager.ChangePasswordAsync(user, password, newPassword);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            throw new CustomException("failed to change password", errors);
        }

        // Update password expiry date
        await passwordExpiryService.UpdateLastPasswordChangeDateAsync(userId, ct);
    }
}