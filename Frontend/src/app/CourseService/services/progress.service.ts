import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

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

  private baseUrl = "https://localhost:7249/api/progress"; // Gateway URL

  constructor(private http: HttpClient) {}

  /** Mark a module as completed */
  markModuleCompleted(data: { userId: string; moduleId: number }) {
    return this.http.post(`${this.baseUrl}/complete-module`, data, {
      responseType: 'text'  // Backend returns plain message string
    });
  }

  /** Fetch user's progress for all modules/courses */
  getUserProgress(userId: string) {
    return this.http.get<ProgressRecord[]>(`${this.baseUrl}/${userId}`);
  }
}
