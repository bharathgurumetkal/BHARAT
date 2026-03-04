import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { AuthResponseDto, LoginRequestDto, RegisterRequestDto } from '../../../core/models/auth.models';
import { TokenService } from '../../../core/services/token.service';

@Injectable({
  providedIn: 'root'
})
export class AuthApiService {
  private apiUrl = `${environment.apiUrl}/Auth`;

  constructor(
    private http: HttpClient,
    private tokenService: TokenService
  ) {}

  login(request: LoginRequestDto): Observable<AuthResponseDto> {
    return this.http.post<AuthResponseDto>(`${this.apiUrl}/login`, request).pipe(
      tap(res => {
        if (res.token) {
          this.tokenService.saveToken(res.token);
          // If the API doesn't return role in response body but it's in token, TokenService will pick it up
          if (res.role) localStorage.setItem('role', res.role); 
          if (res.email) localStorage.setItem('email', res.email);
        }
      })
    );
  }

  register(request: RegisterRequestDto): Observable<AuthResponseDto> {
    return this.http.post<AuthResponseDto>(`${this.apiUrl}/register`, request);
  }

  logout(): void {
    this.tokenService.removeToken();
    localStorage.removeItem('role');
    localStorage.removeItem('email');
  }

  isLoggedIn(): boolean {
    return !!this.tokenService.getToken() && !this.tokenService.isTokenExpired();
  }

  getUserRole(): string | null {
    return this.tokenService.getRole() || localStorage.getItem('role');
  }
}
