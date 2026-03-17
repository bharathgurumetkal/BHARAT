import { Component, OnInit, signal, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { CustomerApiService } from '../../../../core/services/customer-api.service';
import { Policy } from '../../../../core/models/insurance.models';

@Component({
  selector: 'app-policy-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './policy-list.html',
  styleUrl: './policy-list.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PolicyListComponent implements OnInit {
  policies = signal<Policy[]>([]);
  isLoading = signal(true);
  isPaying = signal(false);
  isRenewing = signal<string | null>(null);

  showSuccessModal = signal(false);
  showRenewalModal = signal(false);
  successMessage = signal<string | null>(null);
  errorMessage = signal<string | null>(null);
  isRenewalSuccess = signal(false);
  renewingPolicyId = signal<string | null>(null);

  /** Cached today at midnight — stable for the entire day, prevents NG0100. */
  private readonly todayMidnight = (() => {
    const d = new Date();
    d.setHours(0, 0, 0, 0);
    return d.getTime();
  })();

  constructor(
    private customerService: CustomerApiService,
    private cdr: ChangeDetectorRef
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
        this.cdr.markForCheck();
      },
      error: () => {
        this.isLoading.set(false);
        this.cdr.markForCheck();
      }
    });
  }

  getPolicyById(id: string | null): Policy | undefined {
    if (!id) return undefined;
    return this.policies().find(p => p.id === id);
  }

  // ─── Payment ────────────────────────────────────────────────
  payPremium(policyId: string): void {
    if (this.isPaying()) return;
    this.isPaying.set(true);
    this.successMessage.set(null);
    this.errorMessage.set(null);

    this.customerService.payPremium(policyId).subscribe({
      next: () => {
        this.successMessage.set('Payment successful! Your policy is now active.');
        this.isPaying.set(false);
        this.isRenewalSuccess.set(false);
        this.showSuccessModal.set(true);
        this.loadPolicies();
      },
      error: (err) => {
        this.errorMessage.set(err.error?.message || 'Payment failed. Please try again.');
        this.isPaying.set(false);
      }
    });
  }

  // ─── Renewal (Modal flow) ────────────────────────────────────
  openRenewalModal(policyId: string): void {
    this.renewingPolicyId.set(policyId);
    this.showRenewalModal.set(true);
  }

  closeRenewalModal(): void {
    this.showRenewalModal.set(false);
    this.renewingPolicyId.set(null);
  }

  confirmRenewal(): void {
    const policyId = this.renewingPolicyId();
    if (!policyId) return;

    this.isRenewing.set(policyId);
    this.customerService.renewPolicy(policyId).subscribe({
      next: () => {
        this.isRenewing.set(null);
        this.showRenewalModal.set(false);
        this.isRenewalSuccess.set(true);
        this.successMessage.set('Policy renewed successfully for the next period.');
        this.showSuccessModal.set(true);
        this.renewingPolicyId.set(null);
        this.loadPolicies();
      },
      error: (err) => {
        this.errorMessage.set(err.error?.message || 'Renewal failed. Please try again.');
        this.isRenewing.set(null);
        this.showRenewalModal.set(false);
      }
    });
  }

  closeModal(): void {
    this.showSuccessModal.set(false);
    this.successMessage.set(null);
    this.errorMessage.set(null);
    this.isRenewalSuccess.set(false);
  }

  // ─── Status helpers ──────────────────────────────────────────
  getStatusClass(status: string): string {
    switch (status) {
      case 'Active':    return 'bg-emerald-50 text-emerald-600 border-emerald-100';
      case 'Draft':     return 'bg-blue-50 text-blue-600 border-blue-100';
      case 'Expired':   return 'bg-gray-100 text-gray-500 border-gray-200';
      case 'Cancelled': return 'bg-red-50 text-red-600 border-red-100';
      default:          return 'bg-gray-50 text-gray-600 border-gray-100';
    }
  }

  // ─── Date / expiry helpers ───────────────────────────────────
  getRemainingDays(endDate: string | null): number {
    if (!endDate) return 0;
    // Use todayMidnight so consecutive CD calls get the same integer
    const diffMs = new Date(endDate).getTime() - this.todayMidnight;
    return Math.ceil(diffMs / (1000 * 60 * 60 * 24));
  }

  isExpired(endDate: string | null): boolean {
    if (!endDate) return false;
    return new Date(endDate) < new Date();
  }

  isEligibleForRenewal(policy: Policy): boolean {
    if (policy.status === 'Cancelled' || policy.status === 'Draft') return false;
    if (policy.status === 'Expired') return true;
    return policy.endDate ? this.getRemainingDays(policy.endDate) <= 15 : false;
  }

  /**
   * Returns percentage of the policy duration that has ELAPSED (so bar fills left→right as time passes).
   * When fresh, bar is nearly full; when expiring, bar is nearly empty — classic countdown bar.
   */
  getExpiryPercent(policy: Policy): number {
    if (!policy.startDate || !policy.endDate) return 0;
    const total = new Date(policy.endDate).getTime() - new Date(policy.startDate).getTime();
    // Use stable todayMidnight — prevents NG0100 floating-point drift between CD runs
    const remaining = new Date(policy.endDate).getTime() - this.todayMidnight;
    if (total <= 0) return 0;
    // Round to 2 decimal places so both CD passes return the exact same value
    const pct = Math.round((remaining / total) * 10000) / 100;
    return Math.max(0, Math.min(100, pct));
  }

  // ─── Documents ───────────────────────────────────────────────
  downloadPolicyDocument(policyId: string, type: string): void {
    this.customerService.downloadDocument(policyId, type).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `Policy_${type}_${policyId.substring(0, 8)}.pdf`;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        a.remove();
      },
      error: () => this.errorMessage.set('Failed to download document. Please try again.')
    });
  }
}
