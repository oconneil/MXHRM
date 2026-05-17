namespace MXHRM.Infrastructure.Auditing;

public class UserActivityLog
{
    public long Id { get; set; }

    public string ActivityType { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? Metadata { get; set; }

    public string? UserId { get; set; }

    public string? UserName { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public string? TraceId { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}