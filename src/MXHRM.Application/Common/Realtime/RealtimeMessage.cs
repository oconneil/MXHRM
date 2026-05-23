namespace MXHRM.Application.Common.Realtime;

public sealed class RealtimeMessage
{
    public string Type { get; set; } = string.Empty;

    public string? Title { get; set; }

    public string? Message { get; set; }

    public object? Data { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}