using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace MXHRM.Api.SignalR;

public sealed class UserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}