import { Component, OnInit, signal, computed, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ClaimsOfficerService } from '../../services/claims-officer.service';
import { Claim } from '../../../../core/models/insurance.models';
import { environment } from '../../../../../environments/environment';

@Component({
  selector: 'app-claims-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './claims-list.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styles: [`
    .custom-scrollbar::-webkit-scrollbar { width: 4px; }
    .custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
    .custom-scrollbar::-webkit-scrollbar-thumb { background: #e2e8f0; border-radius: 10px; }
    .custom-scrollbar::-webkit-scrollbar-thumb:hover { background: #cbd5e1; }
  `]
})
export class ClaimsListComponent implements OnInit {
  allClaims = signal<Claim[]>([]);
  isLoading = signal(true);
  isRefreshing = signal(false);
  processingMap = signal<{ [key: string]: boolean }>({});
  successMessage = signal<string | null>(null);
  errorMessage = signal<string | null>(null);
  selectedClaim = signal<Claim | null>(null);
  activeFilter = signal<'all' | 'Submitted' | 'UnderReview' | 'Approved' | 'Settled' | 'Rejected' | 'high'>('all');

  claims = computed(() => {
    const f = this.activeFilter();
    if (f === 'all')  return this.allClaims();
    if (f === 'high') return this.allClaims().filter(c => (c.aiRiskScore ?? 0) >= 70);
    return this.allClaims().filter(c => c.status === f);
  });

  filterCounts = computed(() => ({
    all:         this.allClaims().length,
    Submitted:   this.allClaims().filter(c => c.status === 'Submitted').length,
    UnderReview: this.allClaims().filter(c => c.status === 'UnderReview').length,
    Approved:    this.allClaims().filter(c => c.status === 'Approved').length,
    Settled:     this.allClaims().filter(c => c.status === 'Settled').length,
    Rejected:    this.allClaims().filter(c => c.status === 'Rejected').length,
    high:        this.allClaims().filter(c => (c.aiRiskScore ?? 0) >= 70).length
  }));

  constructor(
    private claimsService: ClaimsOfficerService
  ) {}

  ngOnInit(): void {
    this.loadClaims();
  }

  loadClaims(): void {
    // Only show full loading if no claims exist
    if (this.allClaims().length === 0) {
      this.isLoading.set(true);
    } else {
      this.isRefreshing.set(true);
    }

    this.claimsService.getClaims().subscribe({
      next: (data: Claim[]) => {
        const sorted = data.sort((a, b) => {
          const aHasScore = a.aiRiskScore != null;
          const bHasScore = b.aiRiskScore != null;
          if (aHasScore && !bHasScore) return -1;
          if (!aHasScore && bHasScore) return 1;
          if (aHasScore && bHasScore) return (b.aiRiskScore! - a.aiRiskScore!);
          return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
        });
        this.allClaims.set(sorted);
        this.isLoading.set(false);
        this.isRefreshing.set(false);
        
        const currentSelected = this.selectedClaim();
        if (currentSelected) {
          const updated = sorted.find(c => c.id === currentSelected.id);
          if (updated) this.selectedClaim.set(updated);
        }
      },
      error: () => {
        this.errorMessage.set('Failed to load claims.');
        this.isLoading.set(false);
        this.isRefreshing.set(false);
      }
    });
  }

  setFilter(f: 'all' | 'Submitted' | 'UnderReview' | 'Approved' | 'Settled' | 'Rejected' | 'high'): void {
    this.activeFilter.set(f);
  }

  getRiskLevel(score: number | null | undefined): string {
    if (score == null) return 'Unknown';
    if (score >= 70) return 'High';
    if (score >= 40) return 'Medium';
    return 'Low';
  }

  getRiskBorderClass(score: number | null | undefined): string {
    const level = this.getRiskLevel(score);
    switch (level) {
      case 'High': return 'border-l-4 border-l-red-500 shadow-[inset_4px_0_0_0_#ef4444]';
      case 'Medium': return 'border-l-4 border-l-amber-500 shadow-[inset_4px_0_0_0_#f59e0b]';
      case 'Low': return 'border-l-4 border-l-emerald-500 shadow-[inset_4px_0_0_0_#10b981]';
      default: return 'border-l-4 border-l-transparent';
    }
  }

