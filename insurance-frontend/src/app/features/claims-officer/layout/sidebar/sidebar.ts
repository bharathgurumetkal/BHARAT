import { Component, ChangeDetectorRef, ChangeDetectionStrategy, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, NavigationEnd } from '@angular/router';
import { Subscription, filter } from 'rxjs';

@Component({
  selector: 'app-claims-officer-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './sidebar.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ClaimsOfficerSidebarComponent implements OnInit, OnDestroy {
  private routerSub!: Subscription;
  currentUrl = '';

  menuItems = [
    { label: 'Dashboard', route: '/claims-officer/dashboard', icon: 'M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6' },
    { label: 'Claims Management', route: '/claims-officer/claims', icon: 'M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z' },
    { label: 'Policies', route: '/claims-officer/policies', icon: 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z' },
  ];

  userName: string = 'Claims Officer';
  userEmail: string = 'officer@secureguard.com';

  constructor(private router: Router, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.currentUrl = this.router.url;
    const email = localStorage.getItem('email');
    if (email) {
      this.userEmail = email;
      this.userName = email.split('@')[0].toUpperCase();
    }

    this.routerSub = this.router.events
      .pipe(filter(e => e instanceof NavigationEnd))
      .subscribe((e: any) => {
        this.currentUrl = e.urlAfterRedirects;
        this.cdr.markForCheck();
      });
  }

  ngOnDestroy(): void {
    this.routerSub?.unsubscribe();
  }

  isActive(route: string): boolean {
    return this.currentUrl === route || this.currentUrl.startsWith(route + '/');
  }
}
