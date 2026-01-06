import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatBadgeModule } from '@angular/material/badge';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AuthService } from '../../services/auth.service';
import { TicketService } from '../../services/ticket.service';
import { AgentTicketViewService } from '../../services/agent-ticket-view.service';
import { SignalrService } from '../../services/signalr.service';
import { Subscription } from 'rxjs';

interface Notification {
  id: string;
  message: string;
  timestamp: Date;
  type: 'ticket-updated' | 'comment-added';
  ticketId?: number;
}

@Component({
  selector: 'app-agent-dashboard-nav',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatSidenavModule,
    MatToolbarModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    MatDividerModule,
    MatBadgeModule,
    MatMenuModule,
    MatTooltipModule
  ],
  templateUrl: './agent-dashboard-nav.html',
  styleUrls: ['./agent-dashboard-nav.scss']
})
export class AgentDashboardNav implements OnInit, OnDestroy {
  assignedCount = signal(0);
  notifications = signal<Notification[]>([]);
  private subscriptions: Subscription[] = [];

  constructor(
    private authService: AuthService,
    private ticketService: TicketService,
    private agentTicketViewService: AgentTicketViewService,
    private signalrService: SignalrService,
    private router: Router
  ) {}

  async ngOnInit() {
    this.loadAssignedCount();

    try {
      await this.signalrService.startConnection();
      this.setupSignalRListeners();
    } catch (error) {
      console.error('SignalR Connection Error:', error);
    }
  }

  ngOnDestroy() {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  private setupSignalRListeners() {
    // 1. Listen for Ticket Updates
    const ticketUpdateSub = this.signalrService.getTicketUpdated$().subscribe(data => {
      if (data) {
        this.addNotification({
          id: crypto.randomUUID(),
          message: `Ticket #${data.ticketId} was updated`,
          timestamp: new Date(),
          type: 'ticket-updated',
          ticketId: data.ticketId
        });
        this.loadAssignedCount();
      }
    });

    // 2. Listen for New Comments
    const commentSub = this.signalrService.getCommentAdded$().subscribe(data => {
      if (data) {
        this.addNotification({
          id: crypto.randomUUID(),
          message: `New comment on Ticket #${data.ticketId}`,
          timestamp: new Date(),
          type: 'comment-added',
          ticketId: data.ticketId
        });
      }
    });

    this.subscriptions.push(ticketUpdateSub, commentSub);
  }

  private addNotification(n: Notification) {
    const current = this.notifications();
    this.notifications.set([n, ...current].slice(0, 10)); // Keep last 10
  }

  loadAssignedCount() {
    this.ticketService.getAssignedTickets().subscribe(tickets => {
      const count = tickets.filter(t => ['Assigned', 'In Progress'].includes(t.status)).length;
      this.assignedCount.set(count);
    });
  }

  removeNotification(id: string, event: Event) {
    event.stopPropagation();
    this.notifications.set(this.notifications().filter(n => n.id !== id));
  }

  clearNotifications() {
    this.notifications.set([]);
  }

  viewTicket(ticketId?: number) {
    if (ticketId) {
      this.router.navigate(['/agent/tickets', ticketId]);
    }
  }

  getNotificationCount() { return this.notifications().length; }
  getUsername() { return this.authService.getDisplayName(); }
  setActiveTickets() { this.agentTicketViewService.setActive(); }
  setAllTickets() { this.agentTicketViewService.setAll(); }
  
  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}