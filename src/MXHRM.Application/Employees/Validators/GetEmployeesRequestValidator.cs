using FluentValidation;
using MXHRM.Application.Employees.DTOs;

namespace MXHRM.Application.Employees.Validators;

public class GetEmployeesRequestValidator : AbstractValidator<GetEmployeesRequest>
{
    private static readonly string[] AllowedSortFields =
    [
        "employeeId",
        "firstName",
        "lastName",
        "email",
        "hireDate",
        "salary"
    ];

    private static readonly string[] AllowedSortDirections =
    [
        "asc",
        "desc"
    ];

    public GetEmployeesRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.Search)
            .MaximumLength(100);

        RuleFor(x => x.CompanyID)
            .MaximumLength(20);

        RuleFor(x => x.SortBy)
            .Must(sortBy => string.IsNullOrWhiteSpace(sortBy) ||
                            AllowedSortFields.Contains(sortBy))
            .WithMessage("SortBy must be one of: employeeId, firstName, lastName, email, hireDate, salary.");

        RuleFor(x => x.SortDirection)
            .Must(direction => string.IsNullOrWhiteSpace(direction) ||
                               AllowedSortDirections.Contains(direction))
            .WithMessage("SortDirection must be asc or desc.");
    }
}