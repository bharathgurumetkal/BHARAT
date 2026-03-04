import { Component, OnInit, ChangeDetectionStrategy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ClaimsOfficerService } from '../../services/claims-officer.service';
import { catchError, of } from 'rxjs';

@Component({
  selector: 'app-claims-officer-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './dashboard.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ClaimsOfficerDashboardComponent implements OnInit {
  stats = signal({
    total: 0,
    submitted: 0,
    underReview: 0,
    approved: 0,
    rejected: 0,
    settled: 0
  });

  recentClaims = signal<any[]>([]);
  isLoading = signal(true);
  errorMessage = signal<string | null>(null);

  constructor(
    private claimsService: ClaimsOfficerService
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.claimsService.getClaims().pipe(
      catchError(err => {
        console.error('Claims fetch error:', err);
        this.errorMessage.set("Unable to synchronize claims data. Please contact IT.");
        return of([]);
      })
    ).subscribe(claims => {
      this.stats.set({
        total: claims.length,
        submitted: claims.filter(c => c.status === 'Submitted').length,
        underReview: claims.filter(c => c.status === 'UnderReview').length,
        approved: claims.filter(c => c.status === 'Approved').length,
        rejected: claims.filter(c => c.status === 'Rejected').length,
        settled: claims.filter(c => c.status === 'Settled').length
      });

      this.recentClaims.set(claims.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()).slice(0, 5));
      this.isLoading.set(false);
    });
  }
}
