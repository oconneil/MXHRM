namespace MXHRM.Application.Reports.DTOs;

public sealed class EmployeeSummaryReportRequest
{
    public string? CompanyID { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? HireDateFrom { get; set; }
    public DateTime? HireDateTo { get; set; }
}