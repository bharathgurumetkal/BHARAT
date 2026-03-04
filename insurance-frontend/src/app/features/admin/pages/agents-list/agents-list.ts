import { Component, OnInit, ChangeDetectionStrategy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AdminApiService } from '../../../../core/services/admin-api.service';

@Component({
  selector: 'app-agents-list',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './agents-list.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AgentsListComponent implements OnInit {
  agents = signal<any[]>([]);
  isLoading = signal(true);
  showAddModal = signal(false);
  agentForm: FormGroup;
  isSubmitting = signal(false);
  
  // Page-level toast (shown after modal closes)
  toastMessage = signal<string | null>(null);
  toastType = signal<'success' | 'error'>('success');
  
  // Modal-level feedback
  modalError = signal<string | null>(null);
  modalSuccess = signal<string | null>(null);

  constructor(
    private adminService: AdminApiService,
    private fb: FormBuilder
  ) {
    this.agentForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      phoneNumber: ['', [Validators.required, Validators.pattern('^[0-9+]{10,15}$')]]
    });
  }

  ngOnInit(): void {
    this.loadAgents();
  }

  loadAgents(): void {
    this.isLoading.set(true);
    this.adminService.getAgents().subscribe({
      next: (data) => {
        this.agents.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.showToast('Failed to load agents.', 'error');
        this.isLoading.set(false);
      }
    });
  }

  openAddModal(): void {
    this.agentForm.reset();
    this.modalError.set(null);
    this.modalSuccess.set(null);
    this.showAddModal.set(true);
  }

  closeModal(): void {
    this.showAddModal.set(false);
  }

  onSubmit(): void {
    if (this.agentForm.invalid) {
      this.agentForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.modalError.set(null);
    this.modalSuccess.set(null);

    this.adminService.addAgent(this.agentForm.value).subscribe({
      next: () => {
        this.modalSuccess.set('✓ Agent account created successfully!');
        this.isSubmitting.set(false);
        // Reload list in background, close modal after user sees success
        this.loadAgents();
        setTimeout(() => {
          this.closeModal();
          this.showToast('Agent created successfully!', 'success');
        }, 1800);
      },
      error: (err) => {
        this.modalError.set(err.error?.message || 'Failed to create agent. Please try again.');
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
