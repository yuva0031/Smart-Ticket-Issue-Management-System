import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { UpdateTicketDto } from '../models/Model';
import { environment } from '../../environments/environment.development';

@Injectable({ providedIn: 'root' })
export class TicketService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/tickets`;

  getAllTickets(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/all`);
  }

  getTicketsByOwner(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/my`);
  }

  getAssignedTickets(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/assigned`);
  }

  getTicketById(ticketId: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${ticketId}`);
  }

  createTicket(ticket: any): Observable<any> {
    return this.http.post<any>(this.apiUrl, ticket);
  }

  updateTicket(ticketId: number, updates: UpdateTicketDto): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${ticketId}`, updates);
  }

  deleteTicket(ticketId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${ticketId}`);
  }

  addComment(ticketId: number, content: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/${ticketId}/comments`, { content });
  }

  getComments(ticketId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/${ticketId}/comments`);
  }
}