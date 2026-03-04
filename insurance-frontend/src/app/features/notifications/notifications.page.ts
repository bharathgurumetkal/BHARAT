import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService, Notification } from '../../core/services/notification.service';

@Component({
  selector: 'app-notifications-page',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="min-h-screen bg-slate-50 p-8">
      <div class="max-w-4xl mx-auto">
        <!-- Header Section -->
        <div class="flex items-end justify-between mb-8">
          <div class="space-y-1">
            <h1 class="text-3xl font-extrabold text-slate-900 tracking-tight">Your Activity</h1>
            <p class="text-sm text-slate-500 font-medium">Keep track of your applications, claims, and policies.</p>
          </div>
          <button *ngIf="hasUnread" (click)="markAllAsRead()" 
                  class="flex items-center gap-2 px-5 py-2.5 bg-white text-sm font-bold text-indigo-600 hover:text-indigo-700 hover:bg-indigo-50 border border-slate-200 hover:border-indigo-200 rounded-xl transition-all shadow-sm active:scale-95 cursor-pointer">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
            </svg>
            Mark all read
          </button>
        </div>

        <!-- Empty State -->
        <div *ngIf="notifications.length === 0" class="flex flex-col items-center justify-center py-32 bg-white rounded-3xl shadow-sm border border-slate-100">
           <div class="w-24 h-24 bg-slate-50 rounded-full flex items-center justify-center mb-6">
             <svg xmlns="http://www.w3.org/2000/svg" class="h-10 w-10 text-slate-300" fill="none" viewBox="0 0 24 24" stroke="currentColor">
               <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
             </svg>
           </div>
           <h3 class="text-xl font-bold text-slate-800 tracking-tight">You're all caught up!</h3>
           <p class="text-slate-500 mt-2 text-center max-w-sm">When there are updates to your account, they will appear here in your activity feed.</p>
        </div>

        <!-- Notifications Grid/List -->
        <div class="space-y-4">
          <div *ngFor="let note of notifications; let i = index" 
               class="bg-white rounded-2xl p-6 transition-all duration-300 relative group overflow-hidden animate-in slide-in-from-bottom-4 border"
               [ngClass]="{
                  'border-indigo-100 shadow-md scale-[1.01] z-10': !note.isRead,
                  'border-slate-100 shadow-sm hover:border-slate-200': note.isRead
               }"
               [style.animation-delay]="(i * 50) + 'ms'">
            
            <!-- Unread Indicator Line -->
            <div *ngIf="!note.isRead" class="absolute left-0 top-0 bottom-0 w-1.5 bg-indigo-600 rounded-l-2xl"></div>

            <div class="flex items-start gap-5" [class.ml-2]="!note.isRead">
              
              <!-- Icon Container -->
              <div class="relative shrink-0 mt-1">
                <div [ngClass]="{
                  'bg-blue-50 text-blue-600 ring-blue-100': note.type === 'Info',
                  'bg-emerald-50 text-emerald-600 ring-emerald-100': note.type === 'Success',
                  'bg-amber-50 text-amber-600 ring-amber-100': note.type === 'Warning',
                  'bg-rose-50 text-rose-600 ring-rose-100': note.type === 'Risk'
                }" class="w-14 h-14 rounded-2xl flex items-center justify-center ring-4 transition-transform group-hover:scale-110">
                  
                  <!-- Dynamic Icons -->
                  <svg *ngIf="note.type === 'Info'" xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                  <svg *ngIf="note.type === 'Success'" xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                  <svg *ngIf="note.type === 'Warning'" xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                  </svg>
                  <svg *ngIf="note.type === 'Risk'" xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                     <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>

                </div>
              </div>

              <!-- Content -->
              <div class="flex-1 min-w-0 pr-12">
                <div class="flex items-center justify-between gap-4 mb-2">
                   <h4 class="text-base font-bold text-slate-900 tracking-tight" [class.text-indigo-900]="!note.isRead">{{ note.title }}</h4>
                   <span class="text-xs font-semibold text-slate-400 whitespace-nowrap bg-slate-50 px-2 py-1 rounded-md">{{ note.createdAt | date:'MMM d, h:mm a' }}</span>
                </div>
                
                <p class="text-sm text-slate-600 leading-relaxed mb-4">{{ note.message }}</p>
                
                <div class="flex items-center gap-3">
                  <span [ngClass]="{
                    'text-blue-700 bg-blue-50 border-blue-100': note.type === 'Info',
                    'text-emerald-700 bg-emerald-50 border-emerald-100': note.type === 'Success',
                    'text-amber-700 bg-amber-50 border-amber-100': note.type === 'Warning',
                    'text-rose-700 bg-rose-50 border-rose-100': note.type === 'Risk'
                  }" class="px-2.5 py-1 text-[10px] font-black uppercase tracking-wider rounded-md border">
                    {{ note.type }}
                  </span>
                </div>
              </div>

              <!-- Action Button -->
              <div *ngIf="!note.isRead" class="absolute right-6 top-1/2 -translate-y-1/2">
                <button (click)="markAsRead(note.id)" 
                        class="w-10 h-10 rounded-full bg-slate-50 text-slate-400 hover:bg-indigo-600 hover:text-white hover:shadow-lg hover:shadow-indigo-200 transition-all flex items-center justify-center opacity-0 group-hover:opacity-100 -translate-x-4 group-hover:translate-x-0 cursor-pointer"
                        title="Mark as Read">
                  <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 13l4 4L19 7" />
                  </svg>
                </button>
              </div>

            </div>
          </div>
        </div>

      </div>
    </div>
  `,
  styles: [`
    :host { display: block; }
  `]
})
export class NotificationsPageComponent implements OnInit {
  notifications: Notification[] = [];

  constructor(private notificationService: NotificationService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.notificationService.getNotifications().subscribe(data => {
      this.notifications = data;
    });
  }

  get hasUnread(): boolean {
    return this.notifications.some(n => !n.isRead);
  }

  markAsRead(id: string): void {
    this.notificationService.markAsRead(id).subscribe(() => this.load());
  }

  markAllAsRead(): void {
    // Sequential fallback if no multi-mark endpoint
    const unread = this.notifications.filter(n => !n.isRead);
    let completed = 0;
    unread.forEach(n => {
      this.notificationService.markAsRead(n.id).subscribe(() => {
        completed++;
        if (completed === unread.length) this.load();
      });
    });
  }
}
