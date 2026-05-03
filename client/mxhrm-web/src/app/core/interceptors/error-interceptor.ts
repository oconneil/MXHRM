import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const authService = inject(AuthService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const isUnauthorized = error.status === 401;
      const isAuthEndpoint =
        req.url.includes('/api/auth/login') ||
        req.url.includes('/api/auth/register') ||
        req.url.includes('/api/auth/refresh-token');

      if (isUnauthorized && !isAuthEndpoint && authService.getRefreshToken()) {
        return authService.refreshToken().pipe(   // Attempt to refresh the token
          switchMap(() => {
            const token = authService.getToken();

            const retryReq = req.clone({
              setHeaders: {
                Authorization: `Bearer ${token}`
              }
            });

            return next(retryReq);
          }),
          catchError(refreshError => {
            authService.logout();
            router.navigate(['/login']);
            return throwError(() => refreshError);
          })
        );
      }

      if (isUnauthorized) {
        authService.logout();
        router.navigate(['/login']);
      }

      return throwError(() => error);
    })
  );

  // return next(req).pipe(
  //   catchError((error: HttpErrorResponse) => {

  //     if (error.status === 401) {
  //       // token หมดอายุ / ไม่ถูกต้อง
  //       authService.logout();
  //       router.navigate(['/login']);
  //     }

  //     if (error.status === 403) {
  //       console.warn('Forbidden: ไม่มีสิทธิ์เข้าถึง resource นี้');
  //     }

  //     if (error.status >= 500) {
  //       console.error('Server error:', error);
  //     }

  //     return throwError(() => error);
  //   })
  // );
};