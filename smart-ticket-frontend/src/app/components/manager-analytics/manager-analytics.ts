import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { RouterModule } from '@angular/router';
import { forkJoin } from 'rxjs';

// MATERIAL
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatMenuModule } from '@angular/material/menu';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatTableModule } from '@angular/material/table';

// SERVICES
import { TicketService } from '../../services/ticket.service';
import { LookupService } from '../../services/lookup.service';
import { AuthService } from '../../services/auth.service';
import { UserService } from '../../services/user.service';
import { AgentService, AgentProfile } from '../../services/agent.service';

// MODELS
import { TicketResponseDto, UserDto } from '../../models/Model';

interface DateRange {
  label: string;
  value: string;
  icon: string;
}

interface ManagerMetrics {
  totalTickets: number;
  unassigned: number;
  active: number;
  resolved: number;
  overdue: number;
  avgResolutionTime: number;
}

interface MetricCard {
  title: string;
  value: number | string;
  change: number;
  changeLabel: string;
  icon: string;
  color: string;
  tooltip: string;
}

interface TeamPerformance {
  agentName: string;
  agentId: string;
  assigned: number;
  resolved: number;
  active: number;
  avgResolutionTime: number;
  resolutionRate: number;
  workload: number;
}

interface CategoryBreakdown {
  category: string;
  total: number;
  unassigned: number;
  assigned: number;
  resolved: number;
  percentage: number;
}

interface PriorityDistribution {
  priority: string;
  count: number;
  percentage: number;
  color: string;
}

interface StatusDistribution {
  status: string;
  count: number;
  percentage: number;
  color: string;
}

interface SLACompliance {
  total: number;
  compliant: number;
  breached: number;
  complianceRate: number;
}

interface TrendData {
  period: string;
  created: number;
  resolved: number;
  avgResolutionTime: number;
  createdHeight: number;
  resolvedHeight: number;
}

interface DonutSegment {
  dashArray: string;
  dashOffset: number;
  color: string;
}

@Component({
  selector: 'app-manager-analytics',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    DatePipe,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatTooltipModule,
    MatMenuModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    MatTableModule
  ],
  templateUrl: './manager-analytics.html',
  styleUrl: './manager-analytics.scss'
})
export class ManagerAnalytics implements OnInit {

  // STATE
  tickets = signal<TicketResponseDto[]>([]);
  agents = signal<UserDto[]>([]);
  agentProfiles = signal<AgentProfile[]>([]);
  loading = signal(false);
  selectedDateRange = signal<string>('all');
  
  // Date Range Options
  dateRanges: DateRange[] = [
    { label: 'All Time', value: 'all', icon: 'all_inclusive' },
    { label: 'Last 7 Days', value: '7days', icon: 'calendar_today' },
    { label: 'Last 14 Days', value: '14days', icon: 'date_range' },
    { label: 'Last 30 Days', value: '30days', icon: 'calendar_month' },
    { label: 'Last 90 Days', value: '90days', icon: 'event_note' }
  ];

  // Table Columns
  displayedColumns = ['agent', 'assigned', 'active', 'resolved', 'resolutionRate', 'avgTime', 'workload'];

  constructor(
    private ticketService: TicketService,
    private lookupService: LookupService,
    private authService: AuthService,
    private userService: UserService,
    private agentService: AgentService
  ) {}

  ngOnInit(): void {
    this.lookupService.loadStatuses();
    this.lookupService.loadPriorities();
    this.loadData();
  }

  // =============================
  // DATA LOADING
  // =============================
  loadData(): void {
    this.loading.set(true);

    forkJoin({
      tickets: this.ticketService.getAllTickets(),
      agents: this.userService.getSupportAgents(),
      agentProfiles: this.agentService.getAllAgentProfiles()
    }).subscribe({
      next: ({ tickets, agents, agentProfiles }) => {
        this.tickets.set(tickets);
        this.agents.set(agents);
        this.agentProfiles.set(agentProfiles);
        this.loading.set(false);
      },
      error: err => {
        console.error('Error loading manager analytics:', err);
        this.loading.set(false);
      }
    });
    console.log(this.agents());
  }

  refreshData(): void {
    this.loadData();
  }

  // =============================
  // DATE RANGE FILTERING
  // =============================
  selectDateRange(range: string): void {
    this.selectedDateRange.set(range);
  }

