import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatDivider } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

import { AgentService, AgentProfile, AgentSkill } from '../../services/agent.service';
import { LookupService } from '../../services/lookup.service';

@Component({
  selector: 'app-agent-profile',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatChipsModule,
    MatIconModule,
    MatButtonModule,
    MatDivider,
    MatSelectModule,
    MatFormFieldModule,
    MatSnackBarModule
  ],
  templateUrl: './agent-profile.html',
  styleUrl: './agent-profile.scss'
})
export class AgentSkillProfile implements OnInit {

  agentProfile = signal<AgentProfile | null>(null);
  skills = signal<AgentSkill[]>([]);
  isEditMode = signal(false);

  availableCategories = computed(() => {
    const selectedIds = this.skills().map(s => s.categoryId);
    return this.lookupService.categories()
      .filter(c => !selectedIds.includes(c.id));
  });

  constructor(
    private agentService: AgentService,
    private lookupService: LookupService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.lookupService.loadCategories();
    this.loadProfile();
  }

  loadProfile(): void {
    this.agentService.getMyProfile().subscribe({
      next: profile => {
        this.agentProfile.set(profile);

        const mappedSkills = profile.skills.map(skill => ({
          ...skill,
          categoryName:
            this.lookupService.categories()
              .find(c => c.id === skill.categoryId)?.name || 'Unknown'
        }));

        this.skills.set(mappedSkills);
      },
      error: () => {
        this.agentProfile.set(null);
        this.toast('Failed to load profile', 'error');
      }
    });
  }

  toggleEditMode(): void {
    this.isEditMode.update(v => !v);
  }

  addSkill(categoryId: number): void {
    const profile = this.agentProfile();
    if (!profile) return;

    this.agentService.addSkill({
      agentProfileId: profile.id,
      categoryId
    }).subscribe({
      next: () => {
        this.toast('Skill added', 'success');
        this.loadProfile();
      },
      error: () => this.toast('Failed to add skill', 'error')
    });
  }

  removeSkill(skill: AgentSkill): void {
    const profile = this.agentProfile();
    if (!profile) return;

    this.agentService.removeSkill({
      agentProfileId: profile.id,
      categoryId: skill.categoryId
    }).subscribe({
      
      next: () => {
        this.toast('Skill removed', 'success');
        this.loadProfile();
      },
      error: () => this.toast('Failed to remove skill', 'error')
    });
  }

  private toast(message: string, type: 'success' | 'error'): void {
    this.snackBar.open(message, 'Close', {
      duration: 3000,
      panelClass: type === 'success'
        ? 'success-snackbar'
        : 'error-snackbar'
    });
  }
}
