using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MXHRM.Application.Reports;
using MXHRM.Application.Reports.DTOs;
using MXHRM.Application.Reports.Exports;
using MXHRM.Domain.Reports;
using MXHRM.Infrastructure.Data;
using MXHRM.Application.Common.Realtime;

namespace MXHRM.Infrastructure.Reports;

public sealed class GeneratedReportJob
{
    private readonly AppDbContext _db;
    private readonly IReportService _reportService;
    private readonly ILogger<GeneratedReportJob> _logger;
    private readonly IRealtimeNotifier _realtimeNotifier;

    public GeneratedReportJob(
    AppDbContext db,
    IReportService reportService,
    IRealtimeNotifier realtimeNotifier,
    ILogger<GeneratedReportJob> logger)
    {
        _db = db;
        _reportService = reportService;
        _realtimeNotifier = realtimeNotifier;
        _logger = logger;
    }

    public async Task ProcessAsync(
        long generatedReportId,
        CancellationToken cancellationToken)
    {
        var generatedReport = await _db.GeneratedReports
            .FirstOrDefaultAsync(x => x.Id == generatedReportId, cancellationToken);

        if (generatedReport is null)
        {
            _logger.LogWarning(
                "Generated report {GeneratedReportId} was not found.",
                generatedReportId);

            return;
        }

        generatedReport.Status = ReportStatuses.Processing;
        generatedReport.StartedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        // Notify client about status update
        await NotifyAsync(generatedReport, cancellationToken);

        try
        {
            var request = JsonSerializer.Deserialize<CreateGeneratedReportRequest>(
                generatedReport.RequestJson ?? "{}");

            if (request is null)
            {
                throw new InvalidOperationException("Generated report request is invalid.");
            }

            var file = await GenerateFileAsync(request, cancellationToken);

            generatedReport.Status = ReportStatuses.Completed;
            generatedReport.FileName = file.FileName;
            generatedReport.ContentType = file.ContentType;
            generatedReport.Content = file.Content;
            generatedReport.CompletedAtUtc = DateTime.UtcNow;
            generatedReport.ErrorMessage = null;

            await _db.SaveChangesAsync(cancellationToken);

            // Notify client about status update
            await NotifyAsync(generatedReport, cancellationToken);

            _logger.LogInformation(
                "Generated report {GeneratedReportId} completed.",
                generatedReportId);
        }
        catch (Exception ex)
        {
            generatedReport.Status = ReportStatuses.Failed;
            generatedReport.ErrorMessage = ex.Message;
            generatedReport.CompletedAtUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);

            // Notify client about status update
            await NotifyAsync(generatedReport, cancellationToken);

            _logger.LogError(
                ex,
                "Generated report {GeneratedReportId} failed.",
                generatedReportId);
        }
    }

    private async Task<ReportFileResponse> GenerateFileAsync(
        CreateGeneratedReportRequest request,
        CancellationToken cancellationToken)
    {
        if (request.ReportType == ReportTypes.EmployeeSummary &&
            request.Format == ReportFormats.Excel)
        {
            return await _reportService.ExportEmployeeSummaryExcelAsync(
                request.EmployeeSummaryRequest ?? new EmployeeSummaryReportRequest(),
                cancellationToken);
        }

        if (request.ReportType == ReportTypes.EmployeeSummary &&
            request.Format == ReportFormats.Pdf)
        {
            return await _reportService.ExportEmployeeSummaryPdfAsync(
                request.EmployeeSummaryRequest ?? new EmployeeSummaryReportRequest(),
                cancellationToken);
        }

        if (request.ReportType == ReportTypes.AuditReport &&
            request.Format == ReportFormats.Excel)
        {
            return await _reportService.ExportAuditReportExcelAsync(
                request.AuditReportRequest ?? new AuditReportRequest(),
                cancellationToken);
        }

        throw new NotSupportedException(
            $"Report type '{request.ReportType}' with format '{request.Format}' is not supported.");
    }

    private async Task NotifyAsync(
    GeneratedReport generatedReport,
    CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(generatedReport.RequestedByUserId))
        {
            return;
        }

        await _realtimeNotifier.SendToUserAsync(
            generatedReport.RequestedByUserId,
            new RealtimeMessage
            {
                Type = "generated-report.updated",
                Title = "Generated report updated",
                Message = $"Report status is {generatedReport.Status}.",
                Data = MapToResponse(generatedReport)
            },
            cancellationToken);
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