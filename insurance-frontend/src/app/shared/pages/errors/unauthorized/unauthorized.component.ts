import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-unauthorized',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gradient-to-br from-slate-950 via-slate-900 to-amber-950 px-4">
      
      <!-- Ambient Background Glow -->
      <div class="absolute inset-0 overflow-hidden pointer-events-none">
        <div class="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[600px] h-[600px] bg-amber-500/10 rounded-full blur-[120px]"></div>
      </div>

      <div class="relative z-10 w-full max-w-lg mx-auto">
        <div class="glass-container p-10 sm:p-14 rounded-[2.5rem] border border-white/10 backdrop-blur-2xl bg-white/5 shadow-2xl text-center relative overflow-hidden">
          
          <!-- Outer decorative ring -->
          <div class="absolute -top-10 -right-10 w-40 h-40 bg-amber-500/5 rounded-full blur-3xl"></div>

          <!-- Icon Section -->
          <div class="flex justify-center mb-10">
            <div class="relative">
              <div class="absolute inset-0 bg-amber-500/20 blur-xl rounded-full scale-150 animate-pulse"></div>
              <div class="w-24 h-24 rounded-full bg-gradient-to-tr from-amber-600 to-yellow-400 p-0.5 shadow-lg relative z-10">
                <div class="w-full h-full rounded-full bg-slate-900 flex items-center justify-center">
                  <svg class="w-12 h-12 text-amber-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5"
                      d="M16.5 10.5V6.75a4.5 4.5 0 10-9 0v3.75m-.75 11.25h10.5a2.25 2.25 0 002.25-2.25v-6.75a2.25 2.25 0 00-2.25-2.25H6.75a2.25 2.25 0 00-2.25 2.25v6.75a2.25 2.25 0 002.25 2.25z"/>
                  </svg>
                </div>
              </div>
            </div>
          </div>

          <!-- Content -->
          <h1 class="text-4xl font-extrabold text-white tracking-tight mb-4">Session Timed Out</h1>
          <p class="text-slate-400 text-lg leading-relaxed mb-10">
            For your security, your session has expired or you need to sign in to access this area.
          </p>

          <!-- Action Buttons -->
          <div class="space-y-4">
            <button (click)="goToLogin()"
              class="w-full px-8 py-4 bg-amber-600 hover:bg-amber-500 text-white font-bold rounded-2xl transition-all duration-300 shadow-lg shadow-amber-900/40 hover:shadow-amber-900/60 hover:-translate-y-1 active:scale-95 flex items-center justify-center gap-3">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 16l-4-4m0 0l4-4m-4 4h14m-5 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h7a3 3 0 013 3v1"/>
              </svg>
              Sign In Again
            </button>
            <button (click)="goBack()"
              class="w-full px-8 py-4 bg-white/5 hover:bg-white/10 text-slate-300 hover:text-white font-bold rounded-2xl border border-white/10 transition-all duration-300">
              Return to Previous Page
            </button>
          </div>

          <p class="mt-8 text-slate-600 text-sm font-medium">Error Code: 401 · Security Protocol</p>
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
export class UnauthorizedComponent {
  constructor(private router: Router) {}

  goToLogin(): void {
    this.router.navigate(['/login']);
  }

  goBack(): void {
    window.history.back();
  }
}
