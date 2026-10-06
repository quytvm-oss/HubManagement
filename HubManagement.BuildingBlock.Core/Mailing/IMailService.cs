namespace HubManagement.BuildingBlock.Core.Mailing;

public interface IMailService
{
    Task SendAsync(MailRequest request, CancellationToken cancellationToken);
}
