using HubManagement.Domain.Entities;
using HubManagement.Domain.Enums;

namespace HubManagement.Infrastructure.SeedData;

public static class SystemEmailTemplates
{
    public static readonly EmailTemplate ConfirmationEmail = new()
    {
        Type = EmailTemplateType.ConfirmationEmail,
        Title = "Confirmation Email",
        Subject = "Confirm Your Email Address",
        Body = """
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>Confirm Your Email</title>
            </head>
            <body style="margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; background-color: #f8fafc;">
                <table role="presentation" style="width: 100%; border-collapse: collapse;">
                    <tr>
                        <td align="center" style="padding: 40px 0;">
                            <table role="presentation" style="width: 100%; max-width: 600px; border-collapse: collapse; background-color: #ffffff; border-radius: 8px; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);">
                                <tr>
                                    <td style="padding: 40px 40px 30px; text-align: center; background-color: #2563eb; border-radius: 8px 8px 0 0;">
                                        <h1 style="margin: 0; color: #ffffff; font-size: 24px; font-weight: 600;">
                                            Confirm Your Email Address
                                        </h1>
                                    </td>
                                </tr>

                                <tr>
                                    <td style="padding: 40px;">
                                        <p style="margin: 0 0 20px; color: #334155; font-size: 16px; line-height: 1.6;">
                                            Hi {{UserName}},
                                        </p>

                                        <p style="margin: 0 0 20px; color: #334155; font-size: 16px; line-height: 1.6;">
                                            Thank you for registering! Please confirm your email address by clicking the button below:
                                        </p>

                                        <table role="presentation" style="width: 100%; border-collapse: collapse;">
                                            <tr>
                                                <td align="center" style="padding: 30px 0;">
                                                    <a href="{{ConfirmationUrl}}"
                                                       style="display: inline-block; padding: 14px 32px; background-color: #2563eb; color: #ffffff; text-decoration: none; font-size: 16px; font-weight: 600; border-radius: 6px;">
                                                        Confirm Email Address
                                                    </a>
                                                </td>
                                            </tr>
                                        </table>

                                        <p style="margin: 0 0 20px; color: #64748b; font-size: 14px; line-height: 1.6;">
                                            If the button doesn't work, copy and paste this link into your browser:
                                        </p>

                                        <p style="margin: 0 0 20px; color: #2563eb; font-size: 14px; line-height: 1.6; word-break: break-all;">
                                            {{ConfirmationUrl}}
                                        </p>

                                        <p style="margin: 30px 0 0; color: #64748b; font-size: 14px; line-height: 1.6;">
                                            If you didn't create an account, you can safely ignore this email.
                                        </p>
                                    </td>
                                </tr>

                                <tr>
                                    <td style="padding: 20px 40px; background-color: #f1f5f9; border-radius: 0 0 8px 8px; text-align: center;">
                                        <p style="margin: 0; color: #94a3b8; font-size: 12px;">
                                            This is an automated message. Please do not reply to this email.
                                        </p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>
            """
    };

    public static IReadOnlyCollection<EmailTemplate> All =>
    [
        ConfirmationEmail
    ];
}