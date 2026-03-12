import { Component, OnInit, signal, computed, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule, CurrencyPipe, DecimalPipe } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AdminApiService } from '../../../../core/services/admin-api.service';
import { catchError, of } from 'rxjs';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './admin-dashboard.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [CurrencyPipe, DecimalPipe]
})
export class AdminDashboardComponent implements OnInit {
  isLoading = signal(true);
  errorMessage = signal<string | null>(null);
  data = signal<any>(null);
  currentTime = signal(new Date());

  constructor(private adminService: AdminApiService) {}

  ngOnInit(): void {
    this.currentTime.set(new Date());
    this.loadData();
  }

  loadData() {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.adminService.getDashboardSummary()
      .pipe(catchError(err => {
        this.errorMessage.set('Failed to load dashboard data. Please check the backend is running.');
        this.isLoading.set(false);
        return of(null);
      }))
      .subscribe(d => {
        if (d) {
          this.data.set(d);
          this.isLoading.set(false);
        }
      });
  }

  // Revenue trend bar widths
  getRevenueBarWidth(value: number): number {
    const d = this.data();
    if (!d?.revenueTrend?.length) return 0;
    const max = Math.max(...d.revenueTrend.map((r: any) => r.revenue), 1);
    return Math.round((value / max) * 100);
  }

  // Claim funnel step widths
  getClaimFunnelWidth(value: number): number {
    const d = this.data();
    if (!d) return 0;
    const max = d.totalClaims || 1;
    return Math.round((value / max) * 100);
  }

  // Risk segment colors
  getRiskColor(label: string): string {
    if (label === 'High Risk')   return '#ef4444';
    if (label === 'Medium Risk') return '#f59e0b';
    return '#10b981';
  }

  getRiskSegments(): { label: string; value: number; percent: number; color: string }[] {
    const d = this.data();
    if (!d || d.scoredClaims === 0) return [];
    const total = d.scoredClaims;
    return [
      { label: 'High Risk',   value: d.highRiskClaims,   percent: Math.round(d.highRiskClaims   / total * 100), color: '#ef4444' },
      { label: 'Medium Risk', value: d.mediumRiskClaims, percent: Math.round(d.mediumRiskClaims / total * 100), color: '#f59e0b' },
      { label: 'Low Risk',    value: d.lowRiskClaims,    percent: Math.round(d.lowRiskClaims    / total * 100), color: '#10b981' },
    ];
  }

  getLossRatioClass(): string {
    const d = this.data();
    if (!d) return 'text-gray-600';
    if (d.lossRatio > 70) return 'text-red-600';
    if (d.lossRatio > 40) return 'text-amber-600';
    return 'text-emerald-600';
  }

  getLossRatioBar(): string {
    const d = this.data();
    if (!d) return '#10b981';
    if (d.lossRatio > 70) return '#ef4444';
    if (d.lossRatio > 40) return '#f59e0b';
    return '#10b981';
  }

  getMoMClass(): string {
    const d = this.data();
    if (!d) return '';
    return d.revenueMoMChange >= 0 ? 'text-emerald-600' : 'text-red-500';
  }

  getMoMIcon(): string {
    const d = this.data();
    if (!d) return '';
    return d.revenueMoMChange >= 0 ? 'trending_up' : 'trending_down';
  }

  formatCurrency(val: number): string {
    if (val >= 10000000) return '₹' + (val / 10000000).toFixed(1) + ' Cr';
    if (val >= 100000)   return '₹' + (val / 100000).toFixed(1) + ' L';
    if (val >= 1000)     return '₹' + (val / 1000).toFixed(1) + 'K';
    return '₹' + val.toFixed(0);
  }

  getTopProductWidth(count: number): number {
    const d = this.data();
    if (!d?.topProducts?.length) return 0;
    const max = Math.max(...d.topProducts.map((p: any) => p.count), 1);
    return Math.round((count / max) * 100);
  }
}
