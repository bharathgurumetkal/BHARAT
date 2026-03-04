import { Component, OnInit, ChangeDetectionStrategy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AgentService } from '../../../../core/services/agent.service';
import { Policy } from '../../../../core/models/insurance.models';

@Component({
  selector: 'app-agent-policies',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './policies.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AgentPoliciesComponent implements OnInit {
  policies = signal<Policy[]>([]);
  isLoading = signal(true);
  statusFilter = signal('');

  constructor(
    private agentService: AgentService
  ) {}

  ngOnInit(): void {
    this.loadPolicies();
  }

  loadPolicies(): void {
    this.isLoading.set(true);
    this.agentService.getPolicies(this.statusFilter() || undefined).subscribe({
      next: (data) => {
        console.log('AGENT_DEBUG: All Policies fetched:', data);
        this.policies.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  onFilterChange(): void {
    this.loadPolicies();
  }

  /** Maps numeric or string PolicyStatus to a human-readable label */
  getStatusLabel(status: string | number): string {
    const numericMap: Record<number, string> = {
      0: 'Draft',
      1: 'Awaiting Payment',
      2: 'Active',
      3: 'Expired',
      4: 'Cancelled'
    };
    if (typeof status === 'number') return numericMap[status] ?? String(status);
    // Handle numeric string e.g. "2"
    const n = Number(status);
    if (!isNaN(n) && numericMap[n]) return numericMap[n];
    // Already a string label — normalise "AwaitingPayment" → "Awaiting Payment"
    return status.replace(/([A-Z])/g, ' $1').trim();
  }

  getStatusClass(status: string | number): string {
    const label = this.getStatusLabel(status).toLowerCase();
    if (label === 'active')            return 'bg-green-100 text-green-700 border-green-200';
    if (label === 'expired')           return 'bg-gray-100 text-gray-500 border-gray-200';
    if (label === 'cancelled')         return 'bg-red-100 text-red-600 border-red-200';
    if (label === 'draft')             return 'bg-amber-100 text-amber-700 border-amber-200';
    if (label === 'awaiting payment')  return 'bg-blue-100 text-blue-700 border-blue-200';
    return 'bg-gray-100 text-gray-500 border-gray-200';
  }
}
