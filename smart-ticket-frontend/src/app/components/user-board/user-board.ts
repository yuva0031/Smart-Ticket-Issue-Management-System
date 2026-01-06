import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

import { TicketService } from '../../services/ticket.service';
import { LookupService } from '../../services/lookup.service';
import { AuthService } from '../../services/auth.service';
import { SignalrService } from '../../services/signalr.service';
import { TicketResponseDto, UserDashboardMetrics } from '../../models/Model';

interface DisplayColumn {
  status: string;
  tickets: TicketResponseDto[];
}

@Component({
  selector: 'app-user-board',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatChipsModule,
    MatIconModule,
    MatButtonModule,
    MatSnackBarModule,
    DragDropModule
  ],
  templateUrl: './user-board.html',
  styleUrl: './user-board.scss'
})
export class UserBoard implements OnInit {
  private ticketService = inject(TicketService);
  private authService = inject(AuthService);
  private lookupService = inject(LookupService);
  private signalr = inject(SignalrService);
  private router = inject(Router);

  // State Signals
  tickets = signal<TicketResponseDto[]>([]);
  selectedPriorities = signal<string[]>([]);
  metrics = signal<UserDashboardMetrics>({
    totalTickets: 0,
    pending: 0,
    resolved: 0,
    open: 0
  });

  // Lookups
  statuses = computed(() => this.lookupService.statuses());
  availablePriorities = signal<string[]>(['Critical', 'High', 'Medium', 'Low']);

  // Reactive Board Logic
  columns = computed<DisplayColumn[]>(() => {
    const allTickets = this.tickets();
    const activePrios = this.selectedPriorities();
    
    // 1. Filter out the columns we don't want to see
    const excludedStatuses = ['Closed', 'Cancelled'];

    // 2. Filter tickets by priority chip selection
    const filtered = activePrios.length > 0 
      ? allTickets.filter(t => activePrios.includes(t.priority)) 
      : allTickets;

    // 3. Map filtered statuses to their respective columns
    return this.statuses()
      .filter(s => !excludedStatuses.includes(s.name))
      .map(status => ({
        status: status.name,
        tickets: filtered.filter(t => t.status === status.name)
      }));
  });

  async ngOnInit(): Promise<void> {
    this.lookupService.loadStatuses();
    this.loadData();

    // SignalR Real-time listener
    await this.signalr.startConnection();
    this.signalr.getTicketUpdated$().subscribe(ev => {
      if (ev) this.loadData(); 
    });
  }

  loadData(): void {
    this.ticketService.getTicketsByOwner().subscribe({
      next: (tickets) => {
        this.tickets.set(tickets);
        this.metrics.set({
          totalTickets: tickets.length,
          pending: tickets.filter(t => ['Assigned', 'In Progress'].includes(t.status)).length,
          resolved: tickets.filter(t => ['Resolved'].includes(t.status)).length,
          open: tickets.filter(t => t.status === 'Open').length
        });
      }
    });
  }

  togglePriority(priority: string): void {
    const current = this.selectedPriorities();
    this.selectedPriorities.set(
      current.includes(priority) ? current.filter(p => p !== priority) : [...current, priority]
    );
  }

  clearFilters(): void {
    this.selectedPriorities.set([]);
  }

  goToTicket(ticketId: number): void {
    this.router.navigate(['/app/tickets', ticketId]);
  }

  // Helper for empty state icons based on column status
  getEmptyIcon(status: string): string {
    switch (status) {
      case 'Open': return 'fiber_new';
      case 'Assigned': return 'assignment_ind';
      case 'In Progress': return 'pending';
      case 'Resolved': return 'verified';
      default: return 'inbox';
    }
  }
}