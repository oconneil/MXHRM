using MXHRM.Api.Common;
using System.ComponentModel.DataAnnotations;

namespace MXHRM.Api.Models;

public class Employee : BaseEntity
{
    [Required(ErrorMessage = "EmployeeID is required.")]
    [MaxLength(20)]
    public string EmployeeID { get; set; } = string.Empty;

    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    public DateTime HireDate { get; set; }

    public decimal Salary { get; set; }

    public bool IsActive { get; set; } = true;
}