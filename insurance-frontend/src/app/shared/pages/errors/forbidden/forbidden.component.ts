import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { TokenService } from '../../../../core/services/token.service';

@Component({
  selector: 'app-forbidden',
  standalone: true,
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gradient-to-br from-slate-950 via-slate-900 to-rose-950 px-4">

      <!-- Glow -->
      <div class="absolute inset-0 overflow-hidden pointer-events-none">
        <div class="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[600px] h-[600px] bg-rose-600/10 rounded-full blur-3xl"></div>
      </div>

      <div class="relative z-10 text-center max-w-lg mx-auto">

        <!-- Icon -->
        <div class="flex justify-center mb-8">
          <div class="w-28 h-28 rounded-3xl bg-rose-500/10 border border-rose-500/20 flex items-center justify-center shadow-2xl shadow-rose-900/30">
            <svg class="w-14 h-14 text-rose-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5"
                d="M18.364 18.364A9 9 0 005.636 5.636m12.728 12.728A9 9 0 015.636 5.636m12.728 12.728L5.636 5.636"/>
            </svg>
          </div>
        </div>

        <!-- Error code -->
        <p class="text-7xl font-black text-rose-400 tracking-tight mb-2">403</p>

        <!-- Heading -->
        <h1 class="text-3xl font-bold text-white mb-4">Access Denied</h1>

        <!-- Description -->
        <p class="text-slate-400 text-base leading-relaxed mb-10">
          You do not have the required permissions to view this page.
          Please contact your administrator if you believe this is an error.
        </p>

        <!-- Role badge -->
        @if (currentRole) {
          <div class="inline-flex items-center gap-2 px-4 py-2 bg-rose-500/10 border border-rose-500/20 rounded-full text-rose-300 text-sm font-medium mb-8">
            <svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 20 20">
              <path fill-rule="evenodd" d="M10 9a3 3 0 100-6 3 3 0 000 6zm-7 9a7 7 0 1114 0H3z" clip-rule="evenodd"/>
            </svg>
            Signed in as <strong>{{ currentRole }}</strong>
          </div>
        }

        <!-- Actions -->
        <div class="flex flex-col sm:flex-row gap-3 justify-center">
          <button (click)="goToDashboard()"
            class="px-6 py-3 bg-rose-600 hover:bg-rose-500 text-white font-semibold rounded-xl transition-all duration-200 shadow-lg shadow-rose-900/40 hover:shadow-rose-900/60 hover:-translate-y-0.5">
            Go to My Dashboard
          </button>
          <button (click)="goBack()"
            class="px-6 py-3 bg-white/5 hover:bg-white/10 text-slate-300 hover:text-white font-semibold rounded-xl border border-white/10 transition-all duration-200">
            Go Back
          </button>
        </div>

      </div>
    </div>
  `
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
