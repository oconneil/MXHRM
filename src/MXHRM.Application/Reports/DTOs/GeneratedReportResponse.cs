namespace MXHRM.Application.Reports.DTOs;

public sealed class GeneratedReportResponse
{
    public long Id { get; set; }

    public string ReportType { get; set; } = string.Empty;

    public string Format { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? FileName { get; set; }

    public string? ContentType { get; set; }

    public string? ErrorMessage { get; set; }

    public string? RequestedByUserId { get; set; }

    public string? RequestedByUserName { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? StartedAtUtc { get; set; }

    public DateTime? CompletedAtUtc { get; set; }
}