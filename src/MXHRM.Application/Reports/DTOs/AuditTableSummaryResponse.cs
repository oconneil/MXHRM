namespace MXHRM.Application.Reports.DTOs;

public sealed class AuditTableSummaryResponse
{
    public string TableName { get; set; } = string.Empty;

    public int Count { get; set; }
}