import { Inject, Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { catchError, map, finalize } from 'rxjs/operators';

import { GatewayAuthService } from '@frontend/api';
import { AuthUser } from '../models/auth-user.model';
import { API_BASE_URL } from '@frontend/core';

@Injectable({ providedIn: 'root' })
export class AuthService {
  constructor(
    @Inject(API_BASE_URL) private readonly apiBaseUrl: string,
    private readonly router: Router
  ) {}

  private readonly authClient = inject(GatewayAuthService);

  // ------------------------------------------------
  // AUTH API
  // ------------------------------------------------

  /** GET /me */
  getMe(): Observable<AuthUser> {
    return this.authClient.getApiGatewayAuthMe<AuthUser>({
      withCredentials: true
    });
  }

  /** POST /refresh */
  refreshTokens(): Observable<boolean> {
    return this.authClient.postApiGatewayAuthRefresh({})
      .pipe(
        map(() => true),
        catchError(() => of(false))
      );
  }

  logout(): void {
  const redirectUrl = encodeURIComponent('http://localhost:4200/login');

  window.location.href =
    `${this.apiBaseUrl}/api/GatewayAuth/logout?redirectUrl=${redirectUrl}`;
}


  // ------------------------------------------------
  // OAUTH redirects
  // ------------------------------------------------

  loginWithGoogle(): void {
    window.location.href =
      `${this.apiBaseUrl}/api/GatewayAuth/google-login`;
  }

  loginWithMicrosoft(): void {
    window.location.href =
      `${this.apiBaseUrl}/api/GatewayAuth/microsoft-login`;
  }
}
