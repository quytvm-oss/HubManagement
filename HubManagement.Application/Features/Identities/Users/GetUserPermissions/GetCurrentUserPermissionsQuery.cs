using Mediator;

namespace HubManagement.Application.Features.Identities.Users.GetUserPermissions;

public sealed record GetCurrentUserPermissionsQuery(string UserId) : IQuery<List<string>?>;