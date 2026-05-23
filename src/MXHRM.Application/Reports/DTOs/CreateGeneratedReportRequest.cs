namespace MXHRM.Application.Reports.DTOs;

public sealed class CreateGeneratedReportRequest
{
    public string ReportType { get; set; } = string.Empty;

    public string Format { get; set; } = string.Empty;

    public EmployeeSummaryReportRequest? EmployeeSummaryRequest { get; set; }

    public AuditReportRequest? AuditReportRequest { get; set; }
}