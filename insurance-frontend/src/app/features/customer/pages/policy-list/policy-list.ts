import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { CustomerApiService } from '../../../../core/services/customer-api.service';
import { Policy } from '../../../../core/models/insurance.models';

@Component({
  selector: 'app-policy-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './policy-list.html',
  styleUrl: './policy-list.css'
})
export class PolicyListComponent implements OnInit {
  policies = signal<Policy[]>([]);
  isLoading = signal(true);
  isPaying = signal(false);
  isRenewing = signal<string | null>(null);
  showSuccessModal = signal(false);
  successMessage = signal<string | null>(null);
  errorMessage = signal<string | null>(null);

  constructor(
    private customerService: CustomerApiService
  ) {}

  ngOnInit(): void {
    this.loadPolicies();
  }

  loadPolicies(): void {
    this.isLoading.set(true);
    this.customerService.getPolicies().subscribe({
      next: (data) => {
        this.policies.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  payPremium(policyId: string): void {
    if (this.isPaying()) return;

    this.isPaying.set(true);
    this.successMessage.set(null);
    this.errorMessage.set(null);

    this.customerService.payPremium(policyId).subscribe({
      next: (res) => {
        this.successMessage.set("Payment successful! Your policy is now active.");
        this.isPaying.set(false);
        this.showSuccessModal.set(true);
        this.loadPolicies();
      },
      error: (err) => {
        this.errorMessage.set(err.error?.message || "Payment failed.");
        this.isPaying.set(false);
      }
    });
  }

  renewPolicy(policyId: string): void {
    if (this.isRenewing() === policyId) return;

    if (!confirm("Are you sure you want to renew this policy?")) return;

    this.isRenewing.set(policyId);
    this.successMessage.set(null);
    this.errorMessage.set(null);

    this.customerService.renewPolicy(policyId).subscribe({
      next: (res) => {
        this.successMessage.set("Policy renewed successfully for the next period.");
        this.isRenewing.set(null);
        this.showSuccessModal.set(true);
        this.loadPolicies();
      },
      error: (err) => {
        this.errorMessage.set(err.error?.message || "Renewal failed.");
        this.isRenewing.set(null);
      }
    });
  }

  closeModal(): void {
    this.showSuccessModal.set(false);
    this.successMessage.set(null);
    this.errorMessage.set(null);
  }

  getStatusClass(status: string) {
    switch (status) {
      case 'Active': return 'bg-emerald-50 text-emerald-600 border-emerald-100';
      case 'Draft': return 'bg-blue-50 text-blue-600 border-blue-100';
      case 'Expired': return 'bg-gray-100 text-gray-500 border-gray-200';
      case 'Cancelled': return 'bg-red-50 text-red-600 border-red-100';
      default: return 'bg-gray-50 text-gray-600 border-gray-100';
    }
  }

  getRemainingDays(endDate: string | null): number {
    if (!endDate) return 0;
    const end = new Date(endDate);
    const today = new Date();
    const diffTime = end.getTime() - today.getTime();
    return Math.ceil(diffTime / (1000 * 60 * 60 * 24));
  }

  isExpired(endDate: string | null): boolean {
    if (!endDate) return false;
    return new Date(endDate) < new Date();
  }

  isEligibleForRenewal(policy: Policy): boolean {
    if (policy.status === 'Cancelled' || policy.status === 'Draft') return false;
    if (policy.status === 'Expired') return true;
    if (policy.endDate) {
      const daysLeft = this.getRemainingDays(policy.endDate);
      return daysLeft <= 15;
    }
    return false;
  }

  downloadPolicyDocument(policyId: string, type: string) {
    this.customerService.downloadDocument(policyId, type).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `${type}_${policyId.substring(0, 8)}.pdf`;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        a.remove();
      },
      error: (err) => {
        this.errorMessage.set("Failed to download document.");
      }
    });
  }
}
