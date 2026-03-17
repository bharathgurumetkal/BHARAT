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
      console.warn('[GlobalErrorHandler] Error suppressed on error page:', error);
      return;
    }

    // Unwrap Angular's zone.js wrapper if present
    const innerError = (error as any).rejection ?? error;

    if (innerError instanceof HttpErrorResponse) {
      // HTTP errors are primarily handled by the ErrorInterceptor.
      // We only log them here to ensure we have a record of unhandled HTTP failures.
      console.error(`[GlobalErrorHandler] Unhandled HTTP ${innerError.status}:`, innerError.message);
      
      // We generally don't redirect here to avoid fighting with the interceptor or component-level handling.
      // However, if it's a critical error (500) and we haven't redirected yet, we could fallback.
      // For now, let's just log and let the Interceptor/Component take precedence.
      return;
    }

    // This is a runtime JavaScript error (ReferenceError, TypeError, etc.)
    console.error('[GlobalErrorHandler] Runtime exception:', innerError);
    
    // Redirect to server-error page with runtime error info
    const errorData = {
      status: 500,
      message: innerError instanceof Error ? innerError.message : 'A critical runtime error occurred in the application.',
      url: window.location.href,
      timestamp: new Date().toISOString(),
      id: 'CRIT-' + Math.random().toString(36).substring(2, 10).toUpperCase()
    };

    this.router.navigate(['/server-error'], { state: { error: errorData } });
  }
}
