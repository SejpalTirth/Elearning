import { Injectable } from '@angular/core';
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

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  private isRefreshing = false;
  private refreshTokenSubject = new BehaviorSubject<string | null>(null);

  constructor(private auth: AuthService) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // Attach access token (if present)
    const access = this.auth.getAccessToken();
    const authReq = access
      ? req.clone({ setHeaders: { Authorization: `Bearer ${access}` } })
      : req;

    return next.handle(authReq).pipe(
      catchError((err: any) => {
        // If 401 and this is NOT the refresh endpoint, try to refresh
        if (
          err instanceof HttpErrorResponse &&
          err.status === 401 &&
          !this.isRefreshUrl(req.url)
        ) {
          console.warn('[HTTP 401] - triggering refresh flow for', req.url);
          return this.handle401Error(authReq, next);
        }

        // propagate other errors
        throw err;
      })
    );
  }

  private isRefreshUrl(url: string): boolean {
    // adjust according to your backend refresh path
    return url.includes('/refresh') || url.includes('/GatewayAuth/refresh');
  }

  private handle401Error(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (!this.isRefreshing) {
      this.isRefreshing = true;
      this.refreshTokenSubject.next(null); // reset

      return this.auth.refreshTokens().pipe(
        switchMap((tokens) => {
          this.isRefreshing = false;

          // If refresh failed, tokens will be null → force logout and stop chain
          if (!tokens) {
            console.warn('Refresh failed. Logging out user.');
            this.auth.logout();
            // return EMPTY to complete observable chain cleanly (no retry)
            return EMPTY;
          }

          // successful refresh: new access token saved by AuthService.storeTokens
          this.refreshTokenSubject.next(tokens.accessToken);

          // retry original request with fresh access token
          const retry = req.clone({
            setHeaders: { Authorization: `Bearer ${tokens.accessToken}` }
          });
          return next.handle(retry);
        })
      );
    }

    // If a refresh is already in progress, wait for it to finish, then retry
    return this.refreshTokenSubject.pipe(
      filter((t) => t !== null),
      take(1),
      switchMap((token) => {
        const retryReq = req.clone({
          setHeaders: { Authorization: `Bearer ${token!}` }
        });
        return next.handle(retryReq);
      })
    );
  }
}
