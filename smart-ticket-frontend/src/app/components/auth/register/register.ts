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
      phoneNumber: ['', [Validators.pattern('^[0-9+ ]*$')]],
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

  get f() { return this.registerForm.controls; }

  register() {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';
    
    const val = this.registerForm.value;
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

    this.authService.register(payload).subscribe({
      next: () => {
        this.successMessage = 'Organizational account created! Redirecting to login...';
        setTimeout(() => this.router.navigate(['/login']), 2500);
      },
      error: err => {
        this.loading = false;
        if (err.status === 409) {
          this.errorMessage = 'This email is already associated with an account.';
        } else {
          this.errorMessage = err.error?.message || 'Onboarding failed. Please contact your IT support.';
        }
      }
    });
  }
}