import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../Environment/environment';

@Injectable({ providedIn: 'root' })
export class CourseApiService {

  private http = inject(HttpClient);
  private baseUrl = `${environment.baseapiurl}/course`;

  // ------------------- COURSES -------------------

  getAll(): Observable<any[]> {
    return this.http.post<any[]>(`${this.baseUrl}/all`, {});
  }

  getById(courseId: number): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/by-id`, { courseId });
  }

  addCourse(payload: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}`, payload);
  }

  updateCourse(payload: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/update`, payload);
  }

  deleteCourse(courseId: number): Observable<any> {
    return this.http.post(`${this.baseUrl}/delete`, { courseId });
  }

  publishCourse(courseId: number): Observable<any> {
    return this.http.post(`${this.baseUrl}/publish`, { courseId });
  }

  restoreCourse(courseId: number): Observable<any> {
    return this.http.post(`${this.baseUrl}/restore`, { courseId });
  }

  // ------------------- MODULES -------------------

  getModules(courseId: number): Observable<any[]> {
    return this.http.post<any[]>(`${this.baseUrl}/modules`, { courseId });
  }

  getModuleById(moduleId: number): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/module`, { moduleId });
  }

  // ------------------- ENROLLMENT -------------------

  enroll(courseId: number): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/enroll`, { courseId });
  }

  getEnrolledCourses(): Observable<any[]> {
    return this.http.post<any[]>(`${this.baseUrl}/enrolled`, {});
  }

  // ------------------- INSTRUCTOR -------------------

  getCoursesByInstructor(): Observable<any[]> {
    return this.http.post<any[]>(`${this.baseUrl}/instructor`, {});
  }

  getInstructorUnfinishedCourses(): Observable<any[]> {
    return this.http.post<any[]>(`${this.baseUrl}/unfinished`, {});
  }

  // ------------------- CATEGORIES -------------------

  getCategories(): Observable<any[]> {
    return this.http.post<any[]>(`${this.baseUrl}/categories`, {});
  }
}
