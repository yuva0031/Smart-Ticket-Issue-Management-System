import { HttpClient } from '@angular/common/http';
import { Injectable, signal, inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { jwtDecode } from 'jwt-decode';
import { RegisterRequest } from '../models/Model';
import { Observable, BehaviorSubject, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { environment } from '../../environments/environment.development';

export interface UserPayload {
  userId: string;
  email: string;
  role: string | string[];
  exp?: number;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private platformId = inject(PLATFORM_ID);
  private apiUrl = `${environment.apiUrl}/auth`;

  user = signal<UserPayload | null>(null);
  isLoggedIn = signal<boolean>(false);

  private userSubject = new BehaviorSubject<UserPayload | null>(null);
  public user$ = this.userSubject.asObservable();

  private isLoggedInSubject = new BehaviorSubject<boolean>(false);
  public isLoggedIn$ = this.isLoggedInSubject.asObservable();

  private redirectUrl: string | null = null;

  constructor() {
    this.restoreSession();
  }

  private isBrowser(): boolean {
    return isPlatformBrowser(this.platformId);
  }


  register(payload: RegisterRequest): Observable<any> {
    console.log("Registration payload:", payload);
    return this.http.post(`${this.apiUrl}/register`, payload).pipe(
      catchError(this.handleError)
    );
  }

  login(payload: { email: string; password: string }): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/login`, payload).pipe(
      tap(response => {
        if (response?.token) {
          this.saveSession(response);
        }
      }),
      catchError(this.handleError)
    );
  }

  /**
   * Save authentication session
   */
  saveSession(loginResponse: { token: string }): void {
    if (!loginResponse?.token || !this.isBrowser()) {
      console.warn('Cannot save session: Invalid token or not in browser');
      return;
    }

    try {
      localStorage.setItem('token', loginResponse.token);
      const decodedUser = this.decodeToken(loginResponse.token);
      
      if (decodedUser) {
        this.updateAuthState(decodedUser, true);
        console.log('Session saved successfully');
      } else {
        console.error('Failed to decode token');
        this.logout();
      }
    } catch (error) {
      console.error('Error saving session:', error);
      this.logout();
    }
  }

  /**
   * Logout and clear session
   */
  logout(): void {
    if (this.isBrowser()) {
      localStorage.removeItem('token');
    }
    this.updateAuthState(null, false);
    this.clearRedirectUrl();
    console.log('User logged out');
  }

  /**
   * Get current user ID
   */
  getUserId(): string | null {
    return this.user()?.userId ?? null;
  }

  /**
   * Get redirect URL based on user role
   */
  getRedirectUrl(): string {
    const currentUser = this.user();
    
    if (!currentUser || !currentUser.role) {
      return '/login';
    }

    if (this.hasRole('Admin')) return '/admin/dashboard';
    if (this.hasRole('SupportManager')) return '/manager/dashboard';
    if (this.hasRole('SupportAgent')) return '/agent/dashboard';
    if (this.hasRole('EndUser')) return '/app/dashboard'; // Added this line
    
    return '/app/dashboard';
  }

  /**
   * Get user's primary role as string (for backward compatibility)
   */
  getUserRole(): string | null {
    const roles = this.getUserRoles();
    if (roles.length === 0) return null;
    
    // Return the first role (or prioritize certain roles)
    if (roles.includes('Admin')) return 'Admin';
    if (roles.includes('SupportManager')) return 'SupportManager';
    if (roles.includes('SupportAgent')) return 'SupportAgent';
    
    return roles[0];
  }

  /**
   * Set custom redirect URL
   */
  setRedirectUrl(url: string): void {
    this.redirectUrl = url;
  }

  /**
   * Get stored redirect URL
   */
  getStoredRedirectUrl(): string | null {
    return this.redirectUrl;
  }

  /**
   * Clear redirect URL
   */
  clearRedirectUrl(): void {
    this.redirectUrl = null;
  }

  /**
   * Check if user has specific role
   */
  hasRole(targetRole: string): boolean {
    const role = this.user()?.role;
    return Array.isArray(role) ? role.includes(targetRole) : role === targetRole;
  }

  /**
   * Check if user has any of the specified roles
   */
  hasAnyRole(roles: string[]): boolean {
    return roles.some(r => this.hasRole(r));
  }

  /**
   * Check if user has all specified roles
   */
  hasAllRoles(roles: string[]): boolean {
    return roles.every(r => this.hasRole(r));
  }

  /**
   * Get all user roles as array
   */
  getUserRoles(): string[] {
    const r = this.user()?.role;
    return Array.isArray(r) ? r : r ? [r] : [];
  }

  /**
   * Check if user is authenticated
   */
  isAuthenticated(): boolean {
    return this.isLoggedIn() && this.isSessionValid();
  }

  /**
   * Get current authentication token
   */
  getToken(): string | null {
    if (!this.isBrowser()) return null;
    return localStorage.getItem('token');
  }

  /**
   * Check if current user is end user (no special roles)
   */
  isEndUser(): boolean {
    return this.hasRole('EndUser') || !this.hasAnyRole(['Admin', 'SupportManager', 'SupportAgent']);
  }

  /**
   * Check if current user is support agent
   */
  isAgent(): boolean {
    return this.hasRole('SupportAgent');
  }

  /**
   * Check if current user is manager or admin
   */
  isManager(): boolean {
    return this.hasAnyRole(['SupportManager', 'Admin']);
  }

  /**
   * Check if current user is admin
   */
  isAdmin(): boolean {
    return this.hasRole('Admin');
  }

  /**
   * Get user's display name
   */
  getDisplayName(): string {
    return this.user()?.email || 'User';
  }

  /**
   * Get user's email
   */
  getUserEmail(): string | null {
    return this.user()?.email ?? null;
  }

  /**
   * Check if session is valid
   */
  isSessionValid(): boolean {
    if (!this.isBrowser()) return false;
    
    const token = localStorage.getItem('token');
    if (!token) return false;

    const decoded = this.decodeToken(token);
    
    // Check if token exists, has expiry, and hasn't expired
    if (!decoded || !decoded.exp) return false;
    
    const isValid = Date.now() < decoded.exp * 1000;
    
    // Auto-logout if token expired
    if (!isValid) {
      console.warn('Session expired');
      this.logout();
    }
    
    return isValid;
  }

  /**
   * Get token expiry time in milliseconds
   */
  getTokenExpiry(): number | null {
    const user = this.user();
    return user?.exp ? user.exp * 1000 : null;
  }

  /**
   * Get remaining session time in milliseconds
   */
  getRemainingSessionTime(): number {
    const expiry = this.getTokenExpiry();
    if (!expiry) return 0;
    
    const remaining = expiry - Date.now();
    return Math.max(0, remaining);
  }

  /**
   * Check if token is about to expire (within specified minutes)
   */
  isTokenExpiringSoon(minutesThreshold: number = 5): boolean {
    const remaining = this.getRemainingSessionTime();
    return remaining > 0 && remaining < minutesThreshold * 60 * 1000;
  }

  /**
   * Decode JWT token
   */
  private decodeToken(token: string): UserPayload | null {
    try {
      const decoded: any = jwtDecode(token);
      
      // Handle various JWT claim formats
      const email = 
        decoded.email || 
        decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || 
        decoded.sub;
      
      let role = 
        decoded.role || 
        decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || 
        [];

      const userId = 
        decoded.id || 
        decoded.sub || 
        decoded.userId ||
        decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];

      return {
        userId: userId,
        email: email,
        role: role,
        exp: decoded.exp
      };
    } catch (error) {
      console.error('Error decoding token:', error);
      return null;
    }
  }

  /**
   * Restore session from localStorage on app initialization
   */
  private restoreSession(): void {
    if (!this.isBrowser()) return;

    const token = localStorage.getItem('token');
    if (!token) return;

    const decodedUser = this.decodeToken(token);
    
    // Check if token is valid and not expired
    if (!decodedUser || (decodedUser.exp && Date.now() >= decodedUser.exp * 1000)) {
      console.warn('Stored token is invalid or expired');
      this.logout();
    } else {
      this.updateAuthState(decodedUser, true);
      console.log('Session restored successfully');
    }
  }

  /**
   * Update authentication state (signals and subjects)
   */
  private updateAuthState(user: UserPayload | null, isLoggedIn: boolean): void {
    // Update signals
    this.user.set(user);
    this.isLoggedIn.set(isLoggedIn);
    
    // Update subjects for reactive subscriptions
    this.userSubject.next(user);
    this.isLoggedInSubject.next(isLoggedIn);
  }

  /**
   * Handle HTTP errors
   */
  private handleError(error: any): Observable<never> {
    let errorMessage = 'An unknown error occurred';
    
    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = `Error: ${error.error.message}`;
    } else {
      // Server-side error
      errorMessage = `Error Code: ${error.status}\nMessage: ${error.message}`;
      
      if (error.error?.message) {
        errorMessage = error.error.message;
      }
    }
    
    console.error('Auth Service Error:', errorMessage);
    return throwError(() => new Error(errorMessage));
  }

  /**
   * Refresh token (placeholder - implement based on your backend)
   */
  refreshToken(): Observable<any> {
    const token = this.getToken();
    if (!token) {
      return throwError(() => new Error('No token available'));
    }

    // Implement your token refresh logic here
    return this.http.post<any>(`${this.apiUrl}/refresh-token`, { token }).pipe(
      tap(response => {
        if (response?.token) {
          this.saveSession(response);
        }
      }),
      catchError(this.handleError)
    );
  }

  /**
   * Validate current session with backend
   */
  validateSession(): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/validate-session`).pipe(
      catchError(() => {
        this.logout();
        return throwError(() => new Error('Session validation failed'));
      })
    );
  }
}