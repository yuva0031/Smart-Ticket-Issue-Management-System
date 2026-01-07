import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { TicketHistory } from '../models/Model';
import { environment } from '../../environments/environment.development';

@Injectable({ providedIn: 'root' })
export class TicketHistoryService {
  private apiUrl = `${environment.apiUrl}/tickets`;
  constructor(private http: HttpClient) {}

  getHistory(ticketId: number) {
    return this.http.get<any[]>(`${this.apiUrl}/${ticketId}/history`);
  }
}