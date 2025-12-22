import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { environment } from 'Environment/environment';

export interface TokenResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt?: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {

  private readonly baseUrl = `${environment.baseapiurl}/GatewayAuth`;
  private readonly userUrl = `${environment.baseapiurl}/GatewayUsers`;

  private readonly http = inject(HttpClient);

  // -------------------- TOKEN STORAGE --------------------

  getAccessToken(): string | null {
    return localStorage.getItem('accessToken');
  }

  getRefreshToken(): string | null {
    return localStorage.getItem('refreshToken');
  }

  storeTokens(tokens: TokenResponse): void {
    if (!tokens) {return;}

    localStorage.setItem('accessToken', tokens.accessToken);
    localStorage.setItem('refreshToken', tokens.refreshToken);

    if (tokens.expiresAt) {
      localStorage.setItem('expiresAt', tokens.expiresAt);
    }
  }

  clearTokens(): void {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('expiresAt');
  }

  // -------------------- REFRESH --------------------

  refreshTokens(): Observable<TokenResponse | null> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      return of(null);
    }

    return this.http
      .post<TokenResponse>(`${this.baseUrl}/refresh`, {
        refreshToken: refreshToken
      })
      .pipe(
        tap(tokens => {
          if (tokens?.accessToken) {
            this.storeTokens(tokens);
          }
        }),
        catchError(err => {
          console.error('Refresh failed', err);
          return of(null);
        })
      );
  }

  // -------------------- LOGOUT --------------------

  logout(): void {
    const refreshToken = this.getRefreshToken();

    if (!refreshToken) {
      this.clearTokens();
      window.location.href = '/';
      return;
    }

    this.http.post(`${this.baseUrl}/logout`, { refreshToken })
      .pipe(catchError(() => of(null)))
      .subscribe(() => {
        this.clearTokens();
        window.location.href = '/';
      });
  }

  // -------------------- PROFILE --------------------

  completeProfile(dto: { name: string; roleId: number }): Observable<any> {
    return this.http.post(`${this.userUrl}/complete-profile`, dto);
  }

  // -------------------- LOGIN --------------------

  loginWithGoogle(): void {
    window.location.href = `${this.baseUrl}/google-login`;
  }

  loginWithMicrosoft(): void {
    window.location.href = `${this.baseUrl}/microsoft-login`;
  }
}
