namespace MXHRM.Application.Auditing.DTOs;

public class GetUserActivityLogsRequest
{
    public string? ActivityType { get; set; }

    public string? UserId { get; set; }

    public string? UserName { get; set; }

    public DateTime? FromUtc { get; set; }

    public DateTime? ToUtc { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}