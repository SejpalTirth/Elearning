import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UserApiService {

  private baseUrl = 'https://localhost:7130/api';

  constructor(private http: HttpClient) {}

  getAllUsers(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/users`);
  }

  getUserRoles(userId: string): Observable<string[]> {
    return this.http.get<string[]>(`${this.baseUrl}/roles/${userId}`);
  }

  updateUserRole(body: { userId: string, roleId: number }): Observable<any> {
  return this.http.put(`${this.baseUrl}/roles/update`, body, { responseType: 'text' });
  }

  getAllRoles(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/roles`);
  }
}
