import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth';

export const permissionGuard: CanActivateFn = route => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const permission = route.data['permission'] as string | undefined;
  const permissions = route.data['permissions'] as string[] | undefined;

  if (!permission && (!permissions || permissions.length === 0)) {
    return true;
  }

  if (permission && authService.hasPermission(permission)) {
    return true;
  }

  if (permissions && authService.hasAnyPermission(permissions)) {
    return true;
  }

  return router.createUrlTree(['/employees']);
};