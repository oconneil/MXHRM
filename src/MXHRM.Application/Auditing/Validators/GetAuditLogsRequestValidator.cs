using FluentValidation;
using MXHRM.Application.Auditing.DTOs;

namespace MXHRM.Application.Auditing.Validators;

public class GetAuditLogsRequestValidator : AbstractValidator<GetAuditLogsRequest>
{
    private static readonly string[] AllowedActions =
    [
        "Insert",
        "Update",
        "Delete"
    ];

    public GetAuditLogsRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.TableName)
            .MaximumLength(128);

        RuleFor(x => x.Action)
            .Must(action => string.IsNullOrWhiteSpace(action) ||
                            AllowedActions.Contains(action))
            .WithMessage("Action must be one of: Insert, Update, Delete.");

        RuleFor(x => x.UserId)
            .MaximumLength(450);

        RuleFor(x => x)
            .Must(x => !x.FromUtc.HasValue ||
                       !x.ToUtc.HasValue ||
                       x.FromUtc.Value <= x.ToUtc.Value)
            .WithMessage("FromUtc must be less than or equal to ToUtc.");
    }
}