  private getFilteredTickets(): TicketResponseDto[] {
    const allTickets = this.tickets();
    const range = this.selectedDateRange();

    if (range === 'all') {
      return allTickets;
    }

    const now = new Date();
    let cutoffDate: Date;

    switch (range) {
      case '7days':
        cutoffDate = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
        break;
      case '14days':
        cutoffDate = new Date(now.getTime() - 14 * 24 * 60 * 60 * 1000);
        break;
      case '30days':
        cutoffDate = new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000);
        break;
      case '90days':
        cutoffDate = new Date(now.getTime() - 90 * 24 * 60 * 60 * 1000);
        break;
      default:
        return allTickets;
    }

    return allTickets.filter(ticket => {
      const ticketDate = new Date(ticket.createdAt || 0);
      return ticketDate >= cutoffDate;
    });
  }

  // =============================
  // COMPUTED: MANAGER METRICS
  // =============================
  managerMetrics = computed<ManagerMetrics>(() => {
    const filteredTickets = this.getFilteredTickets();
    const now = new Date().getTime();
    
    const resolved = filteredTickets.filter(t => 
      t.status === 'Resolved' || t.status === 'Closed'
    );

    let totalResolutionDays = 0;
    resolved.forEach(t => {
      const created = new Date(t.createdAt || 0).getTime();
      const updated = new Date(t.updatedAt || t.createdAt || 0).getTime();
      totalResolutionDays += (updated - created) / (1000 * 60 * 60 * 24);
    });

    const avgResolutionTime = resolved.length > 0 
      ? Math.round((totalResolutionDays / resolved.length) * 10) / 10
      : 0;
    
    return {
      totalTickets: filteredTickets.length,
      unassigned: filteredTickets.filter(t => !t.assignedToId).length,
      active: filteredTickets.filter(t => 
        t.status === 'Assigned' || t.status === 'In Progress'
      ).length,
      resolved: resolved.length,
      overdue: filteredTickets.filter(t => {
        if (!t.dueDate) return false;
        return new Date(t.dueDate).getTime() < now && 
               t.status !== 'Resolved' && 
               t.status !== 'Closed';
      }).length,
      avgResolutionTime
    };
  });

  // COMPUTED: Metric Cards
  metricCards = computed<MetricCard[]>(() => {
    const metrics = this.managerMetrics();
    const total = metrics.totalTickets || 1;
    
    const resolvedRate = Math.round((metrics.resolved / total) * 100);
    const unassignedRate = metrics.unassigned > 0 ? -Math.round((metrics.unassigned / total) * 100) : 0;
    
    return [
      {
        title: 'Total Tickets',
        value: metrics.totalTickets,
        change: 15,
        changeLabel: 'vs previous period',
        icon: 'confirmation_number',
        color: 'primary',
        tooltip: 'All tickets in the system for selected period'
      },
      {
        title: 'Unassigned',
        value: metrics.unassigned,
        change: unassignedRate,
        changeLabel: 'needs assignment',
        icon: 'assignment_late',
        color: 'warn',
        tooltip: 'Tickets waiting for agent assignment'
      },
      {
        title: 'Active',
        value: metrics.active,
        change: 8,
        changeLabel: 'in progress',
        icon: 'pending_actions',
        color: 'accent',
        tooltip: 'Tickets currently being worked on'
      },
      {
        title: 'Resolved',
        value: metrics.resolved,
        change: resolvedRate > 0 ? 12 : 0,
        changeLabel: `${resolvedRate}% resolution rate`,
        icon: 'check_circle',
        color: 'success',
        tooltip: 'Successfully resolved tickets'
      },
      {
        title: 'Overdue',
        value: metrics.overdue,
        change: metrics.overdue > 0 ? -10 : 0,
        changeLabel: 'SLA breached',
        icon: 'warning',
        color: 'error',
        tooltip: 'Tickets past their due date'
      },
      {
        title: 'Avg Resolution',
        value: `${metrics.avgResolutionTime}d`,
        change: -5,
        changeLabel: 'improvement',
        icon: 'schedule',
        color: 'info',
        tooltip: 'Average time to resolve tickets'
      }
    ];
  });

  // =============================
  // COMPUTED: TEAM PERFORMANCE
  // =============================
  teamPerformance = computed<TeamPerformance[]>(() => {
    const filteredTickets = this.getFilteredTickets();
    const agents = this.agents();
    const profiles = this.agentProfiles();

    return agents.map(agent => {
      const agentTickets = filteredTickets.filter(t => t.assignedToId === agent.userId);
      const resolved = agentTickets.filter(t => 
        t.status === 'Resolved' || t.status === 'Closed'
      );
      const active = agentTickets.filter(t => 
        t.status === 'Assigned' || t.status === 'In Progress'
      );

      let totalResolutionDays = 0;
      resolved.forEach(t => {
        const created = new Date(t.createdAt || 0).getTime();
        const updated = new Date(t.updatedAt || t.createdAt || 0).getTime();
        totalResolutionDays += (updated - created) / (1000 * 60 * 60 * 24);
      });

      const avgResolutionTime = resolved.length > 0 
        ? Math.round((totalResolutionDays / resolved.length) * 10) / 10
        : 0;

      const resolutionRate = agentTickets.length > 0
        ? Math.round((resolved.length / agentTickets.length) * 100)
        : 0;

      const profile = profiles.find(p => p.userId === agent.userId);
      const workload = profile?.currentWorkload || 0;

      return {
        agentName: agent.email.split('@')[0],
        agentId: agent.userId,
        assigned: agentTickets.length,
        resolved: resolved.length,
        active: active.length,
        avgResolutionTime,
        resolutionRate,
        workload
      };
    }).sort((a, b) => b.resolved - a.resolved);
  });

  // =============================
  // COMPUTED: CATEGORY BREAKDOWN
  // =============================
  categoryBreakdown = computed<CategoryBreakdown[]>(() => {
    const filteredTickets = this.getFilteredTickets();
    const total = filteredTickets.length || 1;

    const categoryMap = new Map<string, TicketResponseDto[]>();
    filteredTickets.forEach(t => {
      const cat = t.category || 'Uncategorized';
      if (!categoryMap.has(cat)) categoryMap.set(cat, []);
      categoryMap.get(cat)!.push(t);
    });

    return Array.from(categoryMap.entries())
      .map(([category, tickets]) => ({
        category,
        total: tickets.length,
        unassigned: tickets.filter(t => !t.assignedToId).length,
        assigned: tickets.filter(t => t.assignedToId && 
          (t.status === 'Assigned' || t.status === 'In Progress')).length,
        resolved: tickets.filter(t => 
          t.status === 'Resolved' || t.status === 'Closed').length,
        percentage: Math.round((tickets.length / total) * 100)
      }))
      .sort((a, b) => b.total - a.total)
      .slice(0, 5);
  });

  // =============================
  // COMPUTED: PRIORITY DISTRIBUTION
  // =============================
  priorityDistribution = computed<PriorityDistribution[]>(() => {
    const filteredTickets = this.getFilteredTickets();
    const total = filteredTickets.length || 1;
    
    const priorities = ['Critical', 'High', 'Medium', 'Low'];
    const colors = ['#d32f2f', '#f44336', '#ff9800', '#4caf50'];
    
    return priorities.map((priority, idx) => {
      const count = filteredTickets.filter(t => t.priority === priority).length;
      return {
        priority,
        count,
        percentage: Math.round((count / total) * 100),
        color: colors[idx]
      };
    }).filter(p => p.count > 0);
  });

  // =============================
  // COMPUTED: STATUS DISTRIBUTION
  // =============================
  statusDistribution = computed<StatusDistribution[]>(() => {
    const filteredTickets = this.getFilteredTickets();
    const total = filteredTickets.length || 1;
    
    const statusMap = new Map<string, number>();
    filteredTickets.forEach(t => {
      statusMap.set(t.status, (statusMap.get(t.status) || 0) + 1);
    });
    
    const colorMap: Record<string, string> = {
      'Open': '#2196f3',
      'Assigned': '#9c27b0',
      'In Progress': '#ff9800',
      'Resolved': '#4caf50',
      'Closed': '#607d8b',
      'On Hold': '#795548'
    };
    
    return Array.from(statusMap.entries())
      .map(([status, count]) => ({
        status,
        count,
        percentage: Math.round((count / total) * 100),
        color: colorMap[status] || '#9e9e9e'
      }))
      .sort((a, b) => b.count - a.count);
  });

  // COMPUTED: Status Donut Segments
  statusSegments = computed<DonutSegment[]>(() => {
    const distribution = this.statusDistribution();
    const circumference = 2 * Math.PI * 80;
    let currentOffset = 0;

    return distribution.map(item => {
      const segmentLength = (item.percentage / 100) * circumference;
      const segment = {
        dashArray: `${segmentLength} ${circumference}`,
        dashOffset: -currentOffset,
        color: item.color
      };
      currentOffset += segmentLength;
      return segment;
    });
  });

  // =============================
  // COMPUTED: SLA COMPLIANCE
  // =============================
  slaCompliance = computed<SLACompliance>(() => {
    const filteredTickets = this.getFilteredTickets();
    const now = new Date().getTime();

    const total = filteredTickets.filter(t => t.dueDate).length;
    const breached = filteredTickets.filter(t => {
      if (!t.dueDate) return false;
      return new Date(t.dueDate).getTime() < now && 
             t.status !== 'Resolved' && 
             t.status !== 'Closed';
    }).length;

    const compliant = total - breached;
    const complianceRate = total > 0 ? Math.round((compliant / total) * 100) : 100;

    return {
      total,
      compliant,
      breached,
      complianceRate
    };
  });

  // =============================
  // COMPUTED: TREND DATA (FIXED)
  // =============================
  trendData = computed<TrendData[]>(() => {
    const filteredTickets = this.getFilteredTickets();
    
    // Group by week for last 4 weeks
    const weeks: TrendData[] = [];
    for (let i = 3; i >= 0; i--) {
      const weekStart = new Date();
      weekStart.setDate(weekStart.getDate() - (i * 7 + 7));
      const weekEnd = new Date();
      weekEnd.setDate(weekEnd.getDate() - (i * 7));

      const weekTickets = filteredTickets.filter(t => {
        const date = new Date(t.createdAt || 0);
        return date >= weekStart && date < weekEnd;
      });

      const resolved = weekTickets.filter(t => 
        t.status === 'Resolved' || t.status === 'Closed'
      );

      let totalResolutionDays = 0;
      resolved.forEach(t => {
        const created = new Date(t.createdAt || 0).getTime();
        const updated = new Date(t.updatedAt || t.createdAt || 0).getTime();
        totalResolutionDays += (updated - created) / (1000 * 60 * 60 * 24);
      });

      weeks.push({
        period: `Week ${4 - i}`,
        created: weekTickets.length,
        resolved: resolved.length,
        avgResolutionTime: resolved.length > 0 
          ? Math.round(totalResolutionDays / resolved.length) 
          : 0,
        createdHeight: 0,
        resolvedHeight: 0
      });
    }

    // Calculate heights to fit within container (max 100%)
    const maxValue = Math.max(...weeks.map(w => Math.max(w.created, w.resolved)));
    
    weeks.forEach(week => {
      // Scale to max 95% to leave some space at top
      week.createdHeight = maxValue > 0 ? Math.min((week.created / maxValue) * 95, 95) : 0;
      week.resolvedHeight = maxValue > 0 ? Math.min((week.resolved / maxValue) * 95, 95) : 0;
    });

    return weeks;
  });

  // =============================
  // UTILITY METHODS
  // =============================
  getStatusColor(status: string): string {
    const colorMap: { [key: string]: string } = {
      'Open': '#2196f3',
      'Assigned': '#9c27b0',
      'In Progress': '#ff9800',
      'Resolved': '#4caf50',
      'Closed': '#607d8b',
      'On Hold': '#795548'
    };
    return colorMap[status] || '#6b778c';
  }

  getPriorityColor(priority: string): string {
    const colorMap: { [key: string]: string } = {
      'Critical': '#d32f2f',
      'High': '#f44336',
      'Medium': '#ff9800',
      'Low': '#4caf50'
    };
    return colorMap[priority] || '#6b778c';
  }

  getWorkloadClass(workload: number): string {
    if (workload >= 80) return 'workload-high';
    if (workload >= 50) return 'workload-medium';
    return 'workload-low';
  }

  getCurrentDateRangeLabel(): string {
    const current = this.selectedDateRange();
    const range = this.dateRanges.find(r => r.value === current);
    return range?.label || 'All Time';
  }

  getCurrentDateRangeIcon(): string {
    const current = this.selectedDateRange();
    const range = this.dateRanges.find(r => r.value === current);
    return range?.icon || 'all_inclusive';
  }

  // Check if user is a manager
  isManager = computed(() => {
    const user = this.authService.user();
    const role = user?.role || '';
    
    if (Array.isArray(role)) {
      return role.includes('SupportManager') || role.includes('Admin');
    }
    
    return role === 'SupportManager' || role === 'Admin';
  });
}