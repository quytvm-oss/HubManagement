namespace HubManagement.BuildingBlock.Infrastructure.Authorization;

// https://github.com/fullstackhero/dotnet-starter-kit/blob/develop/src/BuildingBlocks/Shared/Identity/ResourceConstants.cs
public static class ResourceConstants
{
    public const string Tenants = nameof(Tenants);
    public const string Dashboard = nameof(Dashboard);
    public const string Hangfire = nameof(Hangfire);
    public const string Users = nameof(Users);
    public const string UserRoles = nameof(UserRoles);
    public const string Roles = nameof(Roles);
    public const string RoleClaims = nameof(RoleClaims);
    public const string AuditTrails = nameof(AuditTrails);
}