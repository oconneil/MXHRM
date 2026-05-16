import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  CreateEmployeeRequest,
  EmployeeResponse,
  GetEmployeesRequest,
  PagedResponse,
  UpdateEmployeeRequest
} from '../models/employee';

@Injectable({
  providedIn: 'root'
})
export class EmployeeService {
  private readonly apiUrl = `${environment.apiBaseUrl}/api/employees`;

  constructor(private readonly http: HttpClient) { }

  getEmployees(request: GetEmployeesRequest): Observable<PagedResponse<EmployeeResponse>> {
    let params = new HttpParams()
      .set('page', request.page)
      .set('pageSize', request.pageSize);

    if (request.search?.trim()) {
      params = params.set('search', request.search.trim());
    }

    if (request.companyID?.trim()) {
      params = params.set('companyID', request.companyID.trim());
    }

    if (request.isActive !== null && request.isActive !== undefined) {
      params = params.set('isActive', request.isActive);
    }

    if (request.sortBy?.trim()) {
      params = params.set('sortBy', request.sortBy.trim());
    }

    if (request.sortDirection) {
      params = params.set('sortDirection', request.sortDirection);
    }

    return this.http.get<PagedResponse<EmployeeResponse>>(this.apiUrl, { params });
  }

  createEmployee(request: CreateEmployeeRequest): Observable<EmployeeResponse> {
    return this.http.post<EmployeeResponse>(this.apiUrl, request);
  }

  getEmployeeById(companyID: string, employeeID: string): Observable<EmployeeResponse> {
    return this.http.get<EmployeeResponse>(`${this.apiUrl}/${companyID}/${employeeID}`);
  }

  updateEmployee(
    companyID: string,
    employeeID: string,
    request: UpdateEmployeeRequest
  ): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${companyID}/${employeeID}`, request);
  }

  deleteEmployee(companyID: string, employeeID: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${companyID}/${employeeID}`);
  }
}
