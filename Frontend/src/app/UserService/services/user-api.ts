import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'Environment/environment';

@Injectable({
  providedIn: 'root'
})
export class UserApiService {

  private baseUrl = `${environment.baseapiurl}/users`;
  private readonly http = inject(HttpClient);

  // ---------------- USERS ----------------

  getAllUsers(): Observable<any[]> {
    return this.http.post<any[]>(`${this.baseUrl}/all`, {});
  }

  getUser(userId: string): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/by-id`, { userId });
  }

  deleteUser(userId: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/delete`, { userId });
  }

  completeProfile(payload: { name: string; roleId: number }): Observable<any> {
    return this.http.post(`${this.baseUrl}/complete-profile`, payload);
  }

  // ---------------- ROLES ----------------

  getAllRoles(): Observable<any[]> {
    return this.http.post<any[]>(`${this.baseUrl}/roles/all`, {});
  }

  getUserRoles(userId: string): Observable<string[]> {
    return this.http.post<string[]>(`${this.baseUrl}/roles/user`, { userId });
  }

  updateUserRole(payload: { userId: string; roleId: number }): Observable<any> {
    return this.http.post(`${this.baseUrl}/roles/update`, payload);
  }
}
