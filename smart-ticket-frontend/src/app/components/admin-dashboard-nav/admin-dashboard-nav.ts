import { Component, signal, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule, RouterLink } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatBadgeModule } from '@angular/material/badge';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDividerModule } from '@angular/material/divider';
import { UserService } from '../../services/user.service';
import { AuthService } from '../../services/auth.service';
import { SignalrService } from '../../services/signalr.service';
import { Subscription } from 'rxjs';

interface Notification {
  id?: string;
  message: string;
  timestamp: Date;
  type: 'user-registered' | 'user-approved';
  data?: any;
}

@Component({
  selector: 'app-admin-dashboard-nav',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    RouterLink,
    MatSidenavModule,
    MatButtonModule,
    MatIconModule,
    MatListModule,
    MatToolbarModule,
    MatBadgeModule,
    MatMenuModule,
    MatTooltipModule,
    MatDividerModule
  ],
  templateUrl: './admin-dashboard-nav.html',
  styleUrl: './admin-dashboard-nav.scss',
})
export class AdminDashboardNav implements OnInit, OnDestroy {
  
  pendingRequestsCount = signal<number>(0);
  notifications = signal<Notification[]>([]);
  
  private refreshInterval: any;
  private subscriptions: Subscription[] = [];

  constructor(
    private userService: UserService,
    private authService: AuthService,
    private router: Router,
    private signalrService: SignalrService
  ) {}

  async ngOnInit(): Promise<void> {
    this.loadPendingRequests();
    
    this.refreshInterval = setInterval(() => {
      this.loadPendingRequests();
    }, 60000);

    try {
      await this.signalrService.startConnection();
      this.setupSignalRListeners();
    } catch (error) {
      console.error('Failed to start SignalR connection:', error);
    }
  }

  ngOnDestroy(): void {
    if (this.refreshInterval) {
      clearInterval(this.refreshInterval);
    }
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  private setupSignalRListeners(): void {
    const userRegisteredSub = this.signalrService.getUserRegistered$().subscribe({
      next: (data) => {
        if (data) {
          console.log('📩 Received UserRegistered notification:', data);
          
          const notification: Notification = {
            id: crypto.randomUUID(),
            message: `New user registered: ${data.fullName} (${data.email})${data.requiresApproval ? ' - Requires approval' : ''}`,
            timestamp: new Date(data.occurredAt || Date.now()),
            type: 'user-registered',
            data: data
          };
          
          this.addNotification(notification);          
          if (data.requiresApproval) {
            this.loadPendingRequests();
          }
        }
      },
      error: (err) => console.error('Error in UserRegistered listener:', err)
    });

    const userApprovedSub = this.signalrService.getUserApproved$().subscribe({
      next: (data) => {
        if (data) {          
          const notification: Notification = {
            id: crypto.randomUUID(),
            message: `User account approved`,
            timestamp: new Date(data.occurredAt || Date.now()),
            type: 'user-approved',
            data: data
          };
          this.addNotification(notification);
          this.loadPendingRequests(); 
        }
      },
      error: (err) => console.error('Error in UserApproved listener:', err)
    });

    this.subscriptions.push(userRegisteredSub, userApprovedSub);
  }

  private addNotification(notification: Notification): void {
    const currentNotifications = this.notifications();
    
    const updatedNotifications = [notification, ...currentNotifications];
    
    if (updatedNotifications.length > 10) {
      updatedNotifications.pop();
    }
    
    this.notifications.set(updatedNotifications);
  }

  clearNotifications(): void {
    this.notifications.set([]);
  }

  removeNotification(id?: string): void {
    if (!id) return;
    
    const updatedNotifications = this.notifications().filter(n => n.id !== id);
    this.notifications.set(updatedNotifications);
  }

  loadPendingRequests(): void {
    this.userService.getPendingRoleRequests().subscribe({
      next: requests => {
        this.pendingRequestsCount.set(requests.length);
      },
      error: err => console.error('Failed to load pending requests', err)
    });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  getUsername(): string {
    return this.authService.getDisplayName();
  }

  viewPendingRequests(): void {
    this.router.navigate(['/admin/requests']);
  }

  getNotificationCount(): number {
    return this.notifications().length;
  }
}