import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-network-error',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gradient-to-br from-slate-950 via-slate-900 to-cyan-950 px-4">
      
      <!-- Ambient Background Glow -->
      <div class="absolute inset-0 overflow-hidden pointer-events-none">
        <div class="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[600px] h-[600px] bg-cyan-600/10 rounded-full blur-[120px]"></div>
      </div>

      <div class="relative z-10 w-full max-w-lg mx-auto">
        <div class="glass-container p-10 sm:p-14 rounded-[2.5rem] border border-white/10 backdrop-blur-2xl bg-white/5 shadow-2xl text-center relative overflow-hidden">
          
          <!-- Outer decorative ring -->
          <div class="absolute -bottom-10 -left-10 w-40 h-40 bg-cyan-500/5 rounded-full blur-3xl"></div>

          <!-- Icon Section -->
          <div class="flex justify-center mb-10">
            <div class="relative w-24 h-24">
              <div class="absolute inset-0 bg-cyan-500/20 blur-xl rounded-full scale-150 animate-pulse"></div>
              <div class="w-full h-full rounded-full bg-gradient-to-tr from-cyan-600 to-teal-400 p-0.5 shadow-lg relative z-10 flex items-center justify-center">
                <div class="w-full h-full rounded-full bg-slate-900 flex items-center justify-center">
                  <svg class="w-12 h-12 text-cyan-500 animate-pulse" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5"
                      d="M8.288 15.038a5.25 5.25 0 017.424 0M5.106 11.856c3.807-3.808 9.98-3.808 13.788 0M1.924 8.674c5.565-5.565 14.587-5.565 20.152 0M12.53 18.22l-.53.53-.53-.53a.75.75 0 011.06 0z"/>
                  </svg>
                </div>
              </div>
            </div>
          </div>

          <!-- Content -->
          <h1 class="text-4xl font-extrabold text-white tracking-tight mb-4">Signal Lost</h1>
          <p class="text-slate-400 text-lg leading-relaxed mb-10">
            We can't seem to reach our servers. Please check your internet connection and try again.
          </p>

          <!-- Status Indicator -->
          <div class="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-black/40 border border-white/5 mb-10">
            <span class="w-2 h-2 rounded-full" [class]="isOnline ? 'bg-emerald-400 animate-pulse shadow-[0_0_8px_rgba(52,211,153,0.6)]' : 'bg-red-400 shadow-[0_0_8px_rgba(248,113,113,0.6)]'"></span>
            <span class="text-xs font-bold uppercase tracking-wider" [class]="isOnline ? 'text-emerald-400' : 'text-red-400'">
              {{ isOnline ? 'System Online' : 'No Connection' }}
            </span>
          </div>

          <!-- Action Buttons -->
          <div class="space-y-4">
            <button (click)="retry()"
              class="w-full px-8 py-4 bg-cyan-600 hover:bg-cyan-500 text-white font-bold rounded-2xl transition-all duration-300 shadow-lg shadow-cyan-900/40 hover:shadow-cyan-900/60 hover:-translate-y-1 active:scale-95 flex items-center justify-center gap-3">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"/>
              </svg>
              Re-establish Connection
            </button>
            <button (click)="goToDashboard()"
              class="w-full px-8 py-4 bg-white/5 hover:bg-white/10 text-slate-300 hover:text-white font-bold rounded-2xl border border-white/10 transition-all duration-300">
              Return Home
            </button>
          </div>

          <p class="mt-8 text-slate-600 text-sm font-medium">Network Error · Protocol Check</p>
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
export class NetworkErrorComponent {
  get isOnline(): boolean {
    return navigator.onLine;
  }

  constructor(private router: Router) {}

  retry(): void {
    window.location.reload();
  }

  goToDashboard(): void {
    this.router.navigate(['/']);
  }
}
