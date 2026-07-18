using HubManagement.Application.Services;
using Mediator;

namespace HubManagement.Application.Features.Identities.Users.GetUserPermissions;

public class GetCurrentUserPermissionsQueryHandler(IUserService userService)
    : IQueryHandler<GetCurrentUserPermissionsQuery, List<string>?>
{
    public async ValueTask<List<string>?> Handle(GetCurrentUserPermissionsQuery query, CancellationToken cancellationToken)
    {
        return await userService.GetPermissionsAsync(query.UserId, cancellationToken);
    }
}