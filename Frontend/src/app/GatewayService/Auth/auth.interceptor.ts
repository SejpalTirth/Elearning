import { Injectable, inject } from '@angular/core';
import {
  HttpInterceptor,
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpErrorResponse
} from '@angular/common/http';

import { Observable, BehaviorSubject, EMPTY, throwError } from 'rxjs';
import { catchError, switchMap, filter, take } from 'rxjs/operators';

import { AuthService } from './auth.service';
import { AuthStateService } from 'app/GatewayService/Auth/auth-state.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {

  private isRefreshing = false;
  private refreshSubject = new BehaviorSubject<boolean>(false);

  private readonly auth = inject(AuthService);
  private readonly authState = inject(AuthStateService);

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

    const token = this.auth.getAccessToken();

    const authReq = token
      ? req.clone({
          setHeaders: { Authorization: `Bearer ${token}` }
        })
      : req;

    return next.handle(authReq).pipe(
      catchError(error => {
        if (
          error instanceof HttpErrorResponse &&
          error.status === 401 &&
          !this.isAuthEndpoint(req.url)
        ) {
          return this.handle401(authReq, next);
        }

        return throwError(() => error);
      })
    );
  }

  private handle401(
    req: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {

    if (!this.isRefreshing) {
      this.isRefreshing = true;
      this.refreshSubject.next(false);

      return this.auth.refreshTokens().pipe(
        switchMap(tokens => {
          this.isRefreshing = false;

          if (!tokens) {
            this.forceLogout();
            return EMPTY;
          }

          // Refresh succeeded
          this.refreshSubject.next(true);

          const retryReq = req.clone({
            setHeaders: {
              Authorization: `Bearer ${this.auth.getAccessToken()}`
            }
          });

          return next.handle(retryReq);
        }),
        catchError(() => {
          this.isRefreshing = false;
          this.forceLogout();
          return EMPTY;
        })
      );
    }

    // Wait for refresh to finish
    return this.refreshSubject.pipe(
      filter(done => done === true),
      take(1),
      switchMap(() => {
        const retryReq = req.clone({
          setHeaders: {
            Authorization: `Bearer ${this.auth.getAccessToken()}`
          }
        });
        return next.handle(retryReq);
      })
    );
  }

  private isAuthEndpoint(url: string): boolean {
    return (
      url.includes('/GatewayAuth/login') ||
      url.includes('/GatewayAuth/refresh') ||
      url.includes('/GatewayAuth/logout') ||
      url.includes('/GatewayAuth/me')
    );
  }

  private forceLogout(): void {
    this.auth.logout();        // Clears tokens
    this.authState.clear();   // Invalidates /me + state
  }
}
