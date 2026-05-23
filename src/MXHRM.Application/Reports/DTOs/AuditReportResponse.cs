namespace MXHRM.Application.Reports.DTOs;

public sealed class AuditReportResponse
{
    public int TotalAuditLogs { get; set; }

    public DateTime GeneratedAtUtc { get; set; }

    public IReadOnlyList<AuditActionSummaryResponse> ByAction { get; set; } =
        [];

    public IReadOnlyList<AuditTableSummaryResponse> ByTable { get; set; } =
        [];

    public IReadOnlyList<AuditUserSummaryResponse> ByUser { get; set; } =
        [];
}