import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UserApiService {

  // Gateway URL (NOT UserService URL)
  private baseUrl = 'https://localhost:7249/api/GatewayUsers';

  constructor(private http: HttpClient) {}

  // --- USERS ---

  getAllUsers(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}`);
  }

  getUser(userId: string): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/${userId}`);
  }

  deleteUser(userId: string): Observable<any> {
    return this.http.delete(`${this.baseUrl}/${userId}`);
  }

  // --- ROLES ---

  getUserRoles(userId: string): Observable<string[]> {
    return this.http.get<string[]>(`${this.baseUrl}/roles/${userId}`);
  }

  updateUserRole(body: { userId: string; roleId: number }): Observable<any> {
    return this.http.put(`${this.baseUrl}/roles/update`, body, { responseType: 'text' });
  }

  getAllRoles(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/roles`);
  }
}
