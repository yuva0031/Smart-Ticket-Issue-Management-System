import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { UserService } from '../../../services/user.service';
import { UserDto } from '../../../models/Model';

@Component({
  selector: 'app-pending-approvals',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MatTableModule, MatPaginatorModule,
    MatSortModule, MatFormFieldModule, MatInputModule, MatButtonModule,
    MatIconModule, MatSelectModule, MatSnackBarModule, MatProgressSpinnerModule
  ],
  templateUrl: './pending-requests.html',
  styleUrl: './pending-requests.scss'
})
export class PendingApprovals implements OnInit {
  private userService = inject(UserService);
  private snackBar = inject(MatSnackBar);

  // State Signals
  users = signal<UserDto[]>([]);
  searchTerm = signal('');
  selectedRole = signal('all');
  loading = signal(false);
  
  // Pagination  and Sort Signals
  pageIndex = signal(0);
  pageSize = signal(10);
  sortField = signal('createdAt');
  sortDirection = signal<'asc' | 'desc' | ''>('desc');

  displayedColumns = ['email', 'role', 'requested', 'actions'];

  // Reactive filtering logic
  filteredUsers = computed(() => {
    let result = this.users();
    const search = this.searchTerm().toLowerCase();
    const role = this.selectedRole();

    if (search) {
      result = result.filter(u => u.email?.toLowerCase().includes(search));
    }

    if (role !== 'all') {
      result = result.filter(u => u.role === role);
    }

    return this.sortData(result);
  });

  // Reactive pagination
  paginatedUsers = computed(() => {
    const start = this.pageIndex() * this.pageSize();
    const end = start + this.pageSize();
    return this.filteredUsers().slice(start, end);
  });

  ngOnInit(): void {
    this.loadPendingUsers();
  }

  loadPendingUsers(): void {
    this.loading.set(true);
    this.userService.getPendingRoleRequests().subscribe({
      next: (data) => {
        this.users.set(data);
        console.log(this.users());
        this.loading.set(false);
      },
      error: () => {
        this.snackBar.open('Failed to load pending approvals', 'Close', { duration: 3000 });
        this.loading.set(false);
      }
    });
  }

  approveUser(userId: string): void {
    if (this.loading()) return;
    this.loading.set(true);
    
    this.userService.approveRoleRequest(userId).subscribe({
      next: () => {
        this.snackBar.open('User approved successfully', 'OK', { duration: 2000 });
        this.users.update(list => list.filter(u => u.userId !== userId));
        this.loading.set(false);
      },
      error: () => {
        this.snackBar.open('Approval failed', 'Close', { duration: 3000 });
        this.loading.set(false);
      }
    });
  }

  rejectUser(userId: string): void {
    if (confirm('Are you sure you want to reject this request?')) {
      this.users.update(list => list.filter(u => u.userId !== userId));
      this.snackBar.open('Request rejected', 'OK', { duration: 2000 });
    }
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

      if (field === 'createdAt') {
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
    this.loadPendingUsers();
  }
}