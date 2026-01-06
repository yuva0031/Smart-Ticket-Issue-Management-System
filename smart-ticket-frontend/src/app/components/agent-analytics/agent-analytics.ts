import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { RouterModule } from '@angular/router';

// MATERIAL
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';

// SERVICES
import { TicketService } from '../../services/ticket.service';
import { LookupService } from '../../services/lookup.service';
import { AuthService } from '../../services/auth.service';

// MODELS
import { TicketResponseDto } from '../../models/Model';

interface DateRange {
  label: string;
  value: string;
  icon: string;
}

interface AgentMetrics {
  totalAssigned: number;
  active: number;
  resolved: number;
  overdue: number;
  highPriority: number;
}

interface MetricCard {
  title: string;
  value: number;
  change: number;
  changeLabel: string;
  icon: string;
  color: string;
  tooltip: string;
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

interface PerformanceMetric {
  label: string;
  value: number;
  unit: string;
  trend: 'up' | 'down' | 'neutral';
  trendValue: number;
}

interface CategoryInsight {
  category: string;
  count: number;
  avgResolutionTime: number;
  percentage: number;
}

interface WorkloadDay {
  day: string;
  count: number;
  percentage: number;
}

interface DonutSegment {
  dashArray: string;
  dashOffset: number;
  color: string;
}

@Component({
  selector: 'app-agent-analytics',
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
    MatSelectModule,
    MatFormFieldModule,
    MatProgressSpinnerModule,
    MatMenuModule,
    MatDividerModule
  ],
  templateUrl: './agent-analytics.html',
  styleUrl: './agent-analytics.scss'
})
export class AgentAnalytics implements OnInit {

  // STATE
  tickets = signal<TicketResponseDto[]>([]);
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

  constructor(
    private ticketService: TicketService,
    private lookupService: LookupService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.lookupService.loadStatuses();
    this.lookupService.loadPriorities();
    this.loadTickets();
  }

  // =============================
  // DATA LOADING
  // =============================
  loadTickets(): void {
    this.loading.set(true);

    this.ticketService.getAssignedTickets().subscribe({
      next: tickets => {
        this.tickets.set(tickets);
        this.loading.set(false);
      },
      error: err => {
        console.error('Error loading agent tickets:', err);
        this.loading.set(false);
      }
    });
  }

