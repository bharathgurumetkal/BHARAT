import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { TokenService } from '../services/token.service';

export const roleGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const tokenService = inject(TokenService);
  const userRole = tokenService.getRole();
  const requiredRole = route.data?.['role'];

  if (!userRole) {
    router.navigate(['/login']);
    return false;
  }

  if (requiredRole && userRole !== requiredRole) {
    // Role mismatch, redirect to dashboard or appropriate page
    console.warn(`Access denied. Role ${userRole} does not match required ${requiredRole}`);
    router.navigate(['/login']);
    return false;
  }

  return true;
};
