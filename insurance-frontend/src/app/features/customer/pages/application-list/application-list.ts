import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { CustomerApiService } from '../../../../core/services/customer-api.service';
import { PolicyApplication } from '../../../../core/models/insurance.models';

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

  constructor(
    private customerService: CustomerApiService
  ) {}

  ngOnInit(): void {
    this.loadApplications();
  }

  loadApplications(): void {
    this.isLoading.set(true);
    this.customerService.getApplications().subscribe({
      next: (data) => {
        this.applications.set(data.sort((a, b) => new Date(b.submittedAt).getTime() - new Date(a.submittedAt).getTime()));
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  getStatusClass(status: string) {
    switch (status) {
      case 'Submitted': return 'bg-blue-50 text-blue-600 border-blue-100';
      case 'AssignedToAgent': return 'bg-purple-50 text-purple-600 border-purple-100';
      case 'ApprovedByAgent': return 'bg-emerald-50 text-emerald-600 border-emerald-100';
      case 'RejectedByAgent': return 'bg-red-50 text-red-600 border-red-100';
      default: return 'bg-gray-50 text-gray-600 border-gray-100';
    }
  }
}
