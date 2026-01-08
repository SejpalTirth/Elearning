import { inject } from '@angular/core';
import {
  HttpErrorResponse,
  HttpEvent,
  HttpInterceptorFn,
} from '@angular/common/http';
import {
  BehaviorSubject,
  EMPTY,
  Observable,
  catchError,
  filter,
  switchMap,
  take,
  throwError,
} from 'rxjs';

import { AuthService } from '../services/auth.service';

let isRefreshing = false;
const refreshSubject = new BehaviorSubject<boolean>(false);

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);

  // IMPORTANT: do NOT attach Authorization headers anymore
  const authReq = req.clone({ withCredentials: true });

  return next(authReq).pipe(
    catchError((error: unknown) => {
      if (
        error instanceof HttpErrorResponse &&
        error.status === 401 &&
        !isAuthEndpoint(authReq.url)
      ) {
        return handle401(authReq, next, auth);
      }

      return throwError(() => error);
    })
  );
};

function handle401(
  req: any,
  next: (req: any) => Observable<HttpEvent<any>>,
  auth: AuthService
): Observable<HttpEvent<any>> {

  if (!isRefreshing) {
    isRefreshing = true;
    refreshSubject.next(false);

    return auth.refreshTokens().pipe(
      switchMap(success => {
        isRefreshing = false;

        if (!success) {
          auth.logout();
          return EMPTY;
        }

        refreshSubject.next(true);
        return next(req);
      }),
      catchError(() => {
        isRefreshing = false;
        auth.logout();
        return EMPTY;
      })
    );
  }

  return refreshSubject.pipe(
    filter(done => done),
    take(1),
    switchMap(() => next(req))
  );
}

function isAuthEndpoint(url: string): boolean {
  return (
    url.includes('/GatewayAuth/refresh') ||
    url.includes('/GatewayAuth/logout') ||
    url.includes('/GatewayAuth/me')
  );
}
