import { Injectable, signal } from '@angular/core';

export type AgentView = 'active' | 'all';

@Injectable({ providedIn: 'root' })
export class AgentTicketViewService {
  agentView = signal<AgentView>('active');

  setActive() {
    this.agentView.set('active');
    console.log('active');
  }

  setAll() {
    this.agentView.set('all');
    console.log('all');
  }
}
