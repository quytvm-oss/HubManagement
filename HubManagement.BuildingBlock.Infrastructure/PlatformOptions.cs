namespace HubManagement.BuildingBlock.Infrastructure;

public class PlatformOptions
{
    public bool EnableCors { get; set; } = true;
    public bool EnableOpenApi { get; set; } = true;
    public bool EnableCaching { get; set; } = false;
    //public bool EnableMailing { get; set; } = false;
    public bool EnableOpenTelemetry { get; set; } = true;
}