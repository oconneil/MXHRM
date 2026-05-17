namespace MXHRM.Application.Auditing.DTOs;

public class CreateUserActivityLogRequest
{
    public string ActivityType { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? Metadata { get; set; }
}