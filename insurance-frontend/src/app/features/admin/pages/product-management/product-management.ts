import { Component, OnInit, ChangeDetectionStrategy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AdminApiService } from '../../../../core/services/admin-api.service';

@Component({
  selector: 'app-product-management',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './product-management.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProductManagementComponent implements OnInit {
  products = signal<any[]>([]);
  isLoading = signal(true);

  showAddModal = signal(false);
  productForm: FormGroup;
  isSubmitting = signal(false);

  toastMessage = signal<string | null>(null);
  toastType = signal<'success' | 'error'>('success');

  modalError = signal<string | null>(null);
  modalSuccess = signal<string | null>(null);

  constructor(
    private adminService: AdminApiService,
    private fb: FormBuilder
  ) {
    this.productForm = this.fb.group({
      name: ['', Validators.required],
      description: ['', Validators.required],
      propertyCategory: ['', Validators.required],
      baseRatePercentage: [null, [Validators.required, Validators.min(0.1)]],
      maxCoverageAmount: [null, [Validators.required, Validators.min(1000)]]
    });
  }

  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts(): void {
    this.isLoading.set(true);
    this.adminService.getProducts().subscribe({
      next: (data) => {
        this.products.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.showToast('Failed to load products.', 'error');
        this.isLoading.set(false);
      }
    });
  }

  openAddModal(): void {
    this.productForm.reset();
    this.modalError.set(null);
    this.modalSuccess.set(null);
    this.showAddModal.set(true);
  }

  closeModal(): void {
    this.showAddModal.set(false);
  }

  onSubmit(): void {
    if (this.productForm.invalid) {
      this.productForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.modalError.set(null);
    this.modalSuccess.set(null);

    this.adminService.createProduct(this.productForm.value).subscribe({
      next: () => {
        this.modalSuccess.set(`✓ Product published successfully!`);
        this.isSubmitting.set(false);
        this.loadProducts();
        setTimeout(() => {
          this.closeModal();
          this.showToast('Product created and published!', 'success');
        }, 1800);
      },
      error: (err) => {
        this.modalError.set(err.error?.message || 'Failed to create product. Please try again.');
        this.isSubmitting.set(false);
      }
    });
  }

  showToast(msg: string, type: 'success' | 'error'): void {
    this.toastMessage.set(msg);
    this.toastType.set(type);
    setTimeout(() => {
      this.toastMessage.set(null);
    }, 4000);
  }
}
