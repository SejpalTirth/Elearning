import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from 'Environment/environment';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AssessmentApiService {

  private baseUrl = `${environment.baseapiurl}/AssessmentGateway`;
  private readonly http = inject(HttpClient);

  // =====================================================
  //  STUDENT + COMMON
  // =====================================================

  getQuizForModule(moduleId: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/quiz/module/${moduleId}`);
  }

  submitQuiz(payload: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/submit`, payload);
  }

  getResult(submissionId: string): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/result/${submissionId}`);
  }

  // =====================================================
  //  INSTRUCTOR — QUIZ CREATION
  // =====================================================

  createQuiz(payload: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/quiz`, payload);
  }

  addQuestion(quizId: number, payload: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/quiz/${quizId}/questions`, payload);
  }

  // =====================================================
  // NEW — QUIZ STATUS CHECKS (For Course Locking Flow)
  // =====================================================

  getQuizStatus(courseId: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/course/${courseId}/quiz-status`);
  }

  getUnquizzedModules(courseId: number): Observable<number[]> {
    return this.http.get<number[]>(`${this.baseUrl}/unquizzed-modules/${courseId}`);
  }
}
