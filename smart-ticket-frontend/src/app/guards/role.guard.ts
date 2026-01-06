import { inject } from '@angular/core';
import { Router, CanActivateFn, ActivatedRouteSnapshot } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const roleGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const requiredRoles = route.data['roles'] as Array<string>;
  const userRoles = authService.getUserRoles();

  console.log('Role Guard Check:');
  console.log('Required roles:', requiredRoles);
  console.log('User roles:', userRoles);
  console.log('Is authenticated:', authService.isAuthenticated());

  if (!authService.isAuthenticated()) {
    console.log('User not authenticated, redirecting to login');
    return router.createUrlTree(['/login']);
  }

  if (requiredRoles && requiredRoles.length > 0) {
    const hasRequiredRole = requiredRoles.some(role => authService.hasRole(role));
    
    console.log('Has required role:', hasRequiredRole);
    
    if (hasRequiredRole) {
      console.log('Access granted');
      return true;
    }
    
    // Redirect to appropriate dashboard based on user's actual role
    console.log('Access denied, redirecting to user dashboard');
    const redirectUrl = authService.getRedirectUrl();
    return router.createUrlTree([redirectUrl]);
  }

  console.log('No role requirements, access granted');
  return true;
};