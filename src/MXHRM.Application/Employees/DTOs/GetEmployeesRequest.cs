namespace MXHRM.Application.Employees.DTOs;

public class GetEmployeesRequest
{
    public string? Search { get; set; }

    public string? CompanyID { get; set; }

    public bool? IsActive { get; set; }

    public string? SortBy { get; set; } = "employeeId";

    public string? SortDirection { get; set; } = "asc";

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}