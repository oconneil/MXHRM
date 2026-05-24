namespace MXHRM.Domain.Notifications;

public sealed class UserNotification
{
    public long Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string? Key { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string Tone { get; set; } = "info";

    public string? DataJson { get; set; }

    public string? Route { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public DateTime? ReadAtUtc { get; set; }
}