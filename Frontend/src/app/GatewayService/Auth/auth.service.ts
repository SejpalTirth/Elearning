import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { environment } from 'Environment/environment';
import { SecureTokenService } from '../Security/secure-token.service';

export interface TokenResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt?: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {

  private baseUrl = `${environment.baseapiurl}/GatewayAuth`;
  private userUrl = `${environment.baseapiurl}/GatewayUsers`;

  private readonly http = inject(HttpClient);
  private readonly secureToken = inject(SecureTokenService);

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
    if (tokens.expiresAt)
    {localStorage.setItem('expiresAt', tokens.expiresAt);}

    this.secureToken.setEncryptedToken(tokens.accessToken);
  }

  refreshTokens(): Observable<TokenResponse | null> {
    const refreshToken = this.getRefreshToken();

    if (!refreshToken) {
      return of(null);
    }

    return this.http
      .post<TokenResponse>(`${this.baseUrl}/refresh`, { refreshToken })
      .pipe(
        tap((tokens) => {
          if (tokens) {
            this.storeTokens(tokens);
          }
        }),
       catchError(() => of(null))
      );
  }

  completeProfile(dto: { userId: string; name: string; roleId: number }): Observable<any> {
    return this.http.post(`${this.userUrl}/complete-profile`, dto);
  }

  logout(): void {
    const refreshToken = this.getRefreshToken();

    if (!refreshToken) {
      localStorage.clear();
      this.secureToken.clear();
      window.location.href = '/';
      return;
    }

    this.http.post(`${this.baseUrl}/logout`, { refreshToken })
      .pipe(
        catchError(() => of(null))
      )
      .subscribe(() => {
        localStorage.clear();
        this.secureToken.clear();
        window.location.href = '/';
      });
  }

  loginWithGoogle(): void {
    window.location.href = `${this.baseUrl}/google-login`;
  }

  loginWithMicrosoft(): void {
    window.location.href = `${this.baseUrl}/microsoft-login`;
  }

}
