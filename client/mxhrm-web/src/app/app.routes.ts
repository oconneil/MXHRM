import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth-guard';
import { Login } from './features/auth/pages/login/login';
import { EmployeeList } from './features/employees/pages/employee-list/employee-list';
import { EmployeeCreate } from './features/employees/pages/employee-create/employee-create';
import { EmployeeEdit } from './features/employees/pages/employee-edit/employee-edit';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'employees',
    pathMatch: 'full'
  },
  {
    path: 'login',
    component: Login
  },
  {
    path: 'employees',
    component: EmployeeList,
    canActivate: [authGuard]
  },
  {
    path: 'employees/create',
    component: EmployeeCreate,
    canActivate: [authGuard]
  },
  {
    path: 'employees/edit/:companyID/:employeeID',
    component: EmployeeEdit,
    canActivate: [authGuard]
  }
];