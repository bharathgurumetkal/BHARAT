import { Component, OnInit, signal, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AgentService } from '../../../../core/services/agent.service';
import { Policy, PolicyApplication, CustomerReport } from '../../../../core/models/insurance.models';
import { forkJoin, catchError, of } from 'rxjs';

@Component({
  selector: 'app-agent-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './dashboard.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AgentDashboardComponent implements OnInit {
  stats = signal({
    totalCustomers: 0,
    totalApplications: 0,
    activePolicies: 0,
    draftPolicies: 0
  });

  recentApplications = signal<PolicyApplication[]>([]);
  recentPolicies = signal<Policy[]>([]);
  isLoading = signal(true);
  errorMessage = signal<string | null>(null);

  constructor(
    private agentService: AgentService
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    forkJoin({
      customers: this.agentService.getMyCustomers().pipe(catchError(() => of([]))),
      applications: this.agentService.getAssignedApplications().pipe(catchError(() => of([]))),
      policies: this.agentService.getPolicies().pipe(catchError(() => of([])))
    }).subscribe({
      next: (data) => {
        console.log('AGENT_DEBUG: Dashboard data loaded:', data);
        this.stats.set({
          totalCustomers: data.customers.length,
          totalApplications: data.applications.length,
          activePolicies: data.policies.filter(p => p.status === 'Active').length,
          draftPolicies: data.policies.filter(p => p.status === 'Draft').length
        });

        this.recentApplications.set(data.applications.slice(0, 5));
        this.recentPolicies.set(data.policies.slice(0, 5));
        
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Agent dashboard load error:', err);
        this.errorMessage.set("Failed to load dashboard data. Please check your connection.");
        this.isLoading.set(false);
      }
    });
  }
}
