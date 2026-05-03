namespace MXHRM.Api.DTOs.Common;

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;

    public string? Detail { get; set; }

    public string? TraceId { get; set; }
}