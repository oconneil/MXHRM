namespace MXHRM.Api.DTOs.Employees;

public class GetEmployeesRequest
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public string? Search { get; set; }
}