  refreshData(): void {
    this.loadTickets();
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
  // COMPUTED: CORE METRICS
  // =============================
  agentMetrics = computed<AgentMetrics>(() => {
    const filteredTickets = this.getFilteredTickets();
    const now = new Date().getTime();
    
    return {
      totalAssigned: filteredTickets.length,
      active: filteredTickets.filter(t => 
        t.status === 'Assigned' || t.status === 'In Progress'
      ).length,
      resolved: filteredTickets.filter(t => 
        t.status === 'Resolved' || t.status === 'Closed'
      ).length,
      overdue: filteredTickets.filter(t => {
        if (!t.dueDate) return false;
        return new Date(t.dueDate).getTime() < now && 
               t.status !== 'Resolved' && 
               t.status !== 'Closed';
      }).length,
      highPriority: filteredTickets.filter(t => t.priority === 'High').length
    };
  });

  // COMPUTED: Metric Cards with Trends
  metricCards = computed<MetricCard[]>(() => {
    const metrics = this.agentMetrics();
    const total = metrics.totalAssigned || 1;
    
    const resolvedRate = Math.round((metrics.resolved / total) * 100);
    const activeRate = Math.round((metrics.active / total) * 100);
    const overdueRate = metrics.overdue > 0 ? -Math.round((metrics.overdue / total) * 100) : 0;
    
    return [
      {
        title: 'Total Assigned',
        value: metrics.totalAssigned,
        change: 12,
        changeLabel: 'vs previous period',
        icon: 'confirmation_number',
        color: 'primary',
        tooltip: 'Total tickets assigned to you in selected period'
      },
      {
        title: 'Active Tickets',
        value: metrics.active,
        change: -5,
        changeLabel: 'vs previous period',
        icon: 'pending_actions',
        color: 'accent',
        tooltip: 'Tickets currently assigned or in progress'
      },
      {
        title: 'Resolved',
        value: metrics.resolved,
        change: resolvedRate > 0 ? 15 : 0,
        changeLabel: `${resolvedRate}% resolution rate`,
        icon: 'check_circle',
        color: 'success',
        tooltip: 'Successfully resolved tickets'
      },
      {
        title: 'Overdue',
        value: metrics.overdue,
        change: overdueRate,
        changeLabel: 'needs attention',
        icon: 'warning',
        color: 'warn',
        tooltip: 'Tickets past their due date'
      },
      {
        title: 'High Priority',
        value: metrics.highPriority,
        change: 8,
        changeLabel: 'vs previous period',
        icon: 'priority_high',
        color: 'error',
        tooltip: 'High priority tickets requiring immediate attention'
      }
    ];
  });

  // =============================
  // COMPUTED: PRIORITY DISTRIBUTION
  // =============================
  priorityDistribution = computed<PriorityDistribution[]>(() => {
    const filteredTickets = this.getFilteredTickets();
    const total = filteredTickets.length || 1;
    
    const priorities = ['High', 'Medium', 'Low'];
    const colors = ['#f44336', '#ff9800', '#4caf50'];
    
    return priorities.map((priority, idx) => {
      const count = filteredTickets.filter(t => t.priority === priority).length;
      return {
        priority,
        count,
        percentage: Math.round((count / total) * 100),
        color: colors[idx]
      };
    });
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
    const circumference = 2 * Math.PI * 80; // radius = 80
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
  // COMPUTED: PERFORMANCE METRICS
  // =============================
  performanceMetrics = computed<PerformanceMetric[]>(() => {
    const filteredTickets = this.getFilteredTickets();
    const resolved = filteredTickets.filter(t => 
      t.status === 'Resolved' || t.status === 'Closed'
    );
    
    // Average resolution time calculation
    const avgResolutionTime = resolved.length > 0 
      ? Math.round(resolved.reduce((acc, t) => {
          const created = new Date(t.createdAt || '').getTime();
          const updated = new Date(t.updatedAt || '').getTime();
          return acc + ((updated - created) / (1000 * 60 * 60 * 24));
        }, 0) / resolved.length)
      : 0;
    
    // First response time (mock - would need actual response data)
    const avgFirstResponse = 2.5;
    
    // Resolution rate
    const resolutionRate = filteredTickets.length > 0 
      ? Math.round((resolved.length / filteredTickets.length) * 100)
      : 0;
    
    // Customer satisfaction (mock - would need actual feedback data)
    const satisfaction = 4.2;
    
    return [
      {
        label: 'Avg Resolution Time',
        value: avgResolutionTime,
        unit: 'days',
        trend: avgResolutionTime <= 3 ? 'down' : avgResolutionTime <= 5 ? 'neutral' : 'up',
        trendValue: 15
      },
      {
        label: 'First Response Time',
        value: avgFirstResponse,
        unit: 'hours',
        trend: 'down',
        trendValue: 8
      },
      {
        label: 'Resolution Rate',
        value: resolutionRate,
        unit: '%',
        trend: resolutionRate >= 75 ? 'up' : resolutionRate >= 50 ? 'neutral' : 'down',
        trendValue: 12
      },
      {
        label: 'Satisfaction Score',
        value: satisfaction,
        unit: '/5',
        trend: satisfaction >= 4 ? 'up' : satisfaction >= 3 ? 'neutral' : 'down',
        trendValue: 5
      }
    ];
  });

  // =============================
  // COMPUTED: CATEGORY INSIGHTS
  // =============================
  categoryInsights = computed<CategoryInsight[]>(() => {
    const filteredTickets = this.getFilteredTickets();
    const total = filteredTickets.length || 1;
    
    const categoryMap = new Map<string, TicketResponseDto[]>();
    filteredTickets.forEach(t => {
      const cat = t.category || 'Uncategorized';
      if (!categoryMap.has(cat)) categoryMap.set(cat, []);
      categoryMap.get(cat)!.push(t);
    });
    
    return Array.from(categoryMap.entries())
      .map(([category, catTickets]) => {
        const resolved = catTickets.filter(t => 
          t.status === 'Resolved' || t.status === 'Closed'
        );
        
        const avgResolutionTime = resolved.length > 0
          ? Math.round(resolved.reduce((acc, t) => {
              const created = new Date(t.createdAt || '').getTime();
              const updated = new Date(t.updatedAt || '').getTime();
              return acc + ((updated - created) / (1000 * 60 * 60 * 24));
            }, 0) / resolved.length)
          : 0;
        
        return {
          category,
          count: catTickets.length,
          avgResolutionTime,
          percentage: Math.round((catTickets.length / total) * 100)
        };
      })
      .sort((a, b) => b.count - a.count)
      .slice(0, 5);
  });

  // =============================
  // COMPUTED: WORKLOAD DISTRIBUTION
  // =============================
  workloadByDay = computed<WorkloadDay[]>(() => {
    const filteredTickets = this.getFilteredTickets();
    const days = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];
    const distribution = new Array(7).fill(0);
    
    filteredTickets.forEach(t => {
      const day = new Date(t.createdAt || '').getDay();
      distribution[day === 0 ? 6 : day - 1]++;
    });
    
    const max = Math.max(...distribution, 1);
    
    return days.map((day, idx) => ({
      day,
      count: distribution[idx],
      percentage: Math.round((distribution[idx] / max) * 100)
    }));
  });

  // =============================
  // COMPUTED: RECENT TICKETS
  // =============================
  recentTickets = computed<TicketResponseDto[]>(() => {
    const filteredTickets = this.getFilteredTickets();
    
    return [...filteredTickets]
      .sort((a, b) => {
        const dateA = new Date(a.createdAt || 0).getTime();
        const dateB = new Date(b.createdAt || 0).getTime();
        return dateB - dateA;
      })
      .slice(0, 5);
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
      'High': '#f44336',
      'Medium': '#ff9800',
      'Low': '#4caf50'
    };
    return colorMap[priority] || '#6b778c';
  }

  getTrendIcon(trend: string): string {
    return trend === 'up' ? 'trending_up' : 
           trend === 'down' ? 'trending_down' : 
           'trending_flat';
  }

  getTrendClass(trend: string): string {
    return `trend-${trend}`;
  }

  getStatusClass(status: string): string {
    return 'status-' + status.toLowerCase().replace(/\s+/g, '-');
  }

  // Check if user is an agent
  isAgent = computed(() => {
    const user = this.authService.user();
    const role = user?.role || '';
    
    if (Array.isArray(role)) {
      return role.includes('SupportAgent');
    }
    
    return role === 'SupportAgent';
  });

  // Helper methods for date range menu
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
}