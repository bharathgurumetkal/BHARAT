import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthApiService } from '../../../auth/services/auth-api.service';
import { NotificationService, Notification } from '../../../../core/services/notification.service';
import { Subscription, interval } from 'rxjs';

@Component({
  selector: 'app-claims-officer-topbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './topbar.html'
})
export class ClaimsOfficerTopbarComponent implements OnInit, OnDestroy {
  currentTime = new Date();
  officerName = 'Officer';
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

    const email = localStorage.getItem('email');
    if (email) {
      this.officerName = email.split('@')[0].toUpperCase();
    }

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
