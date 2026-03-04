import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthApiService } from '../../../auth/services/auth-api.service';
import { NotificationService, Notification } from '../../../../core/services/notification.service';
import { Subscription, interval } from 'rxjs';

@Component({
  selector: 'app-admin-topbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './topbar.html'
})
export class AdminTopbarComponent implements OnInit, OnDestroy {
  currentTime = new Date();
  unreadCount = 0;
  latestNotifications: Notification[] = [];
  showNotifications = false;
  private pollSubscription?: Subscription;

  constructor(
    private router: Router,
    private authService: AuthApiService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    setInterval(() => {
      this.currentTime = new Date();
    }, 1000);

    this.fetchNotifications();
    this.pollSubscription = interval(30000).subscribe(() => {
      this.fetchNotifications();
    });
  }

  ngOnDestroy(): void {
    this.pollSubscription?.unsubscribe();
  }

  fetchNotifications(): void {
    this.notificationService.getUnreadCount().subscribe(res => {
      this.unreadCount = res.count;
    });

    this.notificationService.getNotifications().subscribe(data => {
      this.latestNotifications = data.slice(0, 5);
    });
  }

  toggleNotifications(): void {
    this.showNotifications = !this.showNotifications;
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
