import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);  // Use inject to get an instance of AuthService
  const token = authService.getToken();

  if (!token) {
    return next(req);   // If no token is available, pass the request through without modification
  }

  // Clone the request and set the Authorization header with the Bearer token
  const authReq = req.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`
    }
  });

  return next(authReq);
};