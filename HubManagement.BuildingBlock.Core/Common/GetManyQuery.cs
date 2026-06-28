namespace HubManagement.BuildingBlock.Core.Common;

public class GetManyQuery
{
    private int? _pageIndex;

    public int? PageIndex
    {
        get
        {
            var pageIndex = _pageIndex.GetValueOrDefault(1);

            return pageIndex > 0 ? pageIndex : 1;
        }
        set => _pageIndex = value;
    }

    public int? PageSize { get; set; } = 20;

    public string OrderColumn { get; set; }

    public string OrderDirection { get; set; } = "desc";

    public void Deconstruct(out int? skip, out int? take)
    {
        (skip, take) = ((PageSize, PageIndex) switch
        {
            ({ } pageSize, { } pageIndex) => (pageIndex - 1) * pageSize,
            _ => null
        }, PageSize);
    }
}