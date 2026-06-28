namespace HubManagement.BuildingBlock.Infrastructure.Authorization;

public record Permission(string Action, string Resource)
{
    public string Name => NameFor(Action, Resource);

    public static string NameFor(string action, string resource)
    {
        return $"Permissions.{resource}.{action}";
    }
}