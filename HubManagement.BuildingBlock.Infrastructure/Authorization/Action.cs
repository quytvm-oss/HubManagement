using System.Collections.ObjectModel;

namespace HubManagement.BuildingBlock.Infrastructure.Authorization;

public class Action
{
     public string Code { get; }

    public string Name { get; }

    public Action(string code, string name)
    {
        Code = code;
        Name = name;
    }

    public static readonly Action View = new(ActionConstants.View, "Xem");

    public static readonly Action Create = new(ActionConstants.Create, "Thêm");

    public static readonly Action Update = new(ActionConstants.Update, "Sửa");

    public static readonly Action Delete = new(ActionConstants.Delete, "Xoá");

    public static readonly Action Export = new(ActionConstants.Export, "Xuất file");
    
    public static readonly Action Generate = new(ActionConstants.Generate, "Xuất file");
    
    public static readonly Action Clean = new(ActionConstants.Clean, "Xuất file");
    
    public static readonly Action Search = new(ActionConstants.Search, "Tìm kiếm");
    
    public static readonly Action UpgradeSubscription = new(ActionConstants.UpgradeSubscription, "Tìm kiếm");

    private static readonly Action[] _crud =
    {
        View,
        Create,
        Update,
        Delete
    };

    public static IReadOnlyList<Action> Crud { get; } = new ReadOnlyCollection<Action>(_crud);

    private static readonly Action[] _all =
    {
        View,
        Create,
        Update,
        Delete,
        Export,
        Generate,
        Clean,
        Search,
        UpgradeSubscription
    };

    public static IReadOnlyList<Action> All { get; } = new ReadOnlyCollection<Action>(_all);
}