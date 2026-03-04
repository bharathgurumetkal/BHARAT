import { Component, OnInit, ChangeDetectionStrategy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AdminApiService } from '../../../../core/services/admin-api.service';

@Component({
  selector: 'app-claims-officers-list',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './claims-officers-list.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ClaimsOfficersListComponent implements OnInit {
  officers = signal<any[]>([]);
  isLoading = signal(true);
  showAddModal = signal(false);
  officerForm: FormGroup;
  isSubmitting = signal(false);

  toastMessage = signal<string | null>(null);
  toastType = signal<'success' | 'error'>('success');
  
  modalError = signal<string | null>(null);
  modalSuccess = signal<string | null>(null);

  constructor(
    private adminService: AdminApiService,
    private fb: FormBuilder
  ) {
    this.officerForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      phoneNumber: ['', [Validators.required, Validators.pattern('^[0-9+]{10,15}$')]]
    });
  }

  ngOnInit(): void {
    this.loadOfficers();
  }

  loadOfficers(): void {
    this.isLoading.set(true);
    this.adminService.getClaimsOfficers().subscribe({
      next: (data) => {
        this.officers.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.showToast('Failed to load claims officers.', 'error');
        this.isLoading.set(false);
      }
    });
  }

  openAddModal(): void {
    this.officerForm.reset();
    this.modalError.set(null);
    this.modalSuccess.set(null);
    this.showAddModal.set(true);
  }

  closeModal(): void {
    this.showAddModal.set(false);
  }

  onSubmit(): void {
    if (this.officerForm.invalid) {
      this.officerForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.modalError.set(null);
    this.modalSuccess.set(null);

    this.adminService.addClaimsOfficer(this.officerForm.value).subscribe({
      next: () => {
        this.modalSuccess.set(`✓ Claims Officer account created successfully!`);
        this.isSubmitting.set(false);
        this.loadOfficers();
        setTimeout(() => {
          this.closeModal();
          this.showToast('Claims Officer created successfully!', 'success');
        }, 1800);
      },
      error: (err) => {
        this.modalError.set(err.error?.message || 'Failed to create claims officer. Please try again.');
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
