using Microsoft.AspNetCore.SignalR;
using MXHRM.Api.Hubs;
using MXHRM.Application.Common.Realtime;

namespace MXHRM.Api.Services;

public sealed class SignalRRealtimeNotifier : IRealtimeNotifier
{
    private readonly IHubContext<RealtimeHub> _hubContext;

    public SignalRRealtimeNotifier(IHubContext<RealtimeHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task SendToUserAsync(
        string userId,
        RealtimeMessage message,
        CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients
            .User(userId)
            .SendAsync("realtimeMessage", message, cancellationToken);
    }

    public Task SendToGroupAsync(
        string groupName,
        RealtimeMessage message,
        CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients
            .Group(groupName)
            .SendAsync("realtimeMessage", message, cancellationToken);
    }

    public Task BroadcastAsync(
        RealtimeMessage message,
        CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients
            .All
            .SendAsync("realtimeMessage", message, cancellationToken);
    }
}