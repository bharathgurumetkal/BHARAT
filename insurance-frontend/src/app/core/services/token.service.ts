import { Injectable } from '@angular/core';
import { jwtDecode } from 'jwt-decode';

@Injectable({
  providedIn: 'root'
})
export class TokenService {
  private readonly TOKEN_KEY = 'token';

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  saveToken(token: string): void {
    localStorage.setItem(this.TOKEN_KEY, token);
  }

  removeToken(): void {
    localStorage.removeItem(this.TOKEN_KEY);
  }

  decodeToken(token: string): any {
    try {
      return jwtDecode(token);
    } catch {
      return null;
    }
  }

  getRole(): string | null {
    const token = this.getToken();
    if (!token) return null;
    
    const decoded: any = this.decodeToken(token);
    // Adjust key based on backend claim name (common AspNetCore key: http://schemas.microsoft.com/ws/2008/06/identity/claims/role)
    return decoded?.['role'] || decoded?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || null;
  }

  isTokenExpired(): boolean {
    const token = this.getToken();
    if (!token) return true;

    const decoded: any = this.decodeToken(token);
    if (!decoded || !decoded.exp) return true;

    const expirationDate = new Date(0);
    expirationDate.setUTCSeconds(decoded.exp);

    return expirationDate < new Date();
  }
}
