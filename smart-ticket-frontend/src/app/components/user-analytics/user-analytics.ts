import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { RouterModule } from '@angular/router';

// MATERIAL
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';

// SERVICES
import { TicketService } from '../../services/ticket.service';
import { LookupService } from '../../services/lookup.service';
import { AuthService } from '../../services/auth.service';

// MODELS
import { TicketResponseDto, UserDashboardMetrics } from '../../models/Model';

interface StatusDistribution {
  status: string;
  count: number;
  percentage: number;
}

interface PriorityDistribution {
  priority: string;
  count: number;
  percentage: number;
}

interface CategoryDistribution {
  category: string;
  count: number;
  percentage: number;
}

interface SLAMetrics {
  onTrack: number;
  breached: number;
  complianceRate: number;
  avgResolutionDays: number;
}

interface DonutSegment {
  dashArray: string;
  dashOffset: number;
  color: string;
}

interface DateRange {
  label: string;
  value: string;
  icon: string;
}

@Component({
  selector: 'app-analytics',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    DatePipe,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatMenuModule,
    MatTooltipModule
  ],
  templateUrl: './user-analytics.html',
  styleUrl: './user-analytics.scss'
})
export class UserAnalytics implements OnInit {

  // STATE
  tickets = signal<TicketResponseDto[]>([]);
  loading = signal(false);
  selectedDateRange = signal<string>('all');

  // Date Range Options
  dateRanges: DateRange[] = [
    { label: 'All Time', value: 'all', icon: 'all_inclusive' },
    { label: 'Last 7 Days', value: '7days', icon: 'calendar_today' },
    { label: 'Last 30 Days', value: '30days', icon: 'calendar_month' },
    { label: 'Last 90 Days', value: '90days', icon: 'date_range' }
  ];

  // COMPUTED: Basic Metrics
  metrics = computed<UserDashboardMetrics>(() => {
    const filteredTickets = this.getFilteredTickets();
    
    return {
      totalTickets: filteredTickets.length,
      open: filteredTickets.filter(t => t.status === 'Open').length,
      pending: filteredTickets.filter(t => 
        ['Assigned', 'In Progress'].includes(t.status)
      ).length,
      resolved: filteredTickets.filter(t => 
        ['Resolved', 'Closed'].includes(t.status)
      ).length
    };
  });

  // COMPUTED: Status Distribution
  statusDistribution = computed<StatusDistribution[]>(() => {
    const filteredTickets = this.getFilteredTickets();
    const total = filteredTickets.length;
    
    if (total === 0) return [];

    const statusMap = new Map<string, number>();
    
    filteredTickets.forEach(ticket => {
      const count = statusMap.get(ticket.status) || 0;
      statusMap.set(ticket.status, count + 1);
    });

    return Array.from(statusMap.entries())
      .map(([status, count]) => ({
        status,
        count,
        percentage: Math.round((count / total) * 100)
      }))
      .sort((a, b) => b.count - a.count);
  });

  // COMPUTED: Priority Distribution
  priorityDistribution = computed<PriorityDistribution[]>(() => {
    const filteredTickets = this.getFilteredTickets();
    const total = filteredTickets.length;
    
    if (total === 0) return [];

    const priorityOrder = ['Critical', 'High', 'Medium', 'Low'];
    const priorityMap = new Map<string, number>();
    
    filteredTickets.forEach(ticket => {
      const count = priorityMap.get(ticket.priority) || 0;
      priorityMap.set(ticket.priority, count + 1);
    });

    return priorityOrder
      .map(priority => ({
        priority,
        count: priorityMap.get(priority) || 0,
        percentage: total > 0 ? Math.round(((priorityMap.get(priority) || 0) / total) * 100) : 0
      }));
  });

  // COMPUTED: Category Distribution
  categoryDistribution = computed<CategoryDistribution[]>(() => {
    const filteredTickets = this.getFilteredTickets();
    const total = filteredTickets.length;
    
    if (total === 0) return [];

    const categoryMap = new Map<string, number>();
    
    filteredTickets.forEach(ticket => {
      const category = ticket.category || 'Uncategorized';
      const count = categoryMap.get(category) || 0;
      categoryMap.set(category, count + 1);
    });

    return Array.from(categoryMap.entries())
      .map(([category, count]) => ({
        category,
        count,
        percentage: Math.round((count / total) * 100)
      }))
      .sort((a, b) => b.count - a.count)
      .slice(0, 5); // Top 5 categories
  });

