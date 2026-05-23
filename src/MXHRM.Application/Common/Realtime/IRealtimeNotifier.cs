namespace MXHRM.Application.Common.Realtime;

public interface IRealtimeNotifier
{
    Task SendToUserAsync(
        string userId,
        RealtimeMessage message,
        CancellationToken cancellationToken = default);

    Task SendToGroupAsync(
        string groupName,
        RealtimeMessage message,
        CancellationToken cancellationToken = default);

    Task BroadcastAsync(
        RealtimeMessage message,
        CancellationToken cancellationToken = default);
}