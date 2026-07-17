using HubManagement.Infrastructure.Authorization.Constants;

namespace HubManagement.Infrastructure.SeedData;

public sealed record PermissionDefinition(string Resource, IReadOnlyCollection<string> Actions);

public static class PermissionDefinitions
{
    public static readonly PermissionDefinition[] Definitions =
    [
        new(
            ResourceConstants.User,
            [
                .. ActionConstants.Crud,
                ActionConstants.Search,
                ActionConstants.Clean
            ]),

        new(
            ResourceConstants.Role,
            [
                .. ActionConstants.Crud,
                ActionConstants.Search
            ])
    ];
}