  // COMPUTED: Recent Tickets
  recentTickets = computed<TicketResponseDto[]>(() => {
    const filteredTickets = this.getFilteredTickets();
    
    return [...filteredTickets]
      .sort((a, b) => {
        const dateA = new Date(a.createdAt || 0).getTime();
        const dateB = new Date(b.createdAt || 0).getTime();
        return dateB - dateA;
      })
      .slice(0, 5); // Top 5 recent
  });

  // COMPUTED: SLA Metrics
  slaMetrics = computed<SLAMetrics>(() => {
    const filteredTickets = this.getFilteredTickets();
    const total = filteredTickets.length;
    
    if (total === 0) {
      return {
        onTrack: 0,
        breached: 0,
        complianceRate: 0,
        avgResolutionDays: 0
      };
    }

    const now = new Date().getTime();
    let breachedCount = 0;
    let totalResolutionDays = 0;
    let resolvedCount = 0;

    filteredTickets.forEach(ticket => {
      // Check if SLA is breached
      if (ticket.dueDate) {
        const dueTime = new Date(ticket.dueDate).getTime();
        if (now > dueTime && !['Resolved', 'Closed'].includes(ticket.status)) {
          breachedCount++;
        }
      }

      // Calculate resolution time
      if (['Resolved', 'Closed'].includes(ticket.status)) {
        resolvedCount++;
        const created = new Date(ticket.createdAt || 0).getTime();
        const updated = new Date(ticket.updatedAt || ticket.createdAt || 0).getTime();
        const resolutionDays = Math.ceil((updated - created) / (1000 * 60 * 60 * 24));
        totalResolutionDays += resolutionDays;
      }
    });

    const onTrack = total - breachedCount;
    const complianceRate = Math.round((onTrack / total) * 100);
    const avgResolutionDays = resolvedCount > 0 
      ? Math.round(totalResolutionDays / resolvedCount) 
      : 0;

    return {
      onTrack,
      breached: breachedCount,
      complianceRate,
      avgResolutionDays
    };
  });

  // COMPUTED: Donut Chart Segments
  statusSegments = computed<DonutSegment[]>(() => {
    const distribution = this.statusDistribution();
    const circumference = 2 * Math.PI * 80; // radius = 80
    let currentOffset = 0;

    return distribution.map((item, index) => {
      const segmentLength = (item.percentage / 100) * circumference;
      const segment = {
        dashArray: `${segmentLength} ${circumference}`,
        dashOffset: -currentOffset,
        color: this.getStatusColor(item.status)
      };
      currentOffset += segmentLength;
      return segment;
    });
  });

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

    this.ticketService.getTicketsByOwner().subscribe({
      next: tickets => {
        this.tickets.set(tickets);
        this.loading.set(false);
      },
      error: err => {
        console.error('Error loading tickets:', err);
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
  // UTILITY METHODS
  // =============================
  getStatusColor(status: string): string {
    const colorMap: { [key: string]: string } = {
      'Open': '#ff991f',
      'Assigned': '#0052cc',
      'In Progress': '#5e35b1',
      'Resolved': '#43a047',
      'Closed': '#6b778c'
    };
    return colorMap[status] || '#6b778c';
  }

  getPriorityColor(priority: string): string {
    const colorMap: { [key: string]: string } = {
      'Critical': '#de350b',
      'High': '#ff5630',
      'Medium': '#ff991f',
      'Low': '#0065ff'
    };
    return colorMap[priority] || '#6b778c';
  }

  // Check if user is a regular user (not agent/manager)
  isUser = computed(() => {
    const user = this.authService.user();
    const role = user?.role || '';
    
    if (Array.isArray(role)) {
      return !role.includes('SupportAgent') && !role.includes('SupportManager');
    }
    
    return role !== 'SupportAgent' && role !== 'SupportManager';
  });
}