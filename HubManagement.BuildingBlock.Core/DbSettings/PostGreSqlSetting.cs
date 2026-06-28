namespace HubManagement.BuildingBlock.Core.DbSettings;

public class PostGreSqlSetting
{
    public string ConnectionString { get; set; } = default!;
    
    public int? commandTimeout { get; set; }
}