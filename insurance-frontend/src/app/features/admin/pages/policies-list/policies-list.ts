import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminApiService } from '../../../../core/services/admin-api.service';

@Component({
  selector: 'app-policies-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './policies-list.html'
})
export class PoliciesListComponent implements OnInit {
  policies = signal<any[]>([]);
  
  isLoading = signal(true);
  errorMessage = signal<string | null>(null);

  searchQuery = signal('');
  statusFilter = signal('');

  // Pagination
  currentPage = signal(1);
  pageSize = signal(10);

  filteredPolicies = computed(() => {
    let filtered = this.policies();
    const query = this.searchQuery().toLowerCase();
    const status = this.statusFilter();
    const page = this.currentPage();
    const size = this.pageSize();

    if (query) {
      filtered = filtered.filter(p => 
        (p.policyNumber && p.policyNumber.toLowerCase().includes(query)) ||
        (p.customerId && p.customerId.toLowerCase().includes(query))
      );
    }

    if (status) {
      filtered = filtered.filter(p => p.status === status);
    }

    const startIndex = (page - 1) * size;
    return filtered.slice(startIndex, startIndex + size);
  });

  totalPages = computed(() => {
    let filtered = this.policies();
    const query = this.searchQuery().toLowerCase();
    const status = this.statusFilter();
    const size = this.pageSize();

    if (query) {
      filtered = filtered.filter(p => 
        (p.policyNumber && p.policyNumber.toLowerCase().includes(query)) ||
        (p.customerId && p.customerId.toLowerCase().includes(query))
      );
    }

    if (status) {
      filtered = filtered.filter(p => p.status === status);
    }

    return Math.ceil(filtered.length / size) || 1;
  });

  pages = computed(() => {
    const total = this.totalPages();
    const p = [];
    for (let i = 1; i <= total; i++) {
      p.push(i);
    }
    return p;
  });

  constructor(private adminService: AdminApiService) {}

  ngOnInit(): void {
    this.loadPolicies();
  }

  loadPolicies(): void {
    this.isLoading.set(true);
    
    import('rxjs').then(({ forkJoin, catchError, of }) => {
      forkJoin({
        policies: this.adminService.getPolicies().pipe(catchError(() => of([]))),
        customers: this.adminService.getCustomers().pipe(catchError(() => of([]))),
        applications: this.adminService.getApplications().pipe(catchError(() => of([])))
      }).subscribe({
        next: (res) => {
          const mapped = res.policies.map(p => {
             const app = res.applications.find((a: any) => a.id === p.applicationId);
             const cus = res.customers.find((c: any) => c.userId === p.customerId);
             return {
                ...p,
                customerName: cus?.user?.name || app?.customerName || 'Unknown',
                customerEmail: cus?.user?.email || 'Unknown',
                productName: app?.productName || 'Unknown'
             };
          });
          this.policies.set(mapped);
          this.isLoading.set(false);
        },
        error: () => {
          this.errorMessage.set("Failed to load policies.");
          this.isLoading.set(false);
        }
      });
    });
  }

  nextPage(): void {
    if (this.currentPage() < this.totalPages()) {
      this.currentPage.update(p => p + 1);
    }
  }

  prevPage(): void {
    if (this.currentPage() > 1) {
      this.currentPage.update(p => p - 1);
    }
  }

  setPage(page: number): void {
    this.currentPage.set(page);
  }

  min(a: number, b: number): number {
    return Math.min(a, b);
  }
}
