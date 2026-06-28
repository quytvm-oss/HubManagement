using System.Collections.ObjectModel;

namespace HubManagement.BuildingBlock.Infrastructure.Authorization;

public class ResourceAction
{
    public Resource Resource { get; set; }

    public IEnumerable<Action> Actions { get; set; }

    private static readonly ResourceAction[] _all =
    {
        new()
        {
            Resource = Resource.Dashboard,
            Actions =
            [
                Action.View
            ]
        },
        new()
        {
            Resource = Resource.Users,
            Actions = Action.Crud
        },
        new()
        {
            Resource = Resource.Roles,
            Actions = Action.Crud
        },
        new()
        {
            Resource = Resource.RoleClaims,
            Actions =
            [
                Action.View
            ]
        },
        new()
        {
            Resource = Resource.AuditTrails,
            Actions =
            [
                Action.View
            ]
        },
        new()
        {
            Resource = Resource.Tenants,
            Actions = Action.Crud
        },
    };

    private static IReadOnlyList<ResourceAction> All { get; } = new ReadOnlyCollection<ResourceAction>(_all);
    
    public static IEnumerable<Permission> AllPermission { get; } = All.SelectMany(
            x => x.Actions,
            (resource, action) => new Permission(action.Code, resource.Resource.Code))
        .ToList();
}