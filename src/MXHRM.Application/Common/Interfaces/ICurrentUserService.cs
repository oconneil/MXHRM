namespace MXHRM.Application.Common.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }

    string? UserName { get; }

    string? TraceId { get; }

    bool IsAuthenticated { get; }
}