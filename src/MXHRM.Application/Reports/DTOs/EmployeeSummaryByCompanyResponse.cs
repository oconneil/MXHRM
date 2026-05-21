namespace MXHRM.Application.Reports.DTOs;

public sealed class EmployeeSummaryByCompanyResponse
{
    public string CompanyID { get; set; } = string.Empty;
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public int InactiveEmployees { get; set; }
    public decimal AverageSalary { get; set; }
    public decimal TotalSalary { get; set; }
}