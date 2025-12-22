import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from 'Environment/environment';
import { BehaviorSubject, Observable, tap } from 'rxjs';

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

  private readonly baseUrl = `${environment.baseapiurl}/progress`;
  private readonly http = inject(HttpClient);

  // Internal state
  private readonly progressSubject =
    new BehaviorSubject<ProgressRecord[]>([]);

  // Public readonly stream
  readonly progress$ = this.progressSubject.asObservable();

  // =====================================================
  //  LOAD PROGRESS
  // =====================================================
  loadUserProgress(): Observable<ProgressRecord[]> {
    return this.http
      .post<ProgressRecord[]>(`${this.baseUrl}/user`, {})
      .pipe(
        tap(progress => this.progressSubject.next(progress))
      );
  }

  // =====================================================
  //  MANUAL REFRESH (useful after updates)
  // =====================================================
  refreshProgress(): void {
    this.loadUserProgress().subscribe();
  }

  // =====================================================
  //  MARK MODULE COMPLETED
  // =====================================================
  markModuleCompleted(moduleId: number): Observable<string> {
    return this.http
      .post(
        `${this.baseUrl}/complete-module`,
        { moduleId },
        { responseType: 'text' }
      )
      .pipe(
        // Auto-refresh progress after completion
        tap(() => this.refreshProgress())
      );
  }

  // =====================================================
  //  OPTIONAL: CLEAR STATE (on logout)
  // =====================================================
  clearProgress(): void {
    this.progressSubject.next([]);
  }
}
