import { Component, OnInit, signal } from '@angular/core';
import { CommonModule, Location } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { SlaRulesService, TicketPriority } from '../../services/sla-rules.service';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-sla-rules',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MatTableModule, MatCardModule, 
    MatButtonModule, MatIconModule, MatInputModule, 
    MatSnackBarModule, MatProgressSpinnerModule
  ],
  templateUrl: './sla-rules.html',
  styleUrl: './sla-rules.scss'
})
export class SlaRules implements OnInit {
  priorities = signal<TicketPriority[]>([]);
  loading = signal(false);
  savingId = signal<number | null>(null); 

  constructor(
    private location: Location,
    private slaService: SlaRulesService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.loading.set(true);
    this.slaService.getPriorities()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe(data => this.priorities.set(data));
  }

  saveChanges(priority: TicketPriority): void {
    this.savingId.set(priority.priorityId);
    
    this.slaService.updateSla(priority.priorityId, priority.slaHours)
      .pipe(finalize(() => this.savingId.set(null)))
      .subscribe({
        next: () => this.snackBar.open(`${priority.priorityName} SLA updated`, 'Close', { duration: 2000 }),
        error: () => this.snackBar.open('Update failed', 'Retry')
      });
  }

  goBack() { this.location.back(); }
}