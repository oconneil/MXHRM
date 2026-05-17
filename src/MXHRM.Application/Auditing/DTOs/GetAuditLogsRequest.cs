namespace MXHRM.Application.Auditing.DTOs;

public class GetAuditLogsRequest
{
    public string? TableName { get; set; }

    public string? Action { get; set; }

    public string? UserId { get; set; }

    public DateTime? FromUtc { get; set; }

    public DateTime? ToUtc { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}