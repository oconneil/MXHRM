using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MXHRM.Application.Reports;
using MXHRM.Application.Reports.DTOs;
using MXHRM.Application.Reports.Exports;
using MXHRM.Domain.Reports;
using MXHRM.Infrastructure.Data;
using MXHRM.Application.Common.Realtime;
using MXHRM.Application.Notifications;
using MXHRM.Application.Notifications.DTOs;

namespace MXHRM.Infrastructure.Reports;

public sealed class GeneratedReportJob
{
    private static readonly JsonSerializerOptions NotificationJsonOptions = new(JsonSerializerDefaults.Web);
    private readonly AppDbContext _db;
    private readonly IReportService _reportService;
    private readonly ILogger<GeneratedReportJob> _logger;
    private readonly IRealtimeNotifier _realtimeNotifier;
    private readonly IUserNotificationService _notificationService;

    public GeneratedReportJob(
    AppDbContext db,
    IReportService reportService,
    IRealtimeNotifier realtimeNotifier,
    IUserNotificationService notificationService,
    ILogger<GeneratedReportJob> logger)
    {
        _db = db;
        _reportService = reportService;
        _realtimeNotifier = realtimeNotifier;
        _notificationService = notificationService;
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
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            generatedReport.Status = ReportStatuses.Failed;
            generatedReport.ErrorMessage = ex.Message;
            generatedReport.CompletedAtUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);

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

        var reportResponse = MapToResponse(generatedReport);
        var content = GetNotificationContent(generatedReport.Status);

        UserNotificationResponse notification;

        try
        {
            notification = await _notificationService.CreateOrUpdateAsync(
                new CreateUserNotificationRequest
                {
                    UserId = generatedReport.RequestedByUserId,
                    Type = "generated-report.updated",
                    Key = $"generated-report:{generatedReport.Id}",
                    Title = content.Title,
                    Message = content.Message,
                    Tone = content.Tone,
                    DataJson = JsonSerializer.Serialize(
                        reportResponse,
                        NotificationJsonOptions),
                    Route = "/reports/generated"
                },
                cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(
                ex,
                "Failed to persist notification for generated report {GeneratedReportId}.",
                generatedReport.Id);

            return;
        }

        try
        {
            await _realtimeNotifier.SendToUserAsync(
                generatedReport.RequestedByUserId,
                new RealtimeMessage
                {
                    NotificationId = notification.Id,
                    Key = notification.Key,
                    Type = notification.Type,
                    Title = notification.Title,
                    Message = notification.Message,
                    Tone = notification.Tone,
                    Route = notification.Route,
                    Data = reportResponse,
                    CreatedAtUtc = notification.UpdatedAtUtc
                },
                cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(
                ex,
                "Notification {NotificationId} for generated report {GeneratedReportId} was persisted, but realtime delivery failed.",
                notification.Id,
                generatedReport.Id);
        }
    }

    private static (string Title, string Message, string Tone) GetNotificationContent(
    string status)
    {
        return status switch
        {
            ReportStatuses.Processing => (
                "Report is being generated",
                "Your report is currently being generated.",
                "info"),

            ReportStatuses.Completed => (
                "Report is ready",
                "Your generated report is ready to download.",
                "success"),

            ReportStatuses.Failed => (
                "Report generation failed",
                "Your report could not be generated. Please try again.",
                "danger"),

            _ => (
                "Report updated",
                $"Report status is {status}.",
                "info")
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