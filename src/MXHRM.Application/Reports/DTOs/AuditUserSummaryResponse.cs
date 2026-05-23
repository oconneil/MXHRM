namespace MXHRM.Application.Reports.DTOs;

public sealed class AuditUserSummaryResponse
{
    public string? UserId { get; set; }

    public string? UserName { get; set; }

    public int Count { get; set; }
}