import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AgentService } from '../../../../core/services/agent.service';

@Component({
  selector: 'app-smart-prospecting',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './smart-prospecting.html',
  styleUrls: ['./smart-prospecting.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SmartProspectingComponent implements OnInit {
  prospects: any[] = [];
  loading = true;
  errorMessage = '';

  constructor(
    private agentService: AgentService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadProspectingData();
  }

  loadProspectingData(force: boolean = false): void {
    this.loading = true;
    this.cdr.markForCheck();
    
    this.agentService.getSmartProspecting(force).subscribe({
      next: (data) => {
        this.prospects = data;
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        console.error('Failed to load prospecting data', err);
        this.errorMessage = 'Failed to analyze customer data. Please try again later.';
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }

  refreshData(): void {
    this.loadProspectingData(true);
  }

  getScoreColor(score: number): string {
    if (score >= 80) return '#10b981'; // Success Green
    if (score >= 60) return '#3b82f6'; // Info Blue
    if (score >= 40) return '#f59e0b'; // Warning Orange
    return '#ef4444'; // Danger Red
  }

  getLikelihoodClass(likelihood: string): string {
    if (!likelihood) return 'badge-unknown';
    switch (likelihood.toLowerCase()) {
      case 'very high': return 'badge-very-high';
      case 'high': return 'badge-high';
      case 'medium': return 'badge-medium';
      case 'low': return 'badge-low';
      default: return 'badge-unknown';
    }
  }
}
