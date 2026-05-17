using FluentValidation;
using MXHRM.Application.Auditing.DTOs;

namespace MXHRM.Application.Auditing.Validators;

public class GetUserActivityLogsRequestValidator : AbstractValidator<GetUserActivityLogsRequest>
{
    private static readonly string[] AllowedActivityTypes =
    [
        "LoginSuccess",
        "LoginFailed",
        "RefreshToken",
        "RefreshTokenFailed",
        "Logout",
        "LogoutFailed",
        "ManualJobTrigger"
    ];

    public GetUserActivityLogsRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.ActivityType)
            .Must(activityType => string.IsNullOrWhiteSpace(activityType) ||
                                  AllowedActivityTypes.Contains(activityType))
            .WithMessage("ActivityType is invalid.");

        RuleFor(x => x.UserId)
            .MaximumLength(450);

        RuleFor(x => x.UserName)
            .MaximumLength(256);

        RuleFor(x => x)
            .Must(x => !x.FromUtc.HasValue ||
                       !x.ToUtc.HasValue ||
                       x.FromUtc.Value <= x.ToUtc.Value)
            .WithMessage("FromUtc must be less than or equal to ToUtc.");
    }
}