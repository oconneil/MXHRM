namespace MXHRM.Application.Common.Realtime;

public sealed class RealtimeMessage
{
    public long? NotificationId { get; set; }

    public string? Key { get; set; }

    public string Type { get; set; } = string.Empty;

    public string? Title { get; set; }

    public string? Message { get; set; }

    public string? Tone { get; set; }

    public string? Route { get; set; }

    public object? Data { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}