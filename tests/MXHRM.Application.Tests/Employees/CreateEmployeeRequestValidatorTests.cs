using FluentValidation.TestHelper;
using MXHRM.Application.Employees.DTOs;
using MXHRM.Application.Employees.Validators;
using Xunit;

namespace MXHRM.Application.Tests.Employees;

public class CreateEmployeeRequestValidatorTests
{
    private readonly CreateEmployeeRequestValidator _validator = new();

    // helper: สร้าง request ที่ "ถูกต้องสมบูรณ์" ไว้เป็นจุดตั้งต้น
    // แล้วแต่ละเทสต์ค่อยแก้ field เดียวให้ผิด → รู้ชัดว่าพังเพราะ field ไหน
    private static CreateEmployeeRequest ValidRequest() => new()
    {
        EmployeeID = "E001",
        FirstName = "John",
        LastName = "Doe",
        Email = "john.doe@example.com",
        HireDate = new DateTime(2024, 1, 1),
        Salary = 50000m
    };

    [Fact]   // [Fact] = เทสต์ 1 เคส
    public void Valid_request_should_pass()
    {
        // Arrange
        var request = ValidRequest();

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_FirstName_should_fail()
    {
        var request = ValidRequest();
        request.FirstName = "";                        // Arrange: ทำให้ผิดจุดเดียว

        var result = _validator.TestValidate(request);  // Act

        result.ShouldHaveValidationErrorFor(x => x.FirstName); // Assert
    }

    [Theory]  // [Theory] = เทสต์เดียว แต่ยิงหลาย input (data-driven)
    [InlineData("not-an-email")]
    [InlineData("missing-at-sign.com")]
    [InlineData("")]
    public void Invalid_Email_should_fail(string email)
    {
        var request = ValidRequest();
        request.Email = email;

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Negative_Salary_should_fail()
    {
        var request = ValidRequest();
        request.Salary = -1m;

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Salary);
    }
}
