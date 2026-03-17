import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { CustomerApiService } from '../../../../core/services/customer-api.service';
import { PolicyApplication } from '../../../../core/models/insurance.models';

type AppStatus = 'Submitted' | 'AssignedToAgent' | 'ApprovedByAgent' | 'RejectedByAgent' | string;

// Step index: 1 → Submitted, 2 → Under Review, 3 → Decision
const STEP_ORDER: Record<string, number> = {
  'Submitted': 1,
  'AssignedToAgent': 2,
  'ApprovedByAgent': 3,
  'RejectedByAgent': 3
};

@Component({
  selector: 'app-application-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './application-list.html',
  styleUrl: './application-list.css'
})
export class ApplicationListComponent implements OnInit {
  applications = signal<PolicyApplication[]>([]);
  isLoading = signal(true);

  constructor(private customerService: CustomerApiService) {}

  ngOnInit(): void {
    this.loadApplications();
  }

  loadApplications(): void {
    this.isLoading.set(true);
    this.customerService.getApplications().subscribe({
      next: (data) => {
        this.applications.set(data.sort((a, b) =>
          new Date(b.submittedAt).getTime() - new Date(a.submittedAt).getTime()
        ));
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  /** Status badge CSS classes */
  getStatusClass(status: AppStatus): string {
    switch (status) {
      case 'Submitted':       return 'bg-blue-50 text-blue-600 border-blue-100';
      case 'AssignedToAgent': return 'bg-purple-50 text-purple-600 border-purple-100';
      case 'ApprovedByAgent': return 'bg-emerald-50 text-emerald-600 border-emerald-100';
      case 'RejectedByAgent': return 'bg-red-50 text-red-500 border-red-100';
      default:                return 'bg-gray-50 text-gray-600 border-gray-100';
    }
  }

  /** Human-readable status labels */
  getStatusLabel(status: AppStatus): string {
    switch (status) {
      case 'Submitted':       return 'Pending Review';
      case 'AssignedToAgent': return 'Under Agent Review';
      case 'ApprovedByAgent': return 'Approved ✓';
      case 'RejectedByAgent': return 'Rejected';
      default:                return status;
    }
  }

  private currentStep(status: AppStatus): number {
    return STEP_ORDER[status] ?? 1;
  }

  /** Whether a step has been completed */
  isStepDone(status: AppStatus, step: number): boolean {
    if (status === 'RejectedByAgent' && step === 3) return false; // show X not ✓
    return this.currentStep(status) > step ||
      (status === 'ApprovedByAgent' && step === 3);
  }

  /** Whether a step is the currently active step */
  isStepActive(status: AppStatus, step: number): boolean {
    return this.currentStep(status) === step;
  }

  /** Tailwind class for each step circle */
  getStepClass(status: AppStatus, step: number): string {
    const cur = this.currentStep(status);

    if (step === 3) {
      if (status === 'ApprovedByAgent') return 'bg-emerald-500 text-white';
      if (status === 'RejectedByAgent') return 'bg-red-400 text-white';
    }

    if (this.isStepDone(status, step)) return 'bg-indigo-500 text-white';
    if (cur === step) return 'bg-indigo-100 text-indigo-700 ring-2 ring-indigo-400';
    return 'bg-gray-100 text-gray-400';
  }
}
