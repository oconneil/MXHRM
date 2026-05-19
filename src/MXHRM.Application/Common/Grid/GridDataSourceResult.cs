namespace MXHRM.Application.Common.Grid;

public sealed class GridDataSourceResult<T>
{
    public IReadOnlyList<T> Data { get; set; } = [];
    public int Total { get; set; }
}