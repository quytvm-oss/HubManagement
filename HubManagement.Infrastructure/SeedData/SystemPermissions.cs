using HubManagement.Application.Constants;
using HubManagement.BuildingBlock.Infrastructure.Authorization;

namespace HubManagement.Infrastructure.SeedData;

public static class SystemPermissions
{
    public static IReadOnlyList<Permission> Admin { get; } = PermissionConstant.All;
    
    public static IReadOnlyList<Permission> Basic { get; } = 
    [
        new("View Users",          ActionConstants.View,   PermissionConstant.Users.Resource),
        new("Search Users",        ActionConstants.Search, PermissionConstant.Users.Resource),
    ];
    
    public static readonly Guid AdminId = Guid.Parse("4316048C-6CE2-4D86-AFE6-F021895CB872");

    public static readonly Guid RoleAdminCompanyId = Guid.Parse("4AE5A2FF-C70B-4C70-AED8-B9E249085165");
    public static readonly Guid RoleAdminPortalId = Guid.Parse("F085F0CB-3828-4620-AF9C-646608802F1B");
    public static readonly Guid RoleUserId = Guid.Parse("014BDB76-7BB3-474C-A4B6-8E249806ACDC");

    public const string DisplayName = "System";
    
}