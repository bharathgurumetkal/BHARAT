import { Component, OnInit, signal, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CustomerApiService } from '../../../../core/services/customer-api.service';
import { CloudinaryUploadService } from '../../../../core/services/cloudinary-upload.service';
import { Claim, Policy } from '../../../../core/models/insurance.models';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { firstValueFrom } from 'rxjs';

import { environment } from '../../../../../environments/environment';

@Component({
  selector: 'app-claim-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './claim-list.html',
  styleUrl: './claim-list.css'
})
export class ClaimListComponent implements OnInit {
  claims = signal<Claim[]>([]);
  allPolicies = signal<Policy[]>([]); 
  selectedPolicy = signal<Policy | null>(null);
  claimForm: FormGroup;
  
  isLoading = signal(true);
  isSubmitting = signal(false);
  showForm = signal(false);
  showConfirmDialog = signal(false); 
  showDetailsModal = signal(false); 
  selectedClaim = signal<Claim | null>(null); 
  selectedFiles = signal<File[]>([]); 
  successMessage = signal<string | null>(null);
  errorMessage = signal<string | null>(null);
  uploadProgress = signal<string | null>(null);

  currentClaimAmount = signal(0);

  /**
   * Calculates the available coverage for the selected policy
   * by subtracting all non-rejected existing claims.
   */
  availableCoverage = computed(() => {
    const policy = this.selectedPolicy();
    if (!policy) return 0;
    
    const utilized = this.claims()
      .filter(c => c.policyId === policy.id && c.status !== 'Rejected')
      .reduce((sum, c) => sum + (c.claimAmount || 0), 0);
      
    return Math.max(0, policy.coverageAmount - utilized);
  });

  isHighClaim = computed(() => {
    const policy = this.selectedPolicy();
    if (!policy) return false;
    const amount = this.currentClaimAmount();
    // High claim is > 80% of the total policy coverage
    return amount > (policy.coverageAmount * 0.8);
  });

  remainingCoverage = computed(() => {
    const available = this.availableCoverage();
    const current = this.currentClaimAmount();
    // Allow negative value here so the UI can show it in red if exceeded
    return available - current;
  });

