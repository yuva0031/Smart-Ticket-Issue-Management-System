import { Injectable, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment.development';

@Injectable({ providedIn: 'root' })
export class LookupService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/lookups`;

  roles = signal<any[]>([]);
  categories = signal<any[]>([]);
  priorities = signal<any[]>([]);
  statuses = signal<any[]>([]);

  private rolesLoaded = false;
  private categoriesLoaded = false;
  private prioritiesLoaded = false;
  private statusesLoaded = false;

  loadRoles() {
    if (this.rolesLoaded) return;
    this.http.get<any[]>(`${this.apiUrl}/roles`).subscribe(data => {
      this.roles.set(data);
      this.rolesLoaded = true;
    });
  }

  loadCategories() {
    if (this.categoriesLoaded) return;
    this.http.get<any[]>(`${this.apiUrl}/categories`).subscribe(data => {
      this.categories.set(data);
      this.categoriesLoaded = true;
    });
  }

  loadPriorities() {
    if (this.prioritiesLoaded) return;
    this.http.get<any[]>(`${this.apiUrl}/priorities`).subscribe(data => {
      this.priorities.set(data);
      this.prioritiesLoaded = true;
    });
  }

  loadStatuses() {
    if (this.statusesLoaded) return;
    this.http.get<any[]>(`${this.apiUrl}/statuses`).subscribe(data => {
      this.statuses.set(data);
      this.statusesLoaded = true;
    });
  }
}