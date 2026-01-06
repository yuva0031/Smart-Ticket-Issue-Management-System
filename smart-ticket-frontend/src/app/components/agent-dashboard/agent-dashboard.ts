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
  selector: 'app-agent-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, MatCardModule, MatChipsModule, MatIconModule, MatButtonModule, MatSnackBarModule, DragDropModule],
  templateUrl: './agent-dashboard.html',
  styleUrl: './agent-dashboard.scss'
})
export class AgentDashboard implements OnInit {
  private ticketService = inject(TicketService);
  private lookupService = inject(LookupService);
  private snackBar = inject(MatSnackBar);
  private router = inject(Router);

  tickets = signal<TicketResponseDto[]>([]);
  selectedPriorities = signal<string[]>([]);
  availablePriorities = signal(['Critical', 'High', 'Medium', 'Low']);

  stats = computed(() => {
    const all = this.tickets();
    return {
      total: all.length,
      active: all.filter(t => ['Assigned', 'In Progress'].includes(t.status)).length,
      resolved: all.filter(t => ['Resolved', 'Closed'].includes(t.status)).length
    };
  });

  columns = computed(() => {
    const activePrios = this.selectedPriorities();
    const excluded = ['Closed', 'Cancelled'];
    const filtered = this.tickets().filter(t => !excluded.includes(t.status));
    const finalTickets = activePrios.length > 0 ? filtered.filter(t => activePrios.includes(t.priority)) : filtered;

    return this.availablePriorities().map(p => ({
      priority: p,
      tickets: finalTickets.filter(t => t.priority === p)
    }));
  });

  ngOnInit(): void {
    this.lookupService.loadStatuses();
    this.lookupService.loadPriorities();
    this.loadTickets();
  }

  loadTickets(): void {
    this.ticketService.getAssignedTickets().subscribe({ next: (t) => this.tickets.set(t) });
  }

  togglePriority(p: string): void {
    this.selectedPriorities.update(curr => curr.includes(p) ? curr.filter(x => x !== p) : [...curr, p]);
  }

  clearFilters(): void { this.selectedPriorities.set([]); }

  drop(event: CdkDragDrop<TicketResponseDto[]>, targetPrio: string): void {
    if (event.previousContainer === event.container) {
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
    } else {
      const ticket = event.previousContainer.data[event.previousIndex];
      const prioObj = this.lookupService.priorities().find(p => p.name === targetPrio);
      const statusObj = this.lookupService.statuses().find(s => s.name === ticket.status);

      if (!prioObj || !statusObj) return;

      transferArrayItem(event.previousContainer.data, event.container.data, event.previousIndex, event.currentIndex);

      const payload: UpdateTicketDto = {
        description: ticket.description,
        priorityId: prioObj.id,
        statusId: statusObj.id,
        categoryId: ticket.categoryId,
        assignedToId: ticket.assignedToId
      };

      this.ticketService.updateTicket(ticket.ticketId, payload).subscribe({
        next: () => this.snackBar.open(`Priority set to ${targetPrio}`, 'OK', { duration: 2000 }),
        error: () => this.loadTickets()
      });
    }
  }

  getEmptyIcon(priority: string): string {
    const icons: any = { 
      'Critical': 'report_problem', 
      'High': 'priority_high', 
      'Medium': 'low_priority', 
      'Low': 'assignment_turned_in' 
    };
    return icons[priority] || 'inbox';
  }

  goToTicket(id: number): void { this.router.navigate(['/agent/tickets', id]); }
}