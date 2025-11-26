import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class AssessmentApiService {

  private baseUrl = 'https://localhost:7249/api/AssessmentGateway';

  constructor(private http: HttpClient) {}

  // Get quiz for module
  getQuizForModule(moduleId: number){
  return this.http.get(`${this.baseUrl}/quiz/module/${moduleId}`);
  }


  // Submit quiz
  submitQuiz(payload: any) {
    return this.http.post(`${this.baseUrl}/submit`, payload);
  }

  // Get stored result (optional)
  getResult(submissionId: string) {
    return this.http.get(`${this.baseUrl}/result/${submissionId}`);
  }
}
