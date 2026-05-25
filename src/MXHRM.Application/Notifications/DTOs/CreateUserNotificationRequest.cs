namespace MXHRM.Application.Notifications.DTOs;

public sealed class CreateUserNotificationRequest
{
    public string UserId { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string? Key { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string Tone { get; set; } = "info";

    public string? DataJson { get; set; }

    public string? Route { get; set; }
}