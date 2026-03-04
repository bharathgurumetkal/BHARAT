import { Component, OnInit, signal, computed, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AdminApiService } from '../../../../core/services/admin-api.service';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AiAnalytics, ClaimsReport, AgentPerformance, AgentPerformanceAnalytics } from '../../../../core/models/insurance.models';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './admin-dashboard.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [DecimalPipe]
})
export class AdminDashboardComponent implements OnInit {
  isLoading = signal(true);
  errorMessage = signal<string | null>(null);

  stats = signal({ activePolicies: 0, totalClaims: 0, pendingClaims: 0, totalAgents: 0 });
  claimsReport = signal<ClaimsReport[]>([]);
  agentPerformanceAnalytics = signal<AgentPerformanceAnalytics[]>([]);
  aiAnalytics = signal<AiAnalytics | null>(null);

  constructor(
    private adminService: AdminApiService
  ) {}

  ngOnInit(): void { this.loadData(); }

  loadData() {
    this.isLoading.set(true);
    
    forkJoin({
      policies:    this.adminService.getPolicies().pipe(catchError(() => of([]))),
      agents:      this.adminService.getAgents().pipe(catchError(() => of([]))),
      claims:      this.adminService.getClaimsReport().pipe(catchError(() => of([]))),
      analytics:   this.adminService.getAgentPerformanceAnalytics().pipe(catchError(() => of([]))),
      aiData:      this.adminService.getAiAnalytics().pipe(catchError(() => of(null)))
    }).subscribe({
      next: (data) => {
        const activePolicies = data.policies.filter((p: any) => p.status === 'Active').length;
        const totalAgents    = data.agents.length;
        const totalClaims    = data.claims.reduce((sum: number, c: any) => sum + c.count, 0);
        const pendingClaims  = data.claims.find((c: any) => c.status === 0)?.count || 0;

        this.stats.set({ activePolicies, totalAgents, totalClaims, pendingClaims });
        this.claimsReport.set(data.claims);
        this.agentPerformanceAnalytics.set(data.analytics);
        this.aiAnalytics.set(data.aiData as AiAnalytics | null);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Failed to load dashboard data.');
        this.isLoading.set(false);
      }
    });
  }

  getStatusName(status: number): string {
    const statuses = ['Submitted', 'Under Review', 'Approved', 'Rejected', 'Settled'];
    return statuses[status] || 'Unknown';
  }

  getTrendBarWidth(avgRisk: number): number {
    const ai = this.aiAnalytics();
    if (!ai?.riskTrendMonthly?.length) return 0;
    const max = Math.max(...ai.riskTrendMonthly.map(m => m.avgRisk), 1);
    return Math.round((avgRisk / max) * 100);
  }

  getTrendBarColor(avgRisk: number): string {
    if (avgRisk >= 70) return '#ef4444';
    if (avgRisk >= 40) return '#f59e0b';
    return '#10b981';
  }

  getDonutSegments(): { color: string; label: string; value: number; percent: number }[] {
    const ai = this.aiAnalytics();
    if (!ai) return [];
    const scored = ai.scoredClaims || 0;
    if (scored === 0) return [];
    return [
      { color: '#ef4444', label: 'High Risk',   value: ai.highRiskClaims,   percent: Math.round(ai.highRiskClaims   / scored * 100) },
      { color: '#f59e0b', label: 'Medium Risk', value: ai.mediumRiskClaims, percent: Math.round(ai.mediumRiskClaims / scored * 100) },
      { color: '#10b981', label: 'Low Risk',    value: ai.lowRiskClaims,    percent: Math.round(ai.lowRiskClaims    / scored * 100) },
    ];
  }

  getExposureClass(score: number): string {
    if (score >= 60) return 'text-red-600';
    if (score >= 30) return 'text-amber-600';
    return 'text-emerald-600';
  }
}
