import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthApiService } from '../../services/auth-api.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class RegisterComponent {
  registerForm: FormGroup;
  isLoading = false;
  errorMessage: string | null = null;

  constructor(
    private fb: FormBuilder,
    private authService: AuthApiService,
    private router: Router
  ) {
    this.registerForm = this.fb.group({
      name: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      phoneNumber: ['', Validators.required],
      role: ['Customer'] // Fixed to Customer as required
    });
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.registerForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  onSubmit(): void {
    if (this.registerForm.invalid) return;

    this.isLoading = true;
    this.errorMessage = null;

    this.authService.register(this.registerForm.value).subscribe({
      next: (response: any) => {
        localStorage.setItem('token', response.token);
        localStorage.setItem('role', response.role);
        localStorage.setItem('email', response.email);

        this.router.navigate(['/dashboard/customer']);
      },
      error: (err: any) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'Registration failed. Check your data or server status.';
      }
    });
  }
}
