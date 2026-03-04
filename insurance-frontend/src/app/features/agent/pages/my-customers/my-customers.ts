import { Component, OnInit, ChangeDetectionStrategy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AgentService } from '../../../../core/services/agent.service';
import { CustomerReport } from '../../../../core/models/insurance.models';

@Component({
  selector: 'app-my-customers',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './my-customers.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MyCustomersComponent implements OnInit {
  customers = signal<CustomerReport[]>([]);
  isLoading = signal(true);
  error = signal<string | null>(null);

  constructor(
    private agentService: AgentService
  ) {}

  ngOnInit(): void {
    this.loadCustomers();
  }

  loadCustomers(): void {
    this.isLoading.set(true);
    this.error.set(null);
    this.agentService.getMyCustomers().subscribe({
      next: (data) => {
        console.log('AGENT_DEBUG: My Customers fetched:', data);
        this.customers.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Customer fetch error:', err);
        this.error.set('Failed to load customers. Please ensure your agent profile is correctly configured.');
        this.isLoading.set(false);
      }
    });
  }
}
