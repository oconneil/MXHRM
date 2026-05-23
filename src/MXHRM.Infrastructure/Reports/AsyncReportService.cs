using System.Text.Json;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using MXHRM.Application.Common;
using MXHRM.Application.Common.Interfaces;
using MXHRM.Application.Reports;
using MXHRM.Application.Reports.DTOs;
using MXHRM.Application.Reports.Exports;
using MXHRM.Domain.Reports;
using MXHRM.Infrastructure.Data;

namespace MXHRM.Infrastructure.Reports;

public sealed class AsyncReportService : IAsyncReportService
{
    private readonly AppDbContext _db;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ICurrentUserService _currentUserService;

    public AsyncReportService(
        AppDbContext db,
        IBackgroundJobClient backgroundJobClient,
        ICurrentUserService currentUserService)
    {
        _db = db;
        _backgroundJobClient = backgroundJobClient;
        _currentUserService = currentUserService;
    }

    public async Task<GeneratedReportResponse> CreateAsync(
        CreateGeneratedReportRequest request,
        CancellationToken cancellationToken)
    {
        var requestJson = JsonSerializer.Serialize(request);

        var generatedReport = new GeneratedReport
        {
            ReportType = request.ReportType,
            Format = request.Format,
            Status = ReportStatuses.Pending,
            RequestJson = requestJson,
            RequestedByUserId = _currentUserService.UserId,
            RequestedByUserName = _currentUserService.UserName,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.GeneratedReports.Add(generatedReport);
        await _db.SaveChangesAsync(cancellationToken);

        _backgroundJobClient.Enqueue<GeneratedReportJob>(
            job => job.ProcessAsync(generatedReport.Id, CancellationToken.None));

        return MapToResponse(generatedReport);
    }

    public async Task<PagedResponse<GeneratedReportResponse>> GetAllAsync(
        GeneratedReportListRequest request,
        CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize;

        var query = _db.GeneratedReports
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.ReportType))
        {
            query = query.Where(x => x.ReportType == request.ReportType.Trim());
        }

        if (!string.IsNullOrWhiteSpace(request.Format))
        {
            query = query.Where(x => x.Format == request.Format.Trim());
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(x => x.Status == request.Status.Trim());
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new GeneratedReportResponse
            {
                Id = x.Id,
                ReportType = x.ReportType,
                Format = x.Format,
                Status = x.Status,
                FileName = x.FileName,
                ContentType = x.ContentType,
                ErrorMessage = x.ErrorMessage,
                RequestedByUserId = x.RequestedByUserId,
                RequestedByUserName = x.RequestedByUserName,
                CreatedAtUtc = x.CreatedAtUtc,
                StartedAtUtc = x.StartedAtUtc,
                CompletedAtUtc = x.CompletedAtUtc
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<GeneratedReportResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<GeneratedReportResponse?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken)
    {
        var generatedReport = await _db.GeneratedReports
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return generatedReport is null
            ? null
            : MapToResponse(generatedReport);
    }

    public async Task<ReportFileResponse?> DownloadAsync(
        long id,
        CancellationToken cancellationToken)
    {
        var generatedReport = await _db.GeneratedReports
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (generatedReport is null ||
            generatedReport.Status != ReportStatuses.Completed ||
            generatedReport.Content is null ||
            string.IsNullOrWhiteSpace(generatedReport.FileName) ||
            string.IsNullOrWhiteSpace(generatedReport.ContentType))
        {
            return null;
        }

        return new ReportFileResponse
        {
            Content = generatedReport.Content,
            ContentType = generatedReport.ContentType,
            FileName = generatedReport.FileName
        };
    }

    private static GeneratedReportResponse MapToResponse(
        GeneratedReport generatedReport)
    {
        return new GeneratedReportResponse
        {
            Id = generatedReport.Id,
            ReportType = generatedReport.ReportType,
            Format = generatedReport.Format,
            Status = generatedReport.Status,
            FileName = generatedReport.FileName,
            ContentType = generatedReport.ContentType,
            ErrorMessage = generatedReport.ErrorMessage,
            RequestedByUserId = generatedReport.RequestedByUserId,
            RequestedByUserName = generatedReport.RequestedByUserName,
            CreatedAtUtc = generatedReport.CreatedAtUtc,
            StartedAtUtc = generatedReport.StartedAtUtc,
            CompletedAtUtc = generatedReport.CompletedAtUtc
        };
    }
}