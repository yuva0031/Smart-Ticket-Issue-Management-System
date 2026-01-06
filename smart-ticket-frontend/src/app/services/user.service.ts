import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, signal, computed, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { UserDto } from '../models/Model';
import { environment } from '../../environments/environment.development'; 

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/user`;

  private allUsersSignal = signal<UserDto[]>([]);

  supportAgents = computed(() => 
    this.allUsersSignal().filter(u => u.roles?.includes('SupportAgent'))
  );

  managers = computed(() => 
    this.allUsersSignal().filter(u => u.roles?.includes('SupportManager'))
  );

  loadAllUsers(): void {
    this.http.get<UserDto[]>(`${this.apiUrl}/all-users`).subscribe({
      next: (users) => this.allUsersSignal.set(users),
      error: (err) => console.error('Error loading users into state', err)
    });
  }

  getAllUsers(): Observable<UserDto[]> {
    return this.http.get<UserDto[]>(`${this.apiUrl}/all-users`);
  }

  getEndUsers(): Observable<UserDto[]>{
    return this.http.get<UserDto[]>(`${this.apiUrl}/all`)
  }

  getPendingRoleRequests(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/pendingapprovals`);
  }

  approveRoleRequest(userId: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/approve-user/${userId}`, {});
  }

  getSupportAgents(categoryId?: number): Observable<UserDto[]> {
    let params = new HttpParams();
    if (categoryId) {
      params = params.set('categoryId', categoryId);
    }
    return this.http.get<UserDto[]>(`${this.apiUrl}/agents`, { params });
  }

  getUserById(userId: string): Observable<UserDto> {
    return this.http.get<UserDto>(`${this.apiUrl}/${userId}`);
  }

  createUser(user: any): Observable<any> {
    return this.http.post(`${this.apiUrl}`, user);
  }

  deleteUser(userId: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${userId}`);
  }
}