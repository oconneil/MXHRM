namespace MXHRM.Application.Reports.DTOs;

public sealed class EmployeeSummaryReportResponse
{
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public int InactiveEmployees { get; set; }
    public decimal AverageSalary { get; set; }
    public decimal TotalSalary { get; set; }
    public DateTime GeneratedAtUtc { get; set; }
    public IReadOnlyList<EmployeeSummaryByCompanyResponse> ByCompany { get; set; } = [];
}