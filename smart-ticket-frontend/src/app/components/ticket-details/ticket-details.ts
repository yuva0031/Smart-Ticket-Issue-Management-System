import { Component, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from '@angular/common';
import { Subscription, forkJoin } from 'rxjs';

// MATERIAL
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';

// SERVICES
import { TicketService } from '../../services/ticket.service';
import { TicketCommentService } from '../../services/ticket-comment.service';
import { TicketHistoryService } from '../../services/ticket-history.service';
import { LookupService } from '../../services/lookup.service';
import { AuthService } from '../../services/auth.service';
import { UserService } from '../../services/user.service';
import { SignalrService } from '../../services/signalr.service';

// MODELS
import {
  TicketComment,
  TicketResponseDto,
  TicketHistory,
  UpdateTicketDto,
  UserDto
} from '../../models/Model';

@Component({
  selector: 'app-ticket-details',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DatePipe,
    MatCardModule,
    MatChipsModule,
    MatInputModule,
    MatButtonModule,
    MatDividerModule,
    MatIconModule,
    MatMenuModule,
    MatSnackBarModule,
    MatTooltipModule
  ],
  templateUrl: './ticket-details.html',
  styleUrl: './ticket-details.scss'
})
export class TicketDetails implements OnInit, OnDestroy {

  // 🔹 STATE
  ticket = signal<TicketResponseDto | null>(null);
  comments = signal<TicketComment[]>([]);
  history = signal<TicketHistory[]>([]);
  reporterName = signal('Loading...');
  reporterEmail = signal('');
  assigneeEmail = signal('');
  agents = signal<UserDto[]>([]);
  allUsers = signal<UserDto[]>([]);
  newComment = signal('');
  internal = signal(false);
  loading = signal(false);
  activeTab = signal<'comments' | 'history'>('comments');

  // 🔹 STAGED CHANGES
  stagedStatusId = signal<number | null>(null);
  stagedPriorityId = signal<number | null>(null);
  stagedAssigneeId = signal<string | null | undefined>(undefined);

  priorities = computed(() => this.lookupService.priorities());
  statuses = computed(() => this.lookupService.statuses());

  private checkRole(roleName: string): boolean {
    const role = this.authService.user()?.role || '';
    return Array.isArray(role) ? role.includes(roleName) : role === roleName;
  }

  isAgent = computed(() => this.checkRole('SupportAgent'));
  isManager = computed(() => this.checkRole('SupportManager'));
  isUser = computed(() => this.checkRole('EndUser'));

  // Check if there are any staged changes
  hasChanges = computed(() => {
    return this.stagedStatusId() !== null || 
           this.stagedPriorityId() !== null || 
           this.stagedAssigneeId() !== undefined;
  });

  // Current display values (staged or original)
  currentStatusId = computed(() => {
    return this.stagedStatusId() ?? this.ticket()?.statusId;
  });

  currentPriorityId = computed(() => {
    return this.stagedPriorityId() ?? this.ticket()?.priorityId;
  });

  currentAssigneeId = computed(() => {
    const staged = this.stagedAssigneeId();
    return staged !== undefined ? staged : this.ticket()?.assignedToId;
  });

  currentStatusName = computed(() => {
    const id = this.currentStatusId();
    const status = this.statuses().find(s => s.id === id);
    return status?.name || this.ticket()?.status || '';
  });

  currentPriorityName = computed(() => {
    const id = this.currentPriorityId();
    const priority = this.priorities().find(p => p.id === id);
    return priority?.name || this.ticket()?.priority || '';
  });

  currentAssigneeName = computed(() => {
    const id = this.currentAssigneeId();
    if (id === null) return 'Unassigned';
    const agent = this.agents().find(a => a.userId === id);
    return agent?.fullName || this.ticket()?.assignedTo || 'Unassigned';
  });

  currentAssigneeEmail = computed(() => {
    const id = this.currentAssigneeId();
    if (id === null) return '';
    const agent = this.agents().find(a => a.userId === id);
    return agent?.email || this.assigneeEmail() || '';
  });

  // O(1) Lookup for emails
  userEmailMap = computed(() => {
    const map = new Map<string, string>();
    this.allUsers().forEach(u => map.set(u.userId, u.email));
    return map;
  });

  getUserEmail(userId?: string): string {
    if (!userId) return '';
    return this.userEmailMap().get(userId) || '';
  }

  // Check if ticket is cancelled or closed
  isTicketClosedOrCancelled = computed(() => {
    const status = this.ticket()?.status?.toLowerCase();
    return status === 'cancelled' || status === 'closed';
  });

