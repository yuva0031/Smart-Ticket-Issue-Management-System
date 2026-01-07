import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { TicketComment } from '../models/Model';
import { environment } from '../../environments/environment.development';

@Injectable({ providedIn: 'root' })
export class TicketCommentService {
  private apiUrl = `${environment.apiUrl}/tickets`;
  constructor(private http: HttpClient) {}

  getComments(ticketId: number) {
    return this.http.get<any[]>(`${this.apiUrl}/${ticketId}/comments`);
  }

  addComment(ticketId: number, payload: { message: string; isInternal: boolean }) {
    return this.http.post(`${this.apiUrl}/${ticketId}/comments`, payload);
  }
}
