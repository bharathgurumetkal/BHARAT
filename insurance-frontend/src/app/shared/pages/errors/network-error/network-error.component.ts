import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-network-error',
  standalone: true,
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gradient-to-br from-slate-950 via-slate-900 to-cyan-950 px-4">

      <!-- Glow -->
      <div class="absolute inset-0 overflow-hidden pointer-events-none">
        <div class="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[600px] h-[600px] bg-cyan-600/10 rounded-full blur-3xl"></div>
      </div>

      <div class="relative z-10 text-center max-w-lg mx-auto">

        <!-- Animated Icon -->
        <div class="flex justify-center mb-8">
          <div class="w-28 h-28 rounded-3xl bg-cyan-500/10 border border-cyan-500/20 flex items-center justify-center shadow-2xl shadow-cyan-900/30 relative">
            <svg class="w-14 h-14 text-cyan-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5"
                d="M8.288 15.038a5.25 5.25 0 017.424 0M5.106 11.856c3.807-3.808 9.98-3.808 13.788 0M1.924 8.674c5.565-5.565 14.587-5.565 20.152 0M12.53 18.22l-.53.53-.53-.53a.75.75 0 011.06 0z"/>
            </svg>
            <!-- Pulse ring -->
            <span class="absolute inset-0 rounded-3xl ring-2 ring-cyan-500/20 animate-ping"></span>
          </div>
        </div>

        <!-- Label -->
        <p class="text-lg font-semibold text-cyan-400 mb-2 uppercase tracking-widest">Connection Lost</p>

        <!-- Heading -->
        <h1 class="text-3xl font-bold text-white mb-4">Network Error</h1>

        <!-- Description -->
        <p class="text-slate-400 text-base leading-relaxed mb-10">
          Unable to reach the server. Please check your internet connection
          and try again. If the problem persists, the service may be temporarily unavailable.
        </p>

        <!-- Connection status indicator -->
        <div class="flex items-center justify-center gap-2 mb-8">
          <span class="w-2 h-2 rounded-full" [class]="isOnline ? 'bg-emerald-400 animate-pulse' : 'bg-red-400'"></span>
          <span class="text-sm" [class]="isOnline ? 'text-emerald-400' : 'text-red-400'">
            {{ isOnline ? 'Connection restored — you can retry now' : 'No internet connection detected' }}
          </span>
        </div>

        <!-- Actions -->
        <div class="flex flex-col sm:flex-row gap-3 justify-center">
          <button (click)="retry()"
            class="px-6 py-3 bg-cyan-600 hover:bg-cyan-500 text-white font-semibold rounded-xl transition-all duration-200 shadow-lg shadow-cyan-900/40 hover:shadow-cyan-900/60 hover:-translate-y-0.5">
            <span class="flex items-center gap-2">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"/>
              </svg>
              Retry Connection
            </span>
          </button>
          <button (click)="goToDashboard()"
            class="px-6 py-3 bg-white/5 hover:bg-white/10 text-slate-300 hover:text-white font-semibold rounded-xl border border-white/10 transition-all duration-200">
            Go to Dashboard
          </button>
        </div>

      </div>
    </div>
  `
})
export class NetworkErrorComponent {
  get isOnline(): boolean {
    return navigator.onLine;
  }

  constructor(private router: Router) {}

  retry(): void {
    window.history.back();
  }

  goToDashboard(): void {
    this.router.navigate(['/login']);
  }
}
