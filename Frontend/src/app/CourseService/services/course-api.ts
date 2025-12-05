import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class CourseApiService {

  private http = inject(HttpClient);
  private baseUrl = 'https://localhost:7249/api/GatewayCourse';

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

  getModules(courseId: number) {
    return this.http.get<any[]>(`${this.baseUrl}/${courseId}/modules`);
  }

  getModuleById(moduleId: number) {
  return this.http.get<any>(`${this.baseUrl}/module/${moduleId}`);
  }


  getEnrolledCourses(userId: string) {
    return this.http.get<any[]>(`${this.baseUrl}/enrolled/${userId}`);
  }

  getCategories(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/categories`);
  }

  //  Fixed missing method
  getCoursesByInstructor(instructorId: string) {
  return this.http.get<any[]>(`${this.baseUrl}/instructor/${instructorId}`);
  }

  deleteCourse(id: number) {
  return this.http.delete(`${this.baseUrl}/${id}`, {
    withCredentials: true,
    responseType: 'text'
  });
}


  publishCourse(courseId: number) {
  return this.http.post(`${this.baseUrl}/publish/${courseId}`, {});
  }

  getInstructorUnpublishedCourse(userId: string) {
  return this.http.get(`${this.baseUrl}/instructor/unpublished/${userId}`);
  }
  getInstructorDraftCourse() {
  return this.http.get(`${this.baseUrl}/instructor/draft`);
  }


}
