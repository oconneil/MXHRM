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
  AuditLogResponse as ApiAuditLogResponse,
  AuditLogResponseGridDataSourceResult as ApiAuditLogGridResult,
  AuditLogResponsePagedResponse as ApiAuditLogPagedResponse,
  AuditLogsClient,
  PermissionResponse as ApiPermissionResponse,
  PermissionsClient,
  RolePermissionResponse as ApiRolePermissionResponse,
  RoleResponse as ApiRoleResponse,
  RolesClient,
  UserActivityLogResponse as ApiUserActivityLogResponse,
  UserActivityLogResponseGridDataSourceResult as ApiUserActivityLogGridResult,
  UserActivityLogResponsePagedResponse as ApiUserActivityLogPagedResponse,
  UserActivityLogsClient,
  UserResponse as ApiUserResponse,
  UserRoleResponse as ApiUserRoleResponse,
  UsersClient
} from '../../../core/api/api-client';

@Injectable({
  providedIn: 'root'
})
export class SecurityAdminService {
  private readonly apiBaseUrl = `${environment.apiBaseUrl}/api`;

  constructor(
    private readonly rolesClient: RolesClient,
    private readonly permissionsClient: PermissionsClient,
    private readonly usersClient: UsersClient,
    private readonly auditLogsClient: AuditLogsClient,
    private readonly userActivityLogsClient: UserActivityLogsClient,
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
    return this.usersClient
      .getAll()
      .pipe(
        map(users => users.map(user => this.mapUser(user)))
      );
  }

  getUserById(id: string): Observable<UserResponse> {
    return this.usersClient
      .getById(id)
      .pipe(
        map(user => this.mapUser(user))
      );
  }

  activateUser(id: string): Observable<void> {
    return this.usersClient.activate(id);
  }

  deactivateUser(id: string): Observable<void> {
    return this.usersClient.deactivate(id);
  }

  getUserRoles(userId: string): Observable<UserRoleResponse> {
    return this.usersClient
      .getRoles(userId)
      .pipe(
        map(response => this.mapUserRole(response))
      );
  }

  updateUserRoles(
    userId: string,
    request: UpdateUserRolesRequest
  ): Observable<void> {
    return this.usersClient.updateRoles(userId, request);
  }

  getAuditLogs(): Observable<PagedResponse<AuditLogResponse>> {
    return this.auditLogsClient
      .getAll()
      .pipe(
        map(response => this.mapAuditLogPagedResponse(response))
      );
  }

  getUserActivityLogs(): Observable<PagedResponse<UserActivityLogResponse>> {
    return this.userActivityLogsClient
      .getAll()
      .pipe(
        map(response => this.mapUserActivityLogPagedResponse(response))
      );
  }

  getAuditLogsGrid(state: State): Observable<GridDataResult> {
    const queryString = toDataSourceRequestString(state);

    return this.http
      .post<ApiAuditLogGridResult>(
        `${this.apiBaseUrl}/audit-logs/grid?${queryString}`,
        {}
      )
      .pipe(
        map(response => ({
          data: (response.data ?? []).map(item => this.mapAuditLog(item)),
          total: response.total ?? 0
        }))
      );
  }

  getUserActivityLogsGrid(state: State): Observable<GridDataResult> {
    const queryString = toDataSourceRequestString(state);

    return this.http
      .post<ApiUserActivityLogGridResult>(
        `${this.apiBaseUrl}/user-activity-logs/grid?${queryString}`,
        {}
      )
      .pipe(
        map(response => ({
          data: (response.data ?? []).map(item => this.mapUserActivityLog(item)),
          total: response.total ?? 0
        }))
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

  private mapUser(response: ApiUserResponse): UserResponse {
    return {
      id: response.id ?? '',
      userName: response.userName ?? '',
      companyID: response.companyID ?? '',
      displayName: response.displayName ?? '',
      isActive: response.isActive ?? false
    };
  }

  private mapUserRole(response: ApiUserRoleResponse): UserRoleResponse {
    return {
      userId: response.userId ?? '',
      userName: response.userName ?? '',
      roles: (response.roles ?? []).map(role => this.mapRole(role))
    };
  }

  private mapAuditLog(response: ApiAuditLogResponse): AuditLogResponse {
    return {
      id: response.id ?? 0,
      tableName: response.tableName ?? '',
      action: response.action ?? '',
      keyValues: response.keyValues ?? null,
      oldValues: response.oldValues ?? null,
      newValues: response.newValues ?? null,
      changedColumns: response.changedColumns ?? null,
      userId: response.userId ?? null,
      userName: response.userName ?? null,
      traceId: response.traceId ?? null,
      createdAtUtc: response.createdAtUtc ?? ''
    };
  }

  private mapAuditLogPagedResponse(
    response: ApiAuditLogPagedResponse
  ): PagedResponse<AuditLogResponse> {
    return {
      items: (response.items ?? []).map(item => this.mapAuditLog(item)),
      page: response.page ?? 1,
      pageSize: response.pageSize ?? 20,
      totalItems: response.totalItems ?? 0,
      totalPages: response.totalPages ?? 0,
      hasNextPage: response.hasNextPage ?? false,
      hasPreviousPage: response.hasPreviousPage ?? false
    };
  }

  private mapUserActivityLog(
    response: ApiUserActivityLogResponse
  ): UserActivityLogResponse {
    return {
      id: response.id ?? 0,
      activityType: response.activityType ?? '',
      description: response.description ?? null,
      metadata: response.metadata ?? null,
      userId: response.userId ?? null,
      userName: response.userName ?? null,
      ipAddress: response.ipAddress ?? null,
      userAgent: response.userAgent ?? null,
      traceId: response.traceId ?? null,
      createdAtUtc: response.createdAtUtc ?? ''
    };
  }

  private mapUserActivityLogPagedResponse(
    response: ApiUserActivityLogPagedResponse
  ): PagedResponse<UserActivityLogResponse> {
    return {
      items: (response.items ?? []).map(item => this.mapUserActivityLog(item)),
      page: response.page ?? 1,
      pageSize: response.pageSize ?? 20,
      totalItems: response.totalItems ?? 0,
      totalPages: response.totalPages ?? 0,
      hasNextPage: response.hasNextPage ?? false,
      hasPreviousPage: response.hasPreviousPage ?? false
    };
  }
}