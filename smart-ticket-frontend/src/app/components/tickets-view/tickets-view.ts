import { Component, OnInit, effect, signal, computed, inject } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDividerModule } from '@angular/material/divider';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterModule } from '@angular/router';
import { forkJoin } from 'rxjs';

import { TicketService } from '../../services/ticket.service';
import { AuthService } from '../../services/auth.service';
import { LookupService } from '../../services/lookup.service';
import { UserService } from '../../services/user.service';
import { TicketResponseDto, UserDto } from '../../models/Model';

@Component({
  selector: 'app-tickets-view',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule, 
    MatTableModule, 
    MatButtonModule, 
    MatPaginatorModule,
    MatIconModule, 
    MatChipsModule, 
    MatSelectModule, 
    MatFormFieldModule, 
    MatInputModule,
    MatDividerModule, 
    RouterModule, 
    DatePipe, 
    MatTooltipModule
  ],
  templateUrl: './tickets-view.html',
  styleUrl: './tickets-view.scss'
})
export class TicketsView implements OnInit {
  private ticketService = inject(TicketService);
  private authService = inject(AuthService);
  private lookupService = inject(LookupService);
  private userService = inject(UserService);

  // User Context
  currentUser = computed(() => this.authService.user());
  isManager = computed(() => this.hasRole('SupportManager') || this.hasRole('Admin'));
  isAgent = computed(() => this.hasRole('SupportAgent'));

  // Signals
  tickets = signal<TicketResponseDto[]>([]);
  allUsers = signal<UserDto[]>([]); 
  searchQuery = signal<string>('');
  pageIndex = signal(0);
  pageSize = signal(10);

  // Multi-Selection Signals
  selectedPriorities = signal<string[]>([]);
  selectedStatuses = signal<string[]>([]);

  // Lookups
  priorities = computed(() => this.lookupService.priorities());
  statuses = computed(() => this.lookupService.statuses());

  // Dynamic Columns based on Role
  displayedColumns = computed(() => {
    if (this.isManager()) {
      return [
        'ticketId', 'title', 'owner', 'category', 'priority', 
        'assignedTo', 'status', 'progress', 'dueDate', 'actions'
      ];
    }
    
    if (this.isAgent()) {
      return [
        'ticketId', 'title', 'owner', 'category', 'priority', 
        'status', 'progress', 'createdAt', 'actions'
      ];
    }

    // Default: End User columns
    return [
      'ticketId', 'title', 'category', 'priority', 
      'assignedTo', 'status', 'progress', 'dueDate', 'actions'
    ];
  });

  constructor() {
    effect(() => {
      this.loadTickets();
    }, { allowSignalWrites: true });
  }

  // O(1) Lookup for emails
  userEmailMap = computed(() => {
    const map = new Map<string, string>();
    this.allUsers().forEach(u => map.set(u.userId, u.email));
    return map;
  });

  getUserEmail(userId?: string): string {
    if (!userId) return 'N/A';
    return this.userEmailMap().get(userId) || 'Checking system...';
  }

  // Main Reactive Filtering & Sorting Logic
  visibleTickets = computed(() => {
    let result = [...this.tickets()];

    // 1. Multi-Priority Filter
    const activePrios = this.selectedPriorities();
    if (activePrios.length > 0) {
      result = result.filter(t => activePrios.includes(t.priority));
    }

    // 2. Multi-Status Filter
    const activeStats = this.selectedStatuses();
    if (activeStats.length > 0) {
      result = result.filter(t => activeStats.includes(t.status));
    }

    // 3. Search Filter (Title, ID, Reporter, Assignee)
    const query = this.searchQuery().toLowerCase().trim();
    if (query) {
      result = result.filter(t => 
        t.title.toLowerCase().includes(query) || 
        t.ticketId.toString().includes(query) ||
        (t.owner && t.owner.toLowerCase().includes(query)) ||
        (t.assignedTo && t.assignedTo.toLowerCase().includes(query))
      );
    }

    // 4. Role-Based Sorting
    result.sort((a, b) => {
      // Logic for Managers: Unassigned first
      if (this.isManager()) {
        const aAssigned = a.assignedToId ? 1 : 0;
        const bAssigned = b.assignedToId ? 1 : 0;
        if (aAssigned !== bAssigned) {
          return aAssigned - bAssigned;
        }
      }

      // Sort by Due Date (Closest/Overdue first)
      const dateA = a.dueDate ? new Date(a.dueDate).getTime() : Infinity;
      const dateB = b.dueDate ? new Date(b.dueDate).getTime() : Infinity;

      if (dateA !== dateB) {
        return dateA - dateB;
      }

      // Fallback: Created Date (Newest first)
      return new Date(b.createdAt || 0).getTime() - new Date(a.createdAt || 0).getTime();
    });

    const start = this.pageIndex() * this.pageSize();
    return result.slice(start, start + this.pageSize());
  });

  ngOnInit(): void {
    this.lookupService.loadPriorities();
    this.lookupService.loadStatuses();
    this.loadInitialData();
  }

  loadInitialData() {
    forkJoin({
      tickets: this.isManager() ? this.ticketService.getAllTickets() : 
               this.isAgent() ? this.ticketService.getAssignedTickets() : 
               this.ticketService.getTicketsByOwner(),
      endUsers: this.userService.getEndUsers(),
      agents: this.userService.getSupportAgents()
    }).subscribe(({ tickets, endUsers, agents }) => {
      this.tickets.set(tickets);
      this.allUsers.set([...endUsers, ...agents]);
    });
  }

  loadTickets() {
    if (this.isManager()) this.ticketService.getAllTickets().subscribe(t => this.tickets.set(t));
    else if (this.isAgent()) this.ticketService.getAssignedTickets().subscribe(t => this.tickets.set(t));
    else this.ticketService.getTicketsByOwner().subscribe(t => this.tickets.set(t));
  }

  // Filter Management
  toggleFilter(filterSignal: any, value: string) {
    const current = filterSignal();
    if (current.includes(value)) {
      filterSignal.set(current.filter((v: string) => v !== value));
    } else {
      filterSignal.set([...current, value]);
    }
    this.pageIndex.set(0);
  }

  clearFilters() {
    this.selectedPriorities.set([]);
    this.selectedStatuses.set([]);
    this.pageIndex.set(0);
  }

  getSLAStatus(dueDate?: string) {
    if (!dueDate) return { label: 'In Track', class: 'in-track' };
    const over = new Date(dueDate).getTime() < new Date().getTime();
    return over ? { label: 'SLA Breached', class: 'breached' } : { label: 'In Track', class: 'in-track' };
  }

  private hasRole(r: string): boolean {
    const u = this.currentUser();
    return Array.isArray(u?.role) ? u.role.includes(r) : u?.role === r;
  }

  onPageChange(e: any) { 
    this.pageIndex.set(e.pageIndex); 
    this.pageSize.set(e.pageSize); 
  }

  onSearch(e: Event) { 
    this.searchQuery.set((e.target as HTMLInputElement).value); 
    this.pageIndex.set(0);
  }
  
  getActionRoute(t: any) { 
    const prefix = this.isManager() ? '/manager' : this.isAgent() ? '/agent' : '/app';
    return `${prefix}/tickets/${t.ticketId}`; 
  }
}