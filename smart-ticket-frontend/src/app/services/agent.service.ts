import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';

export interface AgentProfile {
  id: string;
  userId: string;
  currentWorkload: number;
  escalationLevel: number;
  skills: AgentSkill[];
}

export interface AgentSkill {
  id: number;
  categoryId: number;
  categoryName?: string;
}

interface AddAgentSkillDto {
  agentProfileId: string;
  categoryId: number;
}

interface RemoveAgentSkillDto {
  agentProfileId: string;
  categoryId: number;
}

@Injectable({
  providedIn: 'root'
})
export class AgentService {
  private apiUrl = `${environment.apiUrl}/agent`;

  constructor(private http: HttpClient) {}

  getMyProfile(): Observable<AgentProfile> {
    return this.http.get<AgentProfile>(`${this.apiUrl}/my-profile`);
  }

  addSkill(dto: AddAgentSkillDto): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(
      `${this.apiUrl}/add-skill`,
      dto
    );
  }

  removeSkill(dto: RemoveAgentSkillDto): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(
      `${this.apiUrl}/remove-skill`,
      {
        body: dto
      }
    );
  }

  getAgentProfileById(agentProfileId: string): Observable<AgentProfile> {
    return this.http.get<AgentProfile>(
      `${this.apiUrl}/profile/${agentProfileId}`);
  }

  getAllAgentProfiles(): Observable<AgentProfile[]> {
    return this.http.get<AgentProfile[]>(
      `${this.apiUrl}/all`);
  }
}