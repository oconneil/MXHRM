namespace MXHRM.Application.Employees.DTOs;

public class UpdateEmployeeRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public decimal Salary { get; set; }
    public bool IsActive { get; set; }

    public byte[] RowVersion { get; set; } = [];
}
