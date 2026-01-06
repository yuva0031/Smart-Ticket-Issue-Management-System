import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { DragDropModule, CdkDragDrop, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { TicketService } from '../../services/ticket.service';
import { LookupService } from '../../services/lookup.service';
import { TicketResponseDto, UpdateTicketDto } from '../../models/Model';

@Component({
  selector: 'app-manager-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, MatCardModule, MatChipsModule, MatIconModule, MatButtonModule, MatSnackBarModule, DragDropModule],
  templateUrl: './manager-dashboard.html',
  styleUrls: ['./manager-dashboard.scss']
})
export class ManagerDashboard implements OnInit {
  private ticketService = inject(TicketService);
  private lookupService = inject(LookupService);
  private snackBar = inject(MatSnackBar);
  private router = inject(Router);

  tickets = signal<TicketResponseDto[]>([]);
  selectedPriorities = signal<string[]>([]);
  availablePriorities = signal(['Critical', 'High', 'Medium', 'Low']);
  
  statuses = computed(() => this.lookupService.statuses());
  allPriorities = computed(() => this.lookupService.priorities());

  metrics = computed(() => {
    const all = this.tickets();
    return {
      totalTickets: all.length,
      pending: all.filter(t => !t.assignedToId).length,
      open: all.filter(t => t.status !== 'Closed' && t.status !== 'Cancelled').length
    };
  });

  columns = computed(() => {
    const activePrios = this.selectedPriorities();
    const excluded = ['Closed', 'Cancelled'];
    const filtered = activePrios.length > 0 ? this.tickets().filter(t => activePrios.includes(t.priority)) : this.tickets();

    return this.statuses().filter(s => !excluded.includes(s.name)).map(s => ({
      status: s.name,
      tickets: filtered.filter(t => t.status === s.name)
    }));
  });

  ngOnInit(): void {
    this.lookupService.loadStatuses();
    this.lookupService.loadPriorities();
    this.loadTickets();
  }

  loadTickets(): void {
    this.ticketService.getAllTickets().subscribe({ next: (t) => this.tickets.set(t) });
  }

  togglePriority(p: string): void {
    this.selectedPriorities.update(curr => curr.includes(p) ? curr.filter(x => x !== p) : [...curr, p]);
  }

  clearFilters(): void { this.selectedPriorities.set([]); }

  drop(event: CdkDragDrop<TicketResponseDto[]>, newStatus: string): void {
    if (event.previousContainer === event.container) {
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
    } else {
      const ticket = event.previousContainer.data[event.previousIndex];
      const statusObj = this.statuses().find(s => s.name === newStatus);
      const prioObj = this.allPriorities().find(p => p.name === ticket.priority);

      if (!statusObj || !prioObj) return;

      transferArrayItem(event.previousContainer.data, event.container.data, event.previousIndex, event.currentIndex);

      const payload: UpdateTicketDto = {
        description: ticket.description,
        statusId: statusObj.id,
        priorityId: prioObj.id,
        categoryId: ticket.categoryId,
        assignedToId: ticket.assignedToId
      };

      this.ticketService.updateTicket(ticket.ticketId, payload).subscribe({
        next: () => this.snackBar.open(`Status updated to ${newStatus}`, 'OK', { duration: 2000 }),
        error: () => this.loadTickets()
      });
    }
  }

  getEmptyIcon(status: string): string {
    const icons: any = { 'Open': 'fiber_new', 'Assigned': 'assignment_ind', 'In Progress': 'pending', 'Resolved': 'verified' };
    return icons[status] || 'inbox';
  }

  goToTicket(id: number): void { this.router.navigate(['/manager/tickets', id]); }
}