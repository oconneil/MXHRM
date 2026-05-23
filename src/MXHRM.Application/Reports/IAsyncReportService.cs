using MXHRM.Application.Common;
using MXHRM.Application.Reports.DTOs;
using MXHRM.Application.Reports.Exports;

namespace MXHRM.Application.Reports;

public interface IAsyncReportService
{
    Task<GeneratedReportResponse> CreateAsync(
        CreateGeneratedReportRequest request,
        CancellationToken cancellationToken = default);

    Task<PagedResponse<GeneratedReportResponse>> GetAllAsync(
        GeneratedReportListRequest request,
        CancellationToken cancellationToken = default);

    Task<GeneratedReportResponse?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<ReportFileResponse?> DownloadAsync(
        long id,
        CancellationToken cancellationToken = default);
}