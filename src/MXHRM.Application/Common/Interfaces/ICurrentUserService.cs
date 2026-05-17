namespace MXHRM.Application.Common.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }

    string? UserName { get; }

    string? TraceId { get; }

    string? IpAddress { get; }

    string? UserAgent { get; }

    bool IsAuthenticated { get; }
}