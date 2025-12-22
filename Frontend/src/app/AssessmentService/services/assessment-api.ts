import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from 'Environment/environment';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AssessmentApiService {

  private baseUrl = `${environment.baseapiurl}/assessment`;
  private readonly http = inject(HttpClient);

  // =====================================================
  //  STUDENT + COMMON
  // =====================================================

  getQuizForModule(moduleId: number): Observable<any> {
    return this.http.post<any>(
      `${this.baseUrl}/quiz/module`,
      { moduleId }
    );
  }

  submitQuiz(payload: any): Observable<any> {
    return this.http.post<any>(
      `${this.baseUrl}/quiz/submit`,
      payload
    );
  }

  getResult(submissionId: string): Observable<any> {
    return this.http.post<any>(
      `${this.baseUrl}/quiz/result`,
      { submissionId }
    );
  }

  // =====================================================
  //  INSTRUCTOR — QUIZ CREATION
  // =====================================================

  createQuiz(payload: any): Observable<any> {
    return this.http.post<any>(
      `${this.baseUrl}/quiz`,
      payload
    );
  }

  addQuestion(payload: any): Observable<any> {
    return this.http.post<any>(
      `${this.baseUrl}/quiz/questions`,
      payload
    );
  }

  // =====================================================
  //  QUIZ STATUS (COURSE LOCKING)
  // =====================================================

  getQuizStatus(courseId: number): Observable<any> {
    return this.http.post<any>(
      `${this.baseUrl}/course/quiz-status`,
      { courseId }
    );
  }

  getUnquizzedModules(courseId: number): Observable<number[]> {
    return this.http.post<number[]>(
      `${this.baseUrl}/course/unquizzed-modules`,
      { courseId }
    );
  }
}
