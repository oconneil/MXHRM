namespace MXHRM.Application.Reports.DTOs;

public class EmployeeSummaryReport
{
    public int TotalEmployees { get; set; }

    public int ActiveEmployees { get; set; }

    public int InactiveEmployees { get; set; }

    public decimal AverageSalary { get; set; }

    public decimal TotalSalary { get; set; }

    public DateTime GeneratedAtUtc { get; set; }
}