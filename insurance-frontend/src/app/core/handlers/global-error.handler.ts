import { ErrorHandler, Injectable, inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';

// Routes that are themselves error pages — never redirect from these
const ERROR_ROUTES = [
  '/not-found',
  '/server-error',
  '/unauthorized',
  '/forbidden',
  '/network-error',
];

@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
  private router = inject(Router);

  handleError(error: unknown): void {
    // Prevent redirect loops: if already on an error page, do nothing
    const currentUrl = this.router.url;
    if (ERROR_ROUTES.some(r => currentUrl.startsWith(r))) {
      console.error('[GlobalErrorHandler] Error on error page (suppressed redirect):', error);
      return;
    }

    if (error instanceof HttpErrorResponse) {
      this.handleHttpError(error);
    } else if (error instanceof Error) {
      // Unwrap Angular's zone.js wrapper if present
      const innerError = (error as any).rejection ?? error;
      if (innerError instanceof HttpErrorResponse) {
        this.handleHttpError(innerError);
      } else {
        console.error('[GlobalErrorHandler] Runtime error:', innerError);
        this.router.navigate(['/server-error']);
      }
    } else {
      console.error('[GlobalErrorHandler] Unknown error:', error);
      this.router.navigate(['/server-error']);
    }
  }

  private handleHttpError(error: HttpErrorResponse): void {
    console.error(`[GlobalErrorHandler] HTTP ${error.status}:`, error.message);
    switch (error.status) {
      case 401: this.router.navigate(['/unauthorized']); break;
      case 403: this.router.navigate(['/forbidden']); break;
      case 404: this.router.navigate(['/not-found']); break;
      case 500:
      case 502:
      case 503: this.router.navigate(['/server-error']); break;
      case 0:   this.router.navigate(['/network-error']); break;
      default:  this.router.navigate(['/server-error']); break;
    }
  }
}
