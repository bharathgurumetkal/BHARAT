import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

// Routes that belong to error pages — interceptor must skip them to avoid loops
const ERROR_ROUTES = [
  '/not-found',
  '/server-error',
  '/unauthorized',
  '/forbidden',
  '/network-error',
];

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);

  // Do not intercept requests made from error pages themselves
  if (ERROR_ROUTES.some(r => router.url.startsWith(r))) {
    return next(req);
  }

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      console.error(`[ErrorInterceptor] HTTP ${error.status} on ${req.url}`);

      switch (error.status) {
        case 401:
          router.navigate(['/unauthorized']);
          break;
        case 403:
          router.navigate(['/forbidden']);
          break;
        case 404:
          router.navigate(['/not-found']);
          break;
        case 500:
        case 502:
        case 503:
          router.navigate(['/server-error']);
          break;
        case 0:
          // Status 0 = network failure (CORS, offline, no connection)
          router.navigate(['/network-error']);
          break;
        default:
          // Other unexpected server errors
          router.navigate(['/server-error']);
          break;
      }

      // Re-throw so GlobalErrorHandler does not also trigger a second redirect
      return throwError(() => error);
    })
  );
};
