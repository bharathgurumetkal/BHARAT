import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CustomerApiService } from '../../../../core/services/customer-api.service';
import { AdminApiService } from '../../../../core/services/admin-api.service';
import { AgentService } from '../../../../core/services/agent.service';
import { ClaimsOfficerService } from '../../../claims-officer/services/claims-officer.service';
import { forkJoin } from 'rxjs';

interface DashboardStat {
  label: string;
  value: string;
  trend?: string;
  detail?: string;
}

interface ActivityItem {
  title: string;
  subtitle: string;
  date: string;
  type: 'claim' | 'app' | 'user' | 'policy';
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class DashboardComponent implements OnInit {
  role: string | null = null;
  userName: string | null = null;
  
  stats: DashboardStat[] = [];
  recentActivity: ActivityItem[] = [];
  isLoading = true;
  errorMessage: string | null = null;

  constructor(
    private customerService: CustomerApiService,
    private adminService: AdminApiService,
    private agentService: AgentService,
    private claimsOfficerService: ClaimsOfficerService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.role = localStorage.getItem('role');
    const email = localStorage.getItem('email');
    this.userName = email ? email.split('@')[0] : 'User';
    
    this.loadDashboardData();
  }

  loadDashboardData() {
    this.isLoading = true;
    this.errorMessage = null;
    
    if (this.role === 'Customer') {
      forkJoin({
        policies: this.customerService.getPolicies(),
        claims: this.customerService.getClaims(),
        apps: this.customerService.getApplications()
      }).subscribe({
        next: (data) => {
          const activePolicies = data.policies.filter(p => p.status === 'Active');
          const totalCoverage = activePolicies.reduce((sum, p) => sum + p.coverageAmount, 0);
          
          this.stats = [
            { label: 'Total Policies', value: data.policies.length.toString(), trend: `${activePolicies.length} Active` },
            { label: 'Active Claims', value: data.claims.filter(c => c.status !== 'Settled' && c.status !== 'Rejected').length.toString(), detail: 'In Progress' },
            { label: 'Protection Value', value: `$${(totalCoverage / 1000000).toFixed(1)}M`, detail: 'Total Coverage' }
          ];

          const activities: ActivityItem[] = [
            ...data.claims.map(c => ({ title: `Claim ${c.status}`, subtitle: c.reason, date: c.createdAt, type: 'claim' as const })),
            ...data.apps.map(a => ({ title: `Application ${a.status}`, subtitle: a.productName, date: a.submittedAt, type: 'app' as const })),
            ...data.policies.map(p => ({ title: `Policy ${p.status}`, subtitle: p.policyNumber, date: p.startDate || new Date().toISOString(), type: 'policy' as const }))
          ];
          
          this.recentActivity = activities.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime()).slice(0, 5);
          this.isLoading = false;
          this.cdr.detectChanges();
        },
        error: (err) => {
          this.errorMessage = "Failed to load customer data. Please ensure the backend is running.";
          this.isLoading = false;
          this.cdr.detectChanges();
        }
      });
    } else if (this.role === 'Admin') {
      forkJoin({
        customers: this.adminService.getCustomers(),
        revenue: this.adminService.getRevenueReport(),
        claims: this.adminService.getClaimsReport()
      }).subscribe({
        next: (data) => {
          const totalRevenue = data.revenue.reduce((sum, r) => sum + r.totalRevenue, 0);
          const totalClaims = data.claims.reduce((sum, c) => sum + c.count, 0);
          
          this.stats = [
            { label: 'Total Customers', value: data.customers.length.toString(), detail: 'Registered' },
            { label: 'Total Revenue', value: `$${(totalRevenue / 1000).toFixed(1)}k`, detail: 'Platform-wide' },
            { label: 'System Claims', value: totalClaims.toString(), detail: 'All Time' }
          ];
          
          this.recentActivity = data.customers.slice(0, 5).map(c => ({
            title: 'Customer Managed',
            subtitle: `Status: ${c.status}`,
            date: new Date().toISOString(),
            type: 'user' as const
          }));
          this.isLoading = false;
          this.cdr.detectChanges();
        },
        error: (err) => {
          this.errorMessage = "Failed to load admin reports.";
          this.isLoading = false;
          this.cdr.detectChanges();
        }
      });
    } else if (this.role === 'Agent') {
      forkJoin({
        customers: this.agentService.getMyCustomers(),
        apps: this.agentService.getAssignedApplications()
      }).subscribe({
        next: (data) => {
          this.stats = [
            { label: 'My Customers', value: data.customers.length.toString(), detail: 'Assigned to you' },
            { label: 'Pending Reviews', value: data.apps.length.toString(), detail: 'New Applications' }
          ];
          
          this.recentActivity = data.apps.map(a => ({
            title: 'New Assignment',
            subtitle: `${a.customerName} applied for ${a.productName}`,
            date: a.assignedAt || a.submittedAt,
            type: 'app' as const
          })).slice(0, 5);
          this.isLoading = false;
          this.cdr.detectChanges();
        },
        error: () => {
          this.errorMessage = "Failed to load agent assignments.";
          this.isLoading = false;
          this.cdr.detectChanges();
        }
      });
    } else if (this.role === 'ClaimsOfficer') {
      this.claimsOfficerService.getClaims().subscribe({
        next: (data) => {
          this.stats = [
            { label: 'Pending Review', value: data.filter(c => c.status === 'Submitted').length.toString(), detail: 'Awaiting Action' },
            { label: 'Under Review', value: data.filter(c => c.status === 'UnderReview').length.toString(), detail: 'Work in Progress' },
            { label: 'Total Settled', value: data.filter(c => c.status === 'Settled').length.toString(), detail: 'Payouts done' }
          ];
          
          this.recentActivity = data.slice(0, 5).map(c => ({
            title: `Claim for Policy #${c.policyNumber}`,
            subtitle: `Reason: ${c.reason}`,
            date: c.createdAt,
            type: 'claim' as const
          }));
          this.isLoading = false;
          this.cdr.detectChanges();
        },
        error: () => {
          this.errorMessage = "Unable to synchronize authorized claims data.";
          this.isLoading = false;
          this.cdr.detectChanges();
        }
      });
    } else {
      this.isLoading = false;
    }
  }
}
