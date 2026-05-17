namespace MXHRM.Application.Auditing.DTOs;

public class AuditLogResponse
{
    public long Id { get; set; }

    public string TableName { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public string? KeyValues { get; set; }

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public string? ChangedColumns { get; set; }

    public string? UserId { get; set; }

    public string? UserName { get; set; }

    public string? TraceId { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}