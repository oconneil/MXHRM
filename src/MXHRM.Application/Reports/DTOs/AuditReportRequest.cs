namespace MXHRM.Application.Reports.DTOs;

public sealed class AuditReportRequest
{
    public string? TableName { get; set; }

    public string? Action { get; set; }

    public string? UserId { get; set; }

    public DateTime? FromUtc { get; set; }

    public DateTime? ToUtc { get; set; }
}