import { Component, OnInit, signal, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminApiService } from '../../../../core/services/admin-api.service';

@Component({
  selector: 'app-admin-claims',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-claims.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AdminClaimsComponent implements OnInit {
  claims        = signal<any[]>([]);
  officers      = signal<any[]>([]);
  isLoading     = signal(true);
  processingId  = signal<string | null>(null);
  successMsg    = signal<string | null>(null);
  errorMsg      = signal<string | null>(null);

  selectedClaim     = signal<any | null>(null);
  selectedOfficerId = '';

  constructor(
    private adminService: AdminApiService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading.set(true);

    this.adminService.getAdminClaims().subscribe({
      next: (data: any[]) => {
        this.claims.set(data.sort((a, b) =>
          new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
        ));
        this.isLoading.set(false);
        this.cdr.markForCheck();
      },
      error: () => {
        this.isLoading.set(false);
        this.cdr.markForCheck();
      }
    });

    this.adminService.getClaimsOfficers().subscribe({
      next: (data: any[]) => {
        this.officers.set(data);
        this.cdr.markForCheck();
      }
    });
  }

  openAssignModal(claim: any): void {
    this.selectedClaim.set(claim);
    this.selectedOfficerId = claim.assignedOfficerId ?? '';
    this.successMsg.set(null);
    this.errorMsg.set(null);
  }

  closeModal(): void {
    this.selectedClaim.set(null);
    this.selectedOfficerId = '';
  }

  assignOfficer(): void {
    const claim = this.selectedClaim();
    if (!claim || !this.selectedOfficerId) return;

    this.processingId.set(claim.id);
    this.adminService.assignOfficerToClaim(claim.id, this.selectedOfficerId).subscribe({
      next: () => {
        const officerName = this.officers().find((o: any) => o.userId === this.selectedOfficerId)?.user?.name ?? 'Officer';
        this.claims.update(list =>
          list.map(c => c.id === claim.id
            ? { ...c, assignedOfficerId: this.selectedOfficerId, assignedOfficerName: officerName }
            : c
          )
        );
        this.successMsg.set(`Claim assigned to ${officerName} successfully!`);
        this.processingId.set(null);
        this.closeModal();
        this.cdr.markForCheck();
        setTimeout(() => { this.successMsg.set(null); this.cdr.markForCheck(); }, 3000);
      },
      error: (err) => {
        this.errorMsg.set(err.error?.message || 'Failed to assign officer.');
        this.processingId.set(null);
        this.cdr.markForCheck();
      }
    });
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'Submitted':   return 'bg-blue-100 text-blue-700';
      case 'UnderReview': return 'bg-amber-100 text-amber-700';
      case 'Approved':    return 'bg-green-100 text-green-700';
      case 'Rejected':    return 'bg-red-100 text-red-700';
      case 'Settled':     return 'bg-emerald-100 text-emerald-700';
      default:            return 'bg-gray-100 text-gray-600';
    }
  }
}
