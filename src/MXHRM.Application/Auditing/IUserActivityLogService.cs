using MXHRM.Application.Auditing.DTOs;
using MXHRM.Application.Common;

namespace MXHRM.Application.Auditing;

public interface IUserActivityLogService
{
    Task LogAsync(
        CreateUserActivityLogRequest request,
        CancellationToken cancellationToken = default);

    Task<PagedResponse<UserActivityLogResponse>> GetAllAsync(
        GetUserActivityLogsRequest request,
        CancellationToken cancellationToken = default);

    Task<UserActivityLogResponse?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default);
}