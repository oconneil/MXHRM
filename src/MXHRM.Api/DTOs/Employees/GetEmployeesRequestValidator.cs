using FluentValidation;
using MXHRM.Api.DTOs.Employees;

namespace MXHRM.Api.Validators.Employees;

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