import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const guestGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isLoggedIn()) {
    console.log('Guest guard: User is logged in, redirecting...');
    console.log('User roles:', authService.getUserRoles());
    
    if (authService.hasRole('Admin')) {
      console.log('Redirecting to admin dashboard');
      return router.createUrlTree(['/admin/dashboard']);
    } else if (authService.hasRole('SupportManager')) {
      console.log('Redirecting to manager tickets');
      return router.createUrlTree(['/manager/dashboard']);
    } else if (authService.hasRole('SupportAgent')) {
      console.log('Redirecting to agent assigned');
      return router.createUrlTree(['/agent/dashboard']);
    } else if (authService.hasRole('EndUser')) {
      console.log('Redirecting to app dashboard');
      return router.createUrlTree(['/app/dashboard']);
    } else {
      console.log('Redirecting to default app dashboard');
      return router.createUrlTree(['/app/dashboard']);
    }
  }
  console.log('Guest guard: User not logged in, allowing access');
  return true;
};