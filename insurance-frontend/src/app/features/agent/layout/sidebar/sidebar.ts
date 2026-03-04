import { Component, ChangeDetectorRef, ChangeDetectionStrategy, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, NavigationEnd } from '@angular/router';
import { Subscription, filter } from 'rxjs';

@Component({
  selector: 'app-agent-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './sidebar.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AgentSidebarComponent implements OnInit, OnDestroy {
  private routerSub!: Subscription;
  currentUrl = '';

  menuItems = [
    { label: 'Dashboard', route: '/agent/dashboard', icon: 'M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6' },
    { label: 'Applications', route: '/agent/applications', icon: 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z' },
    { label: 'Policies', route: '/agent/policies', icon: 'M9 12l2 2 4-4M7.835 4.697a3.42 3.42 0 001.946-.806 3.42 3.42 0 014.438 0 3.42 3.42 0 001.946.806 3.42 3.42 0 013.138 3.138z' },
    { label: 'Commissions', route: '/agent/commissions', icon: 'M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z' },
  ];

  userName: string = 'Agent Name';
  userEmail: string = 'agent@secureguard.com';

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
