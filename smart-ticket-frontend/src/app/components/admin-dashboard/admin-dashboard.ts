import { Component, OnInit, OnDestroy, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Subscription, forkJoin } from 'rxjs';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDividerModule } from '@angular/material/divider';

import { UserService } from '../../services/user.service';
import { SignalrService } from '../../services/signalr.service';
import { UserActivityService, Notification } from '../../services/user-activity.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [
    CommonModule, RouterModule, MatIconModule, MatButtonModule, 
    MatTableModule, MatChipsModule, MatSnackBarModule, MatDividerModule
  ],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.scss'
})
export class AdminDashboard implements OnInit, OnDestroy {
  private userService = inject(UserService);
  private activityService = inject(UserActivityService);
  private signalrService = inject(SignalrService);
  private snackBar = inject(MatSnackBar);

  stats = signal({ activeEndUsers: 0, activeAgents: 0, activeManagers: 0, pendingRequests: 0 });
  pendingRequests = signal<any[]>([]);
  recentActivity = signal<Notification[]>([]); // Using backend interface
  displayedColumns = ['user', 'requestedRole', 'date', 'actions'];
  
  private subs = new Subscription();

  async ngOnInit(): Promise<void> {
    this.loadAllData();
    
    await this.signalrService.startConnection();
    this.registerRealtimeListeners();
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
  }

  private registerRealtimeListeners(): void {
    // Listen for SignalR events and refresh the logs
    this.subs.add(this.signalrService.getUserRegistered$().subscribe(data => {
      if (data) {
        this.snackBar.open(`New User Registered: ${data.email}`, 'OK', { duration: 3000 });
        this.loadRecentActivity(); // Refresh from DB
        this.loadAllData();
      }
    }));

    this.subs.add(this.signalrService.getUserApproved$().subscribe(() => {
        this.loadRecentActivity();
        this.loadAllData();
    }));
  }

loadAllData(): void {
  forkJoin({
    users: this.userService.getAllUsers(),
    requests: this.userService.getPendingRoleRequests()
  }).subscribe(({ users, requests }) => {
    this.stats.set({
      activeEndUsers: users.filter(u => u.role === 'EndUser' || !u.role).length,
      activeAgents: users.filter(u => u.role === 'SupportAgent').length,
      activeManagers: users.filter(u => u.role === 'SupportManager').length,
      pendingRequests: requests.length
    });

    const sortedRequests = [...requests].sort((a, b) => {
      const dateA = new Date(a.createdAt || 0).getTime();
      const dateB = new Date(b.createdAt || 0).getTime();
      return dateB - dateA;
    });

    this.pendingRequests.set(sortedRequests.slice(0, 5));
  });

  this.loadRecentActivity();
}

  loadRecentActivity(): void {
    this.activityService.getNotifications(1, 10).subscribe(logs => {
      this.recentActivity.set(logs);
    });
  }

  approveRequest(request: any): void {
    this.userService.approveRoleRequest(request.userId || request.id).subscribe(() => {
      this.snackBar.open('User Approved', 'Close', { duration: 2000 });
      this.loadAllData();
    });
  }

  formatRole(role: string): string {
    return role === 'SupportAgent' ? 'Agent' : role === 'SupportManager' ? 'Manager' : 'User';
  }

  getActivityIcon(msg: string): string {
    if (msg.toLowerCase().includes('registered')) return 'person_add';
    if (msg.toLowerCase().includes('approved')) return 'verified';
    return 'notifications';
  }

  getActivityClass(msg: string): string {
    if (msg.toLowerCase().includes('registered')) return 'registration';
    if (msg.toLowerCase().includes('approved')) return 'approved';
    return 'default';
  }
}