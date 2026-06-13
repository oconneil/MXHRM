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

export interface GetEmployeesRequest {
  search?: string;
  companyID?: string;
  isActive?: boolean | null;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
  page: number;
  pageSize: number;
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

export interface EmployeeDocument {
  id: string;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  documentType: string;
  uploadedAt: string;
  uploadedBy: string;
}
