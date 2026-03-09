import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ClaimsOfficerApiService } from '../../../../core/services/claims-officer-api.service';
import { ClaimsOfficerService } from '../../services/claims-officer.service';
import { Claim } from '../../../../core/models/insurance.models';
import { environment } from '../../../../../environments/environment';

@Component({
  selector: 'app-manage-claims',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './manage-claims.html',
  styleUrl: './manage-claims.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ManageClaimsComponent implements OnInit {
  claims         = signal<Claim[]>([]);
  isLoading      = signal(true);
  processingId   = signal<string | null>(null);
  successMessage = signal<string | null>(null);
  errorMessage   = signal<string | null>(null);

  /** Maps each action to the status the claim immediately moves to */
  private readonly nextStatus: Record<string, string> = {
    start:   'UnderReview',
    approve: 'Approved',
    reject:  'Rejected',
    settle:  'Settled'
  };

  /** For rollback if the API call fails */
  private readonly prevStatus: Record<string, string> = {
    start:   'Submitted',
    approve: 'UnderReview',
    reject:  'UnderReview',
    settle:  'Approved'
  };

  constructor(
    private officerApiService: ClaimsOfficerApiService,
    private officerService: ClaimsOfficerService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadAllClaims();
  }

  loadAllClaims(): void {
    this.isLoading.set(true);
    this.officerApiService.getClaims().subscribe({
      next: (data) => {
        this.claims.set(
          data.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
        );
        this.isLoading.set(false);
        this.cdr.markForCheck();
      },
      error: () => {
        this.isLoading.set(false);
        this.cdr.markForCheck();
      }
    });
  }

  process(id: string, action: 'start' | 'approve' | 'reject' | 'settle'): void {
    this.processingId.set(id);
    this.successMessage.set(null);
    this.errorMessage.set(null);

    // ── Optimistic Update: update status in UI instantly ──────────────────
    this.claims.update(list =>
      list.map(c => c.id === id ? { ...c, status: this.nextStatus[action] } : c)
    );
    this.cdr.markForCheck();
    // ──────────────────────────────────────────────────────────────────────

    let obs$;
    switch (action) {
      case 'start':   obs$ = this.officerApiService.startReview(id);        break;
      case 'approve': obs$ = this.officerApiService.reviewClaim(id, true);  break;
      case 'reject':  obs$ = this.officerApiService.reviewClaim(id, false); break;
      case 'settle':  obs$ = this.officerApiService.settleClaim(id);        break;
    }

    obs$?.subscribe({
      next: () => {
        this.processingId.set(null);
        this.successMessage.set(`Claim ${action.toUpperCase()} successful`);
        // Bust shared cache so dashboard + other components also see fresh data
        this.officerService.invalidateClaimsCache();
        // Silent background refresh to reconcile with server truth
        this.officerApiService.getClaims().subscribe({
          next: (data) => {
            this.claims.set(
              data.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
            );
            this.cdr.markForCheck();
          }
        });
        this.cdr.markForCheck();
        setTimeout(() => { this.successMessage.set(null); this.cdr.markForCheck(); }, 3000);
      },
      error: (err) => {
        // ── Rollback if API fails ─────────────────────────────────────────
        this.claims.update(list =>
          list.map(c => c.id === id ? { ...c, status: this.prevStatus[action] } : c)
        );
        this.errorMessage.set(err.error?.message || `Failed to ${action} claim.`);
        this.processingId.set(null);
        this.cdr.markForCheck();
      }
    });
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'Submitted':   return 'bg-blue-50 text-blue-600 border-blue-100';
      case 'UnderReview': return 'bg-amber-50 text-amber-600 border-amber-100';
      case 'Approved':    return 'bg-emerald-50 text-emerald-600 border-emerald-100';
      case 'Rejected':    return 'bg-red-50 text-red-600 border-red-100';
      case 'Settled':     return 'bg-emerald-600 text-white border-emerald-600';
      default:            return 'bg-gray-50 text-gray-600 border-gray-100';
    }
  }

  getDownloadUrl(filePath: string): string {
    const base = environment.apiUrl.replace('/api', '');
    if (filePath.startsWith('http')) return filePath;
    return base + (filePath.startsWith('/') ? '' : '/') + filePath;
  }
}
