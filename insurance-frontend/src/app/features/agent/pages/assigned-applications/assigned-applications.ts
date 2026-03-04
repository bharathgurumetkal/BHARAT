import { Component, OnInit, ChangeDetectionStrategy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AgentService } from '../../../../core/services/agent.service';
import { PolicyApplication } from '../../../../core/models/insurance.models';

@Component({
  selector: 'app-assigned-applications',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './assigned-applications.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styles: [`
    .custom-scrollbar::-webkit-scrollbar { width: 4px; }
    .custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
    .custom-scrollbar::-webkit-scrollbar-thumb { background: #e2e8f0; border-radius: 10px; }
  `]
})
export class AssignedApplicationsComponent implements OnInit {
  applications = signal<PolicyApplication[]>([]);
  isLoading = signal(true);
  processingId = signal<string | null>(null);
  successMessage = signal<string | null>(null);
  errorMessage = signal<string | null>(null);
  
  selectedApp = signal<PolicyApplication | null>(null);

  constructor(
    private agentService: AgentService
  ) {}

  ngOnInit(): void {
    this.loadApplications();
  }

  loadApplications(): void {
    this.isLoading.set(true);
    this.agentService.getAssignedApplications().subscribe({
      next: (data) => {
        const sorted = data.sort((a, b) => new Date(b.submittedAt).getTime() - new Date(a.submittedAt).getTime());
        this.applications.set(sorted);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  selectApplication(app: PolicyApplication): void {
    this.selectedApp.set(app);
  }

  closeDrawer(): void {
    this.selectedApp.set(null);
  }

  process(id: string, action: 'approve' | 'reject'): void {
    this.processingId.set(id);

    const obs = action === 'approve' 
      ? this.agentService.approveApplication(id)
      : this.agentService.rejectApplication(id);

    obs.subscribe({
      next: (res: any) => {
        // Immediate UI update: remove from local state
        this.applications.update(apps => apps.filter(a => a.id !== id));
        if (this.selectedApp()?.id === id) {
          this.selectedApp.set(null);
        }
        
        this.successMessage.set((typeof res === 'string') ? res : (res.message || "Operation successful."));
        this.processingId.set(null);
        setTimeout(() => {
          this.successMessage.set(null);
        }, 3000);
      },
      error: (err) => {
        // If the error indicates it's already processed, treat it as a ghost record and remove it
        if (err.error?.message?.includes('Current status')) {
           this.applications.update(apps => apps.filter(a => a.id !== id));
           if (this.selectedApp()?.id === id) this.selectedApp.set(null);
        }

        this.errorMessage.set(err.error?.message || "Action failed.");
        this.processingId.set(null);
        setTimeout(() => {
          this.errorMessage.set(null);
        }, 5000);
      }
    });
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'AssignedToAgent': return 'bg-blue-100 text-blue-700';
      case 'ApprovedByAgent': return 'bg-green-100 text-green-700';
      case 'RejectedByAgent': return 'bg-red-100 text-red-700';
      default: return 'bg-gray-100 text-gray-700';
    }
  }
}
