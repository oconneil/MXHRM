namespace MXHRM.Application.Common;

public class PagedResponse<T>
{
    public List<T> Items { get; set; } = [];

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalItems { get; set; }

    public int TotalPages { get; set; }

    public bool HasNextPage => Page < TotalPages;

    public bool HasPreviousPage => Page > 1;
}
