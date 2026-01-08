import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

import { AuthService } from '@frontend/auth';
import { APP_ALLOWED_ROLE } from '../app-role';

@Injectable({ providedIn: 'root' })
export class AppBoundaryGuard implements CanActivate {

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  canActivate(): Observable<boolean> {
    return this.authService.getMe().pipe(
      map(user => {
        if (user.role !== APP_ALLOWED_ROLE) {
          this.router.navigateByUrl('/access-denied', { replaceUrl: true });
          return false;
        }
        return true;
      }),
      catchError(() => {
        this.router.navigateByUrl('/access-denied', { replaceUrl: true });
        return of(false);
      })
    );
  }
}
