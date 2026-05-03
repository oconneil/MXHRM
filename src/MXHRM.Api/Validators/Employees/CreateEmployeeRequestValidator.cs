using FluentValidation;
using MXHRM.Api.DTOs.Employees;

namespace MXHRM.Api.Validators.Employees;

public class CreateEmployeeRequestValidator : AbstractValidator<CreateEmployeeRequest>
{
    public CreateEmployeeRequestValidator()
    {
        RuleFor(x => x.CompanyID)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.EmployeeID)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(150);

        RuleFor(x => x.HireDate)
            .NotEmpty();

        RuleFor(x => x.Salary)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.CreatedBy)
            .NotEmpty()
            .MaximumLength(100);
    }
}