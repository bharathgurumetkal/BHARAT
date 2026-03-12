import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TokenService } from '../../../../core/services/token.service';

@Component({
  selector: 'app-forbidden',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gradient-to-br from-slate-950 via-slate-900 to-rose-950 px-4">
      
      <!-- Ambient Background Glow -->
      <div class="absolute inset-0 overflow-hidden pointer-events-none">
        <div class="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[600px] h-[600px] bg-rose-600/10 rounded-full blur-[120px]"></div>
      </div>

      <div class="relative z-10 w-full max-w-lg mx-auto">
        <div class="glass-container p-10 sm:p-14 rounded-[2.5rem] border border-white/10 backdrop-blur-2xl bg-white/5 shadow-2xl text-center relative overflow-hidden">
          
          <!-- Outer decorative ring -->
          <div class="absolute -top-10 -right-10 w-40 h-40 bg-rose-500/5 rounded-full blur-3xl"></div>

          <!-- Icon Section -->
          <div class="flex justify-center mb-10">
            <div class="relative">
              <div class="absolute inset-0 bg-rose-500/20 blur-xl rounded-full scale-150 animate-pulse"></div>
              <div class="w-24 h-24 rounded-full bg-gradient-to-tr from-rose-600 to-red-400 p-0.5 shadow-lg relative z-10">
                <div class="w-full h-full rounded-full bg-slate-900 flex items-center justify-center">
                  <svg class="w-12 h-12 text-rose-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5"
                      d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"/>
                  </svg>
                </div>
              </div>
            </div>
          </div>

          <!-- Content -->
          <h1 class="text-4xl font-extrabold text-white tracking-tight mb-4">Restricted Access</h1>
          <p class="text-slate-400 text-lg leading-relaxed mb-8">
            You don't have the necessary clearance to access this module. If you believe this is an error, please contact your administrator.
          </p>

          <!-- User Identity Badge -->
          @if (currentRole) {
            <div class="inline-flex items-center gap-2 px-4 py-2 bg-rose-500/10 border border-rose-500/20 rounded-full text-rose-300 text-sm font-medium mb-10">
              <div class="w-2 h-2 rounded-full bg-rose-500 animate-pulse"></div>
              Signed in as <span class="font-bold text-white uppercase ml-1">{{ currentRole }}</span>
            </div>
          }

          <!-- Action Buttons -->
          <div class="space-y-4">
            <button (click)="goToDashboard()"
              class="w-full px-8 py-4 bg-rose-600 hover:bg-rose-500 text-white font-bold rounded-2xl transition-all duration-300 shadow-lg shadow-rose-900/40 hover:shadow-rose-900/60 hover:-translate-y-1 active:scale-95 flex items-center justify-center gap-3">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6"/>
              </svg>
              Back to Dashboard
            </button>
            <button (click)="goBack()"
              class="w-full px-8 py-4 bg-white/5 hover:bg-white/10 text-slate-300 hover:text-white font-bold rounded-2xl border border-white/10 transition-all duration-300">
              Go Back
            </button>
          </div>

          <p class="mt-8 text-slate-600 text-sm font-medium">Error Code: 403 · Access Forbidden</p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .glass-container {
      background: rgba(15, 23, 42, 0.4);
      box-shadow: 0 8px 32px 0 rgba(0, 0, 0, 0.37);
    }
  `]
})
export class ForbiddenComponent {
  private router = inject(Router);
  private tokenService = inject(TokenService);

  get currentRole(): string | null {
    return this.tokenService.getRole();
  }

  goToDashboard(): void {
    const role = this.tokenService.getRole() ?? '';
    const roleRoutes: Record<string, string> = {
      Admin: '/admin/dashboard',
      Agent: '/agent/dashboard',
      Customer: '/customer/products',
      ClaimsOfficer: '/claims-officer/dashboard',
    };
    this.router.navigate([roleRoutes[role] ?? '/login']);
  }

  goBack(): void {
    window.history.back();
  }
}
