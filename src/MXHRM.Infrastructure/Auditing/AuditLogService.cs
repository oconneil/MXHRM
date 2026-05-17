using MXHRM.Application.Auditing;
using MXHRM.Application.Auditing.DTOs;
using MXHRM.Application.Common.Interfaces;
using MXHRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using MXHRM.Application.Common;

namespace MXHRM.Infrastructure.Auditing;

public class AuditLogService : IAuditLogService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUserService;

    public AuditLogService(
        AppDbContext db,
        ICurrentUserService currentUserService)
    {
        _db = db;
        _currentUserService = currentUserService;
    }

    public async Task LogAsync(
        CreateAuditLogRequest request,
        CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            TableName = request.TableName,
            Action = request.Action,
            KeyValues = request.KeyValues,
            OldValues = request.OldValues,
            NewValues = request.NewValues,
            ChangedColumns = request.ChangedColumns,
            UserId = _currentUserService.UserId,
            UserName = _currentUserService.UserName,
            TraceId = _currentUserService.TraceId,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.AuditLogs.Add(auditLog);

        await _db.SaveChangesAsync(cancellationToken);
    }
    public async Task<PagedResponse<AuditLogResponse>> GetAllAsync(
    GetAuditLogsRequest request,
    CancellationToken cancellationToken = default)
    {
        var query = _db.AuditLogs
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.TableName))
        {
            query = query.Where(x => x.TableName == request.TableName.Trim());
        }

        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            query = query.Where(x => x.Action == request.Action.Trim());
        }

        if (!string.IsNullOrWhiteSpace(request.UserId))
        {
            query = query.Where(x => x.UserId == request.UserId.Trim());
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
            .Select(x => new AuditLogResponse
            {
                Id = x.Id,
                TableName = x.TableName,
                Action = x.Action,
                KeyValues = x.KeyValues,
                OldValues = x.OldValues,
                NewValues = x.NewValues,
                ChangedColumns = x.ChangedColumns,
                UserId = x.UserId,
                UserName = x.UserName,
                TraceId = x.TraceId,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<AuditLogResponse>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize)
        };
    }

    public async Task<AuditLogResponse?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        return await _db.AuditLogs
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new AuditLogResponse
            {
                Id = x.Id,
                TableName = x.TableName,
                Action = x.Action,
                KeyValues = x.KeyValues,
                OldValues = x.OldValues,
                NewValues = x.NewValues,
                ChangedColumns = x.ChangedColumns,
                UserId = x.UserId,
                UserName = x.UserName,
                TraceId = x.TraceId,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}