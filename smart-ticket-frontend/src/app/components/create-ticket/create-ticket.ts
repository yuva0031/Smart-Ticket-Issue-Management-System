import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

// Material Imports
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { TicketService } from '../../services/ticket.service';
import { LookupService } from '../../services/lookup.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-create-ticket',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MatCardModule, MatInputModule, MatIconModule,
    MatDividerModule, MatButtonModule, MatSelectModule, MatSnackBarModule, MatProgressSpinnerModule
  ],
  templateUrl: './create-ticket.html',
  styleUrl: './create-ticket.scss'
})
export class CreateTicket implements OnInit {
  private ticketService = inject(TicketService);
  private lookupService = inject(LookupService);
  private authService = inject(AuthService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);

  // Form State Signals
  title = signal('');
  description = signal('');
  priorityId = signal<number | null>(null);
  loading = signal(false);

  // Lookups
  priorities = computed(() => this.lookupService.priorities());
  currentUser = computed(() => this.authService.user());
  
  // Validation
  isValid = computed(() =>
    this.title().trim().length >= 4 &&
    this.description().trim().length >= 6 &&
    this.priorityId() !== null
  );

  ngOnInit(): void {
    this.lookupService.loadPriorities();
  }

  submit(): void {
    if (!this.isValid() || this.loading()) return;
    this.loading.set(true);

    const payload = {
      title: this.title().trim(),
      description: this.description().trim(),
      priorityId: this.priorityId()!
    };

    this.ticketService.createTicket(payload).subscribe({
      next: () => {
        this.loading.set(false);
        this.showMessage('Ticket created successfully!', 'success');
        this.goBack();
      },
      error: (err) => {
        console.error(err);
        this.loading.set(false);
        this.showMessage('Failed to create ticket', 'error');
      }
    });
  }

  goBack(): void {
    const role = this.authService.user()?.role;
    const path = role === 'SupportManager' ? '/manager/tickets' : 
                 role === 'SupportAgent' ? '/agent/assigned' : '/app/tickets';
    this.router.navigate([path]);
  }

  private showMessage(message: string, type: 'success' | 'error'): void {
    this.snackBar.open(message, 'Close', {
      duration: 3000,
      panelClass: [`snackbar-${type}`]
    });
  }
}