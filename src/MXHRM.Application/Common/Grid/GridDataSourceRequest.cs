namespace MXHRM.Application.Common.Grid;

public sealed class GridDataSourceRequest
{
    public int Skip { get; set; }
    public int Take { get; set; } = 20;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public List<GridSortDescriptor> Sorts { get; set; } = [];
    public List<GridFilterDescriptor> Filters { get; set; } = [];
    public GridFilterDescriptor? Filter { get; set; }
    public string FilterLogic { get; set; } = "and";
}

public sealed class GridSortDescriptor
{
    public string Field { get; set; } = string.Empty;
    public string Dir { get; set; } = "asc";
}

public sealed class GridFilterDescriptor
{
    public string Field { get; set; } = string.Empty;
    public string Operator { get; set; } = "contains";
    public string? Value { get; set; }
    public string Logic { get; set; } = "and";
    public List<GridFilterDescriptor> Filters { get; set; } = [];
    public bool IsComposite => Filters.Count > 0;
}
