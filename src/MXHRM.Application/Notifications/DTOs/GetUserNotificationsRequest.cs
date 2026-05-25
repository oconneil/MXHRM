namespace MXHRM.Application.Notifications.DTOs;

public sealed class GetUserNotificationsRequest
{
    public bool? IsRead { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}