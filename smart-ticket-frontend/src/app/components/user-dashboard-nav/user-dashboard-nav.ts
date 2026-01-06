import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, RouterLink } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatBadgeModule } from '@angular/material/badge'; 
import { MatMenuModule } from '@angular/material/menu';    
import { MatTooltipModule } from '@angular/material/tooltip'; 
import { MatDividerModule } from '@angular/material/divider';
import { AuthService } from '../../services/auth.service';
import { SignalrService } from '../../services/signalr.service';
import { Subscription } from 'rxjs';

interface Notification {
  id: string;
  message: string;
  timestamp: Date;
  type: 'ticket-update' | 'account-alert';
  ticketId?: number;
}

@Component({
  selector: 'app-user-dashboard-nav',
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
  templateUrl: './user-dashboard-nav.html',
  styleUrl: './user-dashboard-nav.scss',
})
export class UserDashboardNav implements OnInit, OnDestroy {
  notifications = signal<Notification[]>([]);
  private subs: Subscription[] = [];

  constructor(
    private router: Router,
    private authService: AuthService,
    private signalrService: SignalrService
  ) {};

  async ngOnInit() {
    try {
      await this.signalrService.startConnection();
      this.setupSignalRListeners();
    } catch (error) {
      console.error('SignalR Error:', error);
    }
  }

  ngOnDestroy() {
    this.subs.forEach(s => s.unsubscribe());
  }

  private setupSignalRListeners() {
    // 1. Listen for Ticket Updates (Sent from Worker HandleTicketUpdated)
    this.subs.push(
      this.signalrService.getTicketUpdated$().subscribe(data => {
        if (data) {
          this.addNotification({
            id: crypto.randomUUID(),
            message: `Update on ticket #${data.ticketId}`,
            timestamp: new Date(),
            type: 'ticket-update',
            ticketId: data.ticketId
          });
        }
      })
    );

    // 2. Listen for Account Activation
    this.subs.push(
      this.signalrService.getAccountActivated$().subscribe(data => {
        if (data) {
          this.addNotification({
            id: crypto.randomUUID(),
            message: data.message,
            timestamp: new Date(),
            type: 'account-alert'
          });
        }
      })
    );
  }

  private addNotification(n: Notification) {
    this.notifications.update(current => [n, ...current].slice(0, 10));
  }

  removeNotification(id: string, event: Event) {
    event.stopPropagation();
    this.notifications.update(current => current.filter(n => n.id !== id));
  }

  clearNotifications() {
    this.notifications.set([]);
  }

  getNotificationCount() { return this.notifications().length; }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}