import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ClaimsOfficerService } from '../../services/claims-officer.service';

@Component({
  selector: 'app-policies-list',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="space-y-8 animate-in fade-in slide-in-from-bottom-2 duration-500 font-sans">
        <div>
            <h1 class="text-2xl font-semibold text-gray-800 tracking-normal">Policy Repository</h1>
            <p class="text-sm text-gray-500 mt-1">Full registry of insurance contracts across the system.</p>
        </div>

        <!-- Loading State -->
        <div *ngIf="isLoading()" class="flex flex-col items-center justify-center py-20">
            <div class="w-10 h-10 border-4 border-gray-100 border-t-indigo-600 rounded-full animate-spin"></div>
        </div>

        <!-- No Policies Found -->
        <div *ngIf="!isLoading() && policies().length === 0" class="bg-white p-20 rounded-2xl border border-gray-100 text-center shadow-sm">
            <h3 class="text-lg font-semibold text-gray-700">No records found</h3>
            <p class="text-sm text-gray-500 mt-2">There are no active policies in the repository.</p>
        </div>

        <!-- Policies Table -->
        <div *ngIf="!isLoading() && policies().length > 0" class="bg-white rounded-2xl border border-gray-100 shadow-sm overflow-hidden">
            <table class="w-full text-left">
                <thead>
                    <tr class="bg-gray-50 border-b border-gray-100">
                        <th class="px-6 py-4 text-sm font-medium text-gray-600 uppercase tracking-wide">Identity</th>
                        <th class="px-6 py-4 text-sm font-medium text-gray-600 uppercase tracking-wide">Coverage</th>
                        <th class="px-6 py-4 text-sm font-medium text-gray-600 uppercase tracking-wide">Premium</th>
                        <th class="px-6 py-4 text-sm font-medium text-gray-600 uppercase tracking-wide">Status</th>
                    </tr>
                </thead>
                <tbody class="divide-y divide-gray-50">
                    <tr *ngFor="let policy of policies()" class="hover:bg-gray-50 transition-colors">
                        <td class="px-6 py-5">
                            <div class="text-sm font-bold text-gray-800">{{ policy.policyNumber }}</div>
                            <div class="text-[11px] text-gray-400 font-medium">ID: {{ policy.id.substring(0,8) }}</div>
                        </td>
                        <td class="px-6 py-5">
                            <span class="text-sm font-semibold text-gray-700">{{ policy.coverageAmount | currency }}</span>
                        </td>
                        <td class="px-6 py-5 text-sm font-semibold text-gray-700">
                            {{ policy.premium | currency }}
                        </td>
                        <td class="px-6 py-5">
                            <span [class]="getStatusClass(policy.status)" class="px-3 py-1 text-[11px] font-bold rounded-full border">
                                {{ policy.status }}
                            </span>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>
  `,
  styles: [`
    :host { display: block; }
  `]
})
export class OfficerPoliciesListComponent implements OnInit {
  policies = signal<any[]>([]);
  isLoading = signal(true);

  constructor(
    private claimsService: ClaimsOfficerService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadPolicies();
  }

  loadPolicies(): void {
    this.isLoading.set(true);
    this.claimsService.getPolicies().subscribe({
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

  getStatusClass(status: string): string {
    switch (status) {
      case 'Active': return 'bg-emerald-50 text-emerald-600 border-emerald-100';
      case 'Draft': return 'bg-amber-50 text-amber-600 border-amber-100';
      case 'Expired': return 'bg-gray-50 text-gray-600 border-gray-100';
      case 'Cancelled': return 'bg-red-50 text-red-600 border-red-100';
      default: return 'bg-blue-50 text-blue-600 border-blue-100';
    }
  }
}
