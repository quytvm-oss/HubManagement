namespace HubManagement.BuildingBlock.Core.Messaging;

public interface IIntegrationEvent
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
}

public abstract record IntegrationEvent : IIntegrationEvent
{
    public Guid Id { get; } = Guid.CreateVersion7();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}