  getRiskBadgeClass(score: number | null | undefined): string {
    const level = this.getRiskLevel(score);
    switch (level) {
      case 'High': return 'bg-red-50 text-red-600 border-red-100';
      case 'Medium': return 'bg-amber-50 text-amber-600 border-amber-100';
      case 'Low': return 'bg-emerald-50 text-emerald-600 border-emerald-100';
      default: return 'bg-gray-50 text-gray-600 border-gray-100';
    }
  }

  getRiskLabel(score: number | null | undefined): string {
    const level = this.getRiskLevel(score);
    return level === 'Unknown' ? 'No AI Score' : `${level} Risk`;
  }

  getCountByStatus(status: string): number {
    return this.allClaims().filter(c => c.status === status).length;
  }

  getHighRiskCount(): number {
    return this.allClaims().filter(c => (c.aiRiskScore ?? 0) >= 70).length;
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'Submitted':   return 'bg-gray-100 text-gray-700';
      case 'UnderReview': return 'bg-blue-100 text-blue-700';
      case 'Approved':    return 'bg-green-100 text-green-700';
      case 'Rejected':    return 'bg-red-100 text-red-700';
      case 'Settled':     return 'bg-emerald-100 text-emerald-700';
      default:            return 'bg-gray-100 text-gray-700';
    }
  }

  selectClaim(claim: Claim): void {
    this.selectedClaim.set(claim);
  }

  closeDrawer(): void {
    this.selectedClaim.set(null);
  }

  /** Maps each action to the status it transitions to */
  private readonly nextStatusMap: Record<string, string> = {
    start:   'UnderReview',
    approve: 'Approved',
    reject:  'Rejected',
    settle:  'Settled'
  };

  /** Rollback map in case the API call fails */
  private readonly prevStatusMap: Record<string, string> = {
    start:   'Submitted',
    approve: 'UnderReview',
    reject:  'UnderReview',
    settle:  'Approved'
  };

  process(id: string, action: 'start' | 'approve' | 'reject' | 'settle'): void {
    this.processingMap.update(prev => ({ ...prev, [id]: true }));
    this.successMessage.set(null);
    this.errorMessage.set(null);

    const newStatus = this.nextStatusMap[action];

    // ── Optimistic Update: update BOTH list and drawer immediately ──────────
    this.allClaims.update(list =>
      list.map(c => c.id === id ? { ...c, status: newStatus } : c)
    );
    // Also update the selected claim so the drawer buttons change instantly
    if (this.selectedClaim()?.id === id) {
      this.selectedClaim.update(c => c ? { ...c, status: newStatus } : c);
    }
    // ────────────────────────────────────────────────────────────────────────

    let obs$;
    switch (action) {
      case 'start':   obs$ = this.claimsService.startReview(id);        break;
      case 'approve': obs$ = this.claimsService.reviewClaim(id, true);  break;
      case 'reject':  obs$ = this.claimsService.reviewClaim(id, false); break;
      case 'settle':  obs$ = this.claimsService.settleClaim(id);        break;
    }

    obs$?.subscribe({
      next: (res: any) => {
        this.successMessage.set(typeof res === 'string' ? res : (res.message || 'Action completed.'));
        this.processingMap.update(prev => ({ ...prev, [id]: false }));
        setTimeout(() => { this.successMessage.set(null); }, 3000);
      },
      error: (err) => {
        // ── Rollback both signals if API fails ──────────────────────────────
        const oldStatus = this.prevStatusMap[action];
        this.allClaims.update(list =>
          list.map(c => c.id === id ? { ...c, status: oldStatus } : c)
        );
        if (this.selectedClaim()?.id === id) {
          this.selectedClaim.update(c => c ? { ...c, status: oldStatus } : c);
        }
        // ────────────────────────────────────────────────────────────────────
        this.errorMessage.set(err.error?.message || `Failed to ${action} claim.`);
        this.processingMap.update(prev => ({ ...prev, [id]: false }));
        setTimeout(() => { this.errorMessage.set(null); }, 3000);
      }
    });
  }

  getWorkflowStep(status: string): number {
    switch (status) {
      case 'Submitted':   return 1;
      case 'UnderReview': return 2;
      case 'Approved':    return 3;
      case 'Settled':     return 4;
      case 'Rejected':    return 0;
      default:            return 1;
    }
  }

  getDownloadUrl(filePath: string | null | undefined): string {
    if (!filePath) return '#';
    if (filePath.startsWith('http')) return filePath;
    const normalizedPath = filePath.replace(/\\/g, '/');
    const base = environment.apiUrl.replace('/api', '');
    return `${base}${normalizedPath.startsWith('/') ? '' : '/'}${normalizedPath}`;
  }
}
