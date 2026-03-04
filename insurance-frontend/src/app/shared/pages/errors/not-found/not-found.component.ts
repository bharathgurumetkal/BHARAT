import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-not-found',
  standalone: true,
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gradient-to-br from-slate-950 via-slate-900 to-indigo-950 px-4">
      
      <!-- Radial glow -->
      <div class="absolute inset-0 overflow-hidden pointer-events-none">
        <div class="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[600px] h-[600px] bg-indigo-600/10 rounded-full blur-3xl"></div>
      </div>

      <div class="relative z-10 text-center max-w-lg mx-auto">

        <!-- Icon -->
        <div class="flex justify-center mb-8">
          <div class="w-28 h-28 rounded-3xl bg-indigo-500/10 border border-indigo-500/20 flex items-center justify-center shadow-2xl shadow-indigo-900/30">
            <svg class="w-14 h-14 text-indigo-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5"
                d="M9.172 16.172a4 4 0 015.656 0M9 10h.01M15 10h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/>
            </svg>
          </div>
        </div>

        <!-- Error code -->
        <p class="text-7xl font-black text-indigo-400 tracking-tight mb-2">404</p>

        <!-- Heading -->
        <h1 class="text-3xl font-bold text-white mb-4">Page Not Found</h1>

        <!-- Description -->
        <p class="text-slate-400 text-base leading-relaxed mb-10">
          The page you are looking for does not exist or may have been moved.
          Please check the address or navigate back to your dashboard.
        </p>

        <!-- Actions -->
        <div class="flex flex-col sm:flex-row gap-3 justify-center">
          <button (click)="goToDashboard()"
            class="px-6 py-3 bg-indigo-600 hover:bg-indigo-500 text-white font-semibold rounded-xl transition-all duration-200 shadow-lg shadow-indigo-900/40 hover:shadow-indigo-900/60 hover:-translate-y-0.5">
            Go to Dashboard
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
export class NotFoundComponent {
  constructor(private router: Router) {}

  goToDashboard(): void {
    this.router.navigate(['/login']);
  }

  goBack(): void {
    window.history.back();
  }
}
