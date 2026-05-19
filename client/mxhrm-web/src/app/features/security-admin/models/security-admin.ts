export interface PermissionResponse {
  id: number;
  code: string;
  name: string;
}

export interface RoleResponse {
  id: string;
  name: string;
}

export interface CreateRoleRequest {
  name: string;
}

export interface UpdateRoleRequest {
  name: string;
}

export interface RolePermissionResponse {
  roleId: string;
  roleName: string;
  permissions: PermissionResponse[];
}

export interface UpdateRolePermissionsRequest {
  permissionIds: number[];
}

export interface UserResponse {
  id: string;
  userName: string;
  companyID: string;
  displayName: string;
  isActive: boolean;
}

export interface UserRoleResponse {
  userId: string;
  userName: string;
  roles: RoleResponse[];
}

export interface UpdateUserRolesRequest {
  roleIds: string[];
}

export interface AuditLogResponse {
  id: number;
  tableName: string;
  action: string;
  keyValues: string | null;
  oldValues: string | null;
  newValues: string | null;
  changedColumns: string | null;
  userId: string | null;
  userName: string | null;
  traceId: string | null;
  createdAtUtc: string;
}

export interface UserActivityLogResponse {
  id: number;
  activityType: string;
  description: string | null;
  metadata: string | null;
  userId: string | null;
  userName: string | null;
  ipAddress: string | null;
  userAgent: string | null;
  traceId: string | null;
  createdAtUtc: string;
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