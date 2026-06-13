import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { GridDataResult } from '@progress/kendo-angular-grid';
import { State, toDataSourceRequestString } from '@progress/kendo-data-query';
import { map, Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  EmployeeResponse as ApiEmployeeResponse,
  EmployeeResponseGridDataSourceResult as ApiEmployeeGridResult,
  EmployeeResponsePagedResponse as ApiEmployeeResponsePagedResponse,
  EmployeesClient
} from '../../../core/api/api-client';
import {
  CreateEmployeeRequest,
  EmployeeResponse,
  GetEmployeesRequest,
  PagedResponse,
  UpdateEmployeeRequest,
  EmployeeDocument
} from '../models/employee';

@Injectable({
  providedIn: 'root'
})
export class EmployeeService {
  private readonly apiUrl = `${environment.apiBaseUrl}/api/Employees`;

  constructor(
    private readonly employeesClient: EmployeesClient,
    private readonly http: HttpClient
  ) { }

  getEmployees(
    request: GetEmployeesRequest
  ): Observable<PagedResponse<EmployeeResponse>> {
    return this.employeesClient
      .getAll(
        request.search?.trim() || undefined,
        request.companyID?.trim() || undefined,
        request.isActive ?? undefined,
        request.sortBy?.trim() || undefined,
        request.sortDirection || undefined,
        request.page,
        request.pageSize
      )
      .pipe(
        map(response => this.mapPagedResponse(response))
      );
  }

  createEmployee(
    request: CreateEmployeeRequest
  ): Observable<EmployeeResponse> {
    return this.employeesClient
      .create(request)
      .pipe(
        map(response => this.mapEmployee(response))
      );
  }

  getEmployeeById(
    companyID: string,
    employeeID: string
  ): Observable<EmployeeResponse> {
    return this.employeesClient
      .getById(companyID, employeeID)
      .pipe(
        map(response => this.mapEmployee(response))
      );
  }

  updateEmployee(
    companyID: string,
    employeeID: string,
    request: UpdateEmployeeRequest
  ): Observable<void> {
    return this.employeesClient.update(
      companyID,
      employeeID,
      request
    );
  }

  deleteEmployee(
    companyID: string,
    employeeID: string
  ): Observable<void> {
    return this.employeesClient.delete(companyID, employeeID);
  }

  uploadPhoto(
    companyID: string,
    employeeID: string,
    file: File
  ): Observable<{ photoPath: string }> {
    const formData = new FormData();
    formData.append('file', file);   // ชื่อ field ต้องตรงกับ param 'file' ใน controller

    return this.http.post<{ photoPath: string }>(
      `${this.apiUrl}/${companyID}/${employeeID}/photo`,
      formData
    );
  }

  getPhotoBlob(companyID: string, employeeID: string): Observable<Blob> {
    // ดึงผ่าน HttpClient → interceptor แนบ token ให้ (img src ธรรมดาทำไม่ได้)
    return this.http.get(
      `${this.apiUrl}/${companyID}/${employeeID}/photo`,
      { responseType: 'blob' }
    );
  }

  deletePhoto(companyID: string, employeeID: string): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/${companyID}/${employeeID}/photo`
    );
  }

  listDocuments(companyID: string, employeeID: string): Observable<EmployeeDocument[]> {
    return this.http.get<EmployeeDocument[]>(
      `${this.apiUrl}/${companyID}/${employeeID}/documents`
    );
  }

  uploadDocument(
    companyID: string,
    employeeID: string,
    file: File,
    documentType: string
  ): Observable<EmployeeDocument> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('documentType', documentType);   // field ตรงกับ [FromForm] string documentType

    return this.http.post<EmployeeDocument>(
      `${this.apiUrl}/${companyID}/${employeeID}/documents`,
      formData
    );
  }

  downloadDocument(companyID: string, employeeID: string, documentId: string): Observable<Blob> {
    return this.http.get(
      `${this.apiUrl}/${companyID}/${employeeID}/documents/${documentId}`,
      { responseType: 'blob' }
    );
  }

  deleteDocument(companyID: string, employeeID: string, documentId: string): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/${companyID}/${employeeID}/documents/${documentId}`
    );
  }

  getEmployeesGrid(state: State): Observable<GridDataResult> {
    const queryString = toDataSourceRequestString(state);

    return this.http
      .post<ApiEmployeeGridResult>(
        `${this.apiUrl}/grid?${queryString}`,
        {}
      )
      .pipe(
        map(response => ({
          data: (response.data ?? []).map(item => this.mapEmployee(item)),
          total: response.total ?? 0
        }))
      );
  }

  private mapPagedResponse(
    response: ApiEmployeeResponsePagedResponse
  ): PagedResponse<EmployeeResponse> {
    return {
      items: (response.items ?? []).map(item => this.mapEmployee(item)),
      page: response.page ?? 1,
      pageSize: response.pageSize ?? 10,
      totalItems: response.totalItems ?? 0,
      totalPages: response.totalPages ?? 0,
      hasNextPage: response.hasNextPage ?? false,
      hasPreviousPage: response.hasPreviousPage ?? false
    };
  }

  private mapEmployee(
    response: ApiEmployeeResponse
  ): EmployeeResponse {
    return {
      companyID: response.companyID ?? '',
      employeeID: response.employeeID ?? '',
      fullName: response.fullName ?? '',
      firstName: response.firstName ?? '',
      lastName: response.lastName ?? '',
      email: response.email ?? '',
      hireDate: response.hireDate ?? '',
      salary: response.salary ?? 0,
      isActive: response.isActive ?? false,
      rowVersion: response.rowVersion ?? ''
    };
  }
}