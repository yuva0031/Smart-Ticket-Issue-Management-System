import { Component, OnInit, effect, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { 
  ReactiveFormsModule, 
  FormBuilder, 
  FormGroup, 
  Validators, 
  AbstractControl, 
  ValidationErrors 
} from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { LookupService } from '../../../services/lookup.service';
import { RegisterRequest } from '../../../models/Model';
import { catchError, finalize } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
    MatProgressSpinnerModule,
    RouterModule
  ],
  templateUrl: './register.html',
  styleUrls: ['./register.scss'],
})
export class RegisterComponent implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  public lookupService = inject(LookupService);
  private router = inject(Router);

  registerForm!: FormGroup;
  loading = false;
  errorMessage = '';
  successMessage = '';
  hidePassword = true;
  hideConfirmPassword = true;

  constructor() {
    // Automatically load categories if a Support Agent role is detected in the signals
    effect(() => {
      const roles = this.lookupService.roles();
      if (!roles.length) return;
      
      const currentRoleId = this.registerForm?.get('roleId')?.value;
      if (this.isSupportAgentById(currentRoleId)) {
        this.lookupService.loadCategories();
      }
    });
  }

  ngOnInit(): void {
    this.lookupService.loadRoles();
    this.initForm();
  }

  private initForm() {
    this.registerForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]],
      phoneNumber: ['', [Validators.pattern('^[0-9+ ()-]*$')]],
      address: [''],
      roleId: [null, [Validators.required]],
      categorySkillIds: [[]]
    }, { validators: this.passwordMatchValidator });

    // Watch role changes to reset/trigger extra fields
    this.registerForm.get('roleId')?.valueChanges.subscribe(roleId => {
      if (this.isSupportAgentById(roleId)) {
        this.lookupService.loadCategories();
      } else {
        this.registerForm.get('categorySkillIds')?.setValue([]);
      }
    });
  }

  passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('password');
    const confirmPassword = control.get('confirmPassword');
    if (password && confirmPassword && password.value !== confirmPassword.value) {
      confirmPassword.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    }
    return null;
  }

  isSupportAgentById(roleId: any): boolean {
    if (!roleId) return false;
    const role = this.lookupService.roles().find(r => r.id === Number(roleId));
    if (!role) return false;
    return role.name.replace(/\s/g, '').toLowerCase() === 'supportagent';
  }

  isManagerById(roleId: any): boolean {
    if (!roleId) return false;
    const role = this.lookupService.roles().find(r => r.id === Number(roleId));
    if (!role) return false;
    return role.name.replace(/\s/g, '').toLowerCase() === 'supportmanager';
  }

  isAdminById(roleId: any): boolean {
    if (!roleId) return false;
    const role = this.lookupService.roles().find(r => r.id === Number(roleId));
    if (!role) return false;
    return role.name.replace(/\s/g, '').toLowerCase() === 'admin';
  }

  // Filter roles to exclude Admin from selection
  getAvailableRoles() {
    return this.lookupService.roles().filter(role => 
      role.name.replace(/\s/g, '').toLowerCase() !== 'admin'
    );
  }

  get f() { return this.registerForm.controls; }

  // Clear error/success messages when user starts typing
  clearMessages(): void {
    if (this.errorMessage || this.successMessage) {
      this.errorMessage = '';
      this.successMessage = '';
    }
  }

  register() {
    // Clear previous messages
    this.errorMessage = '';
    this.successMessage = '';

    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      this.errorMessage = 'Please fill in all required fields correctly.';
      return;
    }

    this.loading = true;
    
    const val = this.registerForm.value;
    const selectedRoleId = val.roleId;

    // Check if trying to register as Admin (shouldn't be possible with filtered list)
    if (this.isAdminById(selectedRoleId)) {
      this.loading = false;
      this.errorMessage = 'Admin accounts cannot be created through registration. Please contact system administrator.';
      return;
    }

    const payload: RegisterRequest = {
      FirstName: val.firstName,
      LastName: val.lastName,
      email: val.email,
      password: val.password,
      roleId: val.roleId,
      categorySkillIds: this.isSupportAgentById(val.roleId) ? val.categorySkillIds : [],
      phoneNumber: val.phoneNumber,
      address: val.address
    };

    this.authService.register(payload).pipe(
      catchError((err) => {
        this.handleError(err);
        return of(null);
      }),
      finalize(() => {
        this.loading = false;
      })
    ).subscribe({
      next: (response) => {
        if (response) {
          this.handleSuccessfulRegistration(selectedRoleId);
        }
      }
    });
  }

  private handleSuccessfulRegistration(roleId: any): void {
    const isManager = this.isManagerById(roleId);
    const isAgent = this.isSupportAgentById(roleId);

    if (isManager || isAgent) {
      const roleType = isManager ? 'Manager' : 'Support Agent';
      this.successMessage = `✓ Registration successful! Your ${roleType} account requires admin approval. You will receive an email once approved.`;
      
      // Redirect to login after 4 seconds to give user time to read
      setTimeout(() => {
        this.router.navigate(['/login']);
      }, 4000);
    } else {
      // For any other role (if applicable)
      this.successMessage = '✓ Registration successful! Redirecting to login...';
      setTimeout(() => {
        this.router.navigate(['/login']);
      }, 2500);
    }
  }

  private handleError(err: any): void {
    console.error('Registration error:', err);
    
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
        this.errorMessage = err.error?.message || 'Invalid registration data. Please check all fields.';
        break;
      
      case 409:
        this.errorMessage = 'This email is already registered. Please use a different email or sign in.';
        break;
      
      case 422:
        this.errorMessage = 'Validation failed. Please check your input and try again.';
        break;
      
      case 500:
      case 502:
      case 503:
      case 504:
        this.errorMessage = 'Server error. Please try again later or contact support.';
        break;
      
      default:
        this.errorMessage = 
          err.error?.message || 
          err.message || 
          'Registration failed. Please contact IT support.';
    }
  }
}