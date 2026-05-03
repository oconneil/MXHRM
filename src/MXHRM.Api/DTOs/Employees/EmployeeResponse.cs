namespace MXHRM.Api.DTOs.Employees;

public class EmployeeResponse
{
    public string CompanyID { get; set; } = string.Empty;
    public string EmployeeID { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public decimal Salary { get; set; }
    public bool IsActive { get; set; }

    public byte[] RowVersion { get; set; } = [];
}