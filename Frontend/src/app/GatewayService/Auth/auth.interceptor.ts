import { inject, Injectable } from '@angular/core';
import {
  HttpInterceptor,
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpErrorResponse
} from '@angular/common/http';

import { Observable, BehaviorSubject, EMPTY } from 'rxjs';
import { catchError, switchMap, filter, take } from 'rxjs/operators';
import { AuthService } from './auth.service';
import { SecureTokenService } from '../Security/secure-token.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {

  private isRefreshing = false;
  private refreshTokenSubject = new BehaviorSubject<string | null>(null);

  private readonly auth = inject(AuthService);
  private readonly tokenService = inject(SecureTokenService);

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

    const decryptedJwt = this.tokenService.getDecryptedToken();

    const authReq = decryptedJwt
      ? req.clone({ setHeaders: { Authorization: `Bearer ${decryptedJwt}` } })
      : req;

    return next.handle(authReq).pipe(
      catchError((err: any) => {
        if (
          err instanceof HttpErrorResponse &&
          err.status === 401 &&
          !this.isRefreshUrl(req.url)
        ) {
          return this.handle401Error(authReq, next);
        }

        throw err;
      })
    );
  }

  private isRefreshUrl(url: string): boolean {
    return url.includes('/refresh') || url.includes('/GatewayAuth/refresh');
  }

  private handle401Error(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

    if (!this.isRefreshing) {
      this.isRefreshing = true;
      this.refreshTokenSubject.next(null);

      return this.auth.refreshTokens().pipe(
        switchMap(tokens => {
          this.isRefreshing = false;

          if (!tokens) {
            this.auth.logout();
            return EMPTY;
          }

          // Decrypted JWT after refresh
          const decrypted = this.tokenService.getDecryptedToken();
          this.refreshTokenSubject.next(decrypted);

          const retryReq = req.clone({
            setHeaders: { Authorization: `Bearer ${decrypted}` }
          });

          return next.handle(retryReq);
        })
      );
    }

    return this.refreshTokenSubject.pipe(
      filter(token => token !== null),
      take(1),
      switchMap(token => {
        const retryReq = req.clone({
          setHeaders: { Authorization: `Bearer ${token}` }
        });
        return next.handle(retryReq);
      })
    );
  }
}