  // Check if SLA is breached
  isSlaBreached = computed(() => {
    const dueDate = this.ticket()?.dueDate;
    if (!dueDate) return false;
    return new Date(dueDate) < new Date();
  });

  // Filtered comments based on role and isInternal flag
  filteredComments = computed(() => {
    const allComments = this.comments();
    const agent = this.isAgent();
    const manager = this.isManager();

    if (agent || manager) {
      return [...allComments].sort((c1, c2) => c1.commentId - c2.commentId);
    } else {
      return allComments
        .filter(c => !c.isInternal)
        .sort((c1, c2) => c1.commentId - c2.commentId);
    }
  });

  // Sorted history in descending order (most recent first)
  sortedHistory = computed(() => {
    return [...this.history()].sort((a, b) => {
      const dateA = new Date(a.changedAt).getTime();
      const dateB = new Date(b.changedAt).getTime();
      return dateB - dateA;
    });
  });

  ticketId!: number;

  private subs: Subscription[] = [];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private location: Location,
    private ticketService: TicketService,
    private commentService: TicketCommentService,
    private historyService: TicketHistoryService,
    private lookupService: LookupService,
    private authService: AuthService,
    private userService: UserService,
    private signalr: SignalrService,
    private snackBar: MatSnackBar
  ) {}

  async ngOnInit(): Promise<void> {
    this.ticketId = Number(this.route.snapshot.paramMap.get('ticketId'));
    this.loadInitialData();
    await this.signalr.startConnection();
    await this.signalr.invoke('JoinTicketRoom', this.ticketId);
    this.snackBar.open('Live updates connected', '', { duration: 2000 });
    this.registerRealtimeListeners();
  }

  ngOnDestroy(): void {
    this.subs.forEach(s => s.unsubscribe());
    this.signalr.invoke('LeaveTicketRoom', this.ticketId)
      .catch(() => {});
  }

  goBack(): void {
    this.location.back();
  }

  private registerRealtimeListeners(): void {
    this.subs.push(
      this.signalr.getTicketUpdated$().subscribe(ev => {
        if (!ev || ev.ticketId !== this.ticketId) return;

        this.snackBar.open('🔄 Ticket updated', 'OK', { duration: 2000 });
        this.loadTicket();
        this.loadHistory();
      })
    );

    this.subs.push(
      this.signalr.getCommentAdded$().subscribe(ev => {
        if (!ev || ev.ticketId !== this.ticketId) return;

        this.snackBar.open('💬 New comment added', 'OK', { duration: 2000 });
        this.loadComments();
        this.loadHistory();
      })
    );
  }

  private loadInitialData(): void {
    this.lookupService.loadPriorities();
    this.lookupService.loadStatuses();
    this.loadAllUsers();
    this.loadTicket();
    this.loadComments();
    this.loadHistory();
  }

  private loadAllUsers(): void {
    forkJoin({
      endUsers: this.userService.getEndUsers(),
      agents: this.userService.getSupportAgents()
    }).subscribe(({ endUsers, agents }) => {
      this.allUsers.set([...endUsers, ...agents]);
    });
  }

  private loadTicket(): void {
    this.ticketService.getTicketById(this.ticketId).subscribe({
      next: t => {
        this.ticket.set(t);
        if (t.owner) this.reporterName.set(t.owner);
        
        // Get reporter email
        if (t.ownerId) {
          const email = this.getUserEmail(t.ownerId);
          this.reporterEmail.set(email);
        }
        
        // Get assignee email
        if (t.assignedToId) {
          const email = this.getUserEmail(t.assignedToId);
          this.assigneeEmail.set(email);
        }
        
        this.loadAgents(t.categoryId);
        this.clearStagedChanges();
      }
    });
  }

  private loadComments(): void {
    this.commentService.getComments(this.ticketId).subscribe({
      next: c => this.comments.set(c)
    });
  }

  private loadHistory(): void {
    this.historyService.getHistory(this.ticketId).subscribe({
      next: h => this.history.set(h)
    });
  }

  private loadAgents(categoryId?: number): void {
    if (this.isAgent() || this.isManager()) {
      if (this.isManager()) {
        // Managers: Check if category is "Miscellaneous"
        const currentTicket = this.ticket();
        const isMiscellaneous = currentTicket?.category?.toLowerCase() === 'miscellaneous' || 
                                currentTicket?.category?.toLowerCase() === 'miscelleneous';
        
        // If miscellaneous or no category, show all agents
        // Otherwise show only agents for that specific category
        if (!categoryId || isMiscellaneous) {
          this.userService.getSupportAgents().subscribe(data => this.agents.set(data));
        } else {
          this.userService.getSupportAgents(categoryId).subscribe(data => this.agents.set(data));
        }
      } else {
        // Agents: Check if category is "Miscellaneous"
        const currentTicket = this.ticket();
        const isMiscellaneous = currentTicket?.category?.toLowerCase() === 'miscellaneous' || 
                                currentTicket?.category?.toLowerCase() === 'miscelleneous';
        
        // If miscellaneous, show all agents; otherwise filter by category
        if (isMiscellaneous) {
          this.userService.getSupportAgents().subscribe(data => this.agents.set(data));
        } else {
          this.userService.getSupportAgents(categoryId).subscribe(data => this.agents.set(data));
        }
      }
    }
  }

  private clearStagedChanges(): void {
    this.stagedStatusId.set(null);
    this.stagedPriorityId.set(null);
    this.stagedAssigneeId.set(undefined);
  }

  getPriorityName(id: string): string { 
    const p = this.priorities().find(x => x.id.toString() === id.toString()); 
    return p ? p.name : id; 
  }

  getStatusName(id: string): string { 
    const s = this.statuses().find(x => x.id.toString() === id.toString()); 
    return s ? s.name : id; 
  }

  getHistoryIcon(fieldName: string): string {
    const iconMap: { [key: string]: string } = {
      'Status': 'swap_horiz',
      'Priority': 'flag',
      'Assignee': 'person',
      'Description': 'description',
      'Category': 'category',
      'Due Date': 'event'
    };
    return iconMap[fieldName] || 'update';
  }

  // STAGE CHANGES
  stageStatus(statusId: number): void {
    this.stagedStatusId.set(statusId);
  }

  stagePriority(priorityId: number): void {
    this.stagedPriorityId.set(priorityId);
  }

  stageAssignee(agentId: string | null): void {
    this.stagedAssigneeId.set(agentId);
  }

  // UPDATE TICKET WITH ALL STAGED CHANGES
  updateTicket(): void {
    const t = this.ticket();
    if (!t) return;

    if (!this.hasChanges()) {
      this.snackBar.open('No changes to update', 'OK', { duration: 2000 });
      return;
    }

    this.loading.set(true);

    const updates: UpdateTicketDto = {
      description: t.description,
      categoryId: t.categoryId,
      statusId: this.stagedStatusId() ?? t.statusId,
      priorityId: this.stagedPriorityId() ?? t.priorityId,
      assignedToId: this.stagedAssigneeId() !== undefined ? this.stagedAssigneeId() : t.assignedToId,
      dueDate: t.dueDate
    };

    this.ticketService.updateTicket(this.ticketId, updates).subscribe({
      next: () => {
        this.snackBar.open('✅ Ticket updated successfully', 'OK', { duration: 3000 });
        this.loading.set(false);
        this.clearStagedChanges();
      },
      error: () => {
        this.snackBar.open('❌ Failed to update ticket', 'OK', { duration: 3000 });
        this.loading.set(false);
      }
    });
  }

  cancelTicket(): void {
    const t = this.ticket();
    if (!t) return;

    this.loading.set(true);

    const cancelledStatus = this.statuses().find(s => s.name.toLowerCase() === 'cancelled');
    
    if (!cancelledStatus) {
      this.snackBar.open('Cancelled status not found', 'OK', { duration: 3000 });
      this.loading.set(false);
      return;
    }

    const updates: UpdateTicketDto = {
      description: t.description,
      categoryId: t.categoryId,
      statusId: cancelledStatus.id,
      priorityId: t.priorityId,
      assignedToId: t.assignedToId,
      dueDate: t.dueDate
    };

    this.ticketService.updateTicket(this.ticketId, updates).subscribe({
      next: () => {
        this.snackBar.open('Ticket cancelled successfully', 'OK', { duration: 3000 });
        this.loading.set(false);
      },
      error: () => {
        this.snackBar.open('Failed to cancel ticket', 'OK', { duration: 3000 });
        this.loading.set(false);
      }
    });
  }

  addComment(): void {
    const text = this.newComment().trim();
    if (!text) return;
    
    // Prevent adding comments if ticket is closed or cancelled
    if (this.isTicketClosedOrCancelled()) {
      this.snackBar.open('Cannot add comments to closed or cancelled tickets', 'OK', { duration: 3000 });
      return;
    }
    
    this.loading.set(true);
    this.commentService.addComment(this.ticketId, { message: text, isInternal: this.internal() }).subscribe({
      next: () => {
        this.newComment.set(''); 
        this.internal.set(false); 
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }
}