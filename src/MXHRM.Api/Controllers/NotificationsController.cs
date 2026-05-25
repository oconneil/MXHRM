using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MXHRM.Application.Common;
using MXHRM.Application.Common.Interfaces;
using MXHRM.Application.Notifications;
using MXHRM.Application.Notifications.DTOs;

namespace MXHRM.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public sealed class NotificationsController(
    IUserNotificationService notificationService,
    ICurrentUserService currentUserService) : BaseApiController
{
    [HttpGet]
    [ProducesResponseType(
        typeof(PagedResponse<UserNotificationResponse>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<UserNotificationResponse>>> GetAll(
        [FromQuery] GetUserNotificationsRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var notifications = await notificationService.GetAllAsync(
            userId,
            request,
            cancellationToken);

        return Ok(notifications);
    }

    [HttpGet("unread-count")]
    [ProducesResponseType(
        typeof(UnreadNotificationCountResponse),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<UnreadNotificationCountResponse>> GetUnreadCount(
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var count = await notificationService.GetUnreadCountAsync(
            userId,
            cancellationToken);

        return Ok(new UnreadNotificationCountResponse
        {
            Count = count
        });
    }

    [HttpPut("{id:long}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(
        long id,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var updated = await notificationService.MarkAsReadAsync(
            id,
            userId,
            cancellationToken);

        if (!updated)
        {
            return NotFoundError("Notification not found.");
        }

        return NoContent();
    }

    [HttpPut("read-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkAllAsRead(
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        await notificationService.MarkAllAsReadAsync(
            userId,
            cancellationToken);

        return NoContent();
    }

    private string? GetCurrentUserId()
    {
        return string.IsNullOrWhiteSpace(currentUserService.UserId)
            ? null
            : currentUserService.UserId;
    }
}