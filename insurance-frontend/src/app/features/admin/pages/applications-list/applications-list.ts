import { Component, OnInit, ChangeDetectionStrategy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminApiService } from '../../../../core/services/admin-api.service';

@Component({
  selector: 'app-applications-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './applications-list.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ApplicationsListComponent implements OnInit {
  applications = signal<any[]>([]);
  agents = signal<any[]>([]);
  
  isLoading = signal(true);
  statusFilter = signal('');

  showAssignModal = signal(false);
  selectedApp = signal<any>(null);
  selectedAgentId = signal('');
  isAssigning = signal(false);

  successMessage = signal<string | null>(null);
  errorMessage = signal<string | null>(null);
  
  toastMessage = signal<string | null>(null);

  showDrawer = signal(false);
  drawerApp = signal<any>(null);

  filteredApps = computed(() => {
    const apps = this.applications();
    const filter = this.statusFilter();
    if (!filter) return apps;
    return apps.filter(a => a.status === filter);
  });

  constructor(
    private adminService: AdminApiService
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading.set(true);
    
    import('rxjs').then(({ forkJoin, catchError, of }) => {
      forkJoin({
        applications: this.adminService.getApplications().pipe(catchError(() => of([]))),
        agents: this.adminService.getAgents().pipe(catchError(() => of([]))),
        customers: this.adminService.getCustomers().pipe(catchError(() => of([])))
      }).subscribe({
        next: (res) => {
          this.agents.set(res.agents);
          const mappedApps = res.applications.map(app => {
            const cus: any = res.customers.find((c: any) => c.userId === app.customerId);
            return {
              ...app,
              customerEmail: cus?.user?.email || 'Unknown'
            };
          });
          this.applications.set(mappedApps);
          this.isLoading.set(false);
        },
        error: () => {
          this.errorMessage.set("Failed to load applications.");
          this.isLoading.set(false);
        }
      });
    });
  }

  openDrawer(app: any): void {
    this.drawerApp.set(app);
    this.showDrawer.set(true);
  }

  closeDrawer(): void {
    this.showDrawer.set(false);
    setTimeout(() => {
        this.drawerApp.set(null);
    }, 300); // Wait for transition
  }

  openAssignModal(app: any): void {
    this.selectedApp.set(app);
    this.selectedAgentId.set(app.assignedAgentId || '');
    this.showAssignModal.set(true);
    this.successMessage.set(null);
    this.errorMessage.set(null);
  }

  closeModal(): void {
    this.showAssignModal.set(false);
    this.selectedApp.set(null);
    this.selectedAgentId.set('');
  }

  confirmAssignment(): void {
    const app = this.selectedApp();
    const agentId = this.selectedAgentId();
    if (!app || !agentId) return;

    this.isAssigning.set(true);
    this.errorMessage.set(null);
    
    this.adminService.assignAgentToApplication(app.id, agentId).subscribe({
      next: () => {
        this.successMessage.set("Agent assigned successfully!");
        this.isAssigning.set(false);
        this.loadData();
        setTimeout(() => {
            this.closeModal();
            this.toastMessage.set('Agent assigned successfully!');
            setTimeout(() => {
                this.toastMessage.set(null);
            }, 3000);
        }, 1500);
      },
      error: (err) => {
        this.errorMessage.set(err.error?.message || "Failed to assign agent.");
        this.isAssigning.set(false);
      }
    });
  }
}

