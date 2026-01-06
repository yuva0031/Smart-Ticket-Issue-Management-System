import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development'; 

export interface Notification {
  id: number;
  userId: string;
  message: string;
  isRead: boolean;
  createdAt: Date;
  channel: number;
}

@Injectable({
  providedIn: 'root'
})
export class UserActivityService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/notification`;

  getNotifications(page: number = 1, size: number = 10): Observable<Notification[]> {
    const params = new HttpParams()
      .set('pageNumber', page.toString())
      .set('pageSize', size.toString());
    return this.http.get<Notification[]>(this.apiUrl, { params });
  }

  markAllAsRead(): Observable<any> {
    return this.http.post(`${this.apiUrl}/mark-all-read`, {});
  }
}