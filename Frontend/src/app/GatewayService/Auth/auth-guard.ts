import { inject, Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { filter, map, take } from 'rxjs/operators';
import { AuthStateService } from './auth-state.service';

@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {

  private readonly authState = inject(AuthStateService);
  private readonly router = inject(Router);

  canActivate(): Observable<boolean> {
    return this.authState.status$.pipe(
      filter(status => status !== 'loading'),
      take(1),
      map(status => {
        if (status === 'authenticated') {
          return true;
        }

        this.router.navigate(['/login']);
        return false;
      })
    );
  }
}
