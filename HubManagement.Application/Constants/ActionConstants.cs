namespace HubManagement.Application.Constants;

// https://github.com/fullstackhero/dotnet-starter-kit/blob/develop/src/BuildingBlocks/Shared/Identity/ActionConstants.cs
public static class ActionConstants
{
    public const string View = nameof(View);
    public const string Search = nameof(Search);
    public const string Create = nameof(Create);
    public const string Update = nameof(Update);
    public const string Delete = nameof(Delete);
    public const string Export = nameof(Export);
    public const string ViewAll = nameof(ViewAll);
    public const string Revoke = nameof(Revoke);
    
    public static IReadOnlyList<string> Crud { get; } =
    [
        View,
        Create,
        Update,
        Delete
    ];

    public static IReadOnlyList<string> All { get; } =
    [
        View,
        Search,
        Create,
        Update,
        Delete,
        Export,
        Revoke,
        ViewAll
    ];
}