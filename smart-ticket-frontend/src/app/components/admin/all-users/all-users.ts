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
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { UserService } from '../../../services/user.service';
import { UserDto } from '../../../models/Model';

@Component({
  selector: 'app-all-users',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MatTableModule, MatPaginatorModule,
    MatSortModule, MatFormFieldModule, MatInputModule, MatButtonModule,
    MatIconModule, MatSnackBarModule, MatProgressSpinnerModule
  ],
  templateUrl: './all-users.html',
  styleUrl: './all-users.scss'
})
export class AllUsers implements OnInit {
  private userService = inject(UserService);
  private snackBar = inject(MatSnackBar);

  // State Signals
  users = signal<UserDto[]>([]);
  searchTerm = signal('');
  loading = signal(false);
  
  // Table State Signals
  pageIndex = signal(0);
  pageSize = signal(10);
  sortField = signal('email');
  sortDirection = signal<'asc' | 'desc' | ''>('asc');

  displayedColumns = ['email', 'fullName', 'role', 'status', 'joined'];

  // Reactive filtering logic
  filteredUsers = computed(() => {
    let result = this.users();
    const query = this.searchTerm().toLowerCase().trim();

    if (query) {
      result = result.filter(u =>
        u.email?.toLowerCase().includes(query) ||
        u.fullName?.toLowerCase().includes(query) ||
        u.role?.toLowerCase().includes(query)
      );
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
    this.loadUsers();
  }

  loadUsers(): void {
    this.loading.set(true);
    this.userService.getAllUsers().subscribe({
      next: (users: UserDto[]) => {
        this.users.set(users);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error loading users', err);
        this.showMessage('Failed to load users', 'error');
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
    this.pageIndex.set(0); // Reset to first page on search
  }

  clearSearch(): void {
    this.searchTerm.set('');
    this.applyFilter();
  }

  refreshData(): void {
    this.loadUsers();
  }

  getInitials(email: string): string {
    if (!email) return '?';
    return email.substring(0, 2).toUpperCase();
  }

  getUserStatus(user: UserDto): 'active' | 'inactive' {
    return user.isActive ? 'active' : 'inactive';
  }

  private showMessage(message: string, type: 'success' | 'error' | 'info'): void {
    this.snackBar.open(message, 'Close', {
      duration: 3000,
      horizontalPosition: 'end',
      verticalPosition: 'top',
      panelClass: [`snackbar-${type}`]
    });
  }
}