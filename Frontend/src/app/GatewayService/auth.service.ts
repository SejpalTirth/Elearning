import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';

export interface TokenResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt?: string; // backend-provided token expiry (note: this is refresh-token expiry in Option 1)
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private baseUrl = 'https://localhost:7249/api/GatewayAuth';

  constructor(private http: HttpClient) {}

  getAccessToken(): string | null {
    return localStorage.getItem('accessToken');
  }

  getRefreshToken(): string | null {
    return localStorage.getItem('refreshToken');
  }

  storeTokens(tokens: TokenResponse): void {
    if (!tokens) return;
    localStorage.setItem('accessToken', tokens.accessToken);
    localStorage.setItem('refreshToken', tokens.refreshToken);
    if (tokens.expiresAt) localStorage.setItem('expiresAt', tokens.expiresAt);
  }

  /**
   * Ask backend to refresh tokens.
   * Returns TokenResponse or null if refresh failed.
   */
  refreshTokens(): Observable<TokenResponse | null> {
    const refreshToken = this.getRefreshToken();
    console.log('[REFRESH REQUEST SENT]', refreshToken);

    if (!refreshToken) {
      console.warn('No refresh token available; cannot refresh.');
      return of(null);
    }

    return this.http.post<TokenResponse>(`${this.baseUrl}/refresh`, { refreshToken }).pipe(
      tap((tokens) => {
        if (tokens) {
          console.log('[REFRESH SUCCESS]', tokens);
          this.storeTokens(tokens);
        }
      }),
      catchError((err) => {
        console.error('[REFRESH ERROR]', err);
        // return null so interceptor can handle logout gracefully
        return of(null);
      })
    );
  }

  logout(): void {
    localStorage.clear();
    // direct navigation to login page (keeps behavior simple and deterministic)
    window.location.href = '/login';
  }
}
