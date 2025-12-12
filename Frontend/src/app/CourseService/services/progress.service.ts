import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from 'Environment/environment';
import { Observable } from 'rxjs';

// Optional: strong typing for progress results
export interface ProgressRecord {
  courseId: number;
  moduleId: number | null;
  progressPercent: number;
  isCompleted: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class ProgressService {

  private baseUrl = `${environment.baseapiurl}/progress`;

  private readonly http = inject(HttpClient);

  /** Mark a module as completed */
  markModuleCompleted(data: { userId: string; moduleId: number }): Observable<string> {
    return this.http.post(`${this.baseUrl}/complete-module`, data, {
      responseType: 'text'  // Backend returns plain message string
    });
  }

  /** Fetch user's progress for all modules/courses */
  getUserProgress(userId: string): Observable<ProgressRecord[]> {
    return this.http.get<ProgressRecord[]>(`${this.baseUrl}/${userId}`);
  }
}
