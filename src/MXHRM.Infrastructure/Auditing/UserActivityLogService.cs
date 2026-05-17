using MXHRM.Application.Auditing;
using MXHRM.Application.Auditing.DTOs;
using MXHRM.Application.Common.Interfaces;
using MXHRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using MXHRM.Application.Common;

namespace MXHRM.Infrastructure.Auditing;

public class UserActivityLogService : IUserActivityLogService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUserService;

    public UserActivityLogService(
        AppDbContext db,
        ICurrentUserService currentUserService)
    {
        _db = db;
        _currentUserService = currentUserService;
    }

    public async Task LogAsync(
        CreateUserActivityLogRequest request,
        CancellationToken cancellationToken = default)
    {
        var activityLog = new UserActivityLog
        {
            ActivityType = request.ActivityType,
            Description = request.Description,
            Metadata = request.Metadata,
            UserId = _currentUserService.UserId,
            UserName = _currentUserService.UserName,
            IpAddress = _currentUserService.IpAddress,
            UserAgent = _currentUserService.UserAgent,
            TraceId = _currentUserService.TraceId,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.UserActivityLogs.Add(activityLog);

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResponse<UserActivityLogResponse>> GetAllAsync(
    GetUserActivityLogsRequest request,
    CancellationToken cancellationToken = default)
    {
        var query = _db.UserActivityLogs
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.ActivityType))
        {
            query = query.Where(x => x.ActivityType == request.ActivityType.Trim());
        }

        if (!string.IsNullOrWhiteSpace(request.UserId))
        {
            query = query.Where(x => x.UserId == request.UserId.Trim());
        }

        if (!string.IsNullOrWhiteSpace(request.UserName))
        {
            query = query.Where(x => x.UserName == request.UserName.Trim());
        }

        if (request.FromUtc.HasValue)
        {
            query = query.Where(x => x.CreatedAtUtc >= request.FromUtc.Value);
        }

        if (request.ToUtc.HasValue)
        {
            query = query.Where(x => x.CreatedAtUtc <= request.ToUtc.Value);
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new UserActivityLogResponse
            {
                Id = x.Id,
                ActivityType = x.ActivityType,
                Description = x.Description,
                Metadata = x.Metadata,
                UserId = x.UserId,
                UserName = x.UserName,
                IpAddress = x.IpAddress,
                UserAgent = x.UserAgent,
                TraceId = x.TraceId,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<UserActivityLogResponse>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize)
        };
    }

    public async Task<UserActivityLogResponse?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        return await _db.UserActivityLogs
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new UserActivityLogResponse
            {
                Id = x.Id,
                ActivityType = x.ActivityType,
                Description = x.Description,
                Metadata = x.Metadata,
                UserId = x.UserId,
                UserName = x.UserName,
                IpAddress = x.IpAddress,
                UserAgent = x.UserAgent,
                TraceId = x.TraceId,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}