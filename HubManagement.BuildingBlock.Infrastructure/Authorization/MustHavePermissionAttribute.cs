using Microsoft.AspNetCore.Authorization;

using HubManagement.BuildingBlock.Core.Authorization;

namespace HubManagement.BuildingBlock.Infrastructure.Authorization;

public class MustHavePermissionAttribute : AuthorizeAttribute
{
    public MustHavePermissionAttribute(string resource, string action)
    {
        Policy = Permission.NameFor(resource, action);
    }

    public MustHavePermissionAttribute(string[] resources, string[] actions)
    {
        Policy = string.Join('|', resources.SelectMany(x => actions, (r, a) => $"{Permission.NameFor(r, a)}"));
    }
}
