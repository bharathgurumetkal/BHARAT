import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gradient-to-br from-slate-950 via-slate-900 to-indigo-950 px-4">
      
      <!-- Ambient Background Glow -->
      <div class="absolute inset-0 overflow-hidden pointer-events-none">
        <div class="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[600px] h-[600px] bg-indigo-600/10 rounded-full blur-[120px]"></div>
        <div class="absolute bottom-0 left-0 w-[400px] h-[400px] bg-blue-600/5 rounded-full blur-[100px]"></div>
      </div>

      <div class="relative z-10 w-full max-w-lg mx-auto">
        <div class="glass-container p-10 sm:p-14 rounded-[2.5rem] border border-white/10 backdrop-blur-2xl bg-white/5 shadow-2xl text-center relative overflow-hidden">
          
          <!-- Decorative element -->
          <div class="absolute -top-12 -left-12 w-48 h-48 bg-indigo-500/10 rounded-full blur-3xl"></div>

          <!-- 404 Illustration -->
          <div class="relative mb-10 h-48 flex items-center justify-center">
            <div class="absolute text-[12rem] font-black text-white/5 select-none">404</div>
            <div class="relative z-10">
              <div class="w-32 h-32 rounded-3xl bg-indigo-500/10 border border-indigo-500/20 flex items-center justify-center animate-bounce duration-[3000ms]">
                <svg class="w-16 h-16 text-indigo-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1"
                    d="M9.172 16.172a4 4 0 015.656 0M9 10h.01M15 10h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/>
                </svg>
              </div>
            </div>
          </div>

          <!-- Content -->
          <h1 class="text-4xl font-extrabold text-white tracking-tight mb-4">Lost in Space</h1>
          <p class="text-slate-400 text-lg leading-relaxed mb-10">
            The page you're searching for has drifted away. Let's get you back on course.
          </p>

          <!-- Action Buttons -->
          <div class="flex flex-col gap-4">
            <button (click)="goToDashboard()"
              class="w-full px-8 py-4 bg-indigo-600 hover:bg-indigo-500 text-white font-bold rounded-2xl transition-all duration-300 shadow-lg shadow-indigo-900/40 hover:shadow-indigo-900/60 hover:-translate-y-1 active:scale-95 flex items-center justify-center gap-3">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6"/>
              </svg>
              Return Home
            </button>
            <button (click)="goBack()"
              class="w-full px-8 py-4 bg-white/5 hover:bg-white/10 text-slate-300 hover:text-white font-bold rounded-2xl border border-white/10 transition-all duration-300">
              Go Back
            </button>
          </div>

          <p class="mt-10 text-slate-600 text-sm font-medium">Page Not Found · Navigation Error</p>
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
export class NotFoundComponent {
  constructor(private router: Router) {}

  goToDashboard(): void {
    this.router.navigate(['/']);
  }

  goBack(): void {
    window.history.back();
  }
}
