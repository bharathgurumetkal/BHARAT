import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-unauthorized',
  standalone: true,
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gradient-to-br from-slate-950 via-slate-900 to-amber-950 px-4">

      <!-- Glow -->
      <div class="absolute inset-0 overflow-hidden pointer-events-none">
        <div class="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[600px] h-[600px] bg-amber-600/10 rounded-full blur-3xl"></div>
      </div>

      <div class="relative z-10 text-center max-w-lg mx-auto">

        <!-- Icon -->
        <div class="flex justify-center mb-8">
          <div class="w-28 h-28 rounded-3xl bg-amber-500/10 border border-amber-500/20 flex items-center justify-center shadow-2xl shadow-amber-900/30">
            <svg class="w-14 h-14 text-amber-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5"
                d="M16.5 10.5V6.75a4.5 4.5 0 10-9 0v3.75m-.75 11.25h10.5a2.25 2.25 0 002.25-2.25v-6.75a2.25 2.25 0 00-2.25-2.25H6.75a2.25 2.25 0 00-2.25 2.25v6.75a2.25 2.25 0 002.25 2.25z"/>
            </svg>
          </div>
        </div>

        <!-- Error code -->
        <p class="text-7xl font-black text-amber-400 tracking-tight mb-2">401</p>

        <!-- Heading -->
        <h1 class="text-3xl font-bold text-white mb-4">Session Expired</h1>

        <!-- Description -->
        <p class="text-slate-400 text-base leading-relaxed mb-10">
          Your session has expired or you are not signed in.
          Please log in again to continue accessing the system.
        </p>

        <!-- Actions -->
        <div class="flex flex-col sm:flex-row gap-3 justify-center">
          <button (click)="goToLogin()"
            class="px-6 py-3 bg-amber-600 hover:bg-amber-500 text-white font-semibold rounded-xl transition-all duration-200 shadow-lg shadow-amber-900/40 hover:shadow-amber-900/60 hover:-translate-y-0.5">
            <span class="flex items-center gap-2">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 16l-4-4m0 0l4-4m-4 4h14m-5 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h7a3 3 0 013 3v1"/>
              </svg>
              Sign In
            </span>
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
export class UnauthorizedComponent {
  constructor(private router: Router) {}

  goToLogin(): void {
    this.router.navigate(['/login']);
  }

  goBack(): void {
    window.history.back();
  }
}
