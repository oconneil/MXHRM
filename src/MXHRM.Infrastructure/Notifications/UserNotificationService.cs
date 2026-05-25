using Microsoft.EntityFrameworkCore;
using MXHRM.Application.Common;
using MXHRM.Application.Notifications;
using MXHRM.Application.Notifications.DTOs;
using MXHRM.Domain.Notifications;
using MXHRM.Infrastructure.Data;

namespace MXHRM.Infrastructure.Notifications;

public sealed class UserNotificationService : IUserNotificationService
{
    private readonly AppDbContext _db;

    public UserNotificationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<UserNotificationResponse> CreateOrUpdateAsync(
        CreateUserNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.UserId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Type);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Title);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Message);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Tone);

        var userId = request.UserId.Trim();
        var key = NormalizeOptional(request.Key);
        var now = DateTime.UtcNow;

        UserNotification? notification = null;

        if (key is not null)
        {
            notification = await _db.UserNotifications
                .FirstOrDefaultAsync(
                    x => x.UserId == userId && x.Key == key,
                    cancellationToken);
        }

        if (notification is null)
        {
            notification = new UserNotification
            {
                UserId = userId,
                Type = request.Type.Trim(),
                Key = key,
                Title = request.Title.Trim(),
                Message = request.Message.Trim(),
                Tone = request.Tone.Trim(),
                DataJson = NormalizeOptional(request.DataJson),
                Route = NormalizeOptional(request.Route),
                IsRead = false,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            _db.UserNotifications.Add(notification);
        }
        else
        {
            notification.Type = request.Type.Trim();
            notification.Title = request.Title.Trim();
            notification.Message = request.Message.Trim();
            notification.Tone = request.Tone.Trim();
            notification.DataJson = NormalizeOptional(request.DataJson);
            notification.Route = NormalizeOptional(request.Route);
            notification.IsRead = false;
            notification.ReadAtUtc = null;
            notification.UpdatedAtUtc = now;
        }

        await _db.SaveChangesAsync(cancellationToken);

        return MapToResponse(notification);
    }

    public async Task<PagedResponse<UserNotificationResponse>> GetAllAsync(
        string userId,
        GetUserNotificationsRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentNullException.ThrowIfNull(request);

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1
            ? 20
            : Math.Min(request.PageSize, 100);

        var query = _db.UserNotifications
            .AsNoTracking()
            .Where(x => x.UserId == userId.Trim());

        if (request.IsRead.HasValue)
        {
            query = query.Where(x => x.IsRead == request.IsRead.Value);
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.UpdatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new UserNotificationResponse
            {
                Id = x.Id,
                Type = x.Type,
                Key = x.Key,
                Title = x.Title,
                Message = x.Message,
                Tone = x.Tone,
                DataJson = x.DataJson,
                Route = x.Route,
                IsRead = x.IsRead,
                CreatedAtUtc = x.CreatedAtUtc,
                UpdatedAtUtc = x.UpdatedAtUtc,
                ReadAtUtc = x.ReadAtUtc
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<UserNotificationResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<int> GetUnreadCountAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        return await _db.UserNotifications
            .AsNoTracking()
            .CountAsync(
                x => x.UserId == userId.Trim() && !x.IsRead,
                cancellationToken);
    }

    public async Task<bool> MarkAsReadAsync(
        long id,
        string userId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var notification = await _db.UserNotifications
            .FirstOrDefaultAsync(
                x => x.Id == id && x.UserId == userId.Trim(),
                cancellationToken);

        if (notification is null)
        {
            return false;
        }

        if (notification.IsRead)
        {
            return true;
        }

        notification.IsRead = true;
        notification.ReadAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task MarkAllAsReadAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var readAtUtc = DateTime.UtcNow;

        await _db.UserNotifications
            .Where(x => x.UserId == userId.Trim() && !x.IsRead)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.IsRead, true)
                    .SetProperty(x => x.ReadAtUtc, (DateTime?)readAtUtc),
                cancellationToken);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static UserNotificationResponse MapToResponse(
        UserNotification notification)
    {
        return new UserNotificationResponse
        {
            Id = notification.Id,
            Type = notification.Type,
            Key = notification.Key,
            Title = notification.Title,
            Message = notification.Message,
            Tone = notification.Tone,
            DataJson = notification.DataJson,
            Route = notification.Route,
            IsRead = notification.IsRead,
            CreatedAtUtc = notification.CreatedAtUtc,
            UpdatedAtUtc = notification.UpdatedAtUtc,
            ReadAtUtc = notification.ReadAtUtc
        };
    }
}