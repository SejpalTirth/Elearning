import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'Environment/environment';

@Injectable({
  providedIn: 'root'
})
export class UserApiService {

  // Gateway URL (NOT UserService)
  private baseUrl = `${environment.baseapiurl}/GatewayUsers`;

  private readonly http = inject(HttpClient);

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

  // NEW — COMPLETE PROFILE
  completeProfile(payload: { userId: string; name: string; roleId: number }): Observable<any> {
    return this.http.post(`${this.baseUrl}/complete-profile`, payload);
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
