namespace MXHRM.Application.Reports.DTOs;

public sealed class GeneratedReportListRequest
{
    public string? ReportType { get; set; }

    public string? Format { get; set; }

    public string? Status { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}