import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  CreateEmployeeRequest,
  EmployeeResponse,
  PagedResponse,
  UpdateEmployeeRequest
} from '../models/employee';

@Injectable({
  providedIn: 'root'
})
export class EmployeeService {
  private readonly apiUrl = `${environment.apiBaseUrl}/api/employees`;

  constructor(private readonly http: HttpClient) { }

  getEmployees(page = 1, pageSize = 10, search = ''): Observable<PagedResponse<EmployeeResponse>> {
    let params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);

    if (search.trim()) {
      params = params.set('search', search.trim());
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