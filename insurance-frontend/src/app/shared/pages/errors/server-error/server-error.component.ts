import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';

interface ErrorDetails {
  status: number;
  message: string;
  url: string;
  timestamp: string;
  id: string;
}

@Component({
  selector: 'app-server-error',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gradient-to-br from-slate-950 via-slate-900 to-red-950 px-4">
      
      <!-- Ambient Background Glow -->
      <div class="absolute inset-0 overflow-hidden pointer-events-none">
        <div class="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[600px] h-[600px] bg-red-600/10 rounded-full blur-[120px]"></div>
        <div class="absolute top-0 right-0 w-[400px] h-[400px] bg-indigo-600/5 rounded-full blur-[100px]"></div>
      </div>

      <div class="relative z-10 w-full max-w-2xl mx-auto">
        <div class="glass-container p-8 sm:p-12 rounded-[2rem] border border-white/10 backdrop-blur-2xl bg-white/5 shadow-2xl overflow-hidden relative">
          
          <!-- Decorative element -->
          <div class="absolute top-0 right-0 w-32 h-32 bg-red-500/10 blur-2xl rounded-full -mr-16 -mt-16"></div>

          <!-- Icon & Title -->
          <div class="flex flex-col items-center text-center mb-10">
            <div class="w-24 h-24 rounded-2xl bg-gradient-to-tr from-red-600/20 to-orange-600/20 border border-red-500/30 flex items-center justify-center mb-6 shadow-lg shadow-red-900/20">
              <svg class="w-12 h-12 text-red-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5"
                  d="M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126zM12 15.75h.007v.008H12v-.008z"/>
              </svg>
            </div>
            <h1 class="text-4xl font-extrabold text-white tracking-tight mb-3">System Encountered a Glitch</h1>
            <p class="text-slate-400 text-lg max-w-md">
              We're experiencing some technical difficulties on our end. Our engineers have been alerted.
            </p>
          </div>

          <!-- Error Details Card -->
          <div class="bg-black/40 border border-white/5 rounded-2xl p-6 mb-8 font-mono text-sm">
            <div class="flex justify-between items-center mb-4 border-b border-white/10 pb-4">
              <span class="text-red-400 font-bold uppercase tracking-widest text-xs">Error Report</span>
              <span class="text-slate-500 text-xs">{{ errorDetails?.timestamp | date:'medium' }}</span>
            </div>
            
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div class="space-y-1">
                <p class="text-slate-500 text-xs uppercase">Resolution ID</p>
                <p class="text-slate-200 font-medium">{{ errorDetails?.id || 'REQ-' + (9999 + (Math.random() * 90000) | number:'1.0-0') }}</p>
              </div>
              <div class="space-y-1">
                <p class="text-slate-500 text-xs uppercase">Status Code</p>
                <p class="text-slate-200 font-medium">{{ errorDetails?.status || 500 }} - Internal Server Error</p>
              </div>
              <div class="space-y-1 sm:col-span-2">
                <p class="text-slate-500 text-xs uppercase">Internal Message</p>
                <p class="text-slate-300 italic">"{{ errorDetails?.message || 'An unexpected condition was encountered manually or by a system process.' }}"</p>
              </div>
            </div>
          </div>

          <!-- Actions -->
          <div class="flex flex-col sm:flex-row gap-4 justify-center items-center">
            <button (click)="retry()"
              class="w-full sm:w-auto px-8 py-4 bg-red-600 hover:bg-red-500 text-white font-bold rounded-xl transition-all duration-300 shadow-lg shadow-red-900/40 hover:shadow-red-900/60 hover:-translate-y-1 active:scale-95 flex items-center justify-center gap-2">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"/>
              </svg>
              Refresh Page
            </button>
            <button (click)="goToDashboard()"
              class="w-full sm:w-auto px-8 py-4 bg-white/5 hover:bg-white/10 text-slate-300 hover:text-white font-bold rounded-xl border border-white/10 transition-all duration-300 flex items-center justify-center gap-2">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6"/>
              </svg>
              Return Home
            </button>
          </div>
          
          <div class="mt-8 text-center">
            <button (click)="copyLink()" class="text-slate-500 hover:text-indigo-400 text-sm transition-colors flex items-center gap-1 mx-auto">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 5H6a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2v-1M8 5a2 2 0 002 2h2a2 2 0 002-2M8 5a2 2 0 012-2h2a2 2 0 012 2m0 0h2a2 2 0 012 2v3m2 4H10m0 0l3-3m-3 3l3 3"/>
              </svg>
              Copy Technical Details
            </button>
          </div>

        </div>
      </div>
    </div>
  `,
  styles: [`
    .glass-container {
      background: rgba(15, 23, 42, 0.6);
      box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.5);
    }
  `]
})
export class ServerErrorComponent implements OnInit {
  errorDetails: ErrorDetails | null = null;
  Math = Math;

  constructor(private router: Router) {
    const navigation = this.router.getCurrentNavigation();
    this.errorDetails = navigation?.extras?.state?.['error'];
  }

  ngOnInit(): void {
    // If no details provided via state, try to get from history directly
    if (!this.errorDetails && history.state?.['error']) {
      this.errorDetails = history.state['error'];
    }
  }

  retry(): void {
    if (this.errorDetails?.url) {
        window.location.reload();
    } else {
        window.history.back();
    }
  }

  goToDashboard(): void {
    this.router.navigate(['/']);
  }

  copyLink(): void {
    const details = JSON.stringify(this.errorDetails || { status: 500, message: 'Server Error' }, null, 2);
    navigator.clipboard.writeText(details).then(() => {
        alert('Technical details copied to clipboard!');
    });
  }
}
