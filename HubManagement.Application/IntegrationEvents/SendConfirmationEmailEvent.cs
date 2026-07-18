using HubManagement.BuildingBlock.Core.Messaging;

namespace HubManagement.Application.IntegrationEvents;

public record SendConfirmationEmailEvent : IntegrationEvent
{
    public Guid UserId { get; set; }

    public string? Email { get; set; }

    public string? Origin { get; set; }
}