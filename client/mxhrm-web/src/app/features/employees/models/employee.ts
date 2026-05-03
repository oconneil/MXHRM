export interface CreateEmployeeRequest {
  companyID: string;
  employeeID: string;
  firstName: string;
  lastName: string;
  email: string;
  hireDate: string;
  salary: number;
  createdBy: string;
}

export interface UpdateEmployeeRequest {
  firstName: string;
  lastName: string;
  email: string;
  hireDate: string;
  salary: number;
  isActive: boolean;
  modifiedBy: string;
  rowVersion: string;
}

export interface EmployeeResponse {
  companyID: string;
  employeeID: string;
  fullName: string;
  firstName: string;
  lastName: string;
  email: string;
  hireDate: string;
  salary: number;
  isActive: boolean;
  rowVersion: string;
}

export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}