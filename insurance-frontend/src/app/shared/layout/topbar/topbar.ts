import { Component, OnInit, OnDestroy, ChangeDetectionStrategy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthApiService } from '../../../features/auth/services/auth-api.service';
import { Router, RouterModule } from '@angular/router';
import { NotificationService, Notification } from '../../../core/services/notification.service';
import { Subscription, interval } from 'rxjs';

@Component({
  selector: 'app-topbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './topbar.html',
  styleUrl: './topbar.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TopbarComponent implements OnInit, OnDestroy {
  email = signal<string | null>(null);
  role = signal<string | null>(null);
  unreadCount = signal(0);
  latestNotifications = signal<Notification[]>([]);
  showNotifications = signal(false);
  private pollSubscription?: Subscription;

  constructor(
    private authService: AuthApiService, 
    private router: Router,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    this.email.set(localStorage.getItem('email'));
    this.role.set(localStorage.getItem('role'));
    
    this.fetchNotifications();
    
    // Refresh every 30 seconds
    this.pollSubscription = interval(30000).subscribe(() => {
      this.fetchNotifications();
    });
  }

  ngOnDestroy(): void {
    this.pollSubscription?.unsubscribe();
  }

  fetchNotifications(): void {
    this.notificationService.getUnreadCount().subscribe(res => {
      this.unreadCount.set(res.count);
    });

    this.notificationService.getNotifications().subscribe(data => {
      this.latestNotifications.set(data.slice(0, 5));
    });
  }

  toggleNotifications(): void {
    this.showNotifications.update(v => !v);
  }

  markAsRead(id: string, event: Event): void {
    event.stopPropagation();
    this.notificationService.markAsRead(id).subscribe(() => {
      this.fetchNotifications();
    });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
