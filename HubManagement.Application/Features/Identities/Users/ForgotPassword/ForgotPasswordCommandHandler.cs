using System.Text;
using HubManagement.BuildingBlock.Core.Mailing;
using HubManagement.BuildingBlock.Core.Options;
using HubManagement.Domain.Entities;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace HubManagement.Application.Features.Identities.Users.ForgotPassword;

public class ForgotPasswordCommandHandler(
    IOptions<OriginOptions> originOptions,
    UserManager<ApplicationUser> userManager,
    IMailService mailService)
    : ICommandHandler<ForgotPasswordCommand, string>
{
    public async ValueTask<string> Handle(ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var origin = originOptions.Value?.OriginUrl?.ToString();
        if (string.IsNullOrWhiteSpace(origin))
        {
            throw new InvalidOperationException("Origin URL is not configured.");
        }
        
        var user = await userManager.FindByEmailAsync(command.Email);

        // Anti-enumeration: respond identically regardless of registration — a real user gets the
        // reset email; an unknown or email-less account silently no-ops with the same 200.
        if (user is null || string.IsNullOrWhiteSpace(user.Email))
        {
            return "If the account exists, a password reset email has been sent.";
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        // Build the SPA reset link with QueryHelpers (matches GetEmailVerificationUriAsync): trim any trailing
        // slash from the configured origin (Uri.ToString adds one for a host-only URL → "//reset-password"
        // misses the client route) and include the tenant the reset page requires. QueryHelpers URL-encodes
        // each value, so reserved chars in the email (e.g. '+') survive.
        var resetPasswordUri = QueryHelpers.AddQueryString(
            $"{origin.TrimEnd('/')}/reset-password",
            new Dictionary<string, string?>
            {
                ["token"] = token,
                ["email"] = command.Email,
            });
        var mailRequest = new MailRequest(
            [user.Email],
            "Reset Password",
            $"Please reset your password using the following link: {resetPasswordUri}");

        await mailService.SendAsync(mailRequest, cancellationToken);
        
        return "If the account exists, a password reset email has been sent.";
    }
}
