using MXHRM.Application.Reports.DTOs;
using MXHRM.Application.Reports.Exports;

namespace MXHRM.Application.Reports;

public interface IReportService
{
    Task<EmployeeSummaryReportResponse> GetEmployeeSummaryAsync(
        EmployeeSummaryReportRequest request,
        CancellationToken cancellationToken = default);

    Task<ReportFileResponse> ExportEmployeeSummaryExcelAsync(
        EmployeeSummaryReportRequest request,
        CancellationToken cancellationToken = default);

    Task<AuditReportResponse> GetAuditReportAsync(
        AuditReportRequest request,
        CancellationToken cancellationToken = default);
}