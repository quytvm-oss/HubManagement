using System.Security.Claims;
using System.Text;
using HubManagement.Application.Contracts;
using HubManagement.Application.IntegrationEvents;
using HubManagement.BuildingBlock.Core.Exceptions;
using HubManagement.BuildingBlock.Infrastructure.Mailing;
using HubManagement.BuildingBlock.Infrastructure.Mailing.Abstractions;
using HubManagement.Domain.Entities;
using HubManagement.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Rebus.Handlers;
using Serilog;

namespace HubManagement.Application.EventHandlers.IntegrationEvents;

public class SendConfirmationEmailEventHandler(ILogger logger, 
    IApplicationDbContext dbContext, 
    UserManager<ApplicationUser> userManager,
    IMailService mailService) : IHandleMessages<SendConfirmationEmailEvent>
{

    public async Task Handle(SendConfirmationEmailEvent message)
    {
        
        if (string.IsNullOrEmpty(message.Email))
        {
            return;
        }
        
        var user = await userManager.FindByEmailAsync(message.Email);
        if (user is null)
            throw new NotFoundException("User not found");
        
        string emailVerificationUri = await GetEmailVerificationUriAsync(user, message.Origin);

        var emailTemplate = await 
            dbContext.EmailTemplates.FirstOrDefaultAsync(x => x.Type == EmailTemplateType.ConfirmationEmail);
        
        if (emailTemplate is null)
            return;
        
        var emailBody = emailTemplate.Body?.Replace("{{ConfirmationUrl}}", emailVerificationUri)
            .Replace("{{UserName}}", user.UserName);
        
        var mailRequest = new MailRequest(
            [user.Email!],
            "Confirm Your Email Address",
            emailBody);
        
        await mailService.SendAsync(mailRequest, CancellationToken.None);
    }
    
    private async Task<string> GetEmailVerificationUriAsync(ApplicationUser user, string? origin)
    {
        string code = await userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        const string route = "api/v1/identity/confirm-email";
        var endpointUri = new Uri(string.Concat($"{origin}/", route));

        string verificationUri = QueryHelpers.AddQueryString(endpointUri.ToString(), "userId", user.Id.ToString());
        verificationUri = QueryHelpers.AddQueryString(verificationUri, "code", code);
        verificationUri = QueryHelpers.AddQueryString(
            verificationUri,
            ClaimTypes.NameIdentifier, user.Id.ToString());

        return verificationUri;
    }
}