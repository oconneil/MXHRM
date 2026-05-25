using MXHRM.Application.Common;
using MXHRM.Application.Notifications.DTOs;

namespace MXHRM.Application.Notifications;

public interface IUserNotificationService
{
    Task<UserNotificationResponse> CreateOrUpdateAsync(
        CreateUserNotificationRequest request,
        CancellationToken cancellationToken = default);

    Task<PagedResponse<UserNotificationResponse>> GetAllAsync(
        string userId,
        GetUserNotificationsRequest request,
        CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<bool> MarkAsReadAsync(
        long id,
        string userId,
        CancellationToken cancellationToken = default);

    Task MarkAllAsReadAsync(
        string userId,
        CancellationToken cancellationToken = default);
}