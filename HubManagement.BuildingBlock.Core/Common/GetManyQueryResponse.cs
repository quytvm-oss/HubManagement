namespace HubManagement.BuildingBlock.Core.Common;

public class GetManyQueryResponse<T>
{
    public int? PageIndex { get; set; }

    public int? PageSize { get; set; }

    public int TotalCount { get; set; }

    public string OrderColumn { get; set; }

    public string OrderDirection { get; set; }

    public IEnumerable<T> Items { get; set; }

    public GetManyQueryResponse(IEnumerable<T> items, int count, int? pageIndex, int? pageSize)
    {
        PageIndex = pageIndex;
        TotalCount = count;
        PageSize = pageSize;
        Items = items;
    }

    public GetManyQueryResponse(IEnumerable<T> items, 
        int count, 
        int? pageIndex,
        int? pageSize, 
        string? orderColumn, 
        string orderDirection)
    {
        PageIndex = pageIndex;
        TotalCount = count;
        PageSize = pageSize;
        Items = items;
        OrderColumn = orderColumn;
        OrderDirection = orderDirection;
    }
}