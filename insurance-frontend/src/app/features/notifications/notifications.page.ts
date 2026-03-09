import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService, Notification } from '../../core/services/notification.service';

type FilterType = 'All' | 'Unread' | 'Info' | 'Success' | 'Warning' | 'Risk';

@Component({
  selector: 'app-notifications-page',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="min-h-screen" style="background: linear-gradient(135deg, #f8faff 0%, #f0f4ff 100%);">

      <!-- Hero Header -->
      <div style="background: linear-gradient(135deg, #4f46e5 0%, #7c3aed 100%);" class="px-8 pt-10 pb-16">
        <div class="max-w-4xl mx-auto">
          <div class="flex flex-col md:flex-row md:items-end justify-between gap-6">
            <div>
              <div class="flex items-center gap-3 mb-3">
                <div class="w-10 h-10 bg-white/20 rounded-2xl flex items-center justify-center">
                  <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
                  </svg>
                </div>
                <span class="text-indigo-200 text-sm font-semibold uppercase tracking-widest">Activity Feed</span>
              </div>
              <h1 class="text-4xl font-black text-white tracking-tight">Your Notifications</h1>
              <p class="text-indigo-200 mt-2 font-medium">Stay on top of every update, alert, and system event.</p>
            </div>

            <!-- Stats -->
            <div class="flex items-center gap-4" *ngIf="!isLoading()">
              <div class="text-center bg-white/10 rounded-2xl px-5 py-3 border border-white/20">
                <div class="text-2xl font-black text-white">{{ allNotifications().length }}</div>
                <div class="text-indigo-200 text-[10px] font-bold uppercase tracking-widest mt-0.5">Total</div>
              </div>
              <div class="text-center bg-white/10 rounded-2xl px-5 py-3 border border-white/20">
                <div class="text-2xl font-black text-rose-300">{{ unreadCount() }}</div>
                <div class="text-indigo-200 text-[10px] font-bold uppercase tracking-widest mt-0.5">Unread</div>
              </div>
              <div class="text-center bg-white/10 rounded-2xl px-5 py-3 border border-white/20">
                <div class="text-2xl font-black text-amber-300">{{ riskCount() }}</div>
                <div class="text-indigo-200 text-[10px] font-bold uppercase tracking-widest mt-0.5">Alerts</div>
              </div>
            </div>
          </div>

          <!-- Filter Tabs + Mark All Read -->
          <div class="flex items-center justify-between mt-6 flex-wrap gap-4">
            <div class="flex flex-wrap gap-2">
              <button *ngFor="let f of filters" (click)="setFilter(f)"
                class="px-4 py-1.5 rounded-full text-xs font-bold transition-all border cursor-pointer"
                [style.background]="activeFilter() === f ? 'white' : 'rgba(255,255,255,0.1)'"
                [style.color]="activeFilter() === f ? '#4f46e5' : 'rgba(255,255,255,0.8)'"
                [style.border-color]="activeFilter() === f ? 'white' : 'rgba(255,255,255,0.25)'">
                {{ f }}
                <span *ngIf="f === 'Unread' && unreadCount() > 0"
                  class="ml-1.5 bg-rose-500 text-white rounded-full px-1.5 text-[9px] font-black">{{ unreadCount() }}</span>
              </button>
            </div>
            <button *ngIf="unreadCount() > 0" (click)="markAllAsRead()"
              class="flex items-center gap-2 px-4 py-2 bg-white/15 hover:bg-white/25 border border-white/30 text-white text-xs font-bold rounded-xl transition-all cursor-pointer">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 13l4 4L19 7" />
              </svg>
              Mark all read
            </button>
          </div>
        </div>
      </div>

      <!-- Content -->
      <div class="max-w-4xl mx-auto px-8 -mt-6 pb-16">

        <!-- Skeleton -->
        <div *ngIf="isLoading()" class="space-y-4">
          <div *ngFor="let s of [1,2,3,4]"
            class="bg-white rounded-3xl p-6 border border-slate-100 shadow-sm flex gap-5 animate-pulse">
            <div class="w-12 h-12 bg-slate-100 rounded-2xl shrink-0"></div>
            <div class="flex-1 space-y-3 pt-1">
              <div class="h-4 bg-slate-100 rounded-full w-1/3"></div>
              <div class="h-3 bg-slate-100 rounded-full w-3/4"></div>
              <div class="h-3 bg-slate-100 rounded-full w-1/2"></div>
            </div>
          </div>
        </div>

        <!-- Empty -->
        <div *ngIf="!isLoading() && filteredNotifications().length === 0"
          class="flex flex-col items-center justify-center text-center py-24 bg-white rounded-3xl border border-slate-100 shadow-sm mt-2">
          <div class="w-24 h-24 bg-indigo-50 rounded-full flex items-center justify-center mb-6">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-10 w-10 text-indigo-300" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
            </svg>
          </div>
          <h3 class="text-xl font-bold text-slate-800">All clear!</h3>
          <p class="text-slate-400 mt-2 max-w-xs">No notifications match this filter. Switch to "All" to see everything.</p>
          <button (click)="setFilter('All')"
            class="mt-6 px-6 py-2.5 bg-indigo-600 text-white text-sm font-bold rounded-xl hover:bg-indigo-700 transition-colors cursor-pointer">
            View All
          </button>
        </div>

        <!-- Cards -->
        <div *ngIf="!isLoading() && filteredNotifications().length > 0" class="space-y-3 mt-2">
          <div *ngFor="let note of filteredNotifications(); let i = index"
            class="group relative bg-white rounded-3xl border shadow-sm transition-all duration-300 overflow-hidden"
            [style.animation-delay]="(i * 40) + 'ms'"
            [ngClass]="{
              'border-indigo-200 shadow-indigo-50 shadow-md': !note.isRead,
              'border-slate-100 hover:border-slate-200 hover:shadow-md': note.isRead
            }">

            <!-- Left accent bar -->
            <div class="absolute left-0 top-0 bottom-0 w-1"
              [ngClass]="{
                'bg-blue-500':    note.type === 'Info',
                'bg-emerald-500': note.type === 'Success',
                'bg-amber-500':   note.type === 'Warning',
                'bg-rose-500':    note.type === 'Risk'
              }">
            </div>

            <div class="flex items-start gap-5 p-6 pl-7">
              <!-- Icon -->
              <div class="shrink-0 mt-0.5">
                <div class="w-12 h-12 rounded-2xl flex items-center justify-center transition-transform group-hover:scale-110"
                  [ngClass]="{
                    'bg-blue-50 text-blue-600':    note.type === 'Info',
                    'bg-emerald-50 text-emerald-600': note.type === 'Success',
                    'bg-amber-50 text-amber-600':  note.type === 'Warning',
                    'bg-rose-50 text-rose-600':    note.type === 'Risk'
                  }">
                  <svg *ngIf="note.type === 'Info'" xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                  <svg *ngIf="note.type === 'Success'" xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                  <svg *ngIf="note.type === 'Warning'" xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                  </svg>
                  <svg *ngIf="note.type === 'Risk'" xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                </div>
              </div>

              <!-- Content -->
              <div class="flex-1 min-w-0">
                <div class="flex items-start justify-between gap-4 mb-1">
                  <div class="flex items-center gap-2 flex-wrap">
                    <h3 class="font-bold text-slate-900 text-base leading-tight"
                      [class.text-indigo-900]="!note.isRead">{{ note.title }}</h3>
                    <span *ngIf="!note.isRead"
                      class="inline-block w-2 h-2 rounded-full bg-indigo-500 shrink-0"></span>
                    <span *ngIf="note.type === 'Risk'"
                      class="px-2 py-0.5 bg-rose-100 text-rose-700 text-[9px] font-black uppercase tracking-widest rounded-full border border-rose-200">
                      AI Alert
                    </span>
                  </div>
                  <span class="text-[11px] text-slate-400 font-semibold whitespace-nowrap bg-slate-50 px-2.5 py-1 rounded-lg shrink-0">
                    {{ note.createdAt | date:'MMM d · h:mm a' }}
                  </span>
                </div>

                <p class="text-sm text-slate-500 leading-relaxed mb-3">{{ note.message }}</p>

                <div class="flex items-center gap-2">
                  <span class="px-2.5 py-1 text-[10px] font-black uppercase tracking-wider rounded-lg border"
                    [ngClass]="{
                      'bg-blue-50 text-blue-700 border-blue-100':      note.type === 'Info',
                      'bg-emerald-50 text-emerald-700 border-emerald-100': note.type === 'Success',
                      'bg-amber-50 text-amber-700 border-amber-100':   note.type === 'Warning',
                      'bg-rose-50 text-rose-700 border-rose-100':      note.type === 'Risk'
                    }">{{ note.type }}</span>
                  <span *ngIf="note.isRead"
                    class="text-[10px] text-slate-300 font-semibold uppercase tracking-wider">Read</span>
                </div>
              </div>

              <!-- Mark read btn -->
              <div *ngIf="!note.isRead" class="shrink-0">
                <button (click)="markAsRead(note.id)"
                  class="w-9 h-9 rounded-full bg-slate-50 text-slate-400 hover:bg-indigo-600 hover:text-white transition-all flex items-center justify-center opacity-0 group-hover:opacity-100 shadow-sm hover:shadow-indigo-200 hover:shadow-lg cursor-pointer"
                  title="Mark as read">
                  <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
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
  styles: [`:host { display: block; }`]
})
export class NotificationsPageComponent implements OnInit {
  readonly filters: FilterType[] = ['All', 'Unread', 'Risk', 'Info', 'Success'];

