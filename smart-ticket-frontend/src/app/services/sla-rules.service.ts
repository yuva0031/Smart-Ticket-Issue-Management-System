
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';

export interface TicketPriority {
  priorityId: number;
  priorityName: string;
  slaHours: number;
}

export interface UpdateTicketPriorityDto {
  slaHours: number;
}

@Injectable({ providedIn: 'root' })
export class SlaRulesService {
  private readonly apiUrl = `${environment.apiUrl}/ticket-priorities`;

  constructor(private http: HttpClient) {}

  getPriorities(): Observable<TicketPriority[]> {
    return this.http.get<TicketPriority[]>(this.apiUrl);
  }

  updateSla(priorityId: number, slaHours: number): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(
      `${this.apiUrl}/${priorityId}/sla/${slaHours}`, {}
    );
  }
}