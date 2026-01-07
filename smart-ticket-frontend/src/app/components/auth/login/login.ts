import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { 
  ReactiveFormsModule, 
  FormBuilder, 
  FormGroup, 
  Validators 
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

// Material Imports
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { AuthService } from '../../../services/auth.service';
import { catchError, finalize } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    RouterLink
  ],
  templateUrl: './login.html',
  styleUrls: ['./login.scss'],
})
export class Login {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  // Form definition with stricter validations for an Org login
  loginForm: FormGroup = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  loading = false;
  errorMessage = '';
  hidePassword = true;

  login() {
    // Clear previous error message
    this.errorMessage = '';

    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    const credentials = this.loginForm.value;
    
    this.authService.login(credentials).pipe(
      catchError((err) => {
        this.handleError(err);
        return of(null); // Return observable to complete the stream
      }),
      finalize(() => {
        this.loading = false; // Always stop loading
      })
    ).subscribe({
      next: (res) => {
        if (res) { // Only navigate if we have a successful response
          this.authService.saveSession(res);
          const destination = this.authService.getRedirectUrl() || '/dashboard';
          this.router.navigateByUrl(destination, { replaceUrl: true });
        }
      }
    });
  }

  private handleError(err: any): void {
    console.error('Login error:', err); // Keep for debugging
    
    // Handle different error scenarios
    if (!err) {
      this.errorMessage = 'An unexpected error occurred. Please try again.';
      return;
    }

    const status = err.status || 0;
    
    switch (status) {
      case 0:
        this.errorMessage = 'Network error. Please check your internet connection and try again.';
        break;
      
      case 400:
        this.errorMessage = err.error?.message || 'Invalid credentials. Please check your email and password.';
        break;
      
      case 401:
        this.errorMessage = 'Invalid email or password. Please try again.';
        break;
      
      case 403:
        this.errorMessage = 'Account suspended. Please contact your administrator.';
        break;
      
      case 404:
        this.errorMessage = 'Account not found. Please check your credentials or register.';
        break;
      
      case 429:
        this.errorMessage = 'Too many login attempts. Please try again later.';
        break;
      
      case 500:
      case 502:
      case 503:
      case 504:
        this.errorMessage = 'Server error. Please try again later.';
        break;
      
      default:
        // Try to extract error message from various possible locations
        this.errorMessage = 
          err.error?.message || 
          err.error?.error || 
          err.message || 
          'An unexpected error occurred. Please try again.';
    }
  }

  // Helper method to clear error when user starts typing
  clearError(): void {
    if (this.errorMessage) {
      this.errorMessage = '';
    }
  }
}