  allNotifications = signal<Notification[]>([]);
  activeFilter     = signal<FilterType>('All');
  isLoading        = signal(true);

  filteredNotifications = computed(() => {
    const f   = this.activeFilter();
    const all = this.allNotifications();
    if (f === 'All')    return all;
    if (f === 'Unread') return all.filter(n => !n.isRead);
    return all.filter(n => n.type === f);
  });

  unreadCount = computed(() => this.allNotifications().filter(n => !n.isRead).length);
  riskCount   = computed(() => this.allNotifications().filter(n => n.type === 'Risk').length);

  constructor(
    private notificationService: NotificationService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.isLoading.set(true);
    this.notificationService.getNotifications().subscribe(data => {
      this.allNotifications.set(data);
      this.isLoading.set(false);
      this.cdr.markForCheck();
    });
  }

  setFilter(f: FilterType): void {
    this.activeFilter.set(f);
    this.cdr.markForCheck();
  }

  markAsRead(id: string): void {
    this.notificationService.markAsRead(id).subscribe(() => this.load());
  }

  markAllAsRead(): void {
    const unread = this.allNotifications().filter(n => !n.isRead);
    if (!unread.length) return;
    let done = 0;
    unread.forEach(n => {
      this.notificationService.markAsRead(n.id).subscribe(() => {
        if (++done === unread.length) this.load();
      });
    });
  }
}
