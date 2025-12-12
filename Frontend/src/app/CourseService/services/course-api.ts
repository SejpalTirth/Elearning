import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../Environment/environment';

@Injectable({
  providedIn: 'root',
})
export class CourseApiService {

  private http = inject(HttpClient);
  private baseUrl = `${environment.baseapiurl}/GatewayCourse`;

  getAll(): Observable<any[]> {
    return this.http.get<any[]>(this.baseUrl);
  }

  getById(id: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/${id}`);
  }

  addCourse(payload: any): Observable<any> {
    return this.http.post<any>(this.baseUrl, payload);
  }

  updateCourse(id: number, payload: any): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/${id}`, payload);
  }

  enroll(payload: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/enroll`, payload);
  }

  getModules(courseId: number): Observable<any> {
    return this.http.get<any[]>(`${this.baseUrl}/${courseId}/modules`);
  }

  getModuleById(moduleId: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/module/${moduleId}`);
  }

  getEnrolledCourses(userId: string): Observable<any> {
    return this.http.get<any[]>(`${this.baseUrl}/enrolled/${userId}`);
  }

  getCategories(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/categories`);
  }

  getCoursesByInstructor(instructorId: string): Observable<any> {
    return this.http.get<any[]>(`${this.baseUrl}/instructor/${instructorId}`);
  }

  deleteCourse(id: number): Observable<string> {
    return this.http.delete(`${this.baseUrl}/${id}`, {
      withCredentials: true,
      responseType: 'text'
    });
  }

  publishCourse(courseId: number): Observable<object> {
    return this.http.post(`${this.baseUrl}/${courseId}/publish`, {});
  }

  getInstructorUnfinishedCourses(instructorId: string): Observable<any> {
    return this.http.get<any[]>(`${this.baseUrl}/unfinished/${instructorId}`);
  }
  restoreCourse(id: number): Observable<string> {
    return this.http.put(`${this.baseUrl}/${id}/restore`, {}, {
      withCredentials: true,
      responseType: 'text'
    });
  }
}
