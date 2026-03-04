import { Component, OnInit, signal, computed, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminApiService } from '../../../../core/services/admin-api.service';
import { AgentPerformanceAnalytics } from '../../../../core/models/insurance.models';
import { catchError, of } from 'rxjs';

@Component({
  selector: 'app-agent-analytics',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './agent-analytics.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AgentAnalyticsComponent implements OnInit {
  agents = signal<AgentPerformanceAnalytics[]>([]);
  isLoading = signal(true);

  top3HighRisk = computed(() => 
    this.agents()
      .filter(a => a.totalClaims > 0)
      .sort((a, b) => b.riskExposureScore - a.riskExposureScore)
      .slice(0, 3)
  );

  constructor(
    private adminService: AdminApiService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading.set(true);
    this.adminService.getAgentPerformanceAnalytics()
      .pipe(catchError(() => of([])))
      .subscribe(data => {
        this.agents.set(data);
        this.isLoading.set(false);
      });
  }

  getExposureClass(score: number): string {
    if (score >= 60) return 'text-red-600 font-extrabold';
    if (score >= 30) return 'text-amber-600 font-extrabold';
    return 'text-emerald-600 font-bold';
  }

  getExposureBadge(score: number): string {
    if (score >= 60) return 'bg-red-50 text-red-600 border-red-100';
    if (score >= 30) return 'bg-amber-50 text-amber-600 border-amber-100';
    return 'bg-emerald-50 text-emerald-600 border-emerald-100';
  }

  getExposureLabel(score: number): string {
    if (score >= 60) return 'High';
    if (score >= 30) return 'Medium';
    return 'Low';
  }

  getExposureBarColor(score: number): string {
    if (score >= 60) return '#ef4444';
    if (score >= 30) return '#f59e0b';
    return '#10b981';
  }

  getHighRiskBarColor(pct: number): string {
    if (pct >= 50) return '#ef4444';
    if (pct >= 25) return '#f59e0b';
    return '#10b981';
  }
}
