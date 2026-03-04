import { Component, OnInit, ChangeDetectionStrategy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ClaimsOfficerApiService } from '../../../../core/services/claims-officer-api.service';
import { CustomerApiService } from '../../../../core/services/customer-api.service';
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
  claims = signal<Claim[]>([]);
  isLoading = signal(true);
  processingId = signal<string | null>(null);
  successMessage = signal<string | null>(null);
  errorMessage = signal<string | null>(null);

  constructor(
    private officerService: ClaimsOfficerApiService,
    private customerService: CustomerApiService
  ) {}

  ngOnInit(): void {
    this.loadAllClaims();
  }

  loadAllClaims(): void {
    this.isLoading.set(true);
    this.officerService.getClaims().subscribe({
      next: (data) => {
        this.claims.set(data.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()));
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  process(id: string, action: 'start' | 'approve' | 'reject' | 'settle'): void {
    this.processingId.set(id);
    this.successMessage.set(null);
    this.errorMessage.set(null);

    let obs;
    switch (action) {
      case 'start': obs = this.officerService.startReview(id); break;
      case 'approve': obs = this.officerService.reviewClaim(id, true); break;
      case 'reject': obs = this.officerService.reviewClaim(id, false); break;
      case 'settle': obs = this.officerService.settleClaim(id); break;
    }

    obs?.subscribe({
      next: (res: any) => {
        this.successMessage.set(`Claim updated: ${action.toUpperCase()}`);
        this.loadAllClaims();
        this.processingId.set(null);
        setTimeout(() => this.successMessage.set(null), 3000);
      },
      error: (err) => {
        this.errorMessage.set(err.error?.message || `Failed to ${action} claim.`);
        this.processingId.set(null);
      }
    });
  }

  getStatusClass(status: string) {
    switch (status) {
      case 'Submitted': return 'bg-blue-50 text-blue-600 border-blue-100';
      case 'UnderReview': return 'bg-amber-50 text-amber-600 border-amber-100';
      case 'Approved': return 'bg-emerald-50 text-emerald-600 border-emerald-100';
      case 'Rejected': return 'bg-red-50 text-red-600 border-red-100';
      case 'Settled': return 'bg-emerald-600 text-white border-emerald-600';
      default: return 'bg-gray-50 text-gray-600 border-gray-100';
    }
  }

  getDownloadUrl(filePath: string): string {
    const base = environment.apiUrl.replace('/api', '');
    if (filePath.startsWith('http')) return filePath;
    return base + (filePath.startsWith('/') ? '' : '/') + filePath;
  }
}
