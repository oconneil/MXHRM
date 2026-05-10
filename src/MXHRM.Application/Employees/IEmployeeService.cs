using MXHRM.Application.Common;
using MXHRM.Application.Employees.DTOs;

namespace MXHRM.Application.Employees;

public interface IEmployeeService
{
    Task<PagedResponse<EmployeeResponse>> GetAllAsync(GetEmployeesRequest request);
    Task<EmployeeResponse?> GetByIdAsync(string companyId, string employeeId);
    Task<EmployeeResponse> CreateAsync(CreateEmployeeRequest request);
    Task<bool> UpdateAsync(string companyId, string employeeId, UpdateEmployeeRequest request);
    Task<bool> DeleteAsync(string companyId, string employeeId);
}
