import { Component, OnInit, ChangeDetectionStrategy, signal, computed } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { AgentService } from '../../../../core/services/agent.service';
import { Commission } from '../../../../core/models/insurance.models';

@Component({
  selector: 'app-agent-commissions',
  standalone: true,
  imports: [CommonModule, CurrencyPipe, DatePipe],
  templateUrl: './commissions.html',
  styleUrl: './commissions.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AgentCommissionsComponent implements OnInit {
  commissions = signal<Commission[]>([]);
  isLoading = signal(true);

  totalEarned = computed(() => 
    this.commissions().reduce((sum, c) => sum + c.commissionAmount, 0)
  );

  paidAmount = computed(() => 
    this.commissions().filter(c => c.isPaid).reduce((sum, c) => sum + c.commissionAmount, 0)
  );

  pendingAmount = computed(() => 
    this.commissions().filter(c => !c.isPaid).reduce((sum, c) => sum + c.commissionAmount, 0)
  );

  paidCount = computed(() => 
    this.commissions().filter(c => c.isPaid).length
  );

  pendingCount = computed(() => 
    this.commissions().filter(c => !c.isPaid).length
  );

  constructor(
    private agentService: AgentService
  ) {}

  ngOnInit(): void {
    this.loadCommissions();
  }

  loadCommissions(): void {
    this.isLoading.set(true);

    this.agentService.getCommissions().subscribe({
      next: (data) => {
        // Sort newest first (API already does this, but defensive)
        const sorted = data.sort(
          (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
        );
        this.commissions.set(sorted);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }
}
