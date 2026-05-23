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
import { permissionGuard } from './core/guards/permission-guard';
import { Permissions } from './core/models/permissions';
import { AccessDenied } from './features/auth/pages/access-denied/access-denied';
import { NotFound } from './features/auth/pages/not-found/not-found';
import { EmployeeSummaryReport } from './features/reports/pages/employee-summary-report/employee-summary-report';

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
        path: 'access-denied',
        component: AccessDenied
      },
      {
        path: 'not-found',
        component: NotFound
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
        component: RolesManagement,
        canActivate: [permissionGuard],
        data: {
          permission: Permissions.Role.Manage
        }
      },
      {
        path: 'security-admin/roles/:roleId/permissions',
        component: RolePermissionsManagement,
        canActivate: [permissionGuard],
        data: {
          permission: Permissions.Role.Manage
        }
      },
      {
        path: 'security-admin/users',
        component: UsersManagement,
        canActivate: [permissionGuard],
        data: {
          permission: Permissions.Role.Manage
        }
      },
      {
        path: 'security-admin/users/:userId/roles',
        component: UserRolesManagement,
        canActivate: [permissionGuard],
        data: {
          permission: Permissions.Role.Manage
        }
      },
      {
        path: 'security-admin/audit-logs',
        component: AuditLogs,
        canActivate: [permissionGuard],
        data: {
          permission: Permissions.Audit.Read
        }
      },
      {
        path: 'security-admin/user-activity-logs',
        component: UserActivityLogs,
        canActivate: [permissionGuard],
        data: {
          permission: Permissions.Activity.Read
        }
      },
      {
        path: 'reports/employee-summary',
        component: EmployeeSummaryReport,
        canActivate: [permissionGuard],
        data: {
          permission: Permissions.Employee.Read
        }
      },
      {
        path: 'reports/audit',
        loadComponent: () =>
          import('./features/reports/pages/audit-report/audit-report')
            .then(m => m.AuditReport),
        canActivate: [permissionGuard],
        data: {
          permission: Permissions.Audit.Read
        }
      }
    ]
  },
  {
    path: '**',
    redirectTo: 'not-found'
  }
];
