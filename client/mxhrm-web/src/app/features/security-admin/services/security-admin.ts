import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  AuditLogResponse,
  CreateRoleRequest,
  PagedResponse,
  PermissionResponse,
  RolePermissionResponse,
  RoleResponse,
  UpdateRolePermissionsRequest,
  UpdateRoleRequest,
  UpdateUserRolesRequest,
  UserActivityLogResponse,
  UserResponse,
  UserRoleResponse
} from '../models/security-admin';
import { GridDataResult } from '@progress/kendo-angular-grid';
import { State, toDataSourceRequestString } from '@progress/kendo-data-query';

@Injectable({
  providedIn: 'root'
})
export class SecurityAdminService {
  private readonly apiBaseUrl = `${environment.apiBaseUrl}/api`;

  constructor(private readonly http: HttpClient) { }

  getRoles(): Observable<RoleResponse[]> {
    return this.http.get<RoleResponse[]>(`${this.apiBaseUrl}/roles`);
  }

  getRoleById(id: string): Observable<RoleResponse> {
    return this.http.get<RoleResponse>(`${this.apiBaseUrl}/roles/${id}`);
  }

  createRole(request: CreateRoleRequest): Observable<RoleResponse> {
    return this.http.post<RoleResponse>(`${this.apiBaseUrl}/roles`, request);
  }

  updateRole(id: string, request: UpdateRoleRequest): Observable<void> {
    return this.http.put<void>(`${this.apiBaseUrl}/roles/${id}`, request);
  }

  deleteRole(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiBaseUrl}/roles/${id}`);
  }

  getPermissions(): Observable<PermissionResponse[]> {
    return this.http.get<PermissionResponse[]>(`${this.apiBaseUrl}/permissions`);
  }

  getRolePermissions(roleId: string): Observable<RolePermissionResponse> {
    return this.http.get<RolePermissionResponse>(
      `${this.apiBaseUrl}/roles/${roleId}/permissions`
    );
  }

  updateRolePermissions(
    roleId: string,
    request: UpdateRolePermissionsRequest
  ): Observable<void> {
    return this.http.put<void>(
      `${this.apiBaseUrl}/roles/${roleId}/permissions`,
      request
    );
  }

  getUsers(): Observable<UserResponse[]> {
    return this.http.get<UserResponse[]>(`${this.apiBaseUrl}/users`);
  }

  getUserById(id: string): Observable<UserResponse> {
    return this.http.get<UserResponse>(`${this.apiBaseUrl}/users/${id}`);
  }

  activateUser(id: string): Observable<void> {
    return this.http.put<void>(`${this.apiBaseUrl}/users/${id}/activate`, {});
  }

  deactivateUser(id: string): Observable<void> {
    return this.http.put<void>(`${this.apiBaseUrl}/users/${id}/deactivate`, {});
  }

  getUserRoles(userId: string): Observable<UserRoleResponse> {
    return this.http.get<UserRoleResponse>(
      `${this.apiBaseUrl}/users/${userId}/roles`
    );
  }

  updateUserRoles(
    userId: string,
    request: UpdateUserRolesRequest
  ): Observable<void> {
    return this.http.put<void>(
      `${this.apiBaseUrl}/users/${userId}/roles`,
      request
    );
  }

  getAuditLogs(): Observable<PagedResponse<AuditLogResponse>> {
    return this.http.get<PagedResponse<AuditLogResponse>>(`${this.apiBaseUrl}/audit-logs`);
  }

  getUserActivityLogs(): Observable<PagedResponse<UserActivityLogResponse>> {
    return this.http.get<PagedResponse<UserActivityLogResponse>>(
      `${this.apiBaseUrl}/user-activity-logs`
    );
  }
  
  getAuditLogsGrid(state: State): Observable<GridDataResult> {
    const queryString = toDataSourceRequestString(state);

    return this.http.post<GridDataResult>(
      `${this.apiBaseUrl}/audit-logs/grid?${queryString}`,
      {}
    );
  }

  getUserActivityLogsGrid(state: State): Observable<GridDataResult> {
    const queryString = toDataSourceRequestString(state);

    return this.http.post<GridDataResult>(
      `${this.apiBaseUrl}/user-activity-logs/grid?${queryString}`,
      {}
    );
  }
}