import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-server-error',
  standalone: true,
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gradient-to-br from-slate-950 via-slate-900 to-red-950 px-4">

      <!-- Glow -->
      <div class="absolute inset-0 overflow-hidden pointer-events-none">
        <div class="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[600px] h-[600px] bg-red-600/10 rounded-full blur-3xl"></div>
      </div>

      <div class="relative z-10 text-center max-w-lg mx-auto">

        <!-- Icon -->
        <div class="flex justify-center mb-8">
          <div class="w-28 h-28 rounded-3xl bg-red-500/10 border border-red-500/20 flex items-center justify-center shadow-2xl shadow-red-900/30">
            <svg class="w-14 h-14 text-red-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5"
                d="M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126zM12 15.75h.007v.008H12v-.008z"/>
            </svg>
          </div>
        </div>

        <!-- Error code -->
        <p class="text-7xl font-black text-red-400 tracking-tight mb-2">500</p>

        <!-- Heading -->
        <h1 class="text-3xl font-bold text-white mb-4">Internal Server Error</h1>

        <!-- Description -->
        <p class="text-slate-400 text-base leading-relaxed mb-10">
          Something went wrong on our end. Our team has been notified and
          is working to resolve the issue. Please try again in a moment.
        </p>

        <!-- Actions -->
        <div class="flex flex-col sm:flex-row gap-3 justify-center">
          <button (click)="retry()"
            class="px-6 py-3 bg-red-600 hover:bg-red-500 text-white font-semibold rounded-xl transition-all duration-200 shadow-lg shadow-red-900/40 hover:shadow-red-900/60 hover:-translate-y-0.5">
            <span class="flex items-center gap-2">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"/>
              </svg>
              Try Again
            </span>
          </button>
          <button (click)="goToDashboard()"
            class="px-6 py-3 bg-white/5 hover:bg-white/10 text-slate-300 hover:text-white font-semibold rounded-xl border border-white/10 transition-all duration-200">
            Go to Dashboard
          </button>
        </div>

        <!-- Subtle hint -->
        <p class="mt-8 text-slate-600 text-sm">Error code: 500 · Please contact support if this persists.</p>

      </div>
    </div>
  `
})
export class ServerErrorComponent {
  constructor(private router: Router) {}

  retry(): void {
    window.history.back();
  }

  goToDashboard(): void {
    this.router.navigate(['/login']);
  }
}
