import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class AssessmentApiService {

  private baseUrl = 'https://localhost:7249/api/AssessmentGateway';

  constructor(private http: HttpClient) {}

  getQuizForModule(moduleId: number){
    return this.http.get(`${this.baseUrl}/quiz/module/${moduleId}`);
  }

  createQuiz(payload: any) {
    return this.http.post(`${this.baseUrl}/quiz`, payload);
  }

  addQuestion(quizId: number, payload: any) {
    return this.http.post(`${this.baseUrl}/quiz/${quizId}/questions`, payload);
  }

  submitQuiz(payload: any) {
    return this.http.post(`${this.baseUrl}/submit`, payload);
  }

  getResult(submissionId: string) {
    return this.http.get(`${this.baseUrl}/result/${submissionId}`);
  }
}
