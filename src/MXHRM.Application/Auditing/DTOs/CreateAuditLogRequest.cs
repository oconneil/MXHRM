namespace MXHRM.Application.Auditing.DTOs;

public class CreateAuditLogRequest
{
    public string TableName { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public string? KeyValues { get; set; }

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public string? ChangedColumns { get; set; }
}