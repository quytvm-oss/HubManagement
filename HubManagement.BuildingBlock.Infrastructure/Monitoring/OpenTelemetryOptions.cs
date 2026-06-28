namespace HubManagement.BuildingBlock.Infrastructure.Monitoring;

public class OpenTelemetryOptions
{
    public string ServiceName { get; set; } = default!;

    public string Endpoint { get; set; } = default!;
}