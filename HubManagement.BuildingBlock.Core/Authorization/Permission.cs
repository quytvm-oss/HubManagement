namespace HubManagement.BuildingBlock.Core.Authorization;

public sealed record Permission(string Description, string Action, string Resource)
{
    public string Name => NameFor(Action, Resource);

    public static string NameFor(string action, string resource)
        => $"Permissions.{resource}.{action}";
}
