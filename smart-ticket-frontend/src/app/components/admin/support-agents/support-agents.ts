import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatMenuModule } from '@angular/material/menu';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { UserService } from '../../../services/user.service';
import { UserDto } from '../../../models/Model';

@Component({
  selector: 'app-support-agents',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MatTableModule, MatPaginatorModule,
    MatSortModule, MatFormFieldModule, MatInputModule, MatSelectModule,
    MatButtonModule, MatIconModule, MatTooltipModule, MatMenuModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './support-agents.html',
  styleUrl: './support-agents.scss'
})
export class SupportAgents implements OnInit {
  private userService = inject(UserService);

  // State Signals
  agents = signal<UserDto[]>([]);
  searchTerm = signal('');
  loading = signal(false);
  
  // Table State Signals
  pageIndex = signal(0);
  pageSize = signal(10);
  sortField = signal('email');
  sortDirection = signal<'asc' | 'desc' | ''>('asc');

  displayedColumns = ['email', 'fullName', 'role', 'status', 'joined', 'actions'];

  // Reactive Multi-field Filter
  filteredAgents = computed(() => {
    let result = this.agents();
    const query = this.searchTerm().toLowerCase().trim();

    if (query) {
      result = result.filter(a =>
        a.email?.toLowerCase().includes(query) ||
        a.fullName?.toLowerCase().includes(query)
      );
    }

    return this.sortData(result);
  });

  // Reactive Pagination
  paginatedAgents = computed(() => {
    const start = this.pageIndex() * this.pageSize();
    const end = start + this.pageSize();
    return this.filteredAgents().slice(start, end);
  });

  ngOnInit(): void {
    this.loadAgents();
  }

  loadAgents(): void {
    this.loading.set(true);
    this.userService.getAllUsers().subscribe({
      next: (users) => {
        const filtered = users.filter(u =>
          u.roles?.includes('SupportAgent') || u.role === 'SupportAgent'
        );
        this.agents.set(filtered);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error loading agents', err);
        this.loading.set(false);
      }
    });
  }

  onSortChange(sort: Sort): void {
    this.sortField.set(sort.active);
    this.sortDirection.set(sort.direction);
  }

  private sortData(data: UserDto[]): UserDto[] {
    const direction = this.sortDirection();
    const field = this.sortField();
    if (!direction) return data;

    return [...data].sort((a, b) => {
      let aVal: any = a[field as keyof UserDto];
      let bVal: any = b[field as keyof UserDto];

      if (field === 'joined' || field === 'createdAt') {
        aVal = new Date(aVal || 0).getTime();
        bVal = new Date(bVal || 0).getTime();
      } else {
        aVal = (aVal || '').toString().toLowerCase();
        bVal = (bVal || '').toString().toLowerCase();
      }

      const isAsc = direction === 'asc';
      return (aVal < bVal ? -1 : 1) * (isAsc ? 1 : -1);
    });
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
  }

  applyFilter(): void {
    this.pageIndex.set(0);
  }

  clearSearch(): void {
    this.searchTerm.set('');
    this.applyFilter();
  }

  refreshData(): void {
    this.loadAgents();
  }

  getInitials(email: string): string {
    if (!email) return '?';
    return email.substring(0, 2).toUpperCase();
  }

  exportData(): void {
    const data = this.filteredAgents();
    const csv = [
      ['Email', 'Full Name', 'Role', 'Joined Date'],
      ...data.map(a => [
        a.email,
        a.fullName || '',
        'Support Agent',
        new Date(a.createdAt || '').toLocaleDateString()
      ])
    ].map(row => row.join(',')).join('\n');

    const blob = new Blob([csv], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `agents-export-${new Date().toISOString().split('T')[0]}.csv`;
    link.click();
    window.URL.revokeObjectURL(url);
  }

  viewProfile(agent: UserDto): void {
    console.log('Viewing agent profile:', agent);
  }
}