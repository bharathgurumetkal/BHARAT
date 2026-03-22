import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CustomerApiService } from '../../../../core/services/customer-api.service';
import { PolicyProduct, Policy, PolicyApplication } from '../../../../core/models/insurance.models';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { forkJoin, catchError, of } from 'rxjs';

declare const google: any;

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './product-list.html',
  styleUrl: './product-list.css'
})
export class ProductListComponent implements OnInit {
  products = signal<PolicyProduct[]>([]);
  policies = signal<Policy[]>([]);
  applications = signal<PolicyApplication[]>([]);
  selectedProduct = signal<PolicyProduct | null>(null);
  applicationForm: FormGroup;
  isLoading = signal(true);
  isSubmitting = signal(false);
  successMessage = signal<string | null>(null);
  errorMessage = signal<string | null>(null);
  estimatedPremium = signal<number>(0);
  
  private autocompleteInstance: any = null;
  private mapInstance: any = null;
  private mapMarker: any = null;

  constructor(
    private customerService: CustomerApiService,
    private fb: FormBuilder
  ) {
    this.applicationForm = this.fb.group({
      propertySubCategory: ['', Validators.required],
      address: ['', Validators.required],
      latitude: [null],
      longitude: [null],
      yearBuilt: [new Date().getFullYear(), [Validators.required, Validators.min(1800)]],
      marketValue: [0, [Validators.required, Validators.min(1000)]],
      riskZone: ['Low', Validators.required],
      hasSecuritySystem: [false],
      requestedCoverageAmount: [0, [Validators.required, Validators.min(1000)]]
    });

    // Automatically recalculate estimate whenever any form field changes
    this.applicationForm.valueChanges.subscribe(() => {
      this.calculateEstimatedPremium();
    });
  }

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading.set(true);

    forkJoin({
      policies: this.customerService.getPolicies().pipe(catchError(() => of([]))),
      applications: this.customerService.getApplications().pipe(catchError(() => of([]))),
      products: this.customerService.getProducts().pipe(catchError(() => of([])))
    }).subscribe({
      next: (res) => {
        this.policies.set(res.policies);
        this.applications.set(res.applications || []);
        this.products.set(res.products.filter(p => p.isActive));
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  hasActivePolicy(productId: string): boolean {
    return this.policies().some(policy => {
      if (policy.status !== 'Active' || !policy.applicationId) return false;
      const app = this.applications().find(a => a.id === policy.applicationId);
      return app?.productId === productId;
    });
  }

  /** Returns a theme config based on the product's propertyCategory */
  getTheme(category: string): { gradient: string; iconBg: string; icon: string; accent: string } {
    const cat = (category || '').toLowerCase();
    if (cat.includes('commercial')) {
      return {
        gradient: 'linear-gradient(135deg,#0f766e 0%,#14b8a6 100%)',
        iconBg: 'rgba(255,255,255,0.22)',
        icon: 'business',
        accent: '#0f766e'
      };
    }
    if (cat.includes('industrial')) {
      return {
        gradient: 'linear-gradient(135deg,#7c3aed 0%,#a78bfa 100%)',
        iconBg: 'rgba(255,255,255,0.22)',
        icon: 'factory',
        accent: '#7c3aed'
      };
    }
    // Default: Residential
    return {
      gradient: 'linear-gradient(135deg,#1d4ed8 0%,#3b82f6 100%)',
      iconBg: 'rgba(255,255,255,0.22)',
      icon: 'home',
      accent: '#1d4ed8'
    };
  }

  openApplyModal(product: PolicyProduct): void {
    if (this.hasActivePolicy(product.id)) return;
    this.selectedProduct.set(product);
    this.successMessage.set(null);
    this.errorMessage.set(null);
    this.estimatedPremium.set(0);

    // Initialize Google Maps Autocomplete
    setTimeout(() => {
      this.initGoogleMapsAutocomplete();
    }, 100);
  }

  private initGoogleMapsAutocomplete(): void {
    const inputElement = document.getElementById('property-address-input') as HTMLInputElement;
    const mapElement = document.getElementById('property-map') as HTMLElement;
    if (!inputElement || !mapElement || typeof google === 'undefined') return;

    if (this.autocompleteInstance) {
      google.maps.event.clearInstanceListeners(this.autocompleteInstance);
    }

    // Default Location
    const initialLocation = { lat: 20.5937, lng: 78.9629 };

    this.mapInstance = new google.maps.Map(mapElement, {
      center: initialLocation,
      zoom: 4,
      mapTypeControl: false,
      streetViewControl: false,
      styles: [{ featureType: 'poi', elementType: 'labels', stylers: [{ visibility: 'off' }] }]
    });

    this.autocompleteInstance = new google.maps.places.Autocomplete(inputElement, {
      types: ['address']
    });

    this.autocompleteInstance.bindTo('bounds', this.mapInstance);
    this.autocompleteInstance.setFields(['address_components', 'geometry', 'formatted_address']);

    this.autocompleteInstance.addListener('place_changed', () => {
      const place = this.autocompleteInstance.getPlace();
      if (!place.geometry || !place.geometry.location) return;

      if (place.geometry.viewport) {
        this.mapInstance.fitBounds(place.geometry.viewport);
      } else {
        this.mapInstance.setCenter(place.geometry.location);
        this.mapInstance.setZoom(17);
      }

      if (!this.mapMarker) {
        this.mapMarker = new google.maps.Marker({
          map: this.mapInstance,
          animation: google.maps.Animation.DROP
        });
      }
      this.mapMarker.setPosition(place.geometry.location);

      this.applicationForm.patchValue({
        address: place.formatted_address,
        latitude: place.geometry.location.lat(),
        longitude: place.geometry.location.lng()
      });
    });
  }

  calculateEstimatedPremium(): void {
    const product = this.selectedProduct();
    const formVals = this.applicationForm.value;
    const coverage = formVals.requestedCoverageAmount || 0;

    if (!product || coverage <= 0) {
      this.estimatedPremium.set(0);
      return;
    }

    // 1. Base Premium (Standard Industry Calculation)
    let premium = coverage * (product.baseRatePercentage / 100);

    // 2. Risk Zone Multiplier Surcharge
    if (formVals.riskZone === 'High') {
      premium *= 1.5; // 50% Surcharge
    } else if (formVals.riskZone === 'Medium') {
      premium *= 1.2; // 20% Surcharge
    }

    // 3. Security System Discount
    if (formVals.hasSecuritySystem) {
      premium *= 0.9; // 10% Discount
    }

    this.estimatedPremium.set(premium);
  }

  onSubmit(): void {
    const product = this.selectedProduct();
    if (this.applicationForm.invalid || !product) return;

    this.isSubmitting.set(true);
    const request = {
      ...this.applicationForm.value,
      productId: product.id
    };

    this.customerService.applyProduct(request).subscribe({
      next: () => {
        this.successMessage.set('Application submitted successfully!');
        this.isSubmitting.set(false);
        this.loadData(); // refresh policy/application state
      },
      error: (err) => {
        this.errorMessage.set(err.error?.message || 'Failed to submit application.');
        this.isSubmitting.set(false);
      }
    });
  }

  closeModal(): void {
    this.selectedProduct.set(null);
    this.successMessage.set(null);
    this.errorMessage.set(null);
    this.applicationForm.reset({
      address: '',
      latitude: null,
      longitude: null,
      yearBuilt: new Date().getFullYear(),
      marketValue: 0,
      riskZone: 'Low',
      hasSecuritySystem: false,
      requestedCoverageAmount: 0
    });
  }
}
