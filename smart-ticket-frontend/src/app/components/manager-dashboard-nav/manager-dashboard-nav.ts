import { Component, signal, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule, RouterLink } from '@angular/router';
import { Subscription } from 'rxjs';

import { MatSidenavModule } from '@angular/material/sidenav';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatBadgeModule } from '@angular/material/badge';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDividerModule } from '@angular/material/divider';
import { TicketService } from '../../services/ticket.service';
import { AuthService } from '../../services/auth.service';
import { SignalrService } from '../../services/signalr.service';

interface Notification {
  id: string;
  message: string;
  timestamp: Date;
  type: 'user-registered' | 'user-approved' | 'ticket-updated';
  data?: any;
}

@Component({
  selector: 'app-manager-dashboard-nav',
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
  templateUrl: './manager-dashboard-nav.html',
  styleUrl: './manager-dashboard-nav.scss',
})
export class ManagerDashboardNav implements OnInit, OnDestroy {
  notifications = signal<Notification[]>([]);
  unassignedCount = signal<number>(0);

  private subs: Subscription[] = [];
  private refreshInterval: any;

  constructor(
    private ticketService: TicketService,
    private authService: AuthService,
    private signalr: SignalrService,
    private router: Router
  ) {}

  async ngOnInit(): Promise<void> {
    this.loadUnassignedCount();

    // Poll for unassigned count every 30s as fallback
    this.refreshInterval = setInterval(() => {
      this.loadUnassignedCount();
    }, 30000);

    try {
      await this.signalr.startConnection();
      this.setupSignalRListeners();
    } catch (error) {
      console.error('Failed to start SignalR connection:', error);
    }
  }

  ngOnDestroy(): void {
    this.subs.forEach(s => s.unsubscribe());
    if (this.refreshInterval) clearInterval(this.refreshInterval);
  }

  private setupSignalRListeners(): void {
    // 1. User Registration Listener
    this.subs.push(
      this.signalr.getUserRegistered$().subscribe(ev => {
        if (!ev) return;
        this.addNotification({
          id: crypto.randomUUID(),
          message: `New User: ${ev.fullName} registered.`,
          timestamp: new Date(ev.occurredAt ?? Date.now()),
          type: 'user-registered',
          data: ev
        });
      })
    );

    // 2. User Approval Listener
    this.subs.push(
      this.signalr.getUserApproved$().subscribe(ev => {
        if (!ev) return;
        this.addNotification({
          id: crypto.randomUUID(),
          message: `User Approved: ${ev.userId}`,
          timestamp: new Date(ev.occurredAt ?? Date.now()),
          type: 'user-approved',
          data: ev
        });
      })
    );

    // 3. Optional: Ticket Update Listener (if managers should see updates)
    this.subs.push(
        this.signalr.getTicketUpdated$().subscribe(ev => {
          if (!ev) return;
          this.loadUnassignedCount(); // Refresh count if a ticket status might have changed
        })
      );
  }

  private addNotification(notification: Notification): void {
    this.notifications.update(current => {
      const next = [notification, ...current];
      return next.slice(0, 10); // Keep only latest 10
    });
  }

  clearNotifications(): void {
    this.notifications.set([]);
  }

  removeNotification(id: string, event: Event): void {
    event.stopPropagation();
    this.notifications.update(current => current.filter(n => n.id !== id));
  }

  loadUnassignedCount(): void {
    this.ticketService.getAllTickets().subscribe({
      next: tickets => {
        const unassigned = tickets.filter(t => !t.assignedToId && t.status !== 'Closed');
        this.unassignedCount.set(unassigned.length);
      },
      error: err => console.error('Failed to load unassigned count', err)
    });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  getUsername(): string {
    return this.authService.getDisplayName();
  }

  getNotificationCount(): number {
    return this.notifications().length;
  }

  viewNotifications(): void {
    this.router.navigate(['/manager/dashboard']);
  }
}