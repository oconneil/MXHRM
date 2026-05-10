using FluentValidation;
using MXHRM.Application.Employees.DTOs;

namespace MXHRM.Application.Employees.Validators;

public class GetEmployeesRequestValidator : AbstractValidator<GetEmployeesRequest>
{
    public GetEmployeesRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.Search)
            .MaximumLength(100);
    }
}
