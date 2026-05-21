export interface EmployeeSummaryReportRequest {
    companyID?: string | null;
    isActive?: boolean | null;
    hireDateFrom?: string | null;
    hireDateTo?: string | null;
}

export interface EmployeeSummaryByCompanyResponse {
    companyID: string;
    totalEmployees: number;
    activeEmployees: number;
    inactiveEmployees: number;
    averageSalary: number;
    totalSalary: number;
}

export interface EmployeeSummaryReportResponse {
    totalEmployees: number;
    activeEmployees: number;
    inactiveEmployees: number;
    averageSalary: number;
    totalSalary: number;
    generatedAtUtc: string;
    byCompany: EmployeeSummaryByCompanyResponse[];
}