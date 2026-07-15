// using System.Collections.ObjectModel;
// using System.Text;
// using HubManagement.Application.Services;
// using HubManagement.BuildingBlock.Core.Exceptions;
// using HubManagement.BuildingBlock.Infrastructure.Mailing;
// using HubManagement.BuildingBlock.Infrastructure.Mailing.Abstractions;
// using HubManagement.Domain.Entities;
// using HubManagement.Infrastructure.DataContext;
// using Microsoft.AspNetCore.Identity;
// using Microsoft.AspNetCore.WebUtilities;
//
// namespace HubManagement.Infrastructure.Services;
//
// internal sealed class UserPasswordService(
//     UserManager<ApplicationUser> userManager,
//     HubDbContext db,
//     IMailService mailService,
//     TimeProvider timeProvider)  : IUserPasswordService
// {
//     public async Task ForgotPasswordAsync(string email, string origin, CancellationToken ct = default)
//     {
//         var user =  await userManager.FindByEmailAsync(email);
//         
//         if (user is null || string.IsNullOrWhiteSpace(user.Email))
//         {
//             return;
//         }
//         
//         var token = await userManager.GeneratePasswordResetTokenAsync(user);
//         token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
//         
//         var resetPasswordUri = QueryHelpers.AddQueryString(
//             $"{origin.TrimEnd('/')}/reset-password",
//             new Dictionary<string, string?>
//             {
//                 ["token"] = token,
//                 ["email"] = email,
//             });
//         
//         var mailRequest = new MailRequest(
//             new Collection<string> { user.Email },
//             "Reset Password",
//             $"Please reset your password using the following link: {resetPasswordUri}");
//         
//         await mailService.SendAsync(mailRequest, CancellationToken.None);
//     }
//
//     public async Task ResetPasswordAsync(string email, string password, string token, CancellationToken ct = default)
//     { 
//         var user = await userManager.FindByEmailAsync(email);
//         if (user == null)
//         {
//             throw new NotFoundException("user not found");
//         }
//         
//         token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
//         var result = await userManager.ResetPasswordAsync(user, token, password);
//         if (!result.Succeeded)
//         {
//             var errors = result.Errors.Select(e => e.Description).ToList();
//             throw new CustomException("error resetting password", errors);
//         }
//     }
//
//     public async Task ChangePasswordAsync(string password, string newPassword, string confirmNewPassword, string userId,
//         CancellationToken ct = default)
//     {
//         var user = await userManager.FindByIdAsync(userId);
//
//         _ = user ?? throw new NotFoundException("user not found");
//
//         var result = await userManager.ChangePasswordAsync(user, password, newPassword);
//
//         if (!result.Succeeded)
//         {
//             var errors = result.Errors.Select(e => e.Description).ToList();
//             throw new CustomException("failed to change password", errors);
//         }
//
//         // Update password expiry date
//         await UpdateLastPasswordChangeDateAsync(userId, ct);
//     }
//
//     private async Task UpdateLastPasswordChangeDateAsync(string userId, CancellationToken cancellationToken = default)
//     {
//         var user = await userManager.FindByIdAsync(userId);
//         if (user is not null)
//         {
//             user.LastPasswordChangeDateTime = timeProvider.GetUtcNow().UtcDateTime;
//             await userManager.UpdateAsync(user);
//         }
//     }
// }