  constructor(
    private customerService: CustomerApiService,
    private cloudinaryService: CloudinaryUploadService,
    private fb: FormBuilder
  ) {
    this.claimForm = this.fb.group({
      policyId: ['', Validators.required],
      claimAmount: [0, [Validators.required, Validators.min(1)]],
      reason: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(500)]]
    });

    // Watch for policy selection changes
    this.claimForm.get('policyId')?.valueChanges.subscribe(id => {
      this.selectedPolicy.set(this.allPolicies().find(p => p.id === id) || null);
      // Validator update is now handled by an effect reacting to selectedPolicy and claims
    });

    // Sync form value to signal for reactivity in computed properties
    this.claimForm.get('claimAmount')?.valueChanges.subscribe(val => {
      this.currentClaimAmount.set(val || 0);
    });

    // effect to handle validator updates reactively
    effect(() => {
      this.updateValidators();
    });
  }

  ngOnInit(): void {
    this.loadClaims();
    this.loadPolicies();
  }

  private updateValidators(): void {
    const policy = this.selectedPolicy();
    const available = this.availableCoverage();
    const amountControl = this.claimForm.get('claimAmount');

    if (amountControl) {
      if (policy) {
        amountControl.setValidators([
          Validators.required, 
          Validators.min(1), 
          Validators.max(available)
        ]);
      } else {
        amountControl.setValidators([Validators.required, Validators.min(1)]);
      }
      amountControl.updateValueAndValidity({ emitEvent: false });
    }
  }

  loadClaims(): void {
    this.isLoading.set(true);
    this.customerService.getClaims().subscribe({
      next: (data) => {
        this.claims.set(data.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()));
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  loadPolicies(): void {
    this.customerService.getPolicies().subscribe({
      next: (data) => {
        this.allPolicies.set(data);
      }
    });
  }



  initiateSubmit(): void {
    if (this.claimForm.invalid || this.selectedPolicy()?.status !== 'Active') return;
    this.showConfirmDialog.set(true);
  }

  onFileSelected(event: any): void {
    const files: FileList = event.target.files;
    if (files.length === 0) return;

    const newFiles = Array.from(files);
    
    // Validate total count
    if (this.selectedFiles().length + newFiles.length > 5) {
      this.errorMessage.set("Maximum 5 files allowed.");
      return;
    }

    // Check sizes
    const maxSize = 5 * 1024 * 1024; // 5MB
    const updatedFiles = [...this.selectedFiles()];
    for (const file of newFiles) {
      if (file.size > maxSize) {
        this.errorMessage.set(`File ${file.name} exceeds 5MB limit.`);
        return;
      }
      updatedFiles.push(file);
    }
    
    this.selectedFiles.set(updatedFiles);
    this.errorMessage.set(null); // Clear if valid
  }

  removeFile(index: number): void {
    this.selectedFiles.update(files => {
      const updated = [...files];
      updated.splice(index, 1);
      return updated;
    });
  }

  async confirmSubmit(): Promise<void> {
    this.showConfirmDialog.set(false);
    this.isSubmitting.set(true);
    this.successMessage.set(null);
    this.errorMessage.set(null);
    this.uploadProgress.set("Uploading documents...");

    try {
      const formData = new FormData();
      formData.append('policyId', this.claimForm.get('policyId')?.value);
      formData.append('claimAmount', this.claimForm.get('claimAmount')?.value);
      formData.append('reason', this.claimForm.get('reason')?.value);

      // Append files
      if (this.selectedFiles().length > 0) {
        for (const file of this.selectedFiles()) {
          formData.append('files', file);
        }
      }

      // 3. Submit to backend using await for consistent error handling
      const res = await firstValueFrom(this.customerService.submitClaim(formData));
      
      this.successMessage.set("Claim submitted successfully! Status: Under Review.");
      this.isSubmitting.set(false);
      this.uploadProgress.set(null);
      this.selectedFiles.set([]); // Clear files
      this.loadClaims();

    } catch (err: any) {
      console.error('Claim submission error details:', err);
      // Use the actual message from the interceptor if available
      this.errorMessage.set(err.message || "Failed to submit claim. Ensure the amount is valid.");
      this.isSubmitting.set(false);
      this.uploadProgress.set(null);
    }
  }

  resetForm(): void {
    this.showForm.set(false);
    this.showConfirmDialog.set(false);
    this.successMessage.set(null);
    this.errorMessage.set(null);
    this.selectedPolicy.set(null);
    this.selectedFiles.set([]);
    this.uploadProgress.set(null);
    this.claimForm.reset({
        policyId: '',
        claimAmount: 0,
        reason: ''
    });
  }

  viewDetails(claim: Claim): void {
    console.log('Viewing claim details. Documents found:', claim.documents);
    this.selectedClaim.set(claim);
    this.showDetailsModal.set(true);
  }

  closeDetailsModal(): void {
    this.showDetailsModal.set(false);
    this.selectedClaim.set(null);
  }

  getStatusClass(status: string) {
    switch (status) {
      case 'Submitted': return 'bg-gray-100 text-gray-600 border-gray-200';
      case 'UnderReview': return 'bg-amber-100 text-amber-700 border-amber-200';
      case 'Approved': return 'bg-emerald-100 text-emerald-700 border-emerald-200';
      case 'Rejected': return 'bg-red-100 text-red-700 border-red-200';
      case 'Settled': return 'bg-emerald-800 text-white border-emerald-900';
      case 'Active': return 'bg-emerald-50 text-emerald-600 border-emerald-100';
      case 'Draft': return 'bg-blue-50 text-blue-600 border-blue-100';
      default: return 'bg-gray-50 text-gray-600 border-gray-100';
    }
  }

  getDownloadUrl(filePath: string): string {
    const base = environment.apiUrl.replace('/api', '');
    if (filePath.startsWith('http')) return filePath;
    return base + (filePath.startsWith('/') ? '' : '/') + filePath;
  }

  getFormattedPolicyLabel(p: Policy): string {
    const end = p.endDate ? new Date(p.endDate).toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' }) : 'N/A';
    return `${p.policyNumber} | ${p.productName} | Coverage: ₹${p.coverageAmount.toLocaleString()} | Valid till: ${end}`;
  }

  isPolicyDisabled(p: Policy): boolean {
    if (p.status !== 'Active') return true;
    if (!p.endDate) return false;
    return new Date(p.endDate) < new Date();
  }

  getRiskLevelClass(level: string | undefined) {
    switch (level) {
      case 'Low': return 'bg-emerald-50 text-emerald-600 border-emerald-100';
      case 'Medium': return 'bg-amber-50 text-amber-600 border-amber-100';
      case 'High': return 'bg-red-50 text-red-600 border-red-100';
      default: return 'bg-gray-50 text-gray-400 border-gray-100';
    }
  }

  getTimelineSteps(claim: Claim) {
    const steps = [
      { id: 1, label: 'Claim Submitted', icon: 'send', status: 'pending' },
      { id: 2, label: 'AI Risk Analysis', icon: 'psychology', status: 'pending' },
      { id: 3, label: 'Under Review', icon: 'rate_review', status: 'pending' },
      { id: 4, label: 'Final Decision', icon: 'gavel', status: 'pending' },
      { id: 5, label: 'Settlement', icon: 'payments', status: 'pending' }
    ];

    // Current State Logic
    if (claim.status === 'Submitted') {
      steps[0].status = 'completed';
      steps[1].status = 'active';
    } else if (claim.status === 'UnderReview') {
      steps[0].status = 'completed';
      steps[1].status = 'completed';
      steps[2].status = 'active';
    } else if (claim.status === 'Approved' || claim.status === 'Rejected') {
      steps[0].status = 'completed';
      steps[1].status = 'completed';
      steps[2].status = 'completed';
      steps[3].status = 'completed';
      steps[3].label = claim.status === 'Approved' ? 'Claim Approved' : 'Claim Rejected';
      steps[3].icon = claim.status === 'Approved' ? 'task_alt' : 'cancel';
      if (claim.status === 'Approved') steps[4].status = 'active';
    } else if (claim.status === 'Settled') {
      steps[0].status = 'completed';
      steps[1].status = 'completed';
      steps[2].status = 'completed';
      steps[3].status = 'completed';
      steps[3].label = 'Claim Approved';
      steps[4].status = 'completed';
    }

    return steps;
  }

  getStepStatusClass(status: string): string {
    switch (status) {
      case 'completed': return 'bg-emerald-500 text-white shadow-emerald-100 ring-emerald-500';
      case 'active': return 'bg-blue-600 text-white shadow-blue-100 ring-blue-600 animate-pulse';
      default: return 'bg-gray-100 text-gray-400 ring-gray-200';
    }
  }
}
