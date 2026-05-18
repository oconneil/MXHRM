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