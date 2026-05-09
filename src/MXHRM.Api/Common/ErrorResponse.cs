namespace MXHRM.Api.Common;

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public object? Details { get; set; }
    public string? TraceId { get; set; }
}
