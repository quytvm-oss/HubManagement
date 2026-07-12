namespace HubManagement.BuildingBlock.Infrastructure.Mailing.Abstractions;

public interface IMailService
{
    Task SendAsync(MailRequest request, CancellationToken ct);
}