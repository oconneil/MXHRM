namespace MXHRM.Application.Reports.DTOs;

public sealed class AuditActionSummaryResponse
{
    public string Action { get; set; } = string.Empty;

    public int Count { get; set; }
}