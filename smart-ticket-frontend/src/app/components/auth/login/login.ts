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
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    const credentials = this.loginForm.value;
    
    this.authService.login(credentials).subscribe({
      next: (res) => {
        this.authService.saveSession(res);
        // Get dynamic redirect URL or default to dashboard
        const destination = this.authService.getRedirectUrl() || '/dashboard';
        
        this.router.navigateByUrl(destination, { replaceUrl: true });
      },
      error: (err) => {
        this.loading = false;
        this.handleError(err);
      }
    });
  }

  private handleError(err: any) {
    if (err.status === 401) {
      this.errorMessage = 'Invalid organization credentials.';
    } else if (err.status === 403) {
      this.errorMessage = 'Account suspended. Contact your administrator.';
    } else if (err.status === 0) {
      this.errorMessage = 'Network error. Please check your internet connection.';
    } else {
      this.errorMessage = err.error?.message || 'An unexpected error occurred during login.';
    }
  }
}