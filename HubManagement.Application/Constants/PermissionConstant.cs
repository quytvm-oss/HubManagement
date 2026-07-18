using HubManagement.BuildingBlock.Infrastructure.Authorization;

namespace HubManagement.Application.Constants;

public static class PermissionConstant
{
    public static class Users
    {
        public const string Resource = nameof(Users);
        public const string View          = $"Permissions.{Resource}.View";
        public const string Search        = $"Permissions.{Resource}.Search";
        public const string Create        = $"Permissions.{Resource}.Create";
        public const string Update        = $"Permissions.{Resource}.Update";
        public const string Delete        = $"Permissions.{Resource}.Delete";
        public const string Export        = $"Permissions.{Resource}.Export";
        public const string ManageRoles   = $"Permissions.{Resource}.ManageRoles";
    }
    
    public static class UserRoles
    {
        public const string Resource = nameof(UserRoles);
        public const string View   = $"Permissions.{Resource}.View";
        public const string Update = $"Permissions.{Resource}.Update";
    }

    public static class Roles
    {
        public const string Resource = nameof(Roles);
        public const string View   = $"Permissions.{Resource}.View";
        public const string Create = $"Permissions.{Resource}.Create";
        public const string Update = $"Permissions.{Resource}.Update";
        public const string Delete = $"Permissions.{Resource}.Delete";
    }

    public static class RoleClaims
    {
        public const string Resource = nameof(RoleClaims);
        public const string View   = $"Permissions.{Resource}.View";
        public const string Update = $"Permissions.{Resource}.Update";
    }

    public static class Sessions
    {
        public const string Resource = nameof(Sessions);
        public const string View      = $"Permissions.{Resource}.View";
        public const string Revoke    = $"Permissions.{Resource}.Revoke";
        public const string ViewAll   = $"Permissions.{Resource}.ViewAll";
        public const string RevokeAll = $"Permissions.{Resource}.RevokeAll";
    }
    
    public static IReadOnlyList<Permission> All { get; } =
    [
        new("View Users",          ActionConstants.View,   Users.Resource),
        new("Search Users",        ActionConstants.Search, Users.Resource),
        new("Create Users",        ActionConstants.Create, Users.Resource),
        new("Update Users",        ActionConstants.Update, Users.Resource),
        new("Delete Users",        ActionConstants.Delete, Users.Resource),
        new("Export Users",        ActionConstants.Export, Users.Resource),
        new("Manage User Roles",   "ManageRoles",          Users.Resource),
        new("Confirm User Email",  "ConfirmEmail",         Users.Resource),

        new("View User Roles",     ActionConstants.View,   UserRoles.Resource),
        new("Update User Roles",   ActionConstants.Update, UserRoles.Resource),

        new("View Roles",          ActionConstants.View,   Roles.Resource),
        new("Create Roles",        ActionConstants.Create, Roles.Resource),
        new("Update Roles",        ActionConstants.Update, Roles.Resource),
        new("Delete Roles",        ActionConstants.Delete, Roles.Resource),

        new("View Role Claims",    ActionConstants.View,   RoleClaims.Resource),
        new("Update Role Claims",  ActionConstants.Update, RoleClaims.Resource),

        new("View My Sessions",    ActionConstants.View,    Sessions.Resource),
        new("Revoke My Sessions",  "Revoke",                Sessions.Resource),
        new("View All Sessions",   "ViewAll",               Sessions.Resource),
        new("Revoke Any Session",  "RevokeAll",             Sessions.Resource)
    ];
}