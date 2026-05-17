using MXHRM.Application.Auditing.DTOs;
using MXHRM.Application.Common;

namespace MXHRM.Application.Auditing;

public interface IAuditLogService
{
    Task LogAsync(CreateAuditLogRequest request, CancellationToken cancellationToken = default);

    Task<PagedResponse<AuditLogResponse>> GetAllAsync(
        GetAuditLogsRequest request,
        CancellationToken cancellationToken = default);

    Task<AuditLogResponse?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default);
}