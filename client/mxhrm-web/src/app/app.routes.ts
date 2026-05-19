import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth-guard';
import { MainLayout } from './core/layouts/main-layout/main-layout';
import { Login } from './features/auth/pages/login/login';
import { EmployeeList } from './features/employees/pages/employee-list/employee-list';
import { EmployeeCreate } from './features/employees/pages/employee-create/employee-create';
import { EmployeeEdit } from './features/employees/pages/employee-edit/employee-edit';
import { RolesManagement } from './features/security-admin/pages/roles-management/roles-management';
import { RolePermissionsManagement } from './features/security-admin/pages/role-permissions-management/role-permissions-management';
import { UsersManagement } from './features/security-admin/pages/users-management/users-management';
import { UserRolesManagement } from './features/security-admin/pages/user-roles-management/user-roles-management';
import { AuditLogs } from './features/security-admin/pages/audit-logs/audit-logs';
import { UserActivityLogs } from './features/security-admin/pages/user-activity-logs/user-activity-logs';

export const routes: Routes = [
  {
    path: 'login',
    component: Login
  },
  {
    path: '',
    component: MainLayout,
    canActivate: [authGuard],
    children: [
      {
        path: '',
        redirectTo: 'employees',
        pathMatch: 'full'
      },
      {
        path: 'employees',
        component: EmployeeList
      },
      {
        path: 'employees/create',
        component: EmployeeCreate
      },
      {
        path: 'employees/edit/:companyID/:employeeID',
        component: EmployeeEdit
      },
      {
        path: 'security-admin/roles',
        component: RolesManagement
      },
      {
        path: 'security-admin/roles/:roleId/permissions',
        component: RolePermissionsManagement
      },
      {
        path: 'security-admin/users',
        component: UsersManagement
      },
      {
        path: 'security-admin/users/:userId/roles',
        component: UserRolesManagement
      },
      {
        path: 'security-admin/audit-logs',
        component: AuditLogs
      },
      {
        path: 'security-admin/user-activity-logs',
        component: UserActivityLogs
      }
    ]
  }
];
