using System.Collections.ObjectModel;

namespace HubManagement.BuildingBlock.Infrastructure.Authorization;

public class Resource
{
    public string Code { get; }

    public string Name { get; }

    public Resource(string code, string name)
    {
        Code = code;
        Name = name;
    }

    public static readonly Resource Dashboard = new(ResourceConstants.Dashboard, "Dashboard");

    public static readonly Resource Hangfire = new(ResourceConstants.Hangfire, "Hangfire");

    public static readonly Resource Tenants = new(ResourceConstants.Tenants, "Danh sách tenant");

    public static readonly Resource Users = new(ResourceConstants.Users, "Danh sách người dùng");

    public static readonly Resource UserRoles = new(ResourceConstants.UserRoles, "Phân quyền người dùng");

    public static readonly Resource Roles = new(ResourceConstants.Roles, "Danh sách vai trò");

    public static readonly Resource RoleClaims = new(ResourceConstants.RoleClaims, "Quyền của vai trò");

    public static readonly Resource AuditTrails = new(ResourceConstants.AuditTrails, "Lịch sử hoạt động");

    private static readonly Resource[] _all =
    [
        Dashboard,
        Hangfire,
        Tenants,
        Users,
        UserRoles,
        Roles,
        RoleClaims,
        AuditTrails
    ];

    public static IReadOnlyList<Resource> All { get; } = new ReadOnlyCollection<Resource>(_all);
}