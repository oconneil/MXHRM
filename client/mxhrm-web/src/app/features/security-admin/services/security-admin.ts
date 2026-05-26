import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
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
import {
  PermissionResponse as ApiPermissionResponse,
  PermissionsClient,
  RolePermissionResponse as ApiRolePermissionResponse,
  RoleResponse as ApiRoleResponse,
  RolesClient
} from '../../../core/api/api-client';

@Injectable({
  providedIn: 'root'
})
export class SecurityAdminService {
  private readonly apiBaseUrl = `${environment.apiBaseUrl}/api`;

  constructor(
    private readonly rolesClient: RolesClient,
    private readonly permissionsClient: PermissionsClient,
    private readonly http: HttpClient
  ) { }

  getRoles(): Observable<RoleResponse[]> {
    return this.rolesClient
      .getAll()
      .pipe(
        map(roles => roles.map(role => this.mapRole(role)))
      );
  }

  getRoleById(id: string): Observable<RoleResponse> {
    return this.rolesClient
      .getById(id)
      .pipe(
        map(role => this.mapRole(role))
      );
  }

  createRole(request: CreateRoleRequest): Observable<RoleResponse> {
    return this.rolesClient
      .create(request)
      .pipe(
        map(role => this.mapRole(role))
      );
  }

  updateRole(id: string, request: UpdateRoleRequest): Observable<void> {
    return this.rolesClient.update(id, request);
  }

  deleteRole(id: string): Observable<void> {
    return this.rolesClient.delete(id);
  }

  getPermissions(): Observable<PermissionResponse[]> {
    return this.permissionsClient
      .getAll()
      .pipe(
        map(permissions => permissions.map(permission => this.mapPermission(permission)))
      );
  }

  getRolePermissions(roleId: string): Observable<RolePermissionResponse> {
    return this.rolesClient
      .getPermissions(roleId)
      .pipe(
        map(response => this.mapRolePermission(response))
      );
  }

  updateRolePermissions(
    roleId: string,
    request: UpdateRolePermissionsRequest
  ): Observable<void> {
    return this.rolesClient.updatePermissions(roleId, request);
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

  private mapRole(response: ApiRoleResponse): RoleResponse {
    return {
      id: response.id ?? '',
      name: response.name ?? ''
    };
  }

  private mapPermission(response: ApiPermissionResponse): PermissionResponse {
    return {
      id: response.id ?? 0,
      code: response.code ?? '',
      name: response.name ?? ''
    };
  }

  private mapRolePermission(
    response: ApiRolePermissionResponse
  ): RolePermissionResponse {
    return {
      roleId: response.roleId ?? '',
      roleName: response.roleName ?? '',
      permissions: (response.permissions ?? []).map(permission =>
        this.mapPermission(permission)
      )
    };
  }
}