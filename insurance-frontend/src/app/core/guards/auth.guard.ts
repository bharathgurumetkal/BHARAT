import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { TokenService } from '../services/token.service';

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const tokenService = inject(TokenService);

  if (tokenService.getToken() && !tokenService.isTokenExpired()) {
    return true;
  }

  // Not logged in or expired, redirect to login
  router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
  